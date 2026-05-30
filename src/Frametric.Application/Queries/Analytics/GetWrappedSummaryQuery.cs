using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using MediatR;

namespace Frametric.Application.Queries.Analytics;

public record GetWrappedSummaryQuery(Guid UserId, int Year) : IRequest<WrappedSummaryDto>;

public class GetWrappedSummaryQueryHandler : IRequestHandler<GetWrappedSummaryQuery, WrappedSummaryDto>
{
    private readonly IAnalyticsService _analyticsService;

    public GetWrappedSummaryQueryHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<WrappedSummaryDto> Handle(GetWrappedSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsService.GetWrappedSummaryAsync(request.UserId, request.Year, cancellationToken);
    }
}
