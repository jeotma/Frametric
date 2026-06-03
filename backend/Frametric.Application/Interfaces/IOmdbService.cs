// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Threading;
using System.Threading.Tasks;

namespace Frametric.Application.Interfaces;

public record OmdbRatingsDto(
    double? ImdbRating,
    double? RottenTomatoesRating,
    double? MetacriticRating
);

public interface IOmdbService
{
    Task<OmdbRatingsDto?> GetMovieRatingsAsync(string imdbId, CancellationToken cancellationToken);
}
