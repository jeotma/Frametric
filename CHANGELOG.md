# Changelog

All notable changes to **Frametric** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
