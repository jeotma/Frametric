// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Text.Json.Serialization;
using Frametric.Application.DTOs.Analytics;

namespace Frametric.Application.DTOs.EntityDetails;

public record MovieDiaryEntryDto(Guid Id, string DateWatched, bool IsRewatch, double? Rating);

public record MovieDetailsDto(
    Guid Id,
    string Title,
    int? ReleaseYear,
    int? RuntimeMinutes,
    string? PosterUrl,
    string? Overview,
    double? TmdbRating,
    double? UserAverageScore,
    IEnumerable<GenreSimpleDto> Genres,
    IEnumerable<DirectorSimpleDto> Directors,
    IEnumerable<ActorSimpleDto> Actors,
    IEnumerable<MovieDiaryEntryDto> DiaryEntries,
    bool IsWatched = false
);

public record ActorDetailsDto(
    Guid Id,
    string Name,
    double AverageRating,
    int WatchCount,
    IEnumerable<MovieSimpleDto> Movies,
    string? ProfilePath = null,
    bool IsDirector = false,
    List<MovieSimpleDto>? DirectedMovies = null,
    int WatchlistCount = 0,
    int LikeCount = 0,
    [property: JsonPropertyName("likedMovieTitles")]
    List<MovieSimpleDto>? LikedMovies = null,
    [property: JsonPropertyName("watchlistMovieTitles")]
    List<MovieSimpleDto>? WatchlistMovies = null
)
{
    public ActorDetailsDto(Guid id, string name, double averageRating, int watchCount, IEnumerable<MovieSimpleDto> movies) 
        : this(id, name, averageRating, watchCount, movies, null, false, null, 0, 0, null, null) {}

    public ActorDetailsDto(Guid id, string name, double averageRating, int watchCount, IEnumerable<MovieSimpleDto> movies, string? profilePath) 
        : this(id, name, averageRating, watchCount, movies, profilePath, false, null, 0, 0, null, null) {}
}

public record DirectorDetailsDto(
    Guid Id,
    string Name,
    double AverageRating,
    int WatchCount,
    IEnumerable<MovieSimpleDto> Movies,
    string? ProfilePath = null,
    bool IsActor = false,
    List<MovieSimpleDto>? ActorMovies = null,
    int WatchlistCount = 0,
    int LikeCount = 0,
    [property: JsonPropertyName("likedMovieTitles")]
    List<MovieSimpleDto>? LikedMovies = null,
    [property: JsonPropertyName("watchlistMovieTitles")]
    List<MovieSimpleDto>? WatchlistMovies = null
)
{
    public DirectorDetailsDto(Guid id, string name, double averageRating, int watchCount, IEnumerable<MovieSimpleDto> movies) 
        : this(id, name, averageRating, watchCount, movies, null, false, null, 0, 0, null, null) {}

    public DirectorDetailsDto(Guid id, string name, double averageRating, int watchCount, IEnumerable<MovieSimpleDto> movies, string? profilePath) 
        : this(id, name, averageRating, watchCount, movies, profilePath, false, null, 0, 0, null, null) {}
}
