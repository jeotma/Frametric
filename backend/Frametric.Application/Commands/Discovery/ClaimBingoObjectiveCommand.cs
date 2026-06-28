using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Discovery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Discovery;

public record ClaimBingoObjectiveCommand(Guid UserId, Guid ObjectiveId, Guid DiaryEntryId) : IRequest<BingoGridDto>;

public class ClaimBingoObjectiveCommandHandler : IRequestHandler<ClaimBingoObjectiveCommand, BingoGridDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public ClaimBingoObjectiveCommandHandler(IApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<BingoGridDto> Handle(ClaimBingoObjectiveCommand request, CancellationToken cancellationToken)
    {
        var objective = await _dbContext.DiscoveryObjectives
            .FirstOrDefaultAsync(o => o.UserId == request.UserId && o.Id == request.ObjectiveId, cancellationToken);

        if (objective == null)
        {
            throw new InvalidOperationException("Bingo objective not found.");
        }

        if (objective.IsAchieved)
        {
            throw new InvalidOperationException("Bingo objective is already achieved.");
        }

        var entry = await _dbContext.DiaryEntries
            .Include(e => e.Movie)
                .ThenInclude(m => m.Genres)
            .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.Id == request.DiaryEntryId, cancellationToken);

        if (entry == null)
        {
            throw new InvalidOperationException("Diary entry not found.");
        }

        // Validate dates (relax EndDate if expired)
        if (objective.StartDate.HasValue && entry.WatchedDate < DateOnly.FromDateTime(objective.StartDate.Value))
        {
            throw new InvalidOperationException("Movie was watched before the board's start date.");
        }
        if (objective.EndDate.HasValue && DateTime.UtcNow <= objective.EndDate.Value)
        {
            if (entry.WatchedDate > DateOnly.FromDateTime(objective.EndDate.Value))
            {
                throw new InvalidOperationException("Movie was watched after the board's active period.");
            }
        }

        // Validate already claimed on this board
        var alreadyClaimed = await _dbContext.DiscoveryObjectives
            .AnyAsync(o => o.UserId == request.UserId && o.BoardId == objective.BoardId && o.FulfillingDiaryEntryId == request.DiaryEntryId, cancellationToken);

        if (alreadyClaimed)
        {
            throw new InvalidOperationException("This diary entry has already been used for another square on this board.");
        }

        // Validate requirement match
        bool matches = DiscoveryObjectiveEvaluator.Matches(objective.RequirementExpression, entry);
        if (!matches)
        {
            throw new InvalidOperationException("This movie does not satisfy the objective requirements.");
        }

        objective.MarkAsAchieved(request.DiaryEntryId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Fetch and return the updated grid
        return await _mediator.Send(new GetBingoGridQuery(request.UserId, objective.GridSize, BoardId: objective.BoardId), cancellationToken);
    }
}
