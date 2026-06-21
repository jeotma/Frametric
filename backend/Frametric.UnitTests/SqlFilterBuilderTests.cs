// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Linq;
using Dapper;
using Frametric.Application.DTOs.Analytics;
using Frametric.Infrastructure.Queries;
using Xunit;

namespace Frametric.UnitTests;

public class SqlFilterBuilderTests
{
    [Fact]
    public void BuildJoins_ShouldReturnEmpty_WhenNoFiltersApplied()
    {
        // Arrange
        var filter = new AnalyticsFilterDto();
        var parameters = new DynamicParameters();
        var builder = new SqlFilterBuilder(filter, parameters);

        // Act
        var result = builder.BuildJoins();

        // Assert
        Assert.Empty(result.Trim());
    }

    [Fact]
    public void BuildJoins_ShouldIncludeActorDirectorGenreJoins_WhenSpecifiedInFilter()
    {
        // Arrange
        var filter = new AnalyticsFilterDto
        {
            Actor = "Keanu Reeves",
            Director = "Wachowskis",
            Genre = "Sci-Fi"
        };
        var parameters = new DynamicParameters();
        var builder = new SqlFilterBuilder(filter, parameters, isMoviesJoined: false);

        // Act
        var result = builder.BuildJoins();

        // Assert
        Assert.Contains("JOIN \"Movies\"", result);
        Assert.Contains("JOIN \"MovieActor\"", result);
        Assert.Contains("JOIN \"Actors\"", result);
        Assert.Contains("JOIN \"MovieDirector\"", result);
        Assert.Contains("JOIN \"Directors\"", result);
        Assert.Contains("JOIN \"MovieGenre\"", result);
        Assert.Contains("JOIN \"Genres\"", result);
    }

    [Fact]
    public void BuildJoins_ShouldIncludeRatingsJoin_WhenMinMaxRatingGiven()
    {
        // Arrange
        var filter = new AnalyticsFilterDto
        {
            MinRating = 8.0m
        };
        var parameters = new DynamicParameters();
        var builder = new SqlFilterBuilder(filter, parameters, isRatingsJoined: false);

        // Act
        var result = builder.BuildJoins();

        // Assert
        Assert.Contains("LEFT JOIN \"MovieRatings\"", result);
    }

    [Fact]
    public void BuildWhereClause_ShouldGenerateCorrectConditionsAndParameters()
    {
        // Arrange
        var filter = new AnalyticsFilterDto
        {
            WatchYear = 2023,
            ReleaseYear = 1999,
            MinRating = 7.0m,
            MaxRating = 9.5m,
            MinCustomRating = 8.0m,
            MaxCustomRating = 9.0m,
            Actor = "DiCaprio",
            Director = "Nolan",
            Genre = "Sci-Fi"
        };
        var parameters = new DynamicParameters();
        var builder = new SqlFilterBuilder(filter, parameters);

        // Act
        var result = builder.BuildWhereClause();

        // Assert
        Assert.Contains("EXTRACT(YEAR FROM", result);
        Assert.Contains("\"ReleaseYear\" = @ReleaseYear", result);
        Assert.Contains("\"Score\" >= @MinRating", result);
        Assert.Contains("\"Score\" <= @MaxRating", result);
        Assert.Contains("\"CustomAverageRating\" >= @MinCustomRating", result);
        Assert.Contains("\"CustomAverageRating\" <= @MaxCustomRating", result);
        Assert.Contains("f_actor.\"Name\" ILIKE @Actor", result);
        Assert.Contains("f_director.\"Name\" ILIKE @Director", result);
        Assert.Contains("f_genre.\"Name\" ILIKE @Genre", result);

        // Assert parameters
        Assert.Equal(2023, parameters.Get<int>("WatchYear"));
        Assert.Equal(1999, parameters.Get<int>("ReleaseYear"));
        Assert.Equal(7.0m, parameters.Get<decimal>("MinRating"));
        Assert.Equal(9.5m, parameters.Get<decimal>("MaxRating"));
        Assert.Equal(8.0m, parameters.Get<decimal>("MinCustomRating"));
        Assert.Equal(9.0m, parameters.Get<decimal>("MaxCustomRating"));
        Assert.Equal("%DiCaprio%", parameters.Get<string>("Actor"));
        Assert.Equal("%Nolan%", parameters.Get<string>("Director"));
        Assert.Equal("%Sci-Fi%", parameters.Get<string>("Genre"));
    }
}
