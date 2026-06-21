// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Application.DTOs.Analytics;

public record AnalyticsFilterDto
{
    public int? WatchYear { get; init; }
    public int? ReleaseYear { get; init; }
    public decimal? MinRating { get; init; }
    public decimal? MaxRating { get; init; }
    public decimal? MinCustomRating { get; init; }
    public decimal? MaxCustomRating { get; init; }
    public string? Genre { get; init; }
    public string? Director { get; init; }
    public string? Actor { get; init; }
}
