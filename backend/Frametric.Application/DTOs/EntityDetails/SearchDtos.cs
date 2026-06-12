// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Application.DTOs.EntityDetails;

public record GlobalSearchResultDto(
    Guid? LocalId,
    int? TmdbId,
    string EntityType, // "Movie", "Actor", "Director"
    string TitleOrName,
    int? ReleaseYear,
    string? ImageUrl,
    bool IsLocal,
    Guid? ActorId = null,
    Guid? DirectorId = null
)
{
    // Dapper demands an EXACT match between returned columns and a constructor.
    public GlobalSearchResultDto(Guid? LocalId, int? TmdbId, string EntityType, string TitleOrName, int? ReleaseYear, string? ImageUrl, bool IsLocal) 
        : this(LocalId, TmdbId, EntityType, TitleOrName, ReleaseYear, ImageUrl, IsLocal, null, null) {}
}
