using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using MediatR;

namespace Frametric.Application.Queries.Analytics;

public record GetWrappedSummaryQuery(Guid UserId, int? Year = null) : IRequest<WrappedSummaryDto>;

public class GetWrappedSummaryQueryHandler : IRequestHandler<GetWrappedSummaryQuery, WrappedSummaryDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ICacheService _cacheService;

    public GetWrappedSummaryQueryHandler(IAnalyticsService analyticsService, ICacheService cacheService)
    {
        _analyticsService = analyticsService;
        _cacheService = cacheService;
    }

    public async Task<WrappedSummaryDto> Handle(GetWrappedSummaryQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"WrappedSummary_{request.UserId}_{request.Year}";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await _analyticsService.GetWrappedSummaryAsync(request.UserId, request.Year, cancellationToken),
            TimeSpan.FromMinutes(15)
        );
    }
}
