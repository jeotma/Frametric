# API Endpoints Specification

This document details the REST API endpoints exposed by the backend services. All endpoints are prefixed with `/api/` (unless specified otherwise) and secured via JWT Bearer authentication (where noted).

---

## 1. Authentication (`/api/Auth`)

Authentication endpoints are publicly accessible (`[AllowAnonymous]`).

### **POST** `/api/Auth/signup`

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

### **POST** `/api/Auth/login`

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

### **POST** `/api/Auth/refresh`

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

---

## 2. Ingestion & Imports (`/api/Import`)

All import endpoints require authentication.

### **POST** `/api/Import/letterboxd`

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

### **GET** `/api/Import/history`

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

### **DELETE** `/api/Import/{id}`

- **Description**: Deletes a specific import batch and executes a cascade delete on all associated diary entries, ratings, watchlist items, and likes.
- **Parameters**:
  - `id` (`Guid` in route)
- **Responses**:
  - `200 OK`: Data deleted.
  - `404 NotFound`: Import not found.

---

## 3. General Analytics (`/api/Analytics`)

Requires authentication.

### **GET** `/api/Analytics/dashboard`

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

### **GET** `/api/Analytics/wrapped`

- **Description**: Generates the Spotify Wrapped style yearly summary.
- **Query Parameters**:
  - `year` (`int?`, optional)
- **Responses**:
  - `200 OK`: Returns complex structured stats for the specified year (or all-time if null).

### **GET** `/api/Analytics/monthly-activity/{year}`

- **Description**: Histogram of watched movies count per month.
- **Parameters**:
  - `year` (`int` in route)
- **Responses**:
  - `200 OK`: Array of monthly values.

### **GET** `/api/Analytics/top-directors`

- **Description**: Leaderboard of top directors.
- **Query Parameters**:
  - `limit` (`int`, default: 10)
- **Responses**:
  - `200 OK`: Returns array of director ranking items.

---

## 4. Advanced Analytics (`/api/analytics/advanced`)

All routes require authentication. Filters are passed via query parameters mapping to `AnalyticsFilterDto` (e.g. `year`, `genre`).

### Category: Watched Metrics (`/api/analytics/advanced/watched/*`)

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

### Category: Watchlist Metrics (`/api/analytics/advanced/watchlist/*`)

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

### Category: Bonus Metrics (`/api/analytics/advanced/bonus/*`)

- **GET** `/bonus/weekend-warrior` - Stats on weekend binge behaviors.
- **GET** `/bonus/hidden-gems` - Movies rated highly by the user that are obscure globally on TMDB.
- **GET** `/bonus/watchlist-graveyard` - Movies added to the watchlist years ago that remain unwatched.
- **GET** `/bonus/cinematic-fatigue` - Extended analysis of viewing streaks, drops in scores, or pauses.

### Category: Final Cut Metrics (`/api/analytics/advanced/final-cut/*`)

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

### **POST** `/api/v1/recommendations/skip/{movieId}`

- **Description**: Excludes a movie from future recommendation generation cycles for 24 hours.
- **Parameters**:
  - `movieId` (`Guid` in route)
- **Responses**:
  - `204 NoContent`: Successfully skipped and cached.
