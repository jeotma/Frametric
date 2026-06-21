// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;

namespace Frametric.Application.DTOs;

public record RecommendedMovieDto(
    Guid MovieId,
    string Title,
    string DirectorName,
    int ReleaseYear,
    double MatchPercentage,
    string RecommendationReason,
    string? PosterUrl = null,
    int? RuntimeMinutes = null,
    double? CustomAverageRating = null,
    string? EasterEggTooltip = null,
    string? WellnessCheckMessage = null
);
