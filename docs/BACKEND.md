# Backend Documentation (`Contacts.Api`)

## 1) Overview

`Contacts.Api` is a .NET Web API that powers the phone-book domain.
It exposes contact/tag endpoints, supports CSV import/export, and persists data in SQL Server.

Location:
- `src/backend/Contacts.Api`

## 2) Core Features

- Contacts CRUD
- Soft delete + restore flow
- Search/filter/sort/pagination for contact listing
- Favorites and tag assignments
- Tag CRUD
- CSV export for filtered contacts
- CSV import with row-level validation errors
- Global exception handling middleware (`ProblemDetails` response on unhandled errors)
- Startup database prepare flow (migration or ensure-created based on config)

## 3) Technology Stack

- .NET `10.0`
- ASP.NET Core Web API
- Entity Framework Core `10.0` with SQL Server provider
- AutoMapper for entity -> DTO mapping
- FluentValidation for request validation
- CsvHelper for CSV read/write

## 4) NuGet Packages

| Package | Version | Purpose |
|---|---:|---|
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | `12.0.1` | AutoMapper DI integration |
| `CsvHelper` | `33.0.1` | CSV import/export handling |
| `FluentValidation.AspNetCore` | `11.3.1` | Request validation pipeline |
| `Microsoft.EntityFrameworkCore.SqlServer` | `10.0.0` | SQL Server ORM provider |
| `Microsoft.EntityFrameworkCore.Design` | `10.0.0` | EF tooling/migrations support |

## 5) Project Structure

- `Controllers` -> REST endpoints (`ContactsController`, `TagsController`)
- `Data` -> `ContactsDbContext`, EF migrations
- `Domain/Entities` -> `Contact`, `Tag`, `ContactTag`
- `Dtos` -> request/response contracts
- `Services` -> tag sync, CSV logic, query filter/sort extensions
- `Validation` -> FluentValidation validators
- `Infrastructure` -> AutoMapper profile + global exception middleware

## 6) API Endpoints

Base:
- `/api/contacts`
- `/api/tags`

### Contacts

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/contacts` | Paginated list with query filters |
| `GET` | `/api/contacts/{id}` | Single contact |
| `POST` | `/api/contacts` | Create contact |
| `PUT` | `/api/contacts/{id}` | Update contact |
| `DELETE` | `/api/contacts/{id}` | Soft delete |
| `POST` | `/api/contacts/{id}/restore` | Restore soft-deleted contact |
| `GET` | `/api/contacts/export` | Export CSV |
| `POST` | `/api/contacts/import` | Import CSV (`multipart/form-data`) |

`GET /api/contacts` query parameters:
- `search`
- `page` (default `1`)
- `pageSize` (default `20`, max `100`)
- `sortBy` (`firstName`, `lastName`, `createdAt`)
- `sortDir` (`asc`, `desc`)
- `favoriteOnly` (`true/false`)
- `tag`
- `company`

### Tags

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/tags` | List tags |
| `POST` | `/api/tags` | Create tag |
| `DELETE` | `/api/tags/{id}` | Delete tag |

## 7) Validation Rules

### Contact Create/Update

- `FirstName`: required, max `100`
- `LastName`: required, max `100`
- `Phone`: required, max `30`
- `Email`: optional, valid email, max `200`
- `Company`: optional, max `200`
- `Tags`: each tag non-empty, max `50`, unique (case-insensitive)

### Tag Create

- `Name`: required, max `50`

Additional business checks in controllers/services:
- Duplicate active phone number is rejected (`409 Conflict`)
- Duplicate tag name is rejected (`409 Conflict`)
- Restore fails if phone number collides with active contact (`409 Conflict`)

## 8) Data Model Summary

### Contact

- `Id` (`Guid`)
- `FirstName`, `LastName`, `Phone` (required)
- `Email`, `Company`, `Notes` (optional)
- `IsFavorite` (`bool`)
- `CreatedAt`, `UpdatedAt`
- `IsDeleted`, `DeletedAt` (soft delete)

### Tag

- `Id` (`int`)
- `Name` (unique)

### ContactTag

- Composite key: `ContactId + TagId`
- Many-to-many relation between contacts and tags

EF configuration highlights:
- Unique filtered index on `Contacts.Phone` where `IsDeleted = 0`
- Query filter to exclude soft-deleted contacts by default
- Indexes on `IsDeleted` and `IsFavorite`

## 9) Configuration

Primary config file:
- `src/backend/Contacts.Api/appsettings.json`

Important keys:
- `ConnectionStrings:DefaultConnection`
- `Database:ApplyMigrationsOnStartup`
- `Cors:AllowedOrigins`

Startup wiring (`Program.cs`):
- SQL Server DbContext registration
- AutoMapper registration
- FluentValidation auto-validation + validator scan
- CORS policy named `Frontend`
- Global exception middleware

## 10) Build, Run, Test

### Run API (local)

```bash
DOTNET_CLI_HOME=/tmp/dotnet dotnet run --project src/backend/Contacts.Api
```

### Build API

```bash
dotnet build src/backend/Contacts.Api/Contacts.Api.csproj
```

### Run Tests

```bash
dotnet test tests/Contacts.Api.Tests/Contacts.Api.Tests.csproj -v minimal
```

Test project dependencies (`tests/Contacts.Api.Tests`):
- xUnit
- ASP.NET Core integration test host (`Microsoft.AspNetCore.Mvc.Testing`)
- EF Core InMemory provider
- Coverlet collector

## 11) Docker

API Dockerfile:
- `src/backend/Contacts.Api/Dockerfile`

With root `docker-compose.yml`:
- SQL Server exposed on `1433`
- API exposed on `5050`
- `Database__ApplyMigrationsOnStartup=true` in container environment

