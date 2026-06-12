// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Application.DTOs.Discovery;

public class DiscoveryMoviePoolItemDto
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? DirectorName { get; set; }
    public int? ReleaseYear { get; set; }
    public int? RuntimeMinutes { get; set; }
    public string? PosterUrl { get; set; }
    public double? TmdbRating { get; set; }
    public double? TmdbPopularity { get; set; }
    public double? CustomAverageRating { get; set; }
    public string? Genres { get; set; }
    public string? Keywords { get; set; }
    public string? Overview { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
}
