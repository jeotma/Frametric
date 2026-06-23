// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Linq;
using Frametric.Application;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Recommendations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Frametric.UnitTests;

public class ApplicationDependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<IUserApplication>());
        Assert.NotNull(provider.GetService<IImportApplication>());
        Assert.NotNull(provider.GetService<IAnalyticsApplication>());

        // Strategies
        var strategies = provider.GetServices<IRecommendationStrategy>().ToList();
        Assert.Equal(9, strategies.Count);
    }
}
