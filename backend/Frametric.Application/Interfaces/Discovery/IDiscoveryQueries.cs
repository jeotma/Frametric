// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Discovery;
using Frametric.Domain.Enums;

namespace Frametric.Application.Interfaces.Discovery;

public interface IDiscoveryQueries
{
    Task<IEnumerable<Guid>> ResolveMovieIdsByTitlesAsync(IEnumerable<string> titles, CancellationToken ct = default);
    Task<IEnumerable<DiscoveryMoviePoolItemDto>> GetDiscoveryPoolAsync(Guid userId, DiscoveryDataSourceScope scope, IEnumerable<Guid>? customSourceIds, bool excludeWatched = true, Guid? partnerUserId = null, CancellationToken ct = default);
    Task<IEnumerable<string>> GetAvailableCountriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetUserTopGenresAsync(Guid userId, CancellationToken ct = default);
    Task<Guid?> GetUserIdByUsernameAsync(string username, CancellationToken ct = default);
}
