// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// (at your option) any later version.

namespace Frametric.Api.DTOs;

public record BingoRequest(
    int GridSize = 3,
    Frametric.Domain.Enums.DiscoveryDataSourceScope Scope = Frametric.Domain.Enums.DiscoveryDataSourceScope.DatabaseOnly,
    System.Collections.Generic.IEnumerable<System.Guid>? CustomSourceIds = null,
    System.Collections.Generic.IEnumerable<string>? CustomSourceTitles = null,
    bool ExcludeWatched = true,
    int? DurationDays = null
);
