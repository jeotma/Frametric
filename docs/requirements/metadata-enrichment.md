# TMDB Metadata Enrichment Flow

Due to the nature of Letterboxd export files, vital cinematic metadata (Genres, Directors, Runtime, Posters) is missing at the point of ingestion. Frametric uses **TMDB (The Movie Database)** to asynchronously enrich this data.

## The Problem

When a user uploads a Letterboxd `.zip` file, the `LetterboxdCsvImporter` can only extract:

- Title
- Release Year
- Letterboxd URI

Analytics heavily depend on Genres, Directors, Runtimes, and Cast. This makes raw CSV data insufficient on its own.

---

## The Asynchronous Solution

To keep the initial ZIP parsing fast and prevent hitting TMDB API rate limits synchronously during the HTTP request, the system uses an in-memory **Producer-Consumer** background queue.

### 1. Ingestion Phase

1. User uploads the ZIP file.
2. The system parses the CSV files and generates `Movie` entities.
3. These `Movie` entities are inserted into the database with `EnrichmentStatus = Pending`.
4. The import batch status is set to `Enriching` inside `ImportHistory`.
5. The API layer fires `ITmdbEnrichmentTrigger.TriggerEnrichment()` which writes to an in-memory `System.Threading.Channels.Channel`.
6. The system returns a success response to the user immediately.

### 2. Enrichment Phase (Background Job)

1. `TmdbEnrichmentBackgroundService` listens to the channel.
2. Upon receiving a trigger (or during application startup sweeps), it queries PostgreSQL for movies where `EnrichmentStatus == Pending` in batches (default: **20**, configurable via `TmdbEnrichment:BatchSize`).
3. For each pending movie, the `ITmdbClient` makes an HTTP request to the TMDB API using the `Title` and `ReleaseYear`.
4. Once a match is found, the system extracts:
   - `RuntimeMinutes`
   - `PosterUrl`
   - List of `Genres` (mapped to local DB)
   - List of `Directors` (via Credits)
   - List of top-billed `Actors` (via Credits, limited to the primary cast to avoid DB bloat)
5. The system saves the new relationships (`GenreMovie`, `DirectorMovie`, `ActorMovie`) to the database.
6. The `Movie.EnrichmentStatus` is updated to `Completed` (or `Failed` if no match was found on TMDB).
7. If a batch contains `0` pending movies, the background worker fires `MarkImportsCompletedCommand` to transition the import batch status from `Enriching` to `Success` inside `ImportHistory`.

---

## Resilience & Rate Limiting

- **Rate Limits**: The background job respects external API limit recommendations. It throttles processing by waiting between database batches (default: **10 seconds**, configurable via `TmdbEnrichment:DelayBetweenBatchesSeconds`).
- **Retries**: Transient failures are logged, and the service sleeps before retrying (default: **30 seconds**, configurable via `TmdbEnrichment:RetryDelaySeconds`) to prevent crashing the worker thread.
- **Unmatched Movies**: If a movie cannot be found on TMDB, its status is marked as `Failed`. This allows the dashboard queries to execute while avoiding repeated API queries on subsequent sweeps.
