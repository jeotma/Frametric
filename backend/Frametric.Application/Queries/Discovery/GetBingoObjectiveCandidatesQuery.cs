using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Discovery.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.Discovery;

public record BingoCandidateDto(
    Guid DiaryEntryId,
    Guid MovieId,
    string MovieTitle,
    int ReleaseYear,
    DateOnly WatchedDate,
    string? PosterUrl,
    bool IsMatching,
    bool IsAlreadyUsed
);

public record GetBingoObjectiveCandidatesQuery(Guid UserId, Guid ObjectiveId) : IRequest<IEnumerable<BingoCandidateDto>>;

public class GetBingoObjectiveCandidatesQueryHandler : IRequestHandler<GetBingoObjectiveCandidatesQuery, IEnumerable<BingoCandidateDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBingoObjectiveCandidatesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<BingoCandidateDto>> Handle(GetBingoObjectiveCandidatesQuery request, CancellationToken cancellationToken)
    {
        var objective = await _dbContext.DiscoveryObjectives
            .FirstOrDefaultAsync(o => o.UserId == request.UserId && o.Id == request.ObjectiveId, cancellationToken);

        if (objective == null)
        {
            throw new InvalidOperationException("Bingo objective not found.");
        }

        // Get other claimed entries on this board
        var claimedDiaryEntryIds = await _dbContext.DiscoveryObjectives
            .Where(o => o.UserId == request.UserId && o.BoardId == objective.BoardId && o.Id != objective.Id && o.FulfillingDiaryEntryId.HasValue)
            .Select(o => o.FulfillingDiaryEntryId!.Value)
            .ToListAsync(cancellationToken);

        // Load all user diary entries
        var query = _dbContext.DiaryEntries
            .Include(entry => entry.Movie)
                .ThenInclude(m => m.Genres)
            .Where(entry => entry.UserId == request.UserId);

        // Filter by StartDate if present
        if (objective.StartDate.HasValue)
        {
            var start = DateOnly.FromDateTime(objective.StartDate.Value);
            query = query.Where(entry => entry.WatchedDate >= start);
        }

        // Filter by EndDate if present AND current time is still within active period
        if (objective.EndDate.HasValue && DateTime.UtcNow <= objective.EndDate.Value)
        {
            var end = DateOnly.FromDateTime(objective.EndDate.Value);
            query = query.Where(entry => entry.WatchedDate <= end);
        }

        var entries = await query.ToListAsync(cancellationToken);

        return entries.Select(entry =>
        {
            bool isMatching = DiscoveryObjectiveEvaluator.Matches(objective.RequirementExpression, entry);
            bool isAlreadyUsed = claimedDiaryEntryIds.Contains(entry.Id);

            return new BingoCandidateDto(
                entry.Id,
                entry.MovieId,
                entry.Movie?.Title ?? "Unknown",
                entry.Movie?.ReleaseYear ?? 0,
                entry.WatchedDate,
                entry.Movie?.PosterUrl,
                isMatching,
                isAlreadyUsed
            );
        }).OrderByDescending(c => c.WatchedDate).ToList();
    }
}
