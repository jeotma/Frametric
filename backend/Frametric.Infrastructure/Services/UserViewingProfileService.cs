// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Application.Queries.Recommendations;
using Frametric.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Frametric.Infrastructure.Services;

public class UserViewingProfileService : BackgroundService, IUserViewingProfileService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserViewingProfileService> _logger;
    private readonly ConcurrentDictionary<Guid, DateTime> _schedules = new();

    public UserViewingProfileService(
        IServiceProvider serviceProvider,
        ICacheService cacheService,
        ILogger<UserViewingProfileService> logger)
    {
        _serviceProvider = serviceProvider;
        _cacheService = cacheService;
        _logger = logger;
    }

    public Task<UserViewingProfile> GetOrCreateProfileAsync(Guid userId)
    {
        var key = $"user_viewing_profile:{userId}";
        return _cacheService.GetOrCreateAsync(key, () => BuildProfileInternalAsync(userId), TimeSpan.FromDays(30));
    }

    public async Task<UserViewingProfile> RebuildProfileAsync(Guid userId)
    {
        _logger.LogInformation("Rebuilding user viewing profile immediately for User {UserId}", userId);
        var key = $"user_viewing_profile:{userId}";
        _cacheService.Remove(key);
        return await GetOrCreateProfileAsync(userId);
    }

    public void ScheduleRebuild(Guid userId)
    {
        var runTime = DateTime.UtcNow.AddMinutes(15);
        _schedules[userId] = runTime;
        _logger.LogInformation("Scheduled profile rebuild for User {UserId} at {RunTime} (sliding delay reset)", userId, runTime);
    }

    private async Task<UserViewingProfile> BuildProfileInternalAsync(Guid userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<IRecommendationQueries>();
        
        _logger.LogInformation("Generating fresh UserViewingProfile for User {UserId} from DB", userId);
        var watchedDetails = await queries.GetWatchedMovieDetailsAsync(userId);
        
        var builder = new ProfileBuilder();
        return builder.Build(watchedDetails.ToList());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserViewingProfileService background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                var now = DateTime.UtcNow;
                var readyUsers = _schedules
                    .Where(kvp => now >= kvp.Value)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var userId in readyUsers)
                {
                    if (_schedules.TryRemove(userId, out _))
                    {
                        try
                        {
                            await RebuildProfileAsync(userId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing scheduled profile rebuild for User {UserId}", userId);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserViewingProfileService background execution loop");
            }
        }

        _logger.LogInformation("UserViewingProfileService background worker stopping.");
    }

    private class ProfileBuilder : RecommendationStrategyBase
    {
        public override RecommendationStrategy Strategy => RecommendationStrategy.PureRandom;

        public override List<RecommendedMovieDto> Recommend(
            List<CandidateMovieDto> candidates,
            List<WatchedMovieDetailDto> watched,
            UserViewingProfile profile,
            int quantity,
            int? maxRuntime = null)
        {
            throw new NotImplementedException();
        }

        public UserViewingProfile Build(List<WatchedMovieDetailDto> watched)
        {
            return BuildViewingProfile(watched);
        }
    }
}
