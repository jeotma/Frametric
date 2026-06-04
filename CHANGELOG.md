# Changelog

All notable changes to **Frametric** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] — 2026-06-04

Integrated probability-based pop culture & cinephile easter eggs across the platform features.

### Added

- **Dynamic Easter Egg Service**: Introduced `EasterEggService` and `EasterEggPipe` in the frontend client to dynamically inject pop culture references, cinephile jokes, and meme badges based on statistics and movie criteria.
- **Loading Screen Memes**: Added randomized pop-culture/cinephile loading phrases to auth flows and analytics loading states.

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
