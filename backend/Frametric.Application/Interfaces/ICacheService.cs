// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Frametric â€” Cinematic Analytics Platform
// Copyright (C) 2026 JesÃºs J. Otero MartÃ­nez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Threading.Tasks;

namespace Frametric.Application.Interfaces;

public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache, or executes the factory function to fetch and cache it if not found.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Removes all items from the cache that start with the specified prefix.
    /// </summary>
    void RemoveByPrefix(string prefix);
}
