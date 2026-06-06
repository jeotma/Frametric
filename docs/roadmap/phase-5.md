# Phase 5: Entity Details & Global Navigation

## Objective

Implement dedicated detail views for Movies, Actors, and Directors. Enhance the global search bar to navigate directly to these entities, and interlink the existing application (stats, posters, titles) to route users to these new detail views seamlessly.

## Scope

### 1. Backend: Entity Details Endpoints

Create new endpoints in the Application and API layers to serve comprehensive data for a single entity:

- **`GET /api/movies/{id}`**: Return movie details, genres, directors, actors, user's average score, and diary entries (watch dates).
- **`GET /api/actors/{id}`**: Return actor details, their movies in the user's database, average rating, etc.
- **`GET /api/directors/{id}`**: Return director details, their movies in the user's database, average rating, etc.
- **`POST /api/movies/{id}/log`**: Manually log or edit a watch entry for a movie, including rating, exact date watched, and rewatch status.

*Architecture Rules*:

- Use CQRS principles (`GetMovieDetailsQuery`, `GetActorDetailsQuery`, `GetDirectorDetailsQuery`, `LogMovieWatchCommand`).
- Fetch the data efficiently using EF Core or Dapper (prefer Dapper for analytical aggregations).
- Map to dedicated DTOs (e.g., `MovieDetailsDto`).

### 2. Frontend: Entity Detail Views

Create three new distinct, highly aesthetic components:

- **Movie Detail View**:
  - Showcase poster, backdrop (if available), synopsis, and metadata (release year, runtime).
  - List of directors and actors (clickable).
  - User's relationship with the movie (rating given, dates watched).
  - **Manual Entry Controls**: Ability to manually mark the movie as watched, assign a score, specify the exact watch date, and toggle rewatch status.
- **Actor Detail View**:
  - Actor's profile picture and name.
  - List of movies featuring this actor in the user's library.
  - Quick stats (average rating, watch count).
- **Director Detail View**:
  - Director's profile picture and name.
  - List of movies directed by this director in the user's library.
  - Quick stats (average rating, watch count).

*Design Rules*:

- Adhere to the glassmorphism, dynamic layouts, and premium aesthetics established in Frametric.
- Create dynamic routing paths (e.g., `/movies/:id`, `/actors/:id`, `/directors/:id`).

### 3. Global Search Integration

- Connect the top navigation search bar to query entities from the backend.
- The search will follow a fallback strategy: first, query the local database. If no results are found, it will fallback to searching TMDB to allow users to find new entities.
- The search bar dropdown should categorize results (Movies, Directors, Actors) and distinguish between Local vs. TMDB results.
- Clicking a result routes the user to the respective Entity Detail View.

### 4. Cross-App Interlinking

- In **Advanced Stats**: Turn movie titles, posters, actor names, and director names into clickable links that navigate to their respective Entity Views.
- In **Recommendations**: Allow clicking on recommended movies to see their full details.
- Ensure the state is maintained when navigating back and forth from stats to an entity view.

## Acceptance Criteria

- [x] Backend endpoints return comprehensive, normalized data for individual entities.
- [x] Users can manually log or edit a movie watch entry (date, score, rewatch status).
- [x] Users can manually unlog a watch entry, with full data cleanup on non-rewatch deletion.
- [x] Global search bar queries the local DB first, then falls back to TMDB, and returns dynamic, categorized results.
- [x] Three new detail views exist and are fully styled.
- [x] All table lists (Advanced Stats, Dashboards) have hyperlinks on titles/posters/names directing to these new views.
- [x] Actor and Director profile photos fetched from TMDB and displayed in detail pages.
