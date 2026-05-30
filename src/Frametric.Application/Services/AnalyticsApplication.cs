using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Analytics;
using MediatR;

namespace Frametric.Application.Services;

public class AnalyticsApplication : IAnalyticsApplication
{
    private readonly IMediator _mediator;

    public AnalyticsApplication(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<WrappedSummaryDto> GetWrappedSummaryAsync(Guid userId, int year, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new GetWrappedSummaryQuery(userId, year), cancellationToken);
    }

    public async Task<MonthlyActivityResponseDto> GetMonthlyActivityAsync(Guid userId, int year, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new GetMonthlyActivityQuery(userId, year), cancellationToken);
    }

    public async Task<List<DirectorLeaderboardDto>> GetTopDirectorsAsync(Guid userId, int limit, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new GetTopDirectorsQuery(userId, limit), cancellationToken);
    }
}
