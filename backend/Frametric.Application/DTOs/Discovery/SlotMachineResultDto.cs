// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Collections.Generic;

namespace Frametric.Application.DTOs.Discovery;

public record SlotReelResultDto(
    string Label,
    string Value
);

public record SlotMachineResultDto(
    Guid MovieId,
    string Title,
    string DirectorName,
    int ReleaseYear,
    string? PosterUrl,
    int? RuntimeMinutes,
    string SelectionMechanismMetadata,
    IReadOnlyList<SlotReelResultDto> ReelResults,
    bool IsJackpot,
    int MatchCount = 0,
    IReadOnlyList<bool>? MatchedReels = null,
    string? Overview = null
);
