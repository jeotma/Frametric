using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watchlist;

// 29. The "Golden Pending Director": Director from the watchlist with the best average ratings
public record GetGoldenPendingDirectorQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<GoldenDirectorDto?>;
public class GetGoldenPendingDirectorQueryHandler : IRequestHandler<GetGoldenPendingDirectorQuery, GoldenDirectorDto?>
{
    private readonly IWatchlistComplexCorrelationsQueries _queries;
    public GetGoldenPendingDirectorQueryHandler(IWatchlistComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<GoldenDirectorDto?> Handle(GetGoldenPendingDirectorQuery request, CancellationToken ct)
    {
        return await _queries.GetGoldenPendingDirectorAsync(request.UserId, request.Filter, ct);
    }
}

// 30. Duration balance (short, medium, long movies)
public record GetPendingDurationBalanceQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<DurationBalanceDto>>;
public class GetPendingDurationBalanceQueryHandler : IRequestHandler<GetPendingDurationBalanceQuery, IEnumerable<DurationBalanceDto>>
{
    private readonly IWatchlistComplexCorrelationsQueries _queries;
    public GetPendingDurationBalanceQueryHandler(IWatchlistComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<IEnumerable<DurationBalanceDto>> Handle(GetPendingDurationBalanceQuery request, CancellationToken ct)
    {
        return await _queries.GetPendingDurationBalanceAsync(request.UserId, request.Filter, ct);
    }
}

// 31. Watchlist by Era
public record GetWatchlistByEraQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<EraBreakdownDto>>;
public class GetWatchlistByEraQueryHandler : IRequestHandler<GetWatchlistByEraQuery, IEnumerable<EraBreakdownDto>>
{
    private readonly IWatchlistComplexCorrelationsQueries _queries;
    public GetWatchlistByEraQueryHandler(IWatchlistComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<IEnumerable<EraBreakdownDto>> Handle(GetWatchlistByEraQuery request, CancellationToken ct)
    {
        return await _queries.GetWatchlistByEraAsync(request.UserId, request.Filter, ct);
    }
}

// 32. "Ghost Actor": Actor in many pending movies but 0 in the history
public record GetGhostActorQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<GhostActorDto?>;
public class GetGhostActorQueryHandler : IRequestHandler<GetGhostActorQuery, GhostActorDto?>
{
    private readonly IWatchlistComplexCorrelationsQueries _queries;
    public GetGhostActorQueryHandler(IWatchlistComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<GhostActorDto?> Handle(GetGhostActorQuery request, CancellationToken ct)
    {
        return await _queries.GetGhostActorAsync(request.UserId, request.Filter, ct);
    }
}



