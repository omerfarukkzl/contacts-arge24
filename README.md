# Contacts ARGE24

Contacts ARGE24 is a phone book web application built with:

- SQL Server (Docker)
- .NET 10 Web API
- Angular 21 (standalone + signals + typed forms)

## Features

- Contacts CRUD
- Search, filter, sort, pagination
- Favorites and tags
- CSV import/export with row-level error report
- Soft delete + restore endpoint
- TR/EN language switching with `ngx-translate`

## Project Structure

- `src/backend/Contacts.Api` -> .NET Web API
- `src/frontend/contacts-web` -> Angular UI
- `tests/Contacts.Api.Tests` -> API unit/integration tests
- `docker-compose.yml` -> SQL Server + API + Web containers

## Component Documentation

- Frontend docs: `docs/FRONTEND.md`
- Backend docs: `docs/BACKEND.md`

## Local Run (without Docker)

1. Start SQL Server container:

```bash
docker compose up -d sqlserver
```

2. Run API:

```bash
DOTNET_CLI_HOME=/tmp/dotnet dotnet run --project src/backend/Contacts.Api
```

3. Run Web:

```bash
cd src/frontend/contacts-web
npm install
npm start
```

## Full Stack Run (Docker)

```bash
docker compose up --build
```

- API: `http://localhost:5050`
- Web: `http://localhost:4200`

## API Highlights

- `GET /api/contacts`
- `GET /api/contacts/{id}`
- `POST /api/contacts`
- `PUT /api/contacts/{id}`
- `DELETE /api/contacts/{id}` (soft delete)
- `POST /api/contacts/{id}/restore`
- `GET /api/contacts/export`
- `POST /api/contacts/import`
- `GET /api/tags`
- `POST /api/tags`
- `DELETE /api/tags/{id}`
