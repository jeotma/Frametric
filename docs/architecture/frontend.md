# Frontend Architecture

This document describes the structure, state management, and design patterns of the **Frametric Angular Client**.

---

## 1. Directory Structure

The frontend code resides in `frontend/src/app` and is organized following a clean, feature-driven folder structure:

```text
app/
 ├── components/           # Shared UI Layout and Standalone Shell Views
 │    ├── dashboard/       # Main overview dashboard
 │    ├── final-cut-teaser/# Teaser link to the cinematic presentation
 │    └── import-center/   # File upload and imports tracking center
 ├── core/                 # Singleton services, interceptors, and guards
 │    ├── api/             # Auto-generated API client code (OpenAPI)
 │    ├── guards/          # Route protection guards (authGuard)
 │    ├── interceptors/    # HTTP interceptors (authInterceptor)
 │    ├── services/        # Central core logic (Auth, TokenStorage, FinalCut, EasterEgg)
 │    └── utils/           # Shared utility functions (slugify, etc.)
 ├── features/             # Lazy-loaded feature domains
 │    ├── actors/          # Actor detail view (/actors/:id/:slug)
 │    ├── auth/            # Sign in and Register pages
 │    ├── directors/       # Director detail view (/directors/:id/:slug)
 │    ├── final-cut/       # "The Username's Cut" Spotify Wrapped-style slideshow
 │    ├── movies/          # Movie detail view with manual log/unlog (/movies/:id)
 │    ├── recommendations/ # Cinematic recommendation engine UI
 │    └── stats/           # Detailed analytics grids and graphs
 ├── app.config.ts         # Angular application bootstrapping and providers
 ├── app.routes.ts         # Core routing configuration with lazy loading
 └── app.scss              # Global styles, variables, and typography definitions
```

---

## 2. Standalone & Lazy Routing

- **Standalone Components**: The application is built using Angular standalone components, eliminating the need for `NgModule`.
- **Lazy Loading**: Route endpoints use dynamic imports (e.g., `loadComponent: () => import('./features/...').then(...)`) to reduce the initial bundle size and optimize load times.
- **Route Protection**: The `authGuard` protects dashboard and analytics views, redirecting unauthenticated users to `/login`.

---

## 3. State Management (Signals)

State is managed using modern Angular **Signals**, providing highly performant and reactive updates across the application.

- **Reactive State Flow**: Signals are used within core services (like `AuthService` to expose the current user state) and features (such as `FinalCutService` storing slides and progress).
- **Benefits**: Enforces deterministic state change cycles and avoids overhead associated with RxJS subscriptions in simple data bindings.

---

## 4. API Client Generation

To maintain strict contract consistency between the backend API and frontend client, TypeScript DTOs and HTTP services are automatically generated.

- **Tools**: `@openapitools/openapi-generator-cli`.
- **Flow**:
  1. The backend exposes the OpenAPI spec at `/swagger/v1/swagger.json` or `openapi.json`.
  2. The generator runs via npm scripts to output strongly typed services to `src/app/core/api`.
  3. Services are injected using Angular's Dependency Injection system.

---

## 5. Security & Authentication Interceptor

Authentication is handled via stateless JSON Web Tokens (JWT) with sliding-expiration Refresh Tokens:

### Auth Interceptor (`auth.interceptor.ts`)

- Appends the stored Access Token as an `Authorization: Bearer <token>` header to all outgoing API requests.
- Intercepts `401 Unauthorized` responses:
  - If a 401 occurs, it halts the failing request.
  - Calls `AuthService.refreshTokenAsync()` to exchange the Refresh Token for a new Access Token.
  - Re-tries the original request with the new token.
  - If refresh fails, it clears local credentials and redirects to `/login`.

### Token Storage (`token-storage.service.ts`)

- Manages secure storing, retrieving, and clearing of access/refresh tokens in `localStorage`.
