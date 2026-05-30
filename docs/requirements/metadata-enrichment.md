# TMDB Metadata Enrichment Flow

Due to the nature of Letterboxd export files, vital cinematic metadata (Genres, Directors, Runtime, Posters) is missing at the point of ingestion. Frametric uses **TMDB (The Movie Database)** to asynchronously enrich this data.

## The Problem

When a user uploads a Letterboxd `.zip` file, the `LetterboxdCsvImporter` can only extract:

- Title
- Release Year
- Letterboxd URI

Analytics heavily depend on Genres and Directors, making this raw data insufficient.

## The Asynchronous Solution

To keep the initial ZIP parsing fast and prevent hitting TMDB API rate limits synchronously during the HTTP request, the system uses a Background Job pattern.

### 1. Ingestion Phase

1. User uploads the ZIP file.
2. The system parses the CSV files and generates `Movie` entities.
3. These `Movie` entities are inserted into the database with an initial `EnrichmentStatus = Pending`.
4. The system returns a success response to the user immediately.

### 2. Enrichment Phase (Background Job)

1. A background processor (e.g., Hangfire or Quartz.NET) periodically polls for `Movie` entities where `EnrichmentStatus == Pending`.
2. For each pending movie, an `ITmdbClient` (Infrastructure layer) makes an HTTP request to the TMDB API.
3. The search is performed using the `Title` and `ReleaseYear`.
4. Once a match is found, the system extracts:
   - `RuntimeMinutes`
   - `PosterUrl`
   - List of `Genres`
   - List of `Directors` (via Credits endpoint)
5. The system saves the new relationships (`GenreMovie`, `DirectorMovie`) to the database.
6. The `Movie.EnrichmentStatus` is updated to `Completed` (or `Failed` if no match was found).

### 3. Resilience & Rate Limiting

- **Rate Limits:** The background job must respect TMDB rate limits. Requests should be throttled (e.g., via `Polly` policies) or processed in safe batches.
- **Retries:** Network failures should trigger a retry mechanism.
- **Failures:** If a movie simply doesn't exist on TMDB, its status becomes `Failed`, allowing the system to ignore it in future sweeps without crashing the analytics.
