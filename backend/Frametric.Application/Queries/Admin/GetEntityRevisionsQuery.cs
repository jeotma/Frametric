using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Admin;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.Admin;

public record GetEntityRevisionsQuery(string EntityType, Guid EntityId) : IRequest<List<RevisionDto>>;

public class GetEntityRevisionsQueryHandler : IRequestHandler<GetEntityRevisionsQuery, List<RevisionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEntityRevisionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RevisionDto>> Handle(GetEntityRevisionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.EntityRevisions
            .Where(r => r.EntityType == request.EntityType && r.EntityId == request.EntityId)
            .OrderByDescending(r => r.ChangedAt)
            .Select(r => new RevisionDto(r.Id, r.EntityType, r.EntityId, r.ChangedAt, r.ChangedBy, r.StateJson))
            .ToListAsync(cancellationToken);
    }
}
