// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Collections.Generic;
using Frametric.Domain.Discovery.Enums;

namespace Frametric.Application.DTOs.Discovery;

public record SingleDieResultDto(
    DiceType DiceType,
    int RollValue,
    string Label,
    string Description
);

public record DiceRollResultDto(
    Guid MovieId,
    string Title,
    string DirectorName,
    int ReleaseYear,
    string? PosterUrl,
    int? RuntimeMinutes,
    string SelectionMechanismMetadata,
    IReadOnlyList<SingleDieResultDto> DiceResults,
    string? SpecialEvent = null,
    string? Overview = null,
    int MatchDistance = 0
);
