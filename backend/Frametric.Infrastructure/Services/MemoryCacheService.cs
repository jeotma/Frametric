// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Frametric.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit for key {CacheKey}", key);
            return cachedValue!;
        }

        _logger.LogDebug("Cache miss for key {CacheKey}. Fetching data...", key);
        var value = await factory();

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        _memoryCache.Set(key, value, cacheEntryOptions);
        _cacheKeys.TryAdd(key, true);

        return value;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
        _logger.LogDebug("Cache invalidated for key {CacheKey}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in keysToRemove)
        {
            Remove(key);
        }
        
        _logger.LogDebug("Cache invalidated for {Count} keys with prefix {Prefix}", keysToRemove.Count, prefix);
    }
}
