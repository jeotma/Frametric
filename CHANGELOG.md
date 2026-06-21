# Changelog

All notable changes to **Frametric** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.6.0] — 2026-06-21

Phase 7 Release: Polish, Performance, and Security.

### Added

- **Password Reset Flow**: Implemented `ForgotPassword` and `ResetPassword` handlers, an `IEmailService` sending emails via SMTP, and new UI views for requesting and entering new passwords.
- **Query Caching**: Integrated `ICacheService` and `MemoryCacheService` to cache expensive queries (e.g., `MoviesQueryHandler` and `DashboardQueryHandler`), reducing database load for frequent hits.
- **Application Observability**: Configured Serilog to provide structured logging output to console and file, and implemented ASP.NET Core Health Checks for database and external provider connections.
- **Skeleton Loaders**: Integrated animated skeleton loaders across the frontend to improve perceived performance during initial data fetches.
- **E2E Testing Coverage**: Added complete Gamified Discovery flow and Password Reset flow Playwright tests to secure application paths.

### Changed

- **UI Polish & Error Handling**: Global `HttpInterceptor` now catches and displays clean error toasts for failed requests. Mobile responsiveness was audited and improved for Gamified Discovery modules.
- **Entity Framework Optimization**: Eliminated N+1 queries by proactively projecting relations in LINQ statements where needed.
- **Routing**: Optimized Angular lazy-loading and decoupled the dashboard from strict authentication (`authGuard`), making it accessible to guests.

### Fixed

- **Test Suite Integrity**: Corrected failing unit tests across `discovery.spec.ts` (mocking injected services) and `easter-egg.service.spec.ts` (correcting silent film assertions).

## [1.5.3] — 2026-06-21

### Added

- **Dice Match Calibration HUD Banner**: A contextual status banner now appears below the dice after a roll, indicating the precision of the match: Perfect Match → Approximate → Deviated → Extended → Maximum Deviation → Calibration Fault. Color and text intensity scale with how much the constraints had to be relaxed, without exposing the word "fallback" to the user.
- **Dynamic Constraint Relaxation**: The dice roll backend now iterates through constraints in a round-robin sequence (Quality → Popularity → Duration → Genre) when no exact match is found, incrementing a `matchDistance` counter returned to the frontend to drive the calibration HUD.
- **Personalized D20 Genre Ordering**: The Genre die now queries the user's all-time watch history and assigns higher die values to genres the user watches most (value 19 = most-watched genre, value 20 = Wildcard unchanged). Falls back to static alphabetical ordering for users without history.
- **Per-Material SVG Dice Visuals**: Each die now has a distinct visual material identity rendered via unique SVG gradient definitions: D3 = Polished Metal (silver-steel), D4 = Ruby Gem (deep red), D6 = Worn Leather (warm sepia), D12 = Emerald Mineral (translucent green), D20 = Legendary Gold (metallic warm gold).

### Changed

- **Dice Types Renamed**: Dice types re-mapped to their cinematic purpose — D3 = Duration (≤90m / 91–149m / ≥150m), D4 = Popularity (Blockbuster / Mainstream / Niche / Hidden Gem), D6 = Risk, D12 = Quality (12 tiers from Terrible to Absolute Cinema), D20 = Genre.
- **D20 Tooltip Updated**: Tooltip now explains that values 1–19 reflect the user's own genre preferences ordered by watch history, with value 20 as Wildcard.
- **Dice Tooltips Corrected**: All five dice tooltips updated to reflect the new dice role definitions and accurate range descriptions.

### Fixed

- **Fumble/Critical Die Names**: The in-game fumble and critical roll status messages were displaying stale die names (`Quality (D3)`, `Rarity (D4)`, etc.). Now correctly shows `Duration (D3)`, `Popularity (D4)`, `Risk (D6)`, `Quality (D12)`, `Genre (D20)`.
- **`matchDistance` API Sync**: Regenerated the OpenAPI client after adding `MatchDistance` to `DiceRollResultDto`, resolving a TypeScript compilation error (`TS2339: Property 'matchDistance' does not exist on type 'DiceRollResultDto'`).

## [1.5.2] — 2026-06-20

### Added

- **Admin Configuration Panel**: Introduced a dedicated administration panel (`/admin`) for users with the `Admin` role to manage system operations, view logs, check provider status, and trigger database maintenance.
- **Backend & Database Health Diagnostics**: Added a self-diagnostic latency check for the Frametric Backend and PostgreSQL database, showing connection status and DB ping times in the API Health tab.
- **User Search Filter**: Integrated a responsive search box in the User Management tab, allowing admins to dynamically filter the registered user list by username or email address.
- **Bingo Reroll Feature**: Added ability to reroll uncompleted bingo squares (limit: 1 for 3x3, 2 for 4x4, 3 for 5x5) with persistent count tracking, UI tooltips, and remaining rerolls indicator.

### Changed

- **Custom Selection IDs**: Modified Roulette, Dice, Slots, Mystery Box, and Bingo discovery components to pass both custom selection titles and unique entity Guid IDs.

### Fixed

- **Dice Roller Repeated Spin**: Fixed dice roller state locking after the first roll by correctly resetting rolling, settled, and special event variables.
- **Slot Machine Genre Override**: Replaced manual text input with a predefined list of movie genres using a cinematic dropdown component.
- **Movie Detail Page TMDB Loading**: Resolved loading failure on unreleased/TMDB-only detail pages by automatically calling TMDB movie enrichment when a numeric TMDB ID is requested.

## [1.5.1] — 2026-06-13

Discovery suite: visual and interaction overhaul with polyhedral dice and comprehensive filtering.

### Added

- **Polyhedral Dice System**: Redesigned the dice rollers to use unique polyhedral SVG silhouettes matching D3, D4, D6, D12, and D20 dice types.
- **Unified Winner Details**: Standardized detailed winner cards across all minigames featuring brand-approved gradients and direct clickable router links to movie detail pages.

### Changed

- **Discovery Visual Overhaul**: Completely redesigned the Discovery page layout to be wider (increased max-width to 1450px) and more immersive, adding slowly pulsing ambient projector light leaks and glassmorphic configuration controls (HUD) with aligned dimensions.
- **SVG-Based Roulette Wheel**: Replaced the flat CSS-gradient wheel with an interactive SVG movie-reel design featuring realistic sprocket holes, dynamic sector math, auto-scaling and truncated text, and pointer tick vibrations. Scaled container size to 550px, made sector colors more vibrant, and corrected the right pointer indicator to point inward.
- **Roulette Spin Physics**: Increased spin speed/count (18-23 spins) and duration (7000ms) with a custom long-tail deceleration curve (`cubic-bezier(0.05, 0.9, 0.1, 1)`) for a smooth, gradual stop.
- **Cylindrical 3D Slot Machine**: Overhauled the slot reels with a metallic cabinet, status lights, curved visor reflections, and continuous scrolling animations. Built an interactive 3D lever that pivots down when pulled.
- **Mystery Canisters Grid**: Reconstructed the mystery boxes as retro metallic film canisters. Accentuating reveals with violent shaking animations, 3D flying popped lids, and blinding glow poster emergence transitions.
- **Dice Logic Bounds**: Re-mapped the backend `DiceRollQueryHandler` boundaries, labels, and constraints to support the D3, D4, D6, D12, and D20 polyhedral layout.

### Fixed

- **Exclude Watched Filter**: Resolved an issue where watched films were still appearing by checking `WatchedMovies`, `DiaryEntries`, and `MovieRatings` tables in `DiscoveryQueriesImpl`.
- **Slot Reel Stop Bug**: Fixed an Angular `ngOnChanges` state deadlock preventing reels from executing stops on response.
- **Mystery Box State Persistence**: Fixed box-open states leaking between reloads by resetting internal flags on new box queries.

## [1.5.0] — 2026-06-07

Discovery suite: gamified interactive selection systems.

### Added

- **Roulette**: Random movie selection with optional persistence threshold mode where a movie must appear multiple times before selection.
- **Dice System**: Five cinematic dice types (Quality, Rarity, Risk, Complexity, Exploration) that determine recommendation characteristics via analytical constraints with critical/fumble events.
- **Slot Machine**: Five-reel search combination system (Genre, Decade, Director, Duration, Country) with random resolution of null reels and jackpot detection for premium matches.
- **Mystery Box**: Hidden movie selection with Standard, Thematic, Premium, FullReveal, and Strategy variants; individual box reveal endpoint.
- **Cinematic Bingo**: Long-term cinephile objectives grid (3×3/4×4/5×5) with automatic diary-based tracking via `DiscoveryObjective` entities and `DiscoveryObjectiveEvaluator`.
- **Discovery API Endpoints**: `POST /discovery/roulette`, `POST /discovery/dice`, `POST /discovery/slot-machine`, `POST /discovery/mystery-box`, `GET /discovery/mystery-box/{boxId}/reveal`, `GET /discovery/bingo`.
- **Discovery Documentation**: Section 10 (Discovery) added to `docs/api/endpoints.md`.
- **Unit Tests**: 25 new tests covering `DiscoveryObjectiveEvaluator`, `RouletteSelectionQueryHandler`, `MysteryBoxGenerationQueryHandler`, `DiceRollQueryHandler`, and `SlotMachineSpinQueryHandler`.

### Changed

- **Final Cut Aesthetics**: Replaced emojis with SVG icons across all slides, optimized layout sizing/margins for readability, and aligned background gradients/highlights with the official color palette (e.g., replacing purple with record red).
- **Visual Identity Rules**: Added strict SVG-only icon requirements, explicit color palette hex definitions, and the "Cinematic Data as Narrative" philosophical design rules to `AGENTS.md`.

### Fixed

- **Cinematic Selects**: Resolved type binding errors in dynamic select components in the stats view.
- **Visual Stacking Issues**: Eliminated rounded corners on posters to seamlessly integrate with sharp crosshair frames and fixed z-index overlapping hiding the top-left bracket.

## [1.4.0] — 2026-06-06

Major analytics expansion: Advanced Statistics page, entity detail page redesigns, UI accessibility overhaul, and search navigation fixes.

### Added

- **Advanced Statistics Page**: New standalone stats feature with dynamic CQRS-style queries, category/metric selection, unified cross-filtering (watch year, release year, rating range, actor/director/genre), per-query inputs, sortable columns with ARIA attributes, pagination, and sessionStorage state persistence for back-navigation.
- **Profile Card Inline Layout**: Actor/director rows in stats tables now show profile photos, names, and meta chips (watched count, average rating, and seen count).
- **Multi-Entity Name Splitting**: Comma-separated director/actor names in stats tables are split into individual clickable links, each navigating to the correct entity detail page.
- **Watchlist & Like Metrics**: Entity detail pages now display watchlist count with tooltip (pending movies) and liked count with tooltip (liked movies).
- **Unwatched Count**: Actor/director detail pages show unwatched film counts with eye icon.
- **Mural Poster Backdrops**: Detail pages display a scrolling mural of movie poster thumbnails behind the profile header.
- **`isWatched` Field**: `MovieDetailsDto` now includes an `isWatched` boolean for the authenticated user.
- **`ActorId`/`DirectorId` in Search**: `GlobalSearchResultDto` now exposes separate `actorId` and `directorId` fields to correctly handle persons who are both actor and director.
- **Unit Tests**: Added test specs for `AuthService`, `EasterEggPipe`, `EasterEggService`, `FinalCutService`, `TokenStorageService`, and `slugify`.
- **Accessibility**: Staggered entrance animations with `prefers-reduced-motion` support, `aria-live` regions, keyboard navigation (tabindex, role, escape-to-close), and ARIA sort indicators.

### Changed

- **Detail Page Redesign**: Actor, director, and movie detail pages rewritten with inline SVG stat icons (film, star, bookmark, heart, eye, clapperboard), clickable stat boxes with section scrolling, and responsive grid layouts.
- **UI Icon Standardization**: All Unicode emoji icons across the application replaced with inline SVG equivalents for consistent rendering.
- **Rating Scale Alignment**: Diary entry ratings and statistic computations now multiply Letterboxd 1–5 scores by 2 to align with the 10-point scale; tooltips explain the conversion.
- **Easter Egg Trigger Rate**: Increased from 2% to 15% for idle-state and auth-tagline easter eggs.
- **Search Query**: Rewritten to expose both `ActorId` and `DirectorId` separately, fixing navigation for persons registered as both roles.

### Fixed

- **Entity Navigation from Stats**: Fixed 404 errors when clicking a director/actor name in stats for persons who exist as both actor and director — the frontend now uses the correct `directorId`/`actorId` from the search result.
- **Dapper Materialization**: Fixed `InvalidOperationException` in `SearchEntitiesAsync` by aligning SELECT column order with the `GlobalSearchResultDto` constructor parameter order.

### Added API Endpoints

- **`GET /api/Directors/{id}`**: Director detail endpoint (now documented).
- **`GET /api/Actors/{id}`**: Actor detail endpoint (now documented).
- **`GET /api/Search?q=`**: Global search endpoint (now documented).

---

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

Integrated probability-based pop culture & cinephile easter eggs across the platform features.

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
