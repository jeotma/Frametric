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

public record GetDirectorDetailsQuery(Guid UserId, Guid DirectorId) : IRequest<DirectorDetailsDto?>;

public class GetDirectorDetailsQueryHandler : IRequestHandler<GetDirectorDetailsQuery, DirectorDetailsDto?>
{
    private readonly IEntityDetailsQueries _queries;

    public GetDirectorDetailsQueryHandler(IEntityDetailsQueries queries)
    {
        _queries = queries;
    }

    public Task<DirectorDetailsDto?> Handle(GetDirectorDetailsQuery request, CancellationToken cancellationToken)
    {
        return _queries.GetDirectorDetailsAsync(request.UserId, request.DirectorId, cancellationToken);
    }
}
