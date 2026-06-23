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
using Frametric.Domain.Discovery.Entities;
using Frametric.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Queries.Discovery;

public class GetBingoGridQueryHandler : IRequestHandler<GetBingoGridQuery, BingoGridDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetBingoGridQueryHandler> _logger;

    public GetBingoGridQueryHandler(IApplicationDbContext dbContext, ILogger<GetBingoGridQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BingoGridDto> Handle(GetBingoGridQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving bingo grid for user {UserId} with size {GridSize}", request.UserId, request.GridSize);

        List<DiscoveryObjective> objectives;

        if (request.BoardId.HasValue)
        {
            // Load specific board
            objectives = await _dbContext.DiscoveryObjectives
                .Where(o => o.UserId == request.UserId && o.BoardId == request.BoardId.Value)
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Fetch all objectives for the user
            var allObjectives = await _dbContext.DiscoveryObjectives
                .Where(o => o.UserId == request.UserId && o.GridSize == request.GridSize)
                .ToListAsync(cancellationToken);

            if (request.DurationDays.HasValue)
            {
                // Generate a new board explicitly
                Guid newBoardId = Guid.NewGuid();
                DateTime? startDate = DateTime.UtcNow;
                DateTime? endDate = request.DurationDays.Value > 0
                    ? DateTime.UtcNow.AddDays(request.DurationDays.Value)
                    : null;

                objectives = BuildDefaultObjectives(request.UserId, newBoardId, request.GridSize, startDate, endDate).ToList();
                foreach (var objective in objectives)
                {
                    _dbContext.DiscoveryObjectives.Add(objective);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else if (allObjectives.Any())
            {
                // Load the most recent active (unfinished) board, or fallback to the most recent board overall
                var grouped = allObjectives.GroupBy(o => o.BoardId).Select(g => new
                {
                    BoardId = g.Key,
                    IsCompleted = g.All(o => o.IsAchieved),
                    StartDate = g.Max(o => o.StartDate ?? DateTime.MinValue)
                }).ToList();

                var activeBoard = grouped.Where(g => !g.IsCompleted).OrderByDescending(g => g.StartDate).FirstOrDefault()
                    ?? grouped.OrderByDescending(g => g.StartDate).FirstOrDefault();

                if (activeBoard != null)
                {
                    objectives = allObjectives.Where(o => o.BoardId == activeBoard.BoardId).ToList();
                }
                else
                {
                    // Fallback to building a default board if grouping failed
                    Guid newBoardId = Guid.NewGuid();
                    objectives = BuildDefaultObjectives(request.UserId, newBoardId, request.GridSize, null, null).ToList();
                    foreach (var objective in objectives)
                    {
                        _dbContext.DiscoveryObjectives.Add(objective);
                    }
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                // No boards exist at all, generate first board
                Guid newBoardId = Guid.NewGuid();
                objectives = BuildDefaultObjectives(request.UserId, newBoardId, request.GridSize, null, null).ToList();
                foreach (var objective in objectives)
                {
                    _dbContext.DiscoveryObjectives.Add(objective);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var watchedEntries = await _dbContext.DiaryEntries
            .Include(entry => entry.Movie)
                .ThenInclude(movie => movie.Genres)
            .Where(entry => entry.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var fulfillingIds = objectives
            .Where(o => o.IsAchieved && o.FulfillingDiaryEntryId.HasValue)
            .Select(o => o.FulfillingDiaryEntryId!.Value)
            .ToList();

        var fulfillingEntries = await _dbContext.DiaryEntries
            .Include(entry => entry.Movie)
            .Where(entry => fulfillingIds.Contains(entry.Id))
            .ToDictionaryAsync(entry => entry.Id, cancellationToken);

        var usedEntryIds = new HashSet<Guid>(fulfillingIds);

        foreach (var objective in objectives)
        {
            if (objective.IsAchieved)
            {
                continue;
            }

            var matchingEntry = watchedEntries.FirstOrDefault(entry => 
            {
                if (usedEntryIds.Contains(entry.Id))
                    return false;

                // Only consider diary entries watched within the objective's active period
                if (objective.StartDate.HasValue && entry.WatchedDate < DateOnly.FromDateTime(objective.StartDate.Value))
                    return false;
                if (objective.EndDate.HasValue && entry.WatchedDate > DateOnly.FromDateTime(objective.EndDate.Value))
                    return false;

                return DiscoveryObjectiveEvaluator.Matches(objective.RequirementExpression, entry);
            });

            if (matchingEntry != null)
            {
                objective.MarkAsAchieved(matchingEntry.Id);
                fulfillingEntries[matchingEntry.Id] = matchingEntry;
                usedEntryIds.Add(matchingEntry.Id);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var squares = objectives
            .OrderBy(o => o.Row)
            .ThenBy(o => o.Column)
            .Select(o => {
                DiaryEntry? diaryEntry = null;
                if (o.FulfillingDiaryEntryId.HasValue)
                {
                    fulfillingEntries.TryGetValue(o.FulfillingDiaryEntryId.Value, out diaryEntry);
                }
                return new BingoSquareDto(
                    o.Id,
                    o.Description,
                    o.IsAchieved,
                    o.CompletionDate,
                    o.Row,
                    o.Column,
                    diaryEntry?.MovieId,
                    diaryEntry?.Movie?.Title,
                    diaryEntry?.WatchedDate,
                    o.RerollCount);
            })
            .ToList();

        var firstObjective = objectives.FirstOrDefault();
        var rerollsUsed = objectives.Sum(o => o.RerollCount);
        Guid finalBoardId = firstObjective?.BoardId ?? Guid.Empty;
        
        return new BingoGridDto(finalBoardId, request.GridSize, squares, firstObjective?.StartDate, firstObjective?.EndDate, rerollsUsed);
    }

    private static IEnumerable<DiscoveryObjective> BuildDefaultObjectives(Guid userId, Guid boardId, int gridSize, DateTime? startDate = null, DateTime? endDate = null)
    {
        var pool = new List<(string Expression, string Description)>
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

        var shuffled = pool.OrderBy(_ => Random.Shared.Next()).ToList();
        
        var totalSquares = gridSize * gridSize;
        var selected = new List<(string Expression, string Description)>();
        for (var i = 0; i < totalSquares; i++)
        {
            selected.Add(shuffled[i % shuffled.Count]);
        }

        for (var index = 0; index < totalSquares; index++)
        {
            var row = (index / gridSize) + 1;
            var column = (index % gridSize) + 1;
            var (expression, description) = selected[index];
            yield return new DiscoveryObjective(Guid.NewGuid(), userId, boardId, gridSize, row, column, expression, description, startDate, endDate);
        }
    }
}
