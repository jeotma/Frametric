// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.EntityDetails;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.EntityDetails;
using MediatR;

namespace Frametric.Application.Queries.EntityDetails;

public record GlobalSearchQuery(Guid UserId, string QueryText) : IRequest<IEnumerable<GlobalSearchResultDto>>;

public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, IEnumerable<GlobalSearchResultDto>>
{
    private readonly IEntityDetailsQueries _queries;
    private readonly ITmdbService _tmdbService;

    public GlobalSearchQueryHandler(IEntityDetailsQueries queries, ITmdbService tmdbService)
    {
        _queries = queries;
        _tmdbService = tmdbService;
    }

    public async Task<IEnumerable<GlobalSearchResultDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var localResults = await _queries.SearchEntitiesAsync(request.UserId, request.QueryText, cancellationToken);
        
        var localResultsList = localResults.ToList();
        if (localResultsList.Any())
        {
            return localResultsList;
        }

        // Fallback to TMDB
        return await _tmdbService.SearchMultiAsync(request.QueryText, cancellationToken);
    }
}
