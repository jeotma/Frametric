# Modules Boundaries & Responsibilities

Frametric is designed as a Modular Monolith. The following modules exist within the application. They communicate through Application layer contracts (Commands/Queries/Events) and avoid direct internal coupling.

## 1. Users

**Responsibility:** Manages internal user profiles, settings, and preferences.
**Boundaries:** Holds the `User` aggregate root. It is not responsible for authentication.

## 2. Authentication

**Responsibility:** Manages identity, JWT token generation, password hashing, and role-based access.
**Boundaries:** Interfaces with the Users module only through defined contracts to verify existence.

## 3. Letterboxd Ingestion

**Responsibility:** Dedicated entirely to reading `.zip` files, parsing CSVs, validating missing fields, and orchestrating the translation of Letterboxd models into application Commands.
**Boundaries:** Exists purely in the Infrastructure/Application layer. It must NEVER pass Letterboxd-specific DTOs directly into the core domain.

## 4. Movies

**Responsibility:** Manages the core cinematic data (`Movie`, `ExternalReference`). Handles deduplication logic when new external references are ingested.
**Boundaries:** Acts as the central reference point for other modules. The Analytics and Wrapped modules read heavily from here.

## 5. Analytics

**Responsibility:** Generates platform-wide and user-specific statistics (Top Genres, Monthly Activity, Total Runtime).
**Boundaries:** Relies heavily on Dapper for high-performance read-only queries against the database views.

## 6. Wrapped

**Responsibility:** Orchestrates the yearly/monthly "Wrapped" summary generation.
**Boundaries:** Aggregates data from the Analytics module and transforms it into highly visual DTOs for the frontend clients.

## 7. Infrastructure

**Responsibility:** Contains EF Core DbContext, Dapper Repositories, CsvHelper logic, external IO operations, TMDB API Client integration, and Background Job orchestration (using `System.Threading.Channels` and custom hosted services) for metadata enrichment and user viewing profile rebuilding.
**Boundaries:** Implements interfaces defined in the Application layer. It is the outermost layer.

---

## 8. Discovery

**Responsibility:** Orchestrates gamified cinematic exploration features (Roulette, Mystery Box, Dice Roll, Slot Machine, Bingo Board, available countries).
**Boundaries:** Communicates via Application layer queries and commands, interacting with user records and watchlist/diary items.

---

## 9. Administration & Diagnostics

**Responsibility:** Provides system monitoring, database statistics, external API diagnostic checks, a log ring buffer, and database cleanups (orphan purges, cache flushing, manual TMDB re-enrichment).
**Boundaries:** Restricts access to users in the Admin role and provides diagnostics for backend modules.
