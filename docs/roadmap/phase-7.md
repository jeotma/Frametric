# Roadmap: Phase 7 (Pre-Launch Polish & Functional Refinement)

This phase focuses on polishing, refining, and improving all functional aspects of the Frametric platform before it is deployed to the cloud in Phase 8. It addresses missing critical workflows, optimizes existing mechanics, and ensures complete production readiness.

---

## 1. Authentication & Security Refinements

A critical missing feature for a production-ready application is the ability for users to recover their accounts.

### Password Reset Workflow

Implement a secure mechanism for users to request and perform a password reset via email.

* **Backend Implementation**:
  * Create an `IEmailService` interface and a concrete implementation (e.g., `SmtpEmailService` or an integration with a provider like SendGrid/Resend).
  * Generate secure, expiring, single-use tokens for the reset link.
  * New Endpoints:
    * `POST /api/auth/forgot-password`: Accepts an email, generates a token, and dispatches the reset link.
    * `POST /api/auth/reset-password`: Accepts the token and the new password, validates the token, and updates the user's credentials securely.
* **Frontend Implementation**:
  * Create "Forgot Password" and "Reset Password" views.
  * Adhere strictly to the platform's dark, premium aesthetic (`var(--bg-primary)`, `var(--accent-emerald)` for success states).

### Security Audits

* Ensure all API endpoints that mutate data require proper authorization.
* Validate all incoming DTOs to prevent injection attacks or malformed data.

---

## 2. UI/UX Polish and State Management

Ensure the platform feels entirely seamless and responsive under various network conditions.

* **Cinematic Loading States**: Replace generic spinners with skeleton loaders for dashboards, entity detail views, and discovery systems to prevent layout shifts and maintain immersion.
* **Graceful Error Handling**: Ensure all external API failures (e.g., TMDB timeouts) and backend validations are caught and displayed to the user via consistent, non-intrusive toast notifications.
* **Mobile Responsiveness Audit**: Thoroughly test and refine the layout of complex UI elements, especially the gamified Discovery systems (Slot Machine reels, Roulette, Bingo grids), to guarantee they function perfectly on mobile devices.

---

## 3. Performance & Architecture Optimization

Prepare the architecture to handle concurrent cloud traffic efficiently.

* **Caching Strategy**: Introduce an `ICacheService` (using `IMemoryCache` initially, abstracted for future Redis integration) to cache heavy analytical Dapper queries (e.g., "Top Directors", "Dynamic Duos") to prevent database bottlenecking on dashboard loads.
* **Angular Bundle Optimization**: Ensure that the gamified discovery routes and administrative dashboards are strictly lazy-loaded to minimize the initial application payload.
* **N+1 Query Prevention**: Perform a final audit of EF Core queries used in the application layer to ensure `Include` / `ThenInclude` or explicit Dapper queries are correctly utilized to prevent N+1 issues when fetching user watchlists or diary entries.

---

## 4. Observability, Telemetry & Testing

Production systems require visibility.

* **Structured Logging**: Implement Serilog (or equivalent) to capture structured logs. Ensure correlation IDs are passed through the HTTP pipeline to trace requests across the application, especially for multi-step gamified logic.
* **Health Check Endpoints**: Implement ASP.NET Core Health Checks at `/health`.
  * Verify the PostgreSQL connection (`Neon` readiness).
  * Verify external provider connectivity (TMDB / OMDB).
* **Test Coverage & Scope Review**: Conduct a comprehensive review of the testing suite, focusing especially on the Angular frontend. Analyze test coverage, identify critical missing paths, and ensure tests are reliable and deterministic.

---

## 5. Codebase Cleanup & Documentation Integrity

Follow the rules established in `AGENTS.md` before finalizing the release candidate.

* **Dead Code Elimination**: Sweep the repository to remove unused components, obsolete experimental DTOs, and commented-out code.
* **Clean Architecture Check**: Verify that the project still follows the clean architecture principles. Update `docs/architecture.md` if necessary.
* **API Specification**: Check and update if necessary every doc in the project, specially `docs/api/endpoints.md` to document the new `auth` endpoints introduced in this phase.
* **AGENTS.md**: Review `AGENTS.md` and update it to include the new features and workflows introduced in this phase.
* **License Verification**: Ensure all newly created C# files (e.g., the Email Service, new Auth Handlers) include the mandatory Frametric copyright and license header.
* **Changelog**: Prepare `CHANGELOG.md` with a comprehensive list of all features developed from Phase 1 through Phase 7 to mark the `v1.0.0` Release Candidate.
