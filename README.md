# Plataforma de Eventos Online

MVP del reto técnico: arquitectura **microservicios + event-driven** con **Clean
Architecture & DDD**, dos APIs en **.NET 9** y un frontend en **React**.

- **EventService** — registra/publica eventos y zonas (SQL Server), cachea lecturas (Redis)
  y publica `EventCreated` a RabbitMQ con **transactional outbox**.
- **NotificationService** — consume `EventCreated` de forma **idempotente** y persiste en
  **MongoDB** (NoSQL).
- **EventPlatformUI** — pantalla "Registrar Evento" (React + Vite + TypeScript + Tailwind).

📐 Diseño completo: [`docs/architecture.md`](docs/architecture.md) ·
☁️ AWS y llaves: [`docs/aws-deployment.md`](docs/aws-deployment.md)

> ☁️ **AWS es opcional.** El reto solo pide *considerar* una arquitectura en nube **AWS o
> híbrido** a nivel de **diseño** (diagrama + sustentación), no desplegar el MVP en AWS. Este
> repositorio corre 100% en local con Docker; la parte AWS está cubierta como **diseño** en
> [`docs/aws-deployment.md`](docs/aws-deployment.md). Provisionar AWS es un paso extra
> opcional para demostrar el despliegue real (capa gratuita).

## 🚀 Quickstart (Docker)

Con **Docker Desktop** abierto:

```bash
cd "D:\Projects\Plataforma De Eventos Online"
docker compose up --build
```

Esto levanta toda la infraestructura (SQL Server, MongoDB, RabbitMQ, Redis), las **2 APIs** y la
**UI**. La BD `EventPlatform` se crea sola (contenedor `sqlserver-init`). La primera vez tarda
unos minutos.

| Servicio | URL |
|---|---|
| UI "Registrar Evento" | http://localhost:8082 |
| EventService (Swagger) | http://localhost:8080/swagger |
| NotificationService (Swagger) | http://localhost:8081/swagger |
| RabbitMQ (consola) | http://localhost:15672 (guest/guest) |

Probar el flujo (PowerShell):

```powershell
$token = (Invoke-RestMethod -Method Post http://localhost:8080/api/auth/token).access_token
$body = @{
  name = "Concierto Demo"; eventDate = "2026-12-01T20:00:00Z"; place = "Estadio Nacional"
  zones = @(@{ name="VIP"; price=200; capacity=100 }, @{ name="General"; price=50; capacity=5000 })
} | ConvertTo-Json
Invoke-RestMethod -Method Post http://localhost:8080/api/events `
  -Headers @{ Authorization = "Bearer $token" } -ContentType "application/json" -Body $body
Invoke-RestMethod http://localhost:8081/api/notifications   # verifica el consumo asíncrono
```

Apagar: `docker compose down` (agrega `-v` para borrar los datos).

> Para correr los 3 proyectos por separado (infra en Docker + APIs/UI a mano), ver
> [Opción B](#opción-b--ejecutar-en-local-sin-contenedores-para-las-apps) más abajo.

## Estructura del repositorio

```
.
├── EventPlatform/                # Backend (.NET 9) — una solución por microservicio
│   └── src/
│       ├── BuildingBlocks/EventPlatform.Contracts   # Contrato EventCreated (compartido)
│       └── Services/
│           ├── EventService/         EventService.sln · Core · Infrastructure · WebAPI
│           └── NotificationService/  NotificationService.sln · Core · Infrastructure · WebAPI
├── EventPlatformUI/              # Frontend React (feature-based)
├── database/                     # Script SQL (SSMS) + init de Mongo
├── docs/                         # Arquitectura y despliegue AWS
└── docker-compose.yml            # Infra + APIs + UI
```

## Requisitos

- **Docker Desktop** (forma recomendada de ejecutar todo), o
- Para desarrollo local: **.NET SDK 9**, **Node 20+**, y SQL Server / Mongo / RabbitMQ / Redis.

---

## Opción A — Ejecutar todo con Docker (recomendado)

```bash
docker compose up --build
```

Esto levanta SQL Server, MongoDB, RabbitMQ, Redis, ambas APIs y la UI. El contenedor
`sqlserver-init` crea la base de datos `EventPlatform`, las tablas y datos de ejemplo.

| Servicio            | URL                                |
|---------------------|------------------------------------|
| UI (Registrar Evento) | http://localhost:8082            |
| EventService (Swagger) | http://localhost:8080/swagger   |
| NotificationService (Swagger) | http://localhost:8081/swagger |
| RabbitMQ console    | http://localhost:15672 (guest/guest) |

Apagar: `docker compose down` (agrega `-v` para borrar los volúmenes de datos).

---

## Opción B — Ejecutar en local (sin contenedores para las apps)

### 1. Infraestructura
Levanta solo la infraestructura con Docker:
```bash
docker compose up -d sqlserver sqlserver-init mongo rabbitmq redis
```
(o usa instancias propias y ajusta las cadenas de conexión).

### 2. Base de datos (SQL Server)
**Con SSMS**: abre y ejecuta [`database/01-eventservice-schema.sql`](database/01-eventservice-schema.sql).
Crea la BD `EventPlatform`, las tablas (`Events`, `Zones`, outbox) y datos de ejemplo.

**Alternativa con EF Core** (migración + seed por script):
```bash
cd EventPlatform
dotnet ef database update \
  --project src/Services/EventService/Infrastructure/EventService.Infrastructure \
  --startup-project src/Services/EventService/Presentation/EventService.WebAPI
```
> El script SQL ya marca la migración como aplicada, por lo que ambos caminos son compatibles.

MongoDB no requiere script: las colecciones e índices se crean automáticamente
(ver `database/mongo-init.js`).

### 3. APIs
```bash
# EventService
dotnet run --project EventPlatform/src/Services/EventService/Presentation/EventService.WebAPI
# NotificationService
dotnet run --project EventPlatform/src/Services/NotificationService/Presentation/NotificationService.WebAPI
```
Revisa las cadenas de conexión en cada `appsettings.json` (apuntan a `localhost`).

### 4. Frontend
```bash
cd EventPlatformUI
cp .env.example .env      # ajusta VITE_API_URL si hace falta
npm install
npm run dev               # http://localhost:5173
```

---

## API EventService

| Método | Ruta               | Auth        | Descripción |
|--------|--------------------|-------------|-------------|
| POST   | `/api/auth/token`  | —           | Token JWT de demo (rol `Admin`). |
| POST   | `/api/events`      | JWT (Admin) | Crea evento + zonas (transacción) y publica `EventCreated`. |
| GET    | `/api/events`      | público     | Búsqueda avanzada de eventos (cache Redis). |
| GET    | `/api/events/{id}` | público     | Detalle de un evento. |
| GET    | `/health`          | —           | Health check. |

### Búsqueda avanzada — `GET /api/events`

Acepta criterios de búsqueda **opcionales** por query string. Se aplican en el **backend (SQL)**
y se combinan con **AND**; sin criterios devuelve todos los eventos. La respuesta se cachea en
Redis por combinación de criterios y se invalida automáticamente al crear un evento.

| Parámetro  | Tipo            | Descripción |
|------------|-----------------|-------------|
| `text`     | string          | Coincidencia parcial (case-insensitive) en **nombre o lugar**. |
| `from`     | fecha ISO-8601  | Eventos con fecha **≥** este valor (inclusive). |
| `to`       | fecha ISO-8601  | Eventos con fecha **≤** este valor (inclusive). |
| `maxPrice` | decimal         | Eventos con **alguna zona** cuyo precio sea **≤** este valor. |
| `status`   | string          | Estado exacto: `Published`, `Draft` o `Cancelled`. |

```bash
# Eventos publicados que contengan "Atlantic", desde sept-2026 y con entrada ≤ 100
curl "http://localhost:8080/api/events?status=Published&text=Atlantic&from=2026-09-01T00:00:00&maxPrice=100"
```

> La pantalla **Buscar** de la UI (`/eventos`) consume este endpoint: envía los criterios al
> backend (con *debounce*) y siempre filtra por `status=Published`.

### Probar el flujo end-to-end

```bash
# 1) Obtener token de demo
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/token | sed -E 's/.*"access_token":"([^"]+)".*/\1/')

# 2) Crear un evento
curl -X POST http://localhost:8080/api/events \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{
    "name": "Concierto Demo",
    "eventDate": "2026-12-01T20:00:00Z",
    "place": "Estadio Nacional",
    "zones": [
      { "name": "VIP", "price": 200, "capacity": 100 },
      { "name": "General", "price": 50, "capacity": 5000 }
    ]
  }'

# 3) Verificar que NotificationService procesó el evento (idempotente)
curl http://localhost:8081/api/notifications
```

La UI hace lo mismo: si `VITE_API_TOKEN` está vacío, pide el token automáticamente.

## Frontend — pantalla "Registrar Evento"

Formulario con **Nombre**, **Fecha**, **Lugar** y **Zonas** (lista editable: nombre, precio,
capacidad). Validación mínima (campos obligatorios, `capacidad > 0`, `precio >= 0`), manejo de
**loading/error** y envío con **JWT**.

## Notas de diseño

- **Outbox transaccional** (MassTransit + EF): el evento de integración se persiste en la
  misma transacción que los datos y se entrega tras el commit (no se pierden mensajes).
- **Idempotencia**: NotificationService reserva el `messageId` en MongoDB (`_id` único).
- **Resiliencia**: reintentos (MassTransit/EF), cache *fail-open* (Redis), health checks.

Ver el detalle y la sustentación en [`docs/architecture.md`](docs/architecture.md).

---

## 🛠️ Anexo operativo — acceso a datos, inspección y troubleshooting

> Esta sección complementa el reto: reúne los datos de conexión a cada motor y comandos útiles
> para inspeccionar el sistema en local. Todo corre en `localhost` (ver `docker-compose.yml`).
> En desarrollo, **Mongo y Redis corren sin autenticación** y SQL Server usa solo el usuario `sa`.

### 1. Conexión a las bases de datos e infraestructura

#### SQL Server (EventService)
GUI recomendada: **SQL Server Management Studio (SSMS)** o Azure Data Studio.

| Campo | Valor |
|---|---|
| Server name | `localhost,1433` |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | `Your_strong_Pass123` |
| Database | `EventPlatform` |
| Trust server certificate | ✅ (certificado autofirmado) |

> En SSMS marca **Trust server certificate** (*Options → Connection Properties*). El puerto se
> separa con coma (`localhost,1433`), no con dos puntos.

#### MongoDB (NotificationService)
GUI recomendada: **MongoDB Compass** (o la extensión *MongoDB for VS Code*).

| Campo | Valor |
|---|---|
| Connection string | `mongodb://localhost:27017` |
| Host / Puerto | `localhost` / `27017` |
| Usuario / Contraseña | *(sin autenticación)* |
| Database | `notificationdb` |

Colecciones: `notification_logs` (auditoría) y `processed_messages` (idempotencia).

#### Redis (cache de EventService)
Redis **no es HTTP** (no abre en el navegador) ni trae GUI propia. Usa `redis-cli` o **RedisInsight**.

| Campo | Valor |
|---|---|
| URL / Host | `localhost` |
| Puerto | `6379` |
| Contraseña | *(sin autenticación)* |
| Connection string (.NET) | `redis:6379` (dentro de Docker) · `localhost:6379` (desde el host) |

#### RabbitMQ (broker de mensajes)

| Campo | Valor |
|---|---|
| Consola web | http://localhost:15672 |
| Usuario / Contraseña | `guest` / `guest` |
| Puerto AMQP | `5672` |

> El usuario `guest` solo funciona desde `localhost` (RabbitMQ lo bloquea en conexiones remotas).
