# Roadmap: Phase 2 (Analytics Engine, Authentication & Import Management)

This phase expands the backend platform by implementing the analytical queries, securing the API using stateless JWT authentication, and providing the user with controls to manage their import history.

## Step 1: User Identity & Security (JWT)

- [x] Implement `User` signup, sign-in, and password hashing in the Application layer.
- [x] Set up JWT Authentication middleware in `Frametric.Api`.
- [x] Implement sliding-expiration Refresh Tokens stored in PostgreSQL.
- [x] Replace the hardcoded MVP `Guid` with user claims extracted from the authenticated HTTP context (`HttpContext.User`).

## Step 2: High-Performance Analytics Engine (Dapper)

- [x] Integrate **Dapper** in the Infrastructure layer alongside EF Core.
- [x] Implement dedicated analytical SQL queries optimized with indexes on `DiaryEntries` (e.g., `WatchDate`, `Rating`, `MovieId`, `UserId`).
- [x] Create efficient queries to aggregate:
  - **Total Watchtime:** Sum of runtime from TMDB-enriched movies.
  - **Genre Distribution:** Count of diary entries grouped by genre.
  - **Director & Cast Leaderboards:** Top directors and actors based on watch counts and rating weights.
  - **Decade Breakdown:** Grouping watched films by their release decade.
  - **Monthly & Weekly Activity:** Histograms of activity patterns throughout the year.

## Step 3: CQRS Queries & Wrapped Summaries

- [x] Create `GetWrappedSummaryQuery` returning the full aggregated Spotify-Wrapped style dataset for a specific year.
- [x] Implement individual analytics queries (`GetTopDirectorsQuery`, `GetGenreDistributionQuery`, `GetMonthlyActivityQuery`) for detailed dashboard views.
- [x] Set up Redis caching wrappers (optional / progressive enhancement) for heavy analytical queries to minimize database load.

## Step 4: Import History & Cascade Deletion

- [x] Introduce an `ImportHistory` entity to log imports, execution time, row count, status (Success, Enriching, Failed), and provider source.
- [x] Implement `GetImportHistoryQuery` to list user imports.
- [x] Create `DeleteImportCommand` with cascade deletion rules to safely remove all `DiaryEntries`, `MovieRatings`, and `WatchlistItems` associated with a specific import batch without leaving orphan records.

## Step 5: Advanced Swagger Documentation & Contract Definition

- [x] Enhance API controllers with clear XML documentation and `[ProducesResponseType]` tags.
- [x] Standardize the OpenAPI specification (`/swagger/v1/swagger.json`) to serve as a robust blueprint for automatic frontend client generation.
