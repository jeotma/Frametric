# API Endpoints Specification

This document details the REST API endpoints exposed by the backend services. Most endpoints are prefixed with `/api/`, while the recommendations and discovery endpoints are prefixed with `/api/v1/`. All endpoints are secured via JWT Bearer authentication (where noted).

---

## 1. Authentication (`/api/v1/auth`)

Authentication endpoints are publicly accessible (`[AllowAnonymous]`).

### **POST** `/api/v1/auth/signup`

- **Description**: Registers a new user.
- **Request Body**:

  ```json
  {
    "username": "string",
    "email": "string",
    "password": "string"
  }
  ```

- **Responses**:
  - `200 OK`: Returns the registered user ID (`Guid`).
  - `400 BadRequest`: If the username or email is already in use or validation fails.

### **POST** `/api/v1/auth/login`

- **Description**: Authenticates user credentials and returns tokens.
- **Request Body**:

  ```json
  {
    "email": "string",
    "password": "string"
  }
  ```

- **Responses**:
  - `200 OK`: Returns the JWT token structure:

    ```json
    {
      "accessToken": "string",
      "refreshToken": "string",
      "expiresAt": "datetime"
    }
    ```

  - `400 BadRequest`: If credentials are invalid.

### **POST** `/api/v1/auth/refresh`

- **Description**: Renews an expired access token using a valid refresh token.
- **Request Body**:

  ```json
  {
    "refreshToken": "string"
  }
  ```

- **Responses**:
  - `200 OK`: Returns a new access token and refresh token.
  - `400 BadRequest`: If the refresh token is expired or invalid.

### **POST** `/api/v1/auth/forgot-password`

- **Description**: Sends a password reset email if the user exists.
- **Request Body**:

  ```json
  {
    "email": "string"
  }
  ```

- **Responses**:
  - `200 OK`: A reset link was sent (if the email exists).

### **POST** `/api/v1/auth/reset-password`

- **Description**: Resets a user's password using the provided token.
- **Request Body**:

  ```json
  {
    "email": "string",
    "token": "string",
    "newPassword": "string"
  }
  ```

- **Responses**:
  - `200 OK`: Password successfully reset.
  - `400 BadRequest`: Invalid token or validation error.

---

## 2. Ingestion & Imports (`/api/v1/import`)

All import endpoints require authentication.

### **POST** `/api/v1/import/letterboxd`

- **Description**: Accepts a Letterboxd exported `.zip` file, starts parsing, and triggers the asynchronous metadata enrichment task.
- **Content-Type**: `multipart/form-data`
- **Parameters**:
  - `file` (`IFormFile`, `.zip` file containing export files)
- **Responses**:
  - `200 OK`: Returns success status and import identifier.

    ```json
    {
      "success": true,
      "importId": "Guid",
      "message": "Archive imported successfully and enrichment has started."
    }
    ```

  - `400 BadRequest`: Missing file, wrong file format (must be `.zip`), or corrupt headers.

### **GET** `/api/v1/import/history`

- **Description**: Retrieves history of past user imports.
- **Responses**:
  - `200 OK`: Returns list of imports, processed row counts, and status:

    ```json
    [
      {
        "id": "Guid",
        "importedAt": "datetime",
        "fileName": "string",
        "status": "string",
        "diaryEntriesCount": 0,
        "ratingsCount": 0,
        "watchlistItemsCount": 0
      }
    ]
    ```

### **DELETE** `/api/v1/import/{id}`

- **Description**: Deletes a specific import batch and executes a cascade delete on all associated diary entries, ratings, watchlist items, and likes.
- **Parameters**:
  - `id` (`Guid` in route)
- **Responses**:
  - `200 OK`: Data deleted.
  - `404 NotFound`: Import not found.

---

## 3. General Analytics (`/api/v1/analytics`)

Requires authentication.

### **GET** `/api/v1/analytics/dashboard`

- **Description**: General metrics dashboard summary.
- **Responses**:
  - `200 OK`: Returns high-level counts:

    ```json
    {
      "totalWatched": 0,
      "totalWatchTimeMinutes": 0,
      "averageRating": 0.0,
      "uniqueFilmsCount": 0
    }
    ```

### **GET** `/api/v1/analytics/wrapped`

- **Description**: Generates the Spotify Wrapped style yearly summary.
- **Query Parameters**:
  - `year` (`int?`, optional)
- **Responses**:
  - `200 OK`: Returns complex structured stats for the specified year (or all-time if null).

### **GET** `/api/v1/analytics/monthly-activity/{year}`

- **Description**: Histogram of watched movies count per month.
- **Parameters**:
  - `year` (`int` in route)
- **Responses**:
  - `200 OK`: Array of monthly values.

### **GET** `/api/v1/analytics/top-directors`

- **Description**: Leaderboard of top directors.
- **Query Parameters**:
  - `limit` (`int`, default: 10)
- **Responses**:
  - `200 OK`: Returns array of director ranking items.

---

## 4. Advanced Analytics (`/api/v1/analytics/advanced`)

All routes require authentication. Filters are passed via query parameters mapping to `AnalyticsFilterDto` (e.g. `year`, `genre`).

### Category: Watched Metrics (`/api/v1/analytics/advanced/watched/*`)

- **GET** `/watched` - Returns full list of watched movies.
- **GET** `/watched/directors` - Returns directors leaderboard.
- **GET** `/watched/actors` - Returns actors leaderboard.
- **GET** `/watched/genres` - Returns genres count breakdown.
- **GET** `/watched/decades` - Returns decade distribution.
- **GET** `/watched/most-repeated-actor` - Returns the actor appearing most in watched films.
- **GET** `/watched/most-watched-director` - Returns the director with the most watch time/count.
- **GET** `/watched/predominant-era` - Returns the most common era/decade details.
- **GET** `/watched/director-ranking` - Leaderboard of directors sorted by average movie rating.
- **GET** `/watched/total-time` - Total runtime. Query parameters: `filterType` (Genre/Director), `filterName`.
- **GET** `/watched/preferred-day` - Most common day of the week for logging/watching.
- **GET** `/watched/rating-evolution` - Trends of user ratings over time.
- **GET** `/watched/genre-streaks` - Sequences of consecutive movies belonging to the same genre.
- **GET** `/watched/longest-movie` - Longest watched movie details.
- **GET** `/watched/casting-repetitions` - Frequently paired actors.

### Category: Watchlist Metrics (`/api/v1/analytics/advanced/watchlist/*`)

- **GET** `/watchlist` - Pending watchlist items list.
- **GET** `/watchlist/directors` - Leaderboard of directors for movies pending on watchlist.
- **GET** `/watchlist/actors` - Leaderboard of actors for movies pending on watchlist.
- **GET** `/watchlist/genres` - Count of watchlist items by genre.
- **GET** `/watchlist/decades` - Decades distribution of watchlist items.
- **GET** `/watchlist/most-anticipated-director` - Top-ranked watchlist directors.
- **GET** `/watchlist/most-anticipated-actor` - Top-ranked watchlist actors.
- **GET** `/watchlist/total-watchtime` - Cumulative runtime needed to watch all pending films.
- **GET** `/watchlist/oldest-pending` - The movie that has spent the most time on the watchlist.
- **GET** `/watchlist/by-era` - Watchlist distribution grouped by era.
- **GET** `/watchlist/ghost-actor` - Actors who feature in watchlist movies but the user has never watched.
- **GET** `/watchlist/golden-director` - Directors with high ratings in watched films who have pending films.
- **GET** `/watchlist/duration-balance` - Balance of short vs. long pending films.
- **GET** `/watchlist/genre-proportion` - Proportional comparison of genres: Watchlist vs. Watched.

### Category: Bonus Metrics (`/api/v1/analytics/advanced/bonus/*`)

- **GET** `/bonus/weekend-warrior` - Stats on weekend binge behaviors.
- **GET** `/bonus/hidden-gems` - Movies rated highly by the user that are obscure globally on TMDB.
- **GET** `/bonus/watchlist-graveyard` - Movies added to the watchlist years ago that remain unwatched.
- **GET** `/bonus/cinematic-fatigue` - Extended analysis of viewing streaks, drops in scores, or pauses.

### Category: Final Cut Metrics (`/api/v1/analytics/advanced/final-cut/*`)

- **GET** `/final-cut/bookends` - The first and last movie watched in a given period.
- **GET** `/final-cut/monthly-extremes` - Months with highest/lowest watch volumes or rating averages.
- **GET** `/final-cut/top-bottom-rated` - Best and worst rated movies list.
- **GET** `/final-cut/most-rewatched` - Movies with the highest rewatch frequency.
- **GET** `/final-cut/best-rookies` - Directors or actors newly discovered (first watch in current period) with high average scores.
- **GET** `/final-cut/prime-time` - Pacing/time distribution of logs (e.g. late night vs early morning).
- **GET** `/final-cut/genre-landscape` - Mapping of genres by average rating.
- **GET** `/final-cut/shortest-movie` - Shortest watched movie.
- **GET** `/final-cut/director-actor-pairs` - Director-Actor pairings that appear frequently in user's library.

---

## 5. Recommendations (`/api/v1/recommendations`)

Requires authentication.

### **POST** `/api/v1/recommendations/generate`

- **Description**: Generates custom movie recommendations for the authenticated user using the specified strategy and scope.
- **Request Body**:

  ```json
  {
    "strategy": "RecentMood",
    "scope": "WatchlistOnly",
    "quantity": 3,
    "maxRuntimeMinutes": 120
  }
  ```

- **Responses**:
  - `200 OK`: Returns an array of recommended movie items:

    ```json
    [
      {
        "movieId": "Guid",
        "title": "string",
        "directorName": "string",
        "releaseYear": 0,
        "matchPercentage": 92.5,
        "recommendationReason": "string",
        "posterUrl": "string",
        "runtimeMinutes": 0,
        "easterEggTooltip": "string"
      }
    ]
    ```

### **POST** `/api/v1/recommendations/skip/{movieid}`

- **Description**: Excludes a movie from future recommendation generation cycles for 24 hours.
- **Parameters**:
  - `movieId` (`Guid` in route)
- **Responses**:
  - `204 NoContent`: Successfully skipped and cached.

### **POST** `/api/v1/recommendations/skip-haunting`

- **Description**: Permanently opts the user out of the Watchlist Haunting easter egg recommendations override.
- **Responses**:
  - `204 NoContent`: Opt-out preference saved in distributed cache permanently.

### **POST** `/api/v1/recommendations/dismiss-wellness`

- **Description**: Temporarily dismisses the consecutive heavy watch "Wellness Check" easter egg banner alert for 7 days.
- **Responses**:
  - `204 NoContent`: Dismissal cached for 7 days.

---

## 6. Movies (`/api/v1/movies`)

Requires authentication.

### **GET** `/api/v1/movies/{id}`

- **Description**: Returns full details for a specific movie including directors, actors, genres, and the authenticated user's diary entries and average rating.
- **Parameters**:
  - `id` (`Guid` in route)
- **Responses**:
  - `200 OK`: Returns `MovieDetailsDto`.
  - `404 NotFound`: Movie not found.

### **POST** `/api/v1/movies/{id}/log`

- **Description**: Manually logs a watch for the authenticated user. Creates a `DiaryEntry` and a `WatchedMovie` record if it does not already exist. Optionally updates the user's rating.
- **Parameters**:
  - `id` (`Guid` in route)
- **Request Body**:

  ```json
  {
    "dateWatched": "2025-12-01",
    "rating": 8.5,
    "isRewatch": false
  }
  ```

- **Responses**:
  - `200 OK`: Watch logged successfully.
  - `404 NotFound`: Movie not found.

### **DELETE** `/api/v1/movies/{id}/log/{entryid}`

- **Description**: Removes a specific diary entry belonging to the authenticated user. If it was the last diary entry for that movie, **all** user associations with the film are purged: the `WatchedMovie` library record, the `MovieRating`, and any `MovieLike`.
- **Parameters**:
  - `id` (`Guid` in route) — Movie ID
  - `entryId` (`Guid` in route) — Diary entry ID to remove
- **Responses**:
  - `204 NoContent`: Entry deleted successfully.
  - `404 NotFound`: Entry not found or does not belong to the authenticated user.

> **Note**: Since [1.4.0], the `MovieDetailsDto` includes an `isWatched` boolean field indicating whether the authenticated user has watched the movie.

---

## 7. Directors (`/api/v1/directors`)

Requires authentication.

### **GET** `/api/v1/directors/{id}`

- **Description**: Returns full details for a specific director including filmography, watch/like/watchlist counts, and average rating. If the director is also an actor (matched by TMDB ID), acting filmography is also returned with an `isActor` flag.
- **Parameters**:
  - `id` (`Guid` in route)
- **Responses**:
  - `200 OK`: Returns `DirectorDetailsDto`.
  - `404 NotFound`: Director not found.

---

## 8. Actors (`/api/v1/actors`)

Requires authentication.

### **GET** `/api/v1/actors/{id}`

- **Description**: Returns full details for a specific actor including filmography, watch/like/watchlist counts, and average rating. If the actor is also a director (matched by TMDB ID), directed filmography is also returned with an `isDirector` flag.
- **Parameters**:
  - `id` (`Guid` in route)
- **Responses**:
  - `200 OK`: Returns `ActorDetailsDto`.
  - `404 NotFound`: Actor not found.

---

## 9. Search (`/api/v1/search`)

Requires authentication.

### **GET** `/api/v1/search`

- **Description**: Global search across movies, actors, and directors. Searches are performed against the local database first. If no local results are found, falls back to the TMDB external provider.
- **Query Parameters**:
  - `q` (`string`) — Search query text (partial matching supported).
- **Responses**:
  - `200 OK`: Returns an array of `GlobalSearchResultDto` with `entityType` (Movie/Actor/Director/Director / Actor), `localId`, `tmdbId`, `actorId`, `directorId`, and display metadata.
- **Behavior**:
  - Returns only local results if a local match exists.
  - Falls back to TMDB results when no local results are found.
  - For persons who are both an actor and a director, the entity type is `Director / Actor` and both `actorId` and `directorId` are populated separately.

---

## 10. Discovery (`/api/v1/discovery`)

Requires authentication.

### **POST** `/api/v1/discovery/roulette`

- **Description**: Selects a random movie from the discovery pool with absolute randomness. Supports optional persistence threshold mode where a movie must appear multiple times before being selected.
- **Request body**: `RouletteRequest` (`Scope`, `PersistenceThreshold?`, `CustomSourceIds?`, `CustomSourceTitles?`)
- **Responses**:
  - `200 OK`: Returns `SelectionResultDto`.
  - `400 BadRequest`: Pool empty or missing custom collection IDs.

### **POST** `/api/v1/discovery/dice`

- **Description**: Rolls one or more cinematic dice (Quality, Rarity, Risk, Complexity, Exploration) to determine the characteristics of the recommended film. Each die maps to analytical constraints (rating, popularity, runtime) that filter the pool.
- **Request body**: `DiceRollRequest` (`Scope`, `DiceTypes?`, `CustomSourceIds?`, `CustomSourceTitles?`)
- **Responses**:
  - `200 OK`: Returns `DiceRollResultDto` with per-die results and optional special event.
  - `400 BadRequest`: Pool empty.

### **POST** `/api/v1/discovery/slot-machine`

- **Description**: Spins 5 reels (Genre, Decade, Director, Duration, Country) to build a search combination. Null reels are randomly resolved from available pool data. Special jackpot combinations trigger premium rewards.
- **Request body**: `SlotMachineRequest` (`Scope`, `Genre?`, `Decade?`, `Director?`, `Duration?`, `Country?`, `CustomSourceIds?`, `CustomSourceTitles?`)
- **Responses**:
  - `200 OK`: Returns `SlotMachineResultDto` with reel results and jackpot flag.
  - `400 BadRequest`: Pool empty.

### **POST** `/api/v1/discovery/mystery-box`

- **Description**: Generates a set of hidden movie boxes for the user to choose from. Supports variant modes: Standard, Thematic (shared genre), Premium (top-rated), FullReveal (ranked), and Strategy (diverse genres with hints).
- **Request body**: `MysteryBoxRequest` (`Scope`, `Variant`, `BoxCount`, `CustomSourceIds?`, `CustomSourceTitles?`)
- **Responses**:
  - `200 OK`: Returns `MysteryBoxDto` with box IDs, variant, and optional hints.
  - `400 BadRequest`: Pool empty.

### **GET** `/api/v1/discovery/mystery-box/{boxid}/reveal`

- **Description**: Reveals the movie inside a specific mystery box by its movie ID.
- **Parameters**:
  - `boxId` (`Guid` in route)
- **Responses**:
  - `200 OK`: Returns `SelectionResultDto` with full movie details.
  - `400 BadRequest`: Movie not found.

### **POST** `/api/v1/discovery/bingo`

- **Description**: Returns the user's bingo grid with objectives and completion status. Creates default objectives when none exist or if request asks for a new board. Evaluates diary entries to automatically mark squares as completed.
- **Request Body**: `BingoRequest` (`GridSize`, `Scope`, `CustomSourceIds?`, `CustomSourceTitles?`, `ExcludeWatched`, `DurationDays?`)
- **Responses**:
  - `200 OK`: Returns `BingoGridDto` with grid size, square states, and rerolls details.

### **POST** `/api/v1/discovery/bingo/reroll/{objectiveid}`

- **Description**: Rerolls a single uncompleted bingo square's objective, replacing it with a new random objective from the pool if the user has rerolls remaining.
- **Parameters**:
  - `objectiveId` (`Guid` in route)
- **Responses**:
  - `200 OK`: Returns the updated `BingoGridDto`.
  - `400 BadRequest`: If the objective is already completed, the user has exceeded their grid-size reroll limit, or the objective does not exist.

---

## 11. Administration & Configuration (`/api/v1/admin`)

Requires authentication. Restricted to users with the `Admin` role.

### **GET** `/api/v1/admin/users`

- **Description**: Lists all registered users on the platform.
- **Responses**:
  - `200 OK`: Returns an array of `UserDto` (`id`, `username`, `email`, `role`).

### **POST** `/api/v1/admin/users/{userid}/promote`

- **Description**: Promotes a standard user to an Admin.
- **Parameters**:
  - `userId` (`Guid` in route)
- **Responses**:
  - `200 OK`: Promotion successful.
  - `404 NotFound`: User not found.

### **GET** `/api/v1/admin/diagnostics/database`

- **Description**: Retrieves aggregate library statistics (Counts of users, movies by enrichment status, TV shows, genres, directors, actors, and diary entries).
- **Responses**:
  - `200 OK`: Returns `DatabaseStatsDto`.

### **GET** `/api/v1/admin/diagnostics/providers`

- **Description**: Pings external API metadata providers (TMDB and OMDb) as well as the local Frametric backend & database connection, returning latency and health validation states.
- **Responses**:
  - `200 OK`: Returns `ProviderDiagnosticsDto` (including local database health status).

### **GET** `/api/v1/admin/diagnostics/logs`

- **Description**: Retrieves the last 50 warning or error logs recorded in the in-memory ring buffer.
- **Responses**:
  - `200 OK`: Returns an array of `LogEntryDto`.

### **POST** `/api/v1/admin/maintenance/purge-orphans`

- **Description**: Deletes genres, directors, and actors that do not have any associated movies.
- **Responses**:
  - `200 OK`: Returns `PurgeOrphanResultDto` with count of deleted rows.

### **POST** `/api/v1/admin/maintenance/clear-cache`

- **Description**: Clears the system-wide recommendations and metadata caches. Clears the in-memory cache and, if a Redis connection is configured, flushes the Redis database.
- **Responses**:
  - `200 OK`: Cache cleared.

### **POST** `/api/v1/admin/enrich/retry-failed`

- **Description**: Manually triggers TMDB metadata enrichment for failed or not found movies.
- **Query Parameters**:
  - `resetPermanentlyFailed` (`bool`, default: `false`) - Force retrying movies that previously failed startup recovery.
  - `batchSize` (`int`, default: `50`) - Max movies to process in this run.
- **Responses**:
  - `200 OK`: Returns count of successfully enriched movies.

### **POST** `/api/movies/enrich-from-tmdb`

- **Description**: Manually triggers TMDB metadata enrichment for a specific movie by its TMDB ID. Restricted to users with the `Admin` role.
- **Request Body**:

  ```json
  {
    "tmdbId": 0
  }
  ```

- **Responses**:
  - `200 OK`: Returns the enriched movie details.
  - `404 NotFound`: Movie not found or is a TV show.
