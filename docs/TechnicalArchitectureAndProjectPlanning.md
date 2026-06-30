# Frametric — Technical Architecture & Project Planning

This document defines the architecture, technical vision, development standards, engineering practices, and long-term roadmap for **Frametric**, a modern cinematic analytics platform inspired by services like Letterboxd and Spotify Wrapped.

The system is centered around a high-performance **.NET 9 backend** capable of ingesting, normalizing, and exposing cinematic activity data from external providers, initially driven by bulk file imports (ZIP) and designed to scale toward automated APIs in the future.

---

## 1. Core Vision

Frametric is **not** a Letterboxd clone. The backend is treated as a centralized data platform rather than a simple support API for a single frontend application.

### Main Goals

* **Aggregate** cinematic activity from multiple providers into a single source of truth.
* **Normalize** heterogeneous external data structures.
* **Generate** high-value statistics, historical trends, and "Wrapped" style insights.
* **Support** multiple clients simultaneously (Web, Mobile, Dashboards).
* **Maintain** long-term scalability, strict boundaries, and enterprise-grade engineering discipline.

---

## 2. High-Level Architecture

The system follows a predictable, unidirectional data pipeline:

User ZIP Upload -> Ingestion Layer -> Normalization Layer -> Persistence Layer -> Public API -> Angular Frontend

### Recommended Architectural Style

* **Modular Monolith:** Kept inside a single solution for deployment simplicity but strictly segregated by modules.
* **Clean Architecture:** Strict dependency flow where the Domain layer has zero external dependencies.
* **CQRS-Inspired Application Layer:** Complete separation of write operations (Commands) and heavy analytical read operations (Queries) using MediatR.

---

## 3. Technology Stack

### Backend (.NET 9 / ASP.NET Core)

* **Framework:** .NET 9 / ASP.NET Core.
* **ORM (Transactional):** Entity Framework Core for standard CRUD, entity state management, and migrations.
* **Micro-ORM (Analytics):** Dapper for high-performance dashboard aggregations and complex raw SQL execution.
* **Mediation & CQRS:** MediatR.
* **Validation:** FluentValidation for strict schema and incoming data checks.
* **Observability:** Serilog (structured logging), Seq, and OpenTelemetry.

### Database

* **PostgreSQL:** Chosen for its advanced relational performance, heavy aggregation indexing, and native `JSONB` support for schema flexibility.
* **Redis:** Configured as a distributed caching layer (via StackExchange.Redis), falling back to in-memory distributed cache if no connection string is provided.

### Frontend (Angular 19)

* **Architecture:** Standalone Component Architecture.
* **State Management:** Signal-based reactive state, keeping global state management minimal.
* **Organization:** Feature folder organization with lazy loading.
* **Contracts:** Typed API clients auto-generated from the OpenAPI spec.

---

## 4. Project Directory Structure

The solution contains four primary core layers organized inside the `backend/` directory:

Frametric/
 ├── Frametric.sln
 ├── SampleData/                     # Ignored local folder for test files
 ├── frontend/                       # Frontend application (Angular)
 └── backend/
      ├── Frametric.Api/             # Minimal controllers, HTTP routing, JWT verification
      ├── Frametric.Application/     # CQRS Handlers, Use cases, DTOs, FluentValidators
      ├── Frametric.Domain/          # Pure business domain entities, Value Objects, Enums
      └── Frametric.Infrastructure/  # ZIP processing, CsvHelper parsers, EfCore DbContext, Dapper Repositories

---

## 5. External Source Ingestion & ZIP Processing

External data structures must **never** leak into the core domain. The system ingests Letterboxd data through a single compiled `.zip` file containing native exports.

[API Layer] Receive IFormFile (.zip)
       ↓
[Infrastructure Layer] Open ZipArchive in memory (No disk writes)
       ↓
[CsvHelper Parsers] Stream read diary.csv, ratings.csv, watched.csv, watchlist.csv
       ↓
[Application Layer] Execute ImportLetterboxdZipCommand
       ↓
[Normalization Layer] Apply data sanitization and deduplication rules
       ↓
[Persistence Layer] Save internal entities to PostgreSQL

### Strict Normalization Rules

1. **Year Formatting and Nulls:** Input data with decimal values representing years (e.g., `2022.0` inside `watchlist.csv`) must be explicitly cast to an `int`. Additionally, completely missing/empty years (which occur for unreleased movies) must be handled gracefully as `int?`.
2. **Boolean Flags (Rewatch):** Undefined or text-based flags inside rows (e.g., `"Yes"` inside the `Rewatch` column) must be automatically resolved into internal boolean default type values (`true` if "Yes", `false` if empty).
3. **Multiline Strings:** The parsing layer (e.g., CsvHelper) must be configured to support internal newlines inside double-quoted text blocks. This is critical for parsing `reviews.csv` and `comments.csv` without breaking the stream.
4. **Multi-table and Multi-value Structures:** Custom lists (`lists/*.csv`) contain initial metadata rows that the parser must explicitly skip before reaching tabular data. Also, fields like `Favorite Films` (in `profile.csv`) require manual string splitting (`.Split(',')`).
5. **Deduplication Strategy:** Prior to entity insertion, the system verifies the `Letterboxd URI` against existing references in the database. If a match is found, the system links the new user activity to the current internal entity instead of duplicating the movie record.

---

## 6. Domain Model

The domain model remains strictly provider-agnostic and focused on pure business logic. It contains no reference to CSV headers, JSON properties, or external URLs.

### Core Entities

* **User:** Represents the platform account.
* **Movie:** Pure cinematic representation containing title and release year metadata.
* **ExternalReference (Value Object):** Composed of `Source` (e.g., "Letterboxd") and `ExternalId` (e.g., the specific URI string).
* **DiaryEntry:** High-fidelity event log capturing watch dates, ratings, rewatch flags, and tags.
* **MovieRating:** Pure score entries detached from diary listings.
* **WatchlistItem:** Tracking metrics for pending lists.

---

## 7. CQRS and Application Layer

CQRS fits efficiently due to the intensive analytical nature of the platform.

### Example Commands (Write Operations)

* `RegisterUserCommand`
* `ImportLetterboxdZipCommand`
* `GenerateWrappedSummaryCommand`

### Example Queries (Read Operations - Handled by Dapper)

* `GetTopGenresQuery`
* `GetTopDirectorsQuery`
* `GetYearSummaryQuery`
* `GetMonthlyActivityQuery`

---

## 8. API & Security Standards

* **Routing:** Endpoints follow REST standards under `/api/` (or `/api/v1/` for recommendations and discovery modules).
* **Documentation:** Real-time OpenAPI/Swagger documentation enforced from day one.
* **Authentication:** Stateless JWT architecture supported by sliding expiration refresh tokens.
* **Authorization:** Strict role-based validation applied across actions.
* **Input Integrity:** Total validation executed via FluentValidation before executing any command.

---

## 9. Background Processing

Frametric implements an in-memory Channel-based background worker pipeline for movie metadata enrichment:

* **Trigger Mechanism**: Signals the background service when imports complete using System.Threading.Channels.
* **Batch Processing**: Enriches movies asynchronously in batches (default: 20, configured via `TmdbEnrichment:BatchSize`) with rate-limit delays (default: 10s delay, configured via `TmdbEnrichment:DelayBetweenBatchesSeconds`) to respect external APIs (TMDB).
* **Future Caching & Tasks**: Caching orchestration and periodic database maintenance remain planned for future updates.

---

## 10. Long-Term Vision (Future Features)

Beyond file processing, the platform's extensible design accommodates a wide feature map without breaking core implementations:

* Direct OAuth integrations with provider platforms.
* Social analysis, profiling metrics, and friend comparison maps.
* AI-driven movie recommendations based on user profiles.
* Native mobile clients compiled from shared TypeScript/C# schemas.

---

## 11. Project Success Criteria

The development iteration is considered successful when:

1. A user can upload an exported Letterboxd `.zip` file successfully.
2. The engine extracts, parses, and normalizes `diary`, `ratings`, `watched`, and `watchlist` schemas smoothly without database failure.
3. The Dapper query pipeline computes aggregated stats (e.g., most watched directors) under industry-standard response limits.
4. The Angular UI displays the charts and metrics responsively.

---

## 12. Quality Assurance and Testing Strategy

### Testing Principles

The testing strategy strictly follows the **Testing Pyramid**, prioritizing fast, isolated unit tests and minimizing slow integration tests. Unit tests must be fast, deterministic, and isolated from external dependencies. Integration tests verify the interactions between core layers, and end-to-end tests validate critical user journeys.

### Unit Testing

* **Scope:** Individual business logic units, DTOs, Value Objects, Handlers, and Validation Rules.
* **Framework:** xUnit.net.
* **Isolation:** Dependencies are mocked using Moq.
* **Key Tests:**
  * Import validation rules (e.g., handling decimals, missing fields).
  * CQRS Command and Query Handlers.
  * FluentValidation Validators.

### Integration Testing

* **Scope:** Inter-layer communication (e.g., API -> Services -> Repositories), Database interactions, and ZIP processing pipelines.
* **Framework:** xUnit.net with `IClassFixture`.
* **Database:** In-memory SQLite for unit-level integration tests; actual PostgreSQL for end-to-end scenarios.
* **Key Tests:**
  * End-to-end Import Workflow (ZIP upload to persistence).
  * CQRS pipeline execution.
  * Database migrations and schema validation.

### End-to-End (E2E) Testing

* **Scope:** Critical user journeys from frontend to database.
* **Tools:** Playwright.
* **Key Scenarios:**
  * User registration and login.
  * ZIP file upload and data ingestion.
  * Navigation to analytics pages and chart rendering.

---

## 13. Deployment and DevOps Strategy

### Containerization

The entire system is designed to run in Docker containers, enabling consistent environments across development, staging, and production.

### Docker Compose

A `docker-compose.yml` orchestrates the full stack:

* `frametric-api`: The .NET 9 backend service.
* `frametric-db`: PostgreSQL database instance.
* `frametric-cache`: Redis cache instance (future use).
* `frametric-seq`: Structured logging viewer.
* `frametric-adminer`: Optional database administration UI.

### Environment Variables

All environment-specific configurations (database credentials, API keys, feature flags) are managed through environment variables, allowing seamless deployment to different environments.

### CI/CD Pipeline

* **GitHub Actions** automates the build, test, and deployment workflow.
* **Build:** Compiles both backend and frontend applications.
* **Test:** Runs all unit, integration, and E2E tests.
* **Deploy:** Pushes Docker images to a container registry (e.g., GitHub Container Registry) and updates the production deployment.
