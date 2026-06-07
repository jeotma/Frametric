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

        var objectives = await _dbContext.DiscoveryObjectives
            .Where(o => o.UserId == request.UserId && o.GridSize == request.GridSize)
            .ToListAsync(cancellationToken);

        if (!objectives.Any())
        {
            objectives = BuildDefaultObjectives(request.UserId, request.GridSize).ToList();
            foreach (var objective in objectives)
            {
                _dbContext.DiscoveryObjectives.Add(objective);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var watchedEntries = await _dbContext.DiaryEntries
            .Include(entry => entry.Movie)
                .ThenInclude(movie => movie.Genres)
            .Where(entry => entry.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var objective in objectives)
        {
            if (objective.IsAchieved)
            {
                continue;
            }

            var matchingEntry = watchedEntries.FirstOrDefault(entry => DiscoveryObjectiveEvaluator.Matches(objective.RequirementExpression, entry));
            if (matchingEntry != null)
            {
                objective.MarkAsAchieved(matchingEntry.Id);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var squares = objectives
            .OrderBy(o => o.Row)
            .ThenBy(o => o.Column)
            .Select(o => new BingoSquareDto(
                o.Id,
                o.Description,
                o.IsAchieved,
                o.CompletionDate,
                o.Row,
                o.Column))
            .ToList();

        return new BingoGridDto(request.GridSize, squares);
    }

    private static IEnumerable<DiscoveryObjective> BuildDefaultObjectives(Guid userId, int gridSize)
    {
        var definitions = new[]
        {
            ("RuntimeMinutes < 90", "Watch a film under 90 minutes"),
            ("Genre == 'Horror'", "Watch a horror film"),
            ("ReleaseYear < 1980", "Watch a classic film"),
            ("IsDocumentary == true", "Watch a documentary"),
            ("TmdbRating >= 8.0", "Watch a highly rated film"),
            ("Language != 'English'", "Watch a foreign-language film"),
            ("Genre == 'Animation'", "Watch an animated film"),
            ("Genre == 'Science Fiction'", "Watch a science fiction film"),
            ("Country != 'USA'", "Watch a film from outside the USA"),
            ("Genre == 'Comedy'", "Watch a comedy"),
            ("Genre == 'Drama'", "Watch a drama"),
            ("Genre == 'Action'", "Watch an action film"),
            ("Genre == 'Romance'", "Watch a romance film"),
            ("Genre == 'Thriller'", "Watch a thriller"),
            ("TmdbRating >= 7.5", "Watch a well-rated film"),
            ("RuntimeMinutes > 120", "Watch a longer film"),
            ("Country == 'Japan'", "Watch a Japanese film"),
            ("Country == 'France'", "Watch a French film")
        };

        var totalSquares = gridSize * gridSize;
        var orderedDefinitions = definitions.Concat(definitions).Take(totalSquares).ToArray();

        for (var index = 0; index < totalSquares; index++)
        {
            var row = (index / gridSize) + 1;
            var column = (index % gridSize) + 1;
            var (expression, description) = orderedDefinitions[index];
            yield return new DiscoveryObjective(Guid.NewGuid(), userId, gridSize, row, column, expression, description);
        }
    }
}
