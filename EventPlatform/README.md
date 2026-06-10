# EventPlatform (backend)

Monorepo .NET 9 con **una solución por microservicio** (independientes, desplegables por
separado) en **Clean Architecture + DDD**. Ambas comparten el *building block*
`EventPlatform.Contracts` (única fuente de verdad del mensaje `EventCreated`).

```
src/
├── BuildingBlocks/EventPlatform.Contracts          # EventCreatedIntegrationEvent (contrato compartido)
└── Services/
    ├── EventService/
    │   ├── EventService.sln                          # Solución del microservicio EventService
    │   ├── Core/EventService.Domain                  # Entidades de dominio (Event, Zone)
    │   ├── Core/EventService.Application             # UseCases (MediatR), validación, abstracciones
    │   ├── Infrastructure/EventService.Infrastructure  # EF Core, MassTransit outbox, Redis
    │   └── Presentation/EventService.WebAPI          # Controllers, JWT, Swagger
    └── NotificationService/
        ├── NotificationService.sln                   # Solución del microservicio NotificationService
        ├── Core/NotificationService.Domain
        ├── Core/NotificationService.Application      # EventCreatedProcessor (idempotente)
        ├── Infrastructure/NotificationService.Infrastructure  # MongoDB, MassTransit consumer, MailKit
        └── Presentation/NotificationService.WebAPI
```

> Cada servicio se compila/despliega por separado vía su `.sln`. El contrato compartido es
> el motivo por el que el **contexto de build de Docker es la carpeta `EventPlatform/`**.

## Build & test

```bash
# Cada microservicio es una solución independiente
dotnet build src/Services/EventService/EventService.sln
dotnet build src/Services/NotificationService/NotificationService.sln
```

## Ejecutar

```bash
dotnet run --project src/Services/EventService/Presentation/EventService.WebAPI
dotnet run --project src/Services/NotificationService/Presentation/NotificationService.WebAPI
```

## Migraciones EF (EventService)

```bash
# Crear una migración nueva
dotnet ef migrations add <Nombre> \
  --project src/Services/EventService/Infrastructure/EventService.Infrastructure \
  --startup-project src/Services/EventService/Presentation/EventService.WebAPI \
  --output-dir Persistence/Migrations

# Aplicar a la base de datos
dotnet ef database update \
  --project src/Services/EventService/Infrastructure/EventService.Infrastructure \
  --startup-project src/Services/EventService/Presentation/EventService.WebAPI
```

Como alternativa, ejecuta `../database/01-eventservice-schema.sql` en SSMS.

## Configuración

Cada WebAPI usa `appsettings.json` (sobrescribible por variables de entorno con `__`):
`ConnectionStrings:Database`, `ConnectionStrings:Redis`, `RabbitMq:*`, `Jwt:*` (EventService),
`Mongo:*`, `Smtp:*` (NotificationService).

## Librerías clave

EF Core · MassTransit (+ EntityFrameworkCore outbox) · MediatR · AutoMapper ·
FluentValidation · Polly · StackExchange.Redis · MongoDB.Driver · MailKit · JwtBearer.
