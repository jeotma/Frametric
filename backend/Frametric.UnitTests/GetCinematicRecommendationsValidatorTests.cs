// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using FluentValidation.TestHelper;
using Frametric.Application.Queries.Recommendations;
using Frametric.Domain.Enums;
using Xunit;

namespace Frametric.UnitTests;

public class GetCinematicRecommendationsValidatorTests
{
    private readonly GetCinematicRecommendationsValidator _validator;

    public GetCinematicRecommendationsValidatorTests()
    {
        _validator = new GetCinematicRecommendationsValidator();
    }

    [Fact]
    public void Validator_ShouldPass_ForValidQuery()
    {
        // Arrange
        var query = new GetCinematicRecommendationsQuery(
            Guid.NewGuid(),
            RecommendationStrategy.PureRandom,
            RecommendationScope.Hybrid,
            5,
            120
        );

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetCinematicRecommendationsQuery(
            Guid.Empty,
            RecommendationStrategy.PureRandom,
            RecommendationScope.Hybrid,
            5,
            120
        );

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validator_ShouldFail_WhenQuantityIsInvalid()
    {
        // Arrange
        var query = new GetCinematicRecommendationsQuery(
            Guid.NewGuid(),
            RecommendationStrategy.PureRandom,
            RecommendationScope.Hybrid,
            4, // Invalid quantity (not in 1, 2, 3, 5, 10)
            120
        );

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage("You can only request 1, 2, 3, 5, or 10 recommendations at a time.");
    }
}
