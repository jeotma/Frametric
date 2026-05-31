// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Application.DTOs.Analytics;

public record DashboardSummaryDto(
    int TotalWatchtimeMinutes,
    int TotalWatches,
    int UniqueMoviesCount,
    List<GenreCountDto> TopGenres,
    List<DirectorCountDto> TopDirectors,
    List<ActorCountDto> TopActors,
    List<DecadeCountDto> DecadeBreakdown
);

