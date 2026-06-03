# Temporary Database Reset Script Reminder

We have successfully enriched the first batch of completed movies using TMDB/OMDb and the file cache. The database now contains 972 fully enriched movies and exactly 547 movies that remain from the old import.

## How to Run (Tomorrow)

1. **Reset the remaining 547 movies to `Pending`**:
   Run this command from the `backend/Frametric.UnitTests` directory to identify and reset the remaining 547 movies:
   ```bash
   dotnet test --filter "GetCinematicRecommendationsQueryHandlerTests.ResetRemainingMoviesToPending"
   ```
   *(Expected output from the test failure message: `Completed: 972\nPending: 547`)*

2. **Start the API backend server to enrich the remaining batch**:
   Run this command from the `backend/Frametric.Api` directory:
   ```bash
   dotnet run
   ```
   The background enrichment job will pick up these 547 movies, fetch TMDB and OMDb metadata (saving responses into `omdb_cache`), and update them.

3. **Verify all movies are enriched**:
   Once the console logs show `No more pending movies. Sleeping...`, stop the API server (Ctrl+C).

---

## Cleanup Checklist (When Completed)
- [ ] Remove this `TEMPORARY_RESET_SCRIPT.md` file.
- [ ] In [Movie.cs](file:///c:/Users/Jeotm/Documents/PersonalProjects/Frametric/backend/Frametric.Domain/Entities/Movie.cs), delete the temporary `ResetToPending()` method.
- [ ] In [GetCinematicRecommendationsQueryHandlerTests.cs](file:///c:/Users/Jeotm/Documents/PersonalProjects/Frametric/backend/Frametric.UnitTests/GetCinematicRecommendationsQueryHandlerTests.cs), delete the temporary `ResetRemainingMoviesToPending()` method.
- [ ] Verify unit tests compile and run cleanly with `dotnet test`.
