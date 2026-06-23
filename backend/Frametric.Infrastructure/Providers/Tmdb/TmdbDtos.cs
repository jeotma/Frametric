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

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("popularity")]
    public double? Popularity { get; set; }
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

    [JsonPropertyName("imdb_id")]
    public string? ImdbId { get; set; }

    [JsonPropertyName("vote_average")]
    public double? VoteAverage { get; set; }

    [JsonPropertyName("popularity")]
    public double? Popularity { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("belongs_to_collection")]
    public TmdbCollectionBelongs? BelongsToCollection { get; set; }
}

public class TmdbCollectionBelongs
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }
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

    [JsonPropertyName("profile_path")]
    public string? ProfilePath { get; set; }
}

public class TmdbCrewItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("job")]
    public string Job { get; set; } = string.Empty;

    [JsonPropertyName("profile_path")]
    public string? ProfilePath { get; set; }
}

public class TmdbTvDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("first_air_date")]
    public string? FirstAirDate { get; set; }

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

    [JsonPropertyName("seasons")]
    public List<TmdbSeasonItem>? Seasons { get; set; }
}

public class TmdbSeasonItem
{
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; set; }
}

public class TmdbSeasonDetails
{
    [JsonPropertyName("episodes")]
    public List<TmdbEpisodeItem> Episodes { get; set; } = new();
}

public class TmdbEpisodeItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }
}

public class TmdbKeywordsResponse
{
    [JsonPropertyName("keywords")]
    public List<TmdbKeywordItem> Keywords { get; set; } = new();
}

public class TmdbKeywordItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbWatchProvidersResponse
{
    [JsonPropertyName("results")]
    public Dictionary<string, TmdbCountryWatchProviders>? Results { get; set; }
}

public class TmdbCountryWatchProviders
{
    [JsonPropertyName("flatrate")]
    public List<TmdbWatchProviderItem>? Flatrate { get; set; }
}

public class TmdbWatchProviderItem
{
    [JsonPropertyName("provider_name")]
    public string ProviderName { get; set; } = string.Empty;
}

public class TmdbCollectionDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("parts")]
    public List<TmdbCollectionPart> Parts { get; set; } = new();
}

public class TmdbCollectionPart
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
}

public class TmdbMultiSearchResponse
{
    [JsonPropertyName("results")]
    public List<TmdbMultiSearchResult> Results { get; set; } = new();
}

public class TmdbMultiSearchResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("profile_path")]
    public string? ProfilePath { get; set; }
}

