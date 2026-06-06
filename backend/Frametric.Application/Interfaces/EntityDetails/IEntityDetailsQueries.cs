// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.EntityDetails;

namespace Frametric.Application.Interfaces.EntityDetails;

public interface IEntityDetailsQueries
{
    Task<MovieDetailsDto?> GetMovieDetailsAsync(Guid userId, Guid movieId, CancellationToken cancellationToken);
    Task<ActorDetailsDto?> GetActorDetailsAsync(Guid userId, Guid actorId, CancellationToken cancellationToken);
    Task<DirectorDetailsDto?> GetDirectorDetailsAsync(Guid userId, Guid directorId, CancellationToken cancellationToken);
    Task<IEnumerable<GlobalSearchResultDto>> SearchEntitiesAsync(Guid userId, string query, CancellationToken cancellationToken);
}
