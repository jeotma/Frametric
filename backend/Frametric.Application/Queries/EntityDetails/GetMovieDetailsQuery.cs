// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.DTOs.EntityDetails;
using Frametric.Application.Interfaces.EntityDetails;
using MediatR;

namespace Frametric.Application.Queries.EntityDetails;

public record GetMovieDetailsQuery(Guid UserId, Guid MovieId) : IRequest<MovieDetailsDto?>;

public class GetMovieDetailsQueryHandler : IRequestHandler<GetMovieDetailsQuery, MovieDetailsDto?>
{
    private readonly IEntityDetailsQueries _queries;

    public GetMovieDetailsQueryHandler(IEntityDetailsQueries queries)
    {
        _queries = queries;
    }

    public Task<MovieDetailsDto?> Handle(GetMovieDetailsQuery request, CancellationToken cancellationToken)
    {
        return _queries.GetMovieDetailsAsync(request.UserId, request.MovieId, cancellationToken);
    }
}
