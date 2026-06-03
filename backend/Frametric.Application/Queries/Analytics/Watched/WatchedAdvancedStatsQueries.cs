using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watched;

// 6. Identify the most repeated actor or actress in watched movies
public record GetMostRepeatedActorQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<ActorCountDto?>;
public class GetMostRepeatedActorQueryHandler : IRequestHandler<GetMostRepeatedActorQuery, ActorCountDto?>
{
    private readonly IWatchedAdvancedStatsQueries _queries;
    public GetMostRepeatedActorQueryHandler(IWatchedAdvancedStatsQueries queries) => _queries = queries;

    public async Task<ActorCountDto?> Handle(GetMostRepeatedActorQuery request, CancellationToken ct)
    {
        return await _queries.GetMostRepeatedActorAsync(request.UserId, request.Filter, ct);
    }
}

// 7. Identify the director with the most movies in the watched history
public record GetMostWatchedDirectorQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<DirectorCountDto?>;
public class GetMostWatchedDirectorQueryHandler : IRequestHandler<GetMostWatchedDirectorQuery, DirectorCountDto?>
{
    private readonly IWatchedAdvancedStatsQueries _queries;
    public GetMostWatchedDirectorQueryHandler(IWatchedAdvancedStatsQueries queries) => _queries = queries;

    public async Task<DirectorCountDto?> Handle(GetMostWatchedDirectorQuery request, CancellationToken ct)
    {
        return await _queries.GetMostWatchedDirectorAsync(request.UserId, request.Filter, ct);
    }
}

// 8. Determine the predominant era (classic or modern) in the user's history
public record GetPredominantEraQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<EraBreakdownDto?>;
public class GetPredominantEraQueryHandler : IRequestHandler<GetPredominantEraQuery, EraBreakdownDto?>
{
    private readonly IWatchedAdvancedStatsQueries _queries;
    public GetPredominantEraQueryHandler(IWatchedAdvancedStatsQueries queries) => _queries = queries;

    public async Task<EraBreakdownDto?> Handle(GetPredominantEraQuery request, CancellationToken ct)
    {
        return await _queries.GetPredominantEraAsync(request.UserId, request.Filter, ct);
    }
}

// 9. Ranking of directors based on user ratings
public record GetDirectorRankingByRatingQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<DirectorLeaderboardDto>>;
public class GetDirectorRankingByRatingQueryHandler : IRequestHandler<GetDirectorRankingByRatingQuery, IEnumerable<DirectorLeaderboardDto>>
{
    private readonly IWatchedAdvancedStatsQueries _queries;
    public GetDirectorRankingByRatingQueryHandler(IWatchedAdvancedStatsQueries queries) => _queries = queries;

    public async Task<IEnumerable<DirectorLeaderboardDto>> Handle(GetDirectorRankingByRatingQuery request, CancellationToken ct)
    {
        return await _queries.GetDirectorRankingByRatingAsync(request.UserId, request.Filter, ct);
    }
}

// 10. Calculation of total time invested watching movies of a specific director or genre
public record GetTotalTimeByDirectorOrGenreQuery(Guid UserId, string FilterType, string FilterName, AnalyticsFilterDto Filter) : IRequest<TimeInvestedDto?>;
public class GetTotalTimeByDirectorOrGenreQueryHandler : IRequestHandler<GetTotalTimeByDirectorOrGenreQuery, TimeInvestedDto?>
{
    private readonly IWatchedAdvancedStatsQueries _queries;
    public GetTotalTimeByDirectorOrGenreQueryHandler(IWatchedAdvancedStatsQueries queries) => _queries = queries;

    public async Task<TimeInvestedDto?> Handle(GetTotalTimeByDirectorOrGenreQuery request, CancellationToken ct)
    {
        return await _queries.GetTotalTimeByDirectorOrGenreAsync(request.UserId, request.FilterType, request.FilterName, request.Filter, ct);
    }
}



