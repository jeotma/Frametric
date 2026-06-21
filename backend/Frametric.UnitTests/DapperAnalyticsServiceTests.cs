// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Infrastructure.Services;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class DapperAnalyticsServiceTests
{
    private readonly Mock<IDbConnectionFactory> _dbConnectionFactoryMock;
    private readonly Mock<DbDataReader> _mockDataReader;
    private readonly DapperAnalyticsService _service;

    public DapperAnalyticsServiceTests()
    {
        _dbConnectionFactoryMock = new Mock<IDbConnectionFactory>();
        _mockDataReader = new Mock<DbDataReader> { CallBase = true };
        
        // Default reader setups
        _mockDataReader.Setup(r => r.Read()).Returns(false);
        _mockDataReader.Setup(r => r.FieldCount).Returns(0);
        _mockDataReader.Setup(r => r.GetSchemaTable()).Returns(new DataTable());
        _mockDataReader.Setup(r => r.GetName(It.IsAny<int>())).Returns(string.Empty);

        var dbCommand = new TestDbCommand(_mockDataReader.Object, 100);
        var dbConnection = new TestDbConnection(dbCommand);

        _dbConnectionFactoryMock.Setup(f => f.CreateConnection()).Returns(dbConnection);
        _service = new DapperAnalyticsService(_dbConnectionFactoryMock.Object);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldExecuteQueriesAndReturnSummary()
    {
        // Act
        var result = await _service.GetDashboardSummaryAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.TotalWatchtimeMinutes);
        Assert.Equal(100, result.TotalWatches);
        Assert.Equal(100, result.UniqueMoviesCount);
    }

    [Fact]
    public async Task GetWrappedSummaryAsync_ShouldReturnWrappedSummary()
    {
        // Act
        var result = await _service.GetWrappedSummaryAsync(Guid.NewGuid(), 2023, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2023, result.Year);
        Assert.Equal(100, result.TotalWatchtimeMinutes);
        Assert.Equal(12, result.MonthlyActivity.Count);
    }

    [Fact]
    public async Task GetMonthlyActivityAsync_ShouldReturnMonthlyActivity()
    {
        // Act
        var result = await _service.GetMonthlyActivityAsync(Guid.NewGuid(), 2023, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.MonthlyActivity.Count);
    }

    [Fact]
    public async Task GetTopDirectorsAsync_ShouldReturnDirectorLeaderboard()
    {
        // Act
        var result = await _service.GetTopDirectorsAsync(Guid.NewGuid(), 5, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
