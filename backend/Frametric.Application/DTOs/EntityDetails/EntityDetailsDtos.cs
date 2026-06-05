// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

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
    IEnumerable<MovieDiaryEntryDto> DiaryEntries
);

public record ActorDetailsDto(
    Guid Id,
    string Name,
    double AverageRating,
    int WatchCount,
    IEnumerable<MovieSimpleDto> Movies,
    string? ProfilePath = null
)
{
    public ActorDetailsDto(Guid id, string name, double averageRating, int watchCount, IEnumerable<MovieSimpleDto> movies) 
        : this(id, name, averageRating, watchCount, movies, null) {}
}

public record DirectorDetailsDto(
    Guid Id,
    string Name,
    double AverageRating,
    int WatchCount,
    IEnumerable<MovieSimpleDto> Movies,
    string? ProfilePath = null
)
{
    public DirectorDetailsDto(Guid id, string name, double averageRating, int watchCount, IEnumerable<MovieSimpleDto> movies) 
        : this(id, name, averageRating, watchCount, movies, null) {}
}
