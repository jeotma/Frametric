// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Application.DTOs.Discovery;

public record BingoSquareDto(
    Guid ObjectiveId,
    string Description,
    bool IsCompleted,
    DateTime? CompletionDate,
    int Row,
    int Column,
    Guid? MovieId = null,
    string? MovieTitle = null,
    DateOnly? WatchedDate = null
);

public record BingoGridDto(
    int GridSize,
    IReadOnlyList<BingoSquareDto> Squares,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
