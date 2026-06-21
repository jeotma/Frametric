// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Discovery;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Commands.Discovery;

public record RerollBingoObjectiveCommand(Guid UserId, Guid ObjectiveId) : IRequest<BingoGridDto>;

public class RerollBingoObjectiveCommandHandler : IRequestHandler<RerollBingoObjectiveCommand, BingoGridDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ILogger<RerollBingoObjectiveCommandHandler> _logger;

    public RerollBingoObjectiveCommandHandler(
        IApplicationDbContext dbContext,
        IMediator mediator,
        ILogger<RerollBingoObjectiveCommandHandler> _logger)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        this._logger = _logger;
    }

    public async Task<BingoGridDto> Handle(RerollBingoObjectiveCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to reroll bingo objective {ObjectiveId} for user {UserId}", request.ObjectiveId, request.UserId);

        var objective = await _dbContext.DiscoveryObjectives
            .FirstOrDefaultAsync(o => o.Id == request.ObjectiveId && o.UserId == request.UserId, cancellationToken);

        if (objective == null)
        {
            throw new InvalidOperationException("Bingo objective not found.");
        }

        if (objective.IsAchieved)
        {
            throw new InvalidOperationException("Cannot reroll an already completed bingo objective.");
        }

        // Get all objectives on the same grid size to calculate limits and current usage
        var boardObjectives = await _dbContext.DiscoveryObjectives
            .Where(o => o.UserId == request.UserId && o.GridSize == objective.GridSize)
            .ToListAsync(cancellationToken);

        int maxRerolls = objective.GridSize switch
        {
            3 => 1,
            4 => 2,
            5 => 3,
            _ => 1
        };

        int currentRerollsUsed = boardObjectives.Sum(o => o.RerollCount);

        if (currentRerollsUsed >= maxRerolls)
        {
            throw new InvalidOperationException($"Maximum rerolls ({maxRerolls}) reached for this board.");
        }

        // Default pool of objectives to pick from (copied from GetBingoGridQueryHandler)
        var defaultPool = new List<(string Expression, string Description)>
        {
            // Runtimes
            ("RuntimeMinutes < 90", "Watch a film under 90 minutes"),
            ("RuntimeMinutes < 75", "Watch a short feature under 75 minutes"),
            ("RuntimeMinutes > 120", "Watch a film longer than 2 hours"),
            ("RuntimeMinutes > 150", "Watch an epic film longer than 2.5 hours"),
            ("RuntimeMinutes >= 180", "Watch an ultra-epic film of 3+ hours"),
            
            // Genres
            ("Genre == 'Horror'", "Watch a horror film"),
            ("Genre == 'Animation'", "Watch an animated film"),
            ("Genre == 'Science Fiction'", "Watch a science fiction film"),
            ("Genre == 'Comedy'", "Watch a comedy"),
            ("Genre == 'Drama'", "Watch a drama"),
            ("Genre == 'Action'", "Watch an action film"),
            ("Genre == 'Romance'", "Watch a romance film"),
            ("Genre == 'Thriller'", "Watch a thriller"),
            ("Genre == 'Fantasy'", "Watch a fantasy film"),
            ("Genre == 'Mystery'", "Watch a mystery film"),
            ("Genre == 'Adventure'", "Watch an adventure film"),
            ("Genre == 'Crime'", "Watch a crime film"),
            ("Genre == 'Documentary'", "Watch a documentary"),
            ("Genre == 'War'", "Watch a war/conflict film"),
            ("Genre == 'History'", "Watch a historical drama"),
            ("Genre == 'Western'", "Watch a western film"),
            ("Genre == 'Family'", "Watch a family movie"),
            ("Genre == 'Music'", "Watch a musical or music-centric film"),
            
            // Documentaries
            ("IsDocumentary == true", "Watch a documentary"),
            
            // Languages & Countries
            ("Language != 'English'", "Watch a non-English language film"),
            ("Country != 'USA'", "Watch a film produced outside the USA"),
            ("Country == 'Japan'", "Watch a Japanese production"),
            ("Country == 'France'", "Watch a French film"),
            ("Country == 'Spain'", "Watch a Spanish production"),
            ("Country == 'Germany'", "Watch a German film"),
            ("Country == 'Italy'", "Watch an Italian film"),
            ("Country == 'United Kingdom'", "Watch a British production"),
            ("Country == 'Canada'", "Watch a Canadian film"),
            ("Country == 'South Korea'", "Watch a South Korean film"),
            ("Country == 'Mexico'", "Watch a Mexican production"),
            ("Country == 'Australia'", "Watch an Australian production"),
            ("Country == 'India'", "Watch an Indian production"),
            ("Country == 'Brazil'", "Watch a Brazilian film"),
            ("Country == 'Hong Kong'", "Watch a Hong Kong cinema film"),
            
            // Eras / Release Years
            ("ReleaseYear < 1960", "Watch a film from the Golden Age (pre-1960)"),
            ("ReleaseYear < 1980", "Watch a classic film (pre-1980)"),
            ("ReleaseYear < 1990", "Watch a retro film (pre-1990)"),
            ("ReleaseYear >= 2020", "Watch a modern film released in the 2020s"),
            ("ReleaseYear >= 2000", "Watch a 21st-century film"),
            ("ReleaseYear >= 1980", "Watch a post-classic era film (1980+)"),
            ("ReleaseYear >= 1990", "Watch a film released since 1990"),
            
            // TMDb Ratings / Critical acclaim
            ("TmdbRating >= 8.2", "Watch a universally acclaimed masterpiece (TMDb 8.2+)"),
            ("TmdbRating >= 8.0", "Watch a highly-rated film (TMDb 8.0+)"),
            ("TmdbRating >= 7.5", "Watch a well-rated film (TMDb 7.5+)"),
            ("TmdbRating >= 7.0", "Watch a solid, positively rated film (TMDb 7.0+)"),
            ("TmdbRating < 6.0", "Watch an underdog or cult film (TMDb < 6.0)"),
            ("TmdbRating < 6.5", "Watch a controversial or mixed-review film (TMDb < 6.5)")
        };

        // Find expressions currently active on the board to avoid picking duplicates
        var activeExpressions = boardObjectives.Select(o => o.RequirementExpression).ToHashSet();

        var availablePool = defaultPool
            .Where(p => !activeExpressions.Contains(p.Expression))
            .ToList();

        // Fallback to all if pool is exhausted
        if (!availablePool.Any())
        {
            availablePool = defaultPool.Where(p => p.Expression != objective.RequirementExpression).ToList();
        }

        var selected = availablePool[Random.Shared.Next(availablePool.Count)];

        objective.Reroll(selected.Expression, selected.Description);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Fetch and return the updated grid
        return await _mediator.Send(new GetBingoGridQuery(request.UserId, objective.GridSize), cancellationToken);
    }
}
