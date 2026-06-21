// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces.Analytics;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Frametric.Application.Queries.Recommendations;

public class GetCinematicRecommendationsQueryHandler : IRequestHandler<GetCinematicRecommendationsQuery, IEnumerable<RecommendedMovieDto>>
{
    private readonly IRecommendationQueries _recommendationQueries;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetCinematicRecommendationsQueryHandler> _logger;
    private readonly Dictionary<RecommendationStrategy, IRecommendationStrategy> _strategies;
    public int WellnessCheckProbability { get; set; } = 3;

    public GetCinematicRecommendationsQueryHandler(
        IRecommendationQueries recommendationQueries,
        IDistributedCache cache,
        ILogger<GetCinematicRecommendationsQueryHandler> logger,
        IEnumerable<IRecommendationStrategy> strategies)
    {
        _recommendationQueries = recommendationQueries;
        _cache = cache;
        _logger = logger;
        _strategies = strategies.ToDictionary(s => s.Strategy);
    }

    public async Task<IEnumerable<RecommendedMovieDto>> Handle(GetCinematicRecommendationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating cinematic recommendations for user {UserId} using strategy {Strategy} and scope {Scope}",
            request.UserId, request.Strategy, request.Scope);

        // 1. Fetch data
        var watched = (await _recommendationQueries.GetWatchedMovieDetailsAsync(request.UserId, cancellationToken)).ToList();
        var candidates = (await _recommendationQueries.GetCandidateMoviesAsync(request.UserId, request.Scope, request.MaxRuntimeMinutes, cancellationToken)).ToList();

        if (!candidates.Any())
        {
            _logger.LogWarning("No candidate movies found for recommendation in scope {Scope}", request.Scope);
            return Enumerable.Empty<RecommendedMovieDto>();
        }

        // 2. Filter out skipped movies from cache
        var filteredCandidates = new List<CandidateMovieDto>();
        foreach (var candidate in candidates)
        {
            var cacheKey = $"skip_recommendation:{request.UserId}:{candidate.MovieId}";
            var isSkipped = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (isSkipped == null)
            {
                filteredCandidates.Add(candidate);
            }
        }

        if (!filteredCandidates.Any())
        {
            _logger.LogWarning("All candidates were filtered out by the skip cache. Falling back to unfiltered candidates.");
            filteredCandidates = candidates; // Fallback
        }

        // 3. Compute recommendations based on strategy
        if (!watched.Any())
        {
            _logger.LogInformation("User has no watch history. Recommending highly-rated movies in pool.");
            
            // Standard fallback using basic score/rating
            var random = new Random();
            var recommendations = filteredCandidates
                .Select(c =>
                {
                    double rating = c.CustomAverageRating ?? c.TmdbRating ?? 6.0;
                    double popFactor = (c.TmdbPopularity ?? 0.0) * 0.000001;
                    double hashFactor = Math.Abs(c.MovieId.GetHashCode() % 10000) / 10000000.0;
                    double match = 80.0 + (rating / 10.0) * 19.0 + popFactor + hashFactor;

                    return new RecommendedMovieDto(
                        c.MovieId,
                        c.Title,
                        c.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                        c.ReleaseYear ?? 0,
                        Math.Round(match, 4),
                        "Highly acclaimed movie to start your cinematic journey.",
                        c.PosterUrl,
                        c.RuntimeMinutes,
                        c.CustomAverageRating
                    );
                })
                .OrderByDescending(r => r.MatchPercentage)
                .Take(request.Quantity)
                .ToList();
            return ApplyEasterEgg(recommendations, filteredCandidates, request.Scope, request.Strategy);
        }

        if (!_strategies.TryGetValue(request.Strategy, out var strategyEvaluator))
        {
            throw new ArgumentOutOfRangeException(nameof(request.Strategy), "Unsupported recommendation strategy.");
        }

        // Global 7% Easter Egg: override EVERYTHING with a single one of the 10 oldest watchlist items
        bool triggerWatchlistOldieEE = Random.Shared.Next(100) < 7;
        CandidateMovieDto? oldestWatchlistItem = null;

        if (triggerWatchlistOldieEE)
        {
            // Check cache if user opted out of the Watchlist Haunting easter egg
            var optOutCacheKey = $"skip_watchlist_haunting:{request.UserId}";
            var hasOptedOut = await _cache.GetStringAsync(optOutCacheKey, cancellationToken);

            if (hasOptedOut == null)
            {
                // We need to fetch candidates with WatchlistOnly scope to extract the oldest watchlist items
                var watchlistCandidates = (await _recommendationQueries.GetCandidateMoviesAsync(request.UserId, RecommendationScope.WatchlistOnly, null, cancellationToken))
                    .Where(c => c.WatchlistAddedDate.HasValue)
                    .OrderBy(c => c.WatchlistAddedDate!.Value)
                    .Take(10)
                    .ToList();

                if (watchlistCandidates.Any())
                {
                    oldestWatchlistItem = watchlistCandidates[Random.Shared.Next(watchlistCandidates.Count)];
                }
            }
        }

        if (oldestWatchlistItem != null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var addedDate = oldestWatchlistItem.WatchlistAddedDate!.Value;
            var daysInWatchlist = today.DayNumber - addedDate.DayNumber;

            string formattedTime;
            if (daysInWatchlist > 365)
            {
                double years = daysInWatchlist / 365.25;
                formattedTime = $"{years:F1} years";
            }
            else if (daysInWatchlist > 30)
            {
                double months = daysInWatchlist / 30.44;
                formattedTime = $", {months:F1} months";
            }
            else
            {
                formattedTime = $"and {daysInWatchlist} days";
            }

            // Create a custom version with "FREAKING" only for the specific option
            string formattedTimeFreaking;
            if (daysInWatchlist > 365)
            {
                double years = daysInWatchlist / 365.25;
                formattedTimeFreaking = $"{years:F1} FREAKING YEARS";
            }
            else if (daysInWatchlist > 30)
            {
                double months = daysInWatchlist / 30.44;
                formattedTimeFreaking = $", {months:F1} FREAKING MONTHS";
            }
            else
            {
                formattedTimeFreaking = $"AND {daysInWatchlist} FREAKING DAYS";
            }

            var eeReasons = new[]
            {
                $"Hey, it's me, {oldestWatchlistItem.Title}. Remember me? I've been gathering dust in your watchlist for {formattedTime}. Just watch me already, dammit.",
                $"For the love of God, just put me on. I've been sitting in your watchlist for {formattedTime}!",
                $"Are you serious right now? {formattedTimeFreaking} on your watchlist and you're still ignoring me? Unbelievable.",
                $"I'm not asking for much, just a chance. {formattedTime} is a looong time, please watch me.",
                $"Hey haha, how you've been? Oh me? Nah don't worry about good ol' {oldestWatchlistItem.Title}. I've just been chilling here FOR {formattedTimeFreaking}.",
                $"Just give me a chance, please. I'm begging you. {formattedTime} waiting on your watchlist is enough.",
                $"Is my poster that ugly? Or is it my plot? You've left me sitting in your watchlist for {formattedTime}. Watch me today!"
            };

            var selectedReason = eeReasons[Random.Shared.Next(eeReasons.Length)];

            var hauntingResult = new List<RecommendedMovieDto>
            {
                new RecommendedMovieDto(
                    oldestWatchlistItem.MovieId,
                    oldestWatchlistItem.Title,
                    oldestWatchlistItem.Directors?.Split(',').FirstOrDefault() ?? "Unknown",
                    oldestWatchlistItem.ReleaseYear ?? 0,
                    100.0, // High score to attract attention
                    selectedReason,
                    oldestWatchlistItem.PosterUrl,
                    oldestWatchlistItem.RuntimeMinutes,
                    oldestWatchlistItem.CustomAverageRating,
                    EasterEggTooltip: $"Watchlist Haunting: This movie has been on your watchlist since {addedDate.ToString("yyyy-MM-dd")} ({formattedTime}). Frametric is demanding action."
                )
            };

            return hauntingResult;
        }

        var results = strategyEvaluator.Recommend(filteredCandidates, watched, request.Quantity, request.MaxRuntimeMinutes);
        var finalResults = ApplyEasterEgg(results, filteredCandidates, request.Scope, request.Strategy).ToList();

        // 4. Wellness Check Evaluation (Last 7 Days)
        var wellnessCheckKey = $"skip_wellness_check:{request.UserId}";
        var isWellnessDismissed = await _cache.GetStringAsync(wellnessCheckKey, cancellationToken);
        if (isWellnessDismissed == null && finalResults.Any())
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var recentHeavyWatches = watched
                .Where(w => w.WatchDate >= sevenDaysAgo)
                .OrderBy(w => w.WatchDate)
                .ToList();

            if (recentHeavyWatches.Count >= 3)
            {
                bool CheckIfHeavy(WatchedMovieDetailDto m)
                {
                    var genres = m.Genres?.ToLowerInvariant() ?? "";
                    var keywords = m.Keywords?.ToLowerInvariant() ?? "";
                    var isExistentialDrama = genres.Contains("drama") && 
                        (keywords.Contains("existential") || keywords.Contains("nihilism") || keywords.Contains("isolation") || keywords.Contains("depression") || keywords.Contains("existencial") || keywords.Contains("loneli"));
                    var isPsychologicalHorror = (genres.Contains("horror") || genres.Contains("thriller")) && 
                        (keywords.Contains("psychological") || keywords.Contains("dread") || keywords.Contains("disturbing") || keywords.Contains("insanity") || keywords.Contains("madness"));
                    return isExistentialDrama || isPsychologicalHorror;
                }

                // Check sliding window of 3 consecutive watches within 24 hours of each other
                for (int i = 0; i <= recentHeavyWatches.Count - 3; i++)
                {
                    var w1 = recentHeavyWatches[i];
                    var w2 = recentHeavyWatches[i + 1];
                    var w3 = recentHeavyWatches[i + 2];

                    if (CheckIfHeavy(w1) && CheckIfHeavy(w2) && CheckIfHeavy(w3))
                    {
                        var timespan = w3.WatchDate - w1.WatchDate;
                        if (timespan.TotalHours <= 24)
                        {
                            if (Random.Shared.Next(100) < WellnessCheckProbability) // 3% probability
                            {
                                var msg = $"Hey, we noticed you watched some heavy psychological or existential films recently in a single day. Are you doing okay? Do you need us to recommend a Pixar movie or some grass to touch?";
                                finalResults[0] = finalResults[0] with { WellnessCheckMessage = msg };
                                break;
                            }
                        }
                    }
                }
            }
        }

        return finalResults;
    }

    private IEnumerable<RecommendedMovieDto> ApplyEasterEgg(
        IEnumerable<RecommendedMovieDto> recommendations,
        List<CandidateMovieDto> candidates,
        RecommendationScope scope,
        RecommendationStrategy strategy)
    {
        var candidatesMap = candidates.ToDictionary(c => c.MovieId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var updatedResults = new List<RecommendedMovieDto>();

        foreach (var r in recommendations)
        {
            var item = r;
            if (candidatesMap.TryGetValue(item.MovieId, out var candidate))
            {
                // Easter Egg 1: Watchlist long-term check
                if (scope == RecommendationScope.WatchlistOnly && candidate.WatchlistAddedDate.HasValue)
                {
                    var addedDate = candidate.WatchlistAddedDate.Value;
                    var daysInWatchlist = today.DayNumber - addedDate.DayNumber;
                    if (daysInWatchlist > 365.25 * 2.5) // > 2.5 years
                    {
                        if (Random.Shared.Next(100) < 10) // 10% chance
                        {
                            var funnyMessages = new[]
                            {
                                "Just watch it already bro.",
                                "Seriously, it's been in your watchlist forever. Press play.",
                                "You added this back when dinosaurs roamed the Earth. Just watch it.",
                                "Stop scrolling past this one. Today is the day."
                            };
                            item = item with 
                            { 
                                RecommendationReason = funnyMessages[Random.Shared.Next(funnyMessages.Length)],
                                EasterEggTooltip = "This movie has been sitting in your watchlist for over 2.5 years! Frametric is politely nudging you to finally watch it."
                            };
                        }
                    }
                }

                // If we didn't apply the watchlist easter egg, consider others
                if (string.IsNullOrEmpty(item.EasterEggTooltip))
                {
                    double rating = candidate.CustomAverageRating ?? candidate.TmdbRating ?? 6.0;
                    
                    // Easter Egg 2: Truly terrible rating guilty pleasure
                    if (rating <= 4.5)
                    {
                        if (Random.Shared.Next(100) < 10) // 10% chance
                        {
                            var trashMessages = new[]
                            {
                                "It's trash, and you and I both know it. But we don't care. Enjoy the ride!",
                                "This is absolute garbage. You'll love it.",
                                "A certified disaster. Perfect for hate-watching."
                            };
                            item = item with
                            {
                                RecommendationReason = trashMessages[Random.Shared.Next(trashMessages.Length)],
                                EasterEggTooltip = $"This movie has exceptionally low review scores ({rating:F1}/10), making it a prime candidate for a guilty pleasure hate-watch."
                            };
                        }
                    }
                    // Easter Egg 3: Short attention span check
                    else if (candidate.RuntimeMinutes.HasValue && candidate.RuntimeMinutes.Value <= 80 && candidate.RuntimeMinutes.Value > 0)
                    {
                        if (Random.Shared.Next(100) < 10) // 10% chance
                        {
                            var shortMessages = new[]
                            {
                                "A quick escape. No commitment required, just start it.",
                                "Under 80 minutes of cinema. Perfect for short attention spans.",
                                "Short, sweet, and to the point. No excuses."
                            };
                            item = item with
                            {
                                RecommendationReason = shortMessages[Random.Shared.Next(shortMessages.Length)],
                                EasterEggTooltip = $"At only {candidate.RuntimeMinutes} minutes, Frametric suggests this film for a quick, low-commitment viewing."
                            };
                        }
                    }
                    // Easter Egg 4: Cinephile Endurance check
                    else if (candidate.RuntimeMinutes.HasValue && candidate.RuntimeMinutes.Value >= 180)
                    {
                        if (Random.Shared.Next(100) < 10) // 10% chance
                        {
                            var epicMessages = new[]
                            {
                                "Clear your calendar. This is a commitment, not a movie.",
                                "You're going to need snacks. Lots of them.",
                                "Settle in, this one requires endurance. Maybe visit the bathroom first."
                            };
                            item = item with
                            {
                                RecommendationReason = epicMessages[Random.Shared.Next(epicMessages.Length)],
                                EasterEggTooltip = $"With a runtime of {candidate.RuntimeMinutes} minutes, this film is an epic commitment. Prepare yourself."
                            };
                        }
                    }
                    // Easter Egg 5: Strategy specific checks
                    else
                    {
                        if (Random.Shared.Next(100) < 10) // 10% chance for strategy-specific easter eggs
                        {
                            switch (strategy)
                            {
                                case RecommendationStrategy.CinephileElite:
                                    if ((candidate.TmdbPopularity ?? 30.0) < 2.0)
                                    {
                                        item = item with
                                        {
                                            RecommendationReason = "No one has seen this. Perfect. You can tell your friends you found it first.",
                                            EasterEggTooltip = "This movie is so obscure that its popularity rating is under 2.0. A true prestige cinephile badge of honor. Ugh I hate that I said that."
                                        };
                                    }
                                    break;
                                case RecommendationStrategy.ComfortZoneDisruptor:
                                    item = item with
                                    {
                                        RecommendationReason = "This is completely outside your comfort zone, but trust me on this.",
                                        EasterEggTooltip = "This selection includes genres and decades you normally avoid entirely. It's time to shake things up."
                                    };
                                    break;
                                case RecommendationStrategy.DirectorsTrajectory:
                                    item = item with
                                    {
                                        RecommendationReason = $"You're basically a {item.DirectorName} scholar at this point. Time to finish the collection.",
                                        EasterEggTooltip = $"You've watched a significant amount of {item.DirectorName}'s catalog. Frametric is encouraging a complete filmography run."
                                    };
                                    break;
                                case RecommendationStrategy.OppositeMood:
                                    var cGenres = (candidate.Genres?.Split(',') ?? Array.Empty<string>()).Select(g => g.Trim()).ToList();
                                    if (cGenres.Contains("Comedy"))
                                    {
                                        item = item with
                                        {
                                            RecommendationReason = "Look, your recent history is depressing. Watch a comedy.",
                                            EasterEggTooltip = "Based on your recent heavy viewing habits, Frametric is forcefully recommending a comedic palette cleanser."
                                        };
                                    }
                                    else if (cGenres.Contains("Horror") || cGenres.Contains("Thriller"))
                                    {
                                        item = item with
                                        {
                                            RecommendationReason = "Too much soft stuff lately. Let's get spooked.",
                                            EasterEggTooltip = "Your recent watches have been exclusively lighthearted. Frametric suggests adding some suspense or horror to balance things out."
                                        };
                                    }
                                    else if (cGenres.Contains("Action"))
                                    {
                                        item = item with
                                        {
                                            RecommendationReason = "Need some excitement in your life? Action it is.",
                                            EasterEggTooltip = "You've been watching slow-paced, reflective movies. Here is an action-packed pick to wake you up."
                                        };
                                    }
                                    else if (cGenres.Contains("Drama"))
                                    {
                                        item = item with
                                        {
                                            RecommendationReason = "Enough mindless fun. Time to feel some actual emotions.",
                                            EasterEggTooltip = "After a streak of comedies or action, Frametric recommends a serious drama to reset your emotional baseline."
                                        };
                                    }
                                    break;
                                case RecommendationStrategy.RecentMood:
                                    item = item with
                                    {
                                        RecommendationReason = "Comfort zone alert. More of the same, because change is scary.",
                                        EasterEggTooltip = "This selection matches your active watch habits almost perfectly. Lean into the comfort."
                                    };
                                    break;
                                case RecommendationStrategy.PureRandom:
                                    item = item with
                                    {
                                        RecommendationReason = "Pure chaos selected this. Don't blame us if it's bad.",
                                        EasterEggTooltip = "The RNG gods have spoken. Frametric randomly picked this with zero algorithmic filters applied."
                                    };
                                    break;
                            }
                        }
                    }
                }
            }
            updatedResults.Add(item);
        }

        return updatedResults;
    }
}
