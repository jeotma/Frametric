// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using Frametric.Application.DTOs;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.DTOs.EntityDetails;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;
using Xunit;

namespace Frametric.UnitTests;

public class DtoTests
{
    [Fact]
    public void Verify_DataTransferObjects_Properties()
    {
        // 1. TimeInvestedDto
        var timeDto = new TimeInvestedDto("Total", 120, 2);
        Assert.Equal("Total", timeDto.Name);
        Assert.Equal(120, timeDto.TotalMinutes);
        Assert.Equal(2, timeDto.TotalHours);

        // 2. PrimeTimeStatsDto
        var primeDto = new PrimeTimeStatsDto(
            "Sunday", 10, "January", 25, "Monday", 1, "December", 3
        );
        Assert.Equal("Sunday", primeDto.PeakDay);
        Assert.Equal(10, primeDto.PeakDayCount);

        // 3. MovieDetailsDto
        var detailsDto = new MovieDetailsDto(
            Guid.NewGuid(),
            "Title",
            2010,
            120,
            "poster",
            "overview",
            8.5,
            7.5,
            new List<GenreSimpleDto>(),
            new List<DirectorSimpleDto>(),
            new List<ActorSimpleDto>(),
            new List<MovieDiaryEntryDto>()
        );
        Assert.Equal("Title", detailsDto.Title);

        // 4. RecommendedMovieDto
        var recDto = new RecommendedMovieDto(
            Guid.NewGuid(),
            "Inception",
            "Nolan",
            2010,
            85.5,
            "Reason",
            "poster",
            148,
            8.8
        );
        Assert.Equal("Inception", recDto.Title);

        // 5. OmdbRatingsDto
        var omdbDto = new OmdbRatingsDto(8.8, 9.0, 8.5, "Writers", "Awards", "BoxOffice", "Language", "Country", "PG-13");
        Assert.Equal(8.8, omdbDto.ImdbRating);
        Assert.Equal("PG-13", omdbDto.Rated);

        // 6. GlobalSearchResultDto
        var searchDto = new GlobalSearchResultDto(Guid.NewGuid(), 101, "Movie", "Name", 2010, "url", true);
        Assert.Equal("Name", searchDto.TitleOrName);

        // 7. CandidateMovieDto
        var candidateDto = new CandidateMovieDto(
            Guid.NewGuid(),
            "Inception",
            2010,
            148,
            "poster",
            8.8,
            120.0,
            8.5,
            "Action",
            "Nolan",
            "Leo",
            "dream",
            "Oscars",
            "Nolan",
            "English",
            "USA",
            "828M",
            "PG-13",
            "Netflix",
            "Overview",
            8.8,
            8.5,
            8.2,
            DateTime.UtcNow
        );
        Assert.Equal("Inception", candidateDto.Title);
        Assert.NotNull(candidateDto.WatchlistAddedDate);
    }

    [Fact]
    public void Verify_Remaining_DataTransferObjects()
    {
        var actorDetails = new ActorDetailsDto(Guid.NewGuid(), "Actor", 8.0, 5, new List<MovieSimpleDto>());
        Assert.Equal("Actor", actorDetails.Name);
        Assert.Equal(8.0, actorDetails.AverageRating);
        Assert.Equal(5, actorDetails.WatchCount);
        Assert.Empty(actorDetails.Movies);

        var directorDetails = new DirectorDetailsDto(Guid.NewGuid(), "Director", 8.0, 5, new List<MovieSimpleDto>());
        Assert.Equal("Director", directorDetails.Name);
        Assert.Equal(8.0, directorDetails.AverageRating);
        Assert.Equal(5, directorDetails.WatchCount);
        Assert.Empty(directorDetails.Movies);

        var diaryEntry = new MovieDiaryEntryDto(Guid.NewGuid(), "2023-01-01", true, 8.5);
        Assert.Equal("2023-01-01", diaryEntry.DateWatched);
        Assert.True(diaryEntry.IsRewatch);
        Assert.Equal(8.5, diaryEntry.Rating);

        var wrappedMovie = new WrappedMovieDto(Guid.NewGuid(), "Title", 2020, "Path", 120, 8.5) { Liked = true };
        Assert.Equal("Title", wrappedMovie.Title);
        Assert.True(wrappedMovie.Liked);

        var progress = new CollectionProgressDto("Name", 5, 10, 50.0);
        Assert.Equal("Name", progress.CollectionName);
        Assert.Equal(5, progress.WatchedCount);
        Assert.Equal(10, progress.TotalCount);
        Assert.Equal(50.0, progress.ProgressPercentage);

        var prefDay = new PreferredDayDto("Sunday", 10);
        Assert.Equal("Sunday", prefDay.DayOfWeek);
        Assert.Equal(10, prefDay.WatchCount);

        var streak = new GenreStreakDto("Action", 5, DateTime.MinValue, DateTime.MaxValue);
        Assert.Equal("Action", streak.GenreName);
        Assert.Equal(5, streak.StreakLength);

        var evolution = new RatingEvolutionDto(1, 8.5);
        Assert.Equal(1, evolution.Month);
        Assert.Equal(8.5, evolution.AverageRating);

        var langDiv = new LanguageDiversityDto("English", 5);
        Assert.Equal("English", langDiv.Language);
        Assert.Equal(5, langDiv.Count);

        var pair = new CastingPairDto("Actor A", "Actor B", 3);
        Assert.Equal("Actor A", pair.Actor1Name);
        Assert.Equal("Actor B", pair.Actor2Name);
        Assert.Equal(3, pair.CollaborationCount);

        var era = new EraBreakdownDto("Classic", 10);
        Assert.Equal("Classic", era.EraName);
        Assert.Equal(10, era.Count);

        var prop = new GenreProportionDto("Action", 10, 5);
        Assert.Equal("Action", prop.GenreName);
        Assert.Equal(10, prop.WatchedCount);
        Assert.Equal(5, prop.PendingCount);

        var golden = new GoldenDirectorDto("Director", 8.5, 3);
        Assert.Equal("Director", golden.DirectorName);
        Assert.Equal(8.5, golden.AverageRatingInHistory);
        Assert.Equal(3, golden.PendingMoviesCount);

        var balance = new DurationBalanceDto("Feature", 15);
        Assert.Equal("Feature", balance.DurationCategory);
        Assert.Equal(15, balance.Count);

        var ghost = new GhostActorDto("Actor", 4);
        Assert.Equal("Actor", ghost.ActorName);
        Assert.Equal(4, ghost.PendingMoviesCount);

        var warrior = new WeekendWarriorDto(12, 8);
        Assert.Equal(12, warrior.WeekendWatches);
        Assert.Equal(8, warrior.WeekdayWatches);

        var fatigue = new CinematicFatigueDto(7.5, 6.5);
        Assert.Equal(7.5, fatigue.AvgRatingLightDays);
        Assert.Equal(6.5, fatigue.AvgRatingHeavyDays);

        var bookends = new BookendsDto(wrappedMovie, wrappedMovie);
        Assert.Equal(wrappedMovie, bookends.OpeningScene);
        Assert.Equal(wrappedMovie, bookends.FadeToBlack);

        var extreme = new MonthlyExtremeDto(1, "January", wrappedMovie, wrappedMovie);
        Assert.Equal("January", extreme.MonthName);
        Assert.Equal(wrappedMovie, extreme.BestMovie);
        Assert.Equal(wrappedMovie, extreme.WorstMovie);

        var topBottom = new TopBottomMoviesDto(new[] { wrappedMovie }, new[] { wrappedMovie });
        Assert.Single(topBottom.TopRated);
        Assert.Single(topBottom.BottomRated);

        var mostRewatched = new MostRewatchedDto("Title", "Poster", 2020, 3);
        Assert.Equal("Title", mostRewatched.Title);
        Assert.Equal(3, mostRewatched.RewatchCount);

        var davidGoliath = new DavidAndGoliathDto(wrappedMovie, wrappedMovie);
        Assert.Equal(wrappedMovie, davidGoliath.Shortest);
        Assert.Equal(wrappedMovie, davidGoliath.Longest);

        var rookie = new RookieDto("Rookie", 5, 8.5);
        Assert.Equal("Rookie", rookie.Name);
        Assert.Equal(5, rookie.MoviesWatchedThisYear);
        Assert.Equal(8.5, rookie.AverageRating);

        var bestRookies = new BestRookiesDto(new[] { rookie }, new[] { rookie });
        Assert.Single(bestRookies.NewDirectors);
        Assert.Single(bestRookies.NewActors);

        var genreRating = new GenreWithRatingDto("Action", 5, 8.2);
        Assert.Equal("Action", genreRating.GenreName);
        Assert.Equal(5, genreRating.Count);
        Assert.Equal(8.2, genreRating.AverageRating);

        var pairDirAct = new DirectorActorPairDto("Director", "Actor", 4);
        Assert.Equal("Director", pairDirAct.DirectorName);
        Assert.Equal("Actor", pairDirAct.ActorName);
        Assert.Equal(4, pairDirAct.CollaborationCount);

        var activity = new MonthActivityCountDto(2, "February", 12);
        Assert.Equal("February", activity.MonthName);
        Assert.Equal(12, activity.WatchCount);

        var fatigueExp = new CinematicFatigueExpandedDto(8.0, 7.0, "Sunday", 15, "January", 20);
        Assert.Equal(8.0, fatigueExp.AvgRatingLightDays);
        Assert.Equal("Sunday", fatigueExp.SlumpDay);
        Assert.Equal(15, fatigueExp.SlumpDayWatchCount);
        Assert.Equal("January", fatigueExp.SlumpMonth);
        Assert.Equal(20, fatigueExp.SlumpMonthWatchCount);

        var wrappedSummary = new WrappedSummaryDto(
            2023, 1000, 10, 8,
            new List<GenreCountDto>(),
            new List<DirectorCountDto>(),
            new List<ActorCountDto>(),
            new List<DecadeCountDto>(),
            new List<MonthlyActivityDto>()
        );
        Assert.Equal(2023, wrappedSummary.Year);
        Assert.Equal(1000, wrappedSummary.TotalWatchtimeMinutes);
        Assert.Equal(10, wrappedSummary.TotalWatches);
        Assert.Equal(8, wrappedSummary.UniqueMoviesCount);

        var dashboard = new DashboardSummaryDto(
            1200, 15, 12,
            new List<GenreCountDto>(),
            new List<DirectorCountDto>(),
            new List<ActorCountDto>(),
            new List<DecadeCountDto>()
        );
        Assert.Equal(1200, dashboard.TotalWatchtimeMinutes);
    }
}
