using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using MediatR;

namespace Frametric.Application.Queries.Analytics;

public record GetMonthlyActivityQuery(Guid UserId, int Year) : IRequest<MonthlyActivityResponseDto>;

public class GetMonthlyActivityQueryHandler : IRequestHandler<GetMonthlyActivityQuery, MonthlyActivityResponseDto>
{
    private readonly IAnalyticsService _analyticsService;

    public GetMonthlyActivityQueryHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<MonthlyActivityResponseDto> Handle(GetMonthlyActivityQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsService.GetMonthlyActivityAsync(request.UserId, request.Year, cancellationToken);
    }
}
