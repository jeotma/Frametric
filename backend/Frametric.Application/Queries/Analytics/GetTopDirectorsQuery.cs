using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using MediatR;

namespace Frametric.Application.Queries.Analytics;

public record GetTopDirectorsQuery(Guid UserId, int Limit = 10) : IRequest<List<DirectorLeaderboardDto>>;

public class GetTopDirectorsQueryHandler : IRequestHandler<GetTopDirectorsQuery, List<DirectorLeaderboardDto>>
{
    private readonly IAnalyticsService _analyticsService;

    public GetTopDirectorsQueryHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<List<DirectorLeaderboardDto>> Handle(GetTopDirectorsQuery request, CancellationToken cancellationToken)
    {
        return await _analyticsService.GetTopDirectorsAsync(request.UserId, request.Limit, cancellationToken);
    }
}



