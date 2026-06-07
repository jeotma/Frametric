// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Domain.Enums;

namespace Frametric.Application.Interfaces.Discovery;

public record DiscoveryMoviePoolItemDto(
    Guid MovieId,
    string Title,
    string? DirectorName,
    int? ReleaseYear,
    int? RuntimeMinutes,
    string? PosterUrl,
    double? TmdbRating,
    double? TmdbPopularity,
    double? CustomAverageRating,
    string? Genres,
    string? Keywords,
    string? Overview,
    string? Language,
    string? Country
);

public interface IDiscoveryQueries
{
    Task<IEnumerable<Guid>> ResolveMovieIdsByTitlesAsync(IEnumerable<string> titles, CancellationToken ct = default);
    Task<IEnumerable<DiscoveryMoviePoolItemDto>> GetDiscoveryPoolAsync(Guid userId, DiscoveryDataSourceScope scope, IEnumerable<Guid>? customSourceIds, CancellationToken ct = default);
}
