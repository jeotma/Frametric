// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Collections.Generic;
using Frametric.Domain.Enums;

namespace Frametric.Api.DTOs;

public record RouletteRequest(
    DiscoveryDataSourceScope Scope,
    int WinningThreshold = 1,
    IEnumerable<Guid>? CustomSourceIds = null,
    IEnumerable<string>? CustomSourceTitles = null,
    bool ExcludeWatched = true
);
