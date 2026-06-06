// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Commands.Auth;
using Frametric.Application.Commands.ImportData;
using Frametric.Application.Commands.Imports;
using Frametric.Application.DTOs;
using Frametric.Application.DTOs.Analytics;
using Frametric.Application.DTOs.Imports;
using Frametric.Application.Queries.Analytics;
using Frametric.Application.Queries.Imports;
using Frametric.Application.Services;
using MediatR;
using Moq;
using Xunit;

namespace Frametric.UnitTests;

public class ApplicationServiceFacadeTests
{
    private readonly Mock<IMediator> _mediatorMock;

    public ApplicationServiceFacadeTests()
    {
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task AnalyticsApplication_ShouldForwardRequestsToMediator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dashboardResult = new DashboardSummaryDto(10, 5, 5, new List<GenreCountDto>(), new List<DirectorCountDto>(), new List<ActorCountDto>(), new List<DecadeCountDto>());
        var wrappedResult = new WrappedSummaryDto(2026, 10, 5, 5, new List<GenreCountDto>(), new List<DirectorCountDto>(), new List<ActorCountDto>(), new List<DecadeCountDto>(), new List<MonthlyActivityDto>());
        var monthlyResult = new MonthlyActivityResponseDto(new List<MonthlyWatchesDto>(), new List<WeeklyWatchesDto>());
        var topDirectorsResult = new List<DirectorLeaderboardDto>();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboardResult);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetWrappedSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wrappedResult);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMonthlyActivityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyResult);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTopDirectorsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(topDirectorsResult);

        var service = new AnalyticsApplication(_mediatorMock.Object);

        // Act
        var dashboard = await service.GetDashboardSummaryAsync(userId, CancellationToken.None);
        var wrapped = await service.GetWrappedSummaryAsync(userId, 2026, CancellationToken.None);
        var monthly = await service.GetMonthlyActivityAsync(userId, 2026, CancellationToken.None);
        var topDirectors = await service.GetTopDirectorsAsync(userId, 5, CancellationToken.None);

        // Assert
        Assert.Same(dashboardResult, dashboard);
        Assert.Same(wrappedResult, wrapped);
        Assert.Same(monthlyResult, monthly);
        Assert.Same(topDirectorsResult, topDirectors);
    }

    [Fact]
    public async Task ImportApplication_ShouldForwardRequestsToMediator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var importId = Guid.NewGuid();
        var historyList = new List<ImportHistoryDto>();

        _mediatorMock.Setup(m => m.Send(It.IsAny<ImportLetterboxdArchiveCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(importId);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetImportHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(historyList);
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteImportCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new ImportApplication(_mediatorMock.Object);
        using var stream = new MemoryStream();

        // Act
        var resultImportId = await service.ImportLetterboxdAsync(userId, stream, CancellationToken.None);
        var resultHistory = await service.GetImportHistoryAsync(userId, CancellationToken.None);
        var resultDelete = await service.DeleteImportAsync(userId, importId, CancellationToken.None);

        // Assert
        Assert.Equal(importId, resultImportId);
        Assert.Same(historyList, resultHistory);
        Assert.True(resultDelete);
    }

    [Fact]
    public async Task UserApplication_ShouldForwardRequestsToMediator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var authResponse = new AuthResponseDto("token", "refresh", DateTime.UtcNow.AddDays(7));

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var service = new UserApplication(_mediatorMock.Object);

        // Act
        var resultRegisterId = await service.RegisterAsync("user", "email", "pass", CancellationToken.None);
        var resultLogin = await service.LoginAsync("email", "pass", CancellationToken.None);
        var resultRefresh = await service.RefreshTokenAsync("refresh", CancellationToken.None);

        // Assert
        Assert.Equal(userId, resultRegisterId);
        Assert.Same(authResponse, resultLogin);
        Assert.Same(authResponse, resultRefresh);
    }
}
