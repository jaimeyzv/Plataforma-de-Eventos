# EventPlatformUI

Frontend en **React 18 + Vite + TypeScript + Tailwind CSS**, con **arquitectura basada en
features**. Implementa la pantalla "Registrar Evento".

```
src/
├── config/env.ts                       # Lee VITE_API_URL / VITE_API_TOKEN
└── features/events/
    ├── api/events.api.ts               # fetch a POST /api/events (+ token demo)
    ├── hooks/useCreateEvent.ts         # estado loading/error/result
    ├── components/event-form.tsx       # formulario + validación
    ├── components/zone-list.tsx        # lista editable de zonas
    ├── views/register-event-view.tsx
    ├── types/event.types.ts
    └── validation.ts                   # campos obligatorios, capacidad>0, precio>=0
```

## Configuración

```bash
cp .env.example .env
```
- `VITE_API_URL` — base del EventService (por defecto `http://localhost:8080`).
- `VITE_API_TOKEN` — JWT fijo opcional. Si está vacío, la app solicita un token de demo a
  `POST /api/auth/token`.

## Scripts

```bash
npm install
npm run dev       # desarrollo (http://localhost:5173)
npm run build     # build de producción (dist/)
npm run preview   # sirve el build
```

## Docker

```bash
docker build --build-arg VITE_API_URL=http://localhost:8080 -t eventplatform-ui .
docker run -p 8082:80 eventplatform-ui
```
