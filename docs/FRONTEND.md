# Frontend Documentation (`contacts-web`)

## 1) Overview

`contacts-web` is the Angular frontend of Contacts ARGE24.
It provides contact management screens and communicates with the backend API.

Location:
- `src/frontend/contacts-web`

## 2) Core Features

- Contacts listing with search, filter, sort, and pagination
- Contact create/edit form with validation feedback
- Contact detail view
- Favorite toggle and delete/restore flow
- Tag-aware filtering and tag submission in forms
- CSV export and CSV import with row-level error report
- TR/EN language switch with persisted selection (`localStorage`)
- Responsive UI (desktop + mobile navigation/layout adaptations)

## 3) Technology Stack

- Angular `21.1.x` (standalone components, lazy-loaded routes)
- TypeScript `~5.9.2`
- RxJS `~7.8.0`
- `@ngx-translate/core` + `@ngx-translate/http-loader` for i18n
- SCSS styling
- Vitest (Angular unit-test builder) for frontend tests
- Nginx for container runtime serving

## 4) NPM Packages

### Runtime Dependencies

| Package | Version | Purpose |
|---|---:|---|
| `@angular/common` | `^21.1.0` | Angular common directives/services |
| `@angular/compiler` | `^21.1.0` | Angular template compiler |
| `@angular/core` | `^21.1.0` | Angular core runtime |
| `@angular/forms` | `^21.1.0` | Reactive forms |
| `@angular/platform-browser` | `^21.1.0` | Browser bootstrap/runtime |
| `@angular/router` | `^21.1.0` | Client-side routing |
| `@ngx-translate/core` | `^17.0.0` | Translation runtime |
| `@ngx-translate/http-loader` | `^17.0.0` | Loads JSON translation files |
| `rxjs` | `~7.8.0` | Reactive streams/operators |
| `tslib` | `^2.3.0` | TS helper library |

### Development Dependencies

| Package | Version | Purpose |
|---|---:|---|
| `@angular/build` | `^21.1.3` | Build/test builders |
| `@angular/cli` | `^21.1.3` | Angular CLI |
| `@angular/compiler-cli` | `^21.1.0` | Angular AOT compiler integration |
| `typescript` | `~5.9.2` | Type checking/build input |
| `vitest` | `^4.0.8` | Test runner |
| `jsdom` | `^27.1.0` | Browser-like test environment |

## 5) Application Architecture

### 5.1 Routing

Defined in:
- `src/frontend/contacts-web/src/app/app.routes.ts`

Routes:
- `/contacts` -> contacts list page
- `/contacts/new` -> create contact page
- `/contacts/:id` -> contact detail page
- `/contacts/:id/edit` -> edit contact page
- `/contacts/import-export` -> CSV import/export page

### 5.2 Main Layers

- `src/app/shared/layout`:
  - Global shell/navigation (`AppShellComponent`)
- `src/app/features/contacts/pages`:
  - Page-level UI for list, form, detail, import/export
- `src/app/core/services`:
  - API clients (`ContactsApiService`, `TagsApiService`)
  - language state (`LanguageService`)
- `src/app/core/models`:
  - frontend contracts (`Contact`, `Tag`, `PagedResult`, import result)

### 5.3 i18n

- Translation files:
  - `src/frontend/contacts-web/public/i18n/tr.json`
  - `src/frontend/contacts-web/public/i18n/en.json`
- Loader config:
  - `src/frontend/contacts-web/src/app/app.config.ts`
- Language persistence key:
  - `contacts.language`

## 6) API Configuration

### Development API Base

- `src/frontend/contacts-web/src/environments/environment.ts`
  - `apiUrl: '/api'`
- Dev proxy:
  - `src/frontend/contacts-web/proxy.conf.json`
  - `/api` -> `http://localhost:5050`

### Production API Base

- `src/frontend/contacts-web/src/environments/environment.prod.ts`
  - `apiUrl: 'https://contacts-api-q3i6.onrender.com/api'`

Frontend services use:
- `${environment.apiUrl}/contacts`
- `${environment.apiUrl}/tags`

## 7) Build, Run, Test

From `src/frontend/contacts-web`:

```bash
npm install
npm start
npm run build
npm test -- --watch=false
```

NPM scripts:
- `start` -> `ng serve --proxy-config proxy.conf.json`
- `build` -> `ng build`
- `watch` -> `ng build --watch --configuration development`
- `test` -> `ng test`

## 8) Docker Runtime

Frontend Dockerfile:
- `src/frontend/contacts-web/Dockerfile`

Runtime behavior:
- Build stage: Node 22 + `npm run build`
- Runtime stage: Nginx 1.27 serving SPA
- Nginx config:
  - `src/frontend/contacts-web/nginx.conf`
  - Proxies `/api/*` requests to `http://api:5050/api/`

