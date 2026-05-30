using System.Text.Json.Serialization;

namespace Frametric.Infrastructure.Providers.Tmdb;

public class TmdbSearchResponse
{
    [JsonPropertyName("results")]
    public List<TmdbSearchResult> Results { get; set; } = new();
}

public class TmdbSearchResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class TmdbMovieDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenreItem> Genres { get; set; } = new();

    [JsonPropertyName("credits")]
    public TmdbCredits? Credits { get; set; }
}

public class TmdbGenreItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbCredits
{
    [JsonPropertyName("cast")]
    public List<TmdbCastItem> Cast { get; set; } = new();

    [JsonPropertyName("crew")]
    public List<TmdbCrewItem> Crew { get; set; } = new();
}

public class TmdbCastItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public class TmdbCrewItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("job")]
    public string Job { get; set; } = string.Empty;
}

public class TmdbTvDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("episode_run_time")]
    public List<int>? EpisodeRunTime { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenreItem> Genres { get; set; } = new();

    [JsonPropertyName("created_by")]
    public List<TmdbCrewItem> CreatedBy { get; set; } = new();

    [JsonPropertyName("credits")]
    public TmdbCredits? Credits { get; set; }
}
