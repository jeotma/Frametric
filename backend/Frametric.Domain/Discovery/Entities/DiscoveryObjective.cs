// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Frametric.Domain.Discovery.Entities;

[Table("DiscoveryObjectives")]
public class DiscoveryObjective
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public Guid UserId { get; private set; }

    [Required]
    public int GridSize { get; private set; }

    [Required]
    public int Row { get; private set; }

    [Required]
    public int Column { get; private set; }

    [Required]
    public string RequirementExpression { get; private set; } = null!;

    [Required]
    public string Description { get; private set; } = null!;

    public bool IsAchieved { get; private set; }

    public DateTime? CompletionDate { get; private set; }

    public Guid? FulfillingDiaryEntryId { get; private set; }

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public int RerollCount { get; private set; }

    private DiscoveryObjective() { }

    public void Reroll(string requirementExpression, string description)
    {
        RequirementExpression = requirementExpression;
        Description = description;
        RerollCount++;
    }

    public DiscoveryObjective(Guid id, Guid userId, int gridSize, int row, int column, string requirementExpression, string description, DateTime? startDate = null, DateTime? endDate = null)
    {
        Id = id;
        UserId = userId;
        GridSize = gridSize;
        Row = row;
        Column = column;
        RequirementExpression = requirementExpression;
        Description = description;
        IsAchieved = false;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void MarkAsAchieved(Guid fulfillingDiaryEntryId)
    {
        if (IsAchieved)
        {
            return;
        }

        IsAchieved = true;
        CompletionDate = DateTime.UtcNow;
        FulfillingDiaryEntryId = fulfillingDiaryEntryId;
    }
}
