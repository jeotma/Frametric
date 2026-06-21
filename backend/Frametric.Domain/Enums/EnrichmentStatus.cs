namespace Frametric.Domain.Enums;

public enum EnrichmentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    /// <summary>
    /// All search strategies were exhausted and the entry could not be resolved in TMDB.
    /// This entry should NOT be retried automatically.
    /// </summary>
    NotFound = 3,
    /// <summary>
    /// Deliberate recovery retry failed or timed out.
    /// This entry will NOT be retried automatically on subsequent startups.
    /// </summary>
    PermanentlyFailed = 4
}

