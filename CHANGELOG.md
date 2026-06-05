# Changelog

All notable changes to **Frametric** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.1] — 2026-06-05

Manual watch management improvements and infrastructure bugfix.

### Added

- **Unlog Watch**: Users can now remove individual diary entries from the movie detail view. Deleting a non-rewatch entry purges all user associations with that film (`WatchedMovie`, `MovieRating`, `MovieLike`). Rewatches are treated as bonus entries and only the diary entry itself is removed.
- **DELETE `/api/Movies/{id}/log/{entryId}`**: New endpoint dispatching `UnlogMovieWatchCommand`.

### Fixed

- **FK Violation on Manual Log**: `LogMovieWatchCommand` was passing `Guid.Empty` as `ImportHistoryId` on `WatchedMovie`, causing a PostgreSQL 23503 FK constraint error. Fixed by making `ImportHistoryId` nullable (`Guid?`) in the domain entity, adding `WatchedMovieConfiguration` with `SetNull` delete behavior, and running the corresponding migration.

---

## [1.3.0] — 2026-06-05

Completed Phase 5: frontend entity detail pages wired and fully navigable across the app.

### Added

- **Movie Detail View**: Cinematic detail page with backdrop blur, poster, metadata, genres, directors, actors, user diary history, and manual log controls.
- **Actor Detail View**: Profile photo, filmography list, average rating, and watch count.
- **Director Detail View**: Profile photo, filmography list, average rating, and watch count.
- **Entity Routing**: Angular routes `/movies/:id`, `/actors/:id/:slug`, `/directors/:id/:slug` with lazy-loaded standalone components.
- **Cross-App Interlinking**: Movie titles, actor names, and director names in Stats and Recommendations are now clickable links routing to entity detail views.

---

## [1.2.3] — 2026-06-05

Recommendation strategy refinements following entity detail integration.

### Changed

- **DirectorsTrajectory Strategy**: Reworked scoring to factor in director filmography depth within the user's library.
- **Strategy Base**: Improved candidate filtering and scoring normalization shared across all strategies.
- **Minor Tuning**: `CinephileElite`, `ComfortZoneDisruptor`, `OppositeMood`, `RecentMood`, and `RuntimeContext` strategies received small logic adjustments for improved candidate diversity.

---

## [1.2.2] — 2026-06-05

Backend entity detail queries and API endpoints for Phase 5.

### Added

- **`GET /api/Movies/{id}`**: Returns movie details, genres, directors, actors, user diary entries, and average rating.
- **`GET /api/Actors/{id}`**: Returns actor details with filmography, watch count, and average rating from user library.
- **`GET /api/Directors/{id}`**: Returns director details with filmography, watch count, and average rating.
- **`POST /api/Movies/{id}/log`**: Manually log a watch entry with date, rating, and rewatch status.
- **`EntityDetailsQueriesImpl`**: Dapper-backed implementation for all three entity detail queries.

---

## [1.2.1] — 2026-06-05

TMDB profile photo enrichment for actors and directors.

### Added

- **ProfilePath on Actor/Director**: Domain entities `Actor` and `Director` now expose a `ProfilePath` property populated from TMDB `profile_path` during enrichment.
- **Migration `AddProfilePathToPeople`**: Adds the `ProfilePath` column to both `Actors` and `Directors` tables.
- **TmdbService update**: Credits response now maps `profile_path` for cast and crew members.

---

Integrated probability-based pop culture & cinephile easter eggs across the platform features.

### Added

- **Dynamic Easter Egg Service**: Introduced `EasterEggService` and `EasterEggPipe` in the frontend client to dynamically inject pop culture references, cinephile jokes, and meme badges based on statistics and movie criteria.
- **Loading Screen Memes**: Added randomized pop-culture/cinephile loading phrases to auth flows and analytics loading states.
- **Wellness Check Banner**: Scans user diary history for 3+ consecutive heavy movies (Existential Drama / Psychological Horror) watched within 24 hours inside the last 7 days. Adds a custom layout alert and skip options.
- **Pretentiometer Warning**: Computes whether the user watches slow arthouse films and dislikes blockbusters, displaying a custom warning banner on stats screen.
- **Mid-Curve Curse**: Triggers a tooltip alert when hovering over columns with exactly a `2.5` rating score.
- **Kevin Bacon Distance**: Mock degree of separation calculator modal for actor leaderboard listings.
- **Silent Film Monocle Projection**: Displays a custom retro icon and warning for movies released in 1920 or earlier.
- **Cult Search Box Commands**: Added keyboard listener triggers for matrix rain, reversed layout flip (Memento/Tenet), Jurasssic raptor peek-a-boo, Rosebud sleds, and bent inputs. Includes a dynamic pulsing easter egg description indicator badge next to the search input.

### Fixed

- **Dapper CandidateMovieDto Mapping**: Added a secondary constructor to `CandidateMovieDto` matching the returned columns and casting from the left-joined watchlist date. Fixes an `InvalidOperationException` where Dapper could not find a default or matching parameter constructor due to ignoring default parameter values in records.

---

## [1.1.3] — 2026-06-04

Mathematical fine-tuning and logic enhancements to recommendation strategies.

### Added

- **Cinephile Elite Refinements**: Redesigned scoring system to calculate low box office rewards, popularity ratio bounds, and dynamic foreign art-house multipliers.
- **Improved Temporal Decay**: Calibrated exponential decay weighting ($\lambda$) to balance older cinephile titles and recent view history context.
- **Pacing Filters**: Fine-tuned runtime context algorithms for shorter movies and endurance-length commitments.

---

## [1.1.2] — 2026-06-04

Refactored recommendation strategies logic to support dynamic metadata overlays and easter egg tooltips.

### Added

- **Easter Egg Tooltips**: Integrated randomized strategy, runtime, and obscurities easter egg triggers directly into recommendation cards via the new `EasterEggTooltip` DTO property.
- **Algorithmic Reason Generation**: Refactored recommendation strategies (`CinephileEliteStrategy`, `ComfortZoneDisruptorStrategy`, `DirectorsTrajectoryStrategy`, `GuiltyPleasureStrategy`, `OppositeMoodStrategy`, `RecentMoodStrategy`, `RuntimeContextStrategy`, and `PureRandomStrategy`) to dynamically construct more varied and randomized user-facing descriptions.

---

## [1.1.1] — 2026-06-03

Database schema migration and complete movies metadata re-enrichment.

### Added

- **Movies Re-enrichment**: Executed a system-wide metadata re-enrichment via the `TmdbEnrichmentBackgroundService` to populate the newly added database columns (awards, box office, streaming providers, country, overview, etc.) for all existing movies.
- **Cleanup**: Cleaned up the temporary database reset scripts, removed migration helper logic, and verified test suite coverage.

---

## [1.1.0] — 2026-06-03

Refactored and upgraded the cinematic recommendation engine.

### Added

- **Mathematical Foundations**: Integrated Cosine Similarity, Jaccard Index, and Exponential Temporal Decay ($e^{-\lambda \cdot t}$) for advanced content-based filtering.
- **Prestige & Discovery Metrics**: Leveraged global popularity indices, award counts (including Oscars), release country diversity, and pacing metrics to evaluate recommendation candidates.
- **Compartmentalized Strategy Pattern**: Refactored recommendation algorithms from a monolithic query handler into separate strategy classes implementing a unified interface.
- **Unique Scoring System**: Added micro-fractional tie-breakers based on movie metadata and ID hashing to guarantee unique match percentages and prevent ordering ties.

---

## [1.0.2] — 2026-06-03

Aesthetic polish and UI improvements to the discovery interface.

### Added

- **Visual Interface Refinement**: Enhanced the recommendations page view with premium glassmorphism layouts, responsive movie poster grids, and alignment indicators.
- **Micro-animations**: Added interactive hover animations on recommended candidate cards with dynamic match badge highlights.

---

## [1.0.1] — 2026-06-02

Early version of the cinematic discovery and recommendations module.

### Added

- **Discovery Page**: Implemented the initial frontend view for user recommendations (`recommendations.html`) connecting to the API client.
- **Monolithic Query Handler**: Configured `GetCinematicRecommendationsQueryHandler` with initial heuristic-based strategy branches (RecentMood, ComfortZoneDisruptor, RuntimeContext, etc.).
- **Basic Skip Cache**: Integrated distributed caching to allow users to temporarily dismiss/skip recommendation entries.

---

## [1.0.0] — 2026-06-02

First official stable release of Frametric, featuring robust automated testing, dynamic filtering, and refined UX layouts.

### Added

- **Playwright E2E Testing**: Fully integrated Playwright testing suite for automated frontend tests (`auth.spec.ts`, `navigation.spec.ts`, `stats_queries.spec.ts`).
- **Dynamic SQL Query Filtering**: System-wide dynamic filters in the analytical queries layer to filter records seamlessly by parameters like year.
- **Autohide Sidebar**: Added a modern hover-to-reveal animated sidebar to maximize cinematic screen estate.
- **Empty State Warning Banner**: Implemented UX notifications on the dashboard when no import history exists.

### Fixed

- **Dapper SQL Parsing**: Fixed missing SQL table FROM-clause entries by dynamically joining the `MovieRatings` table.
- **Final Cut Ratings Exclusion**: Prevented watched rating leaks and polished rewatch badge counts.

---

## [0.9.0] — 2026-06-01

Polished and launched the interactive slide experience.

### Added

- **Cinematic Presentation**: Implemented the full 20-slide year-in-review interactive deck ("The Final Cut") with custom exit handlers and layouts.
- **Escape Key Trigger**: Quick exit feature to close the slide deck.

### Fixed

- **Vite Cache & Styling**: Resolved Vite cache corruption issues and text overflows in slide cards.

---

## [0.8.0] — 2026-05-31

Overhauled user onboarding animations and sidebar layouts.

### Added

- **Final Cut Teaser**: Premium teaser card triggers and fade-in animations on the main dashboard.
- **Unique vs Rewatches Logic**: Segmented analytics counts to distinguish unique films from total logs.

---

## [0.7.0] — 2026-05-31

Introduced the advanced database analytics queries.

### Added

- **36 Advanced Queries**: Heavy-duty CQRS query handlers executing raw Dapper queries for genres proportions, casting repetition, oldest pending, and weekend binge behaviors.
- **Interactive Stats Explorer**: A frontend grid view allowing users to sort, search, and page through their logs with advanced column filters.

---

## [0.6.0] — 2026-05-31

Enhanced bulk file ingestion systems and user profiles dashboard.

### Added

- **ZIP Validation & Deduplication**: Added file header validations and switched movie deduplication strategies to map to title-year keys.
- **Dashboard Overview**: High-level indicator widgets (total watchtime, movie count, averages).
- **Import Center UI**: Dedicated page tracking import history batches and reprocessing triggers.

---

## [0.5.0] — 2026-05-30

Bootstrapped the client application.

### Added

- **Angular 19+ Setup**: Configured standalone component workspace with SCSS tokens.
- **OpenAPI Client Compilation**: Hooked up automated spec downloaders and code generator commands.
- **Authentication Flows**: Interactive Login and Registration frontend views.

---

## [0.4.0] — 2026-05-30

Secured the REST API and optimized queries.

### Added

- **JWT Authorization Pipeline**: Stateless access tokens with sliding-expiration PostgreSQL-persisted Refresh Tokens.
- **Dapper Integration**: Integrated micro-ORM alongside EF Core for fast read paths.
- **Backend Test Suite**: Implemented comprehensive handler and validator unit tests using xUnit and Moq.

---

## [0.3.0] — 2026-05-30

Built the asynchronous metadata enrichment process.

### Added

- **Channels Background Service**: In-memory Producer-Consumer queue via `System.Threading.Channels` for TMDB API enrichment.
- **Enrichment Search Engine**: Multi-tiered search fallbacks matching movies, TV shows, and miniseries by title-year vectors.

---

## [0.2.0] — 2026-05-30

Developed the ingestion parser core.

### Added

- **Letterboxd ZIP Importer**: Streaming parser mapping `diary.csv`, `ratings.csv`, `watched.csv`, and `watchlist.csv` using CsvHelper maps.
- **Movie Likes Mapping**: Added support for mapping explicit likes (`MovieLike` entity).

---

## [0.1.0] — 2026-05-30

Initial repository initialization.

### Added

- **Architecture Skeleton**: Modular Monolith solution setup (.NET 9 Domain, Application, Infrastructure, Api) with GNU GPLv3 license headers.
- **Entity Models & DB Context**: Initial EF Core schema maps for PostgreSQL and migrations.
