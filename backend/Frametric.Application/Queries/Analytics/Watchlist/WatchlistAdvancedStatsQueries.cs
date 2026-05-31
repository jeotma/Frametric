using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watchlist;

// 23. Identify the director with the most accumulated pending movies for the user
public record GetMostAnticipatedDirectorQuery(Guid UserId) : IRequest<DirectorCountDto?>;
public class GetMostAnticipatedDirectorQueryHandler : IRequestHandler<GetMostAnticipatedDirectorQuery, DirectorCountDto?>
{
    private readonly IWatchlistAdvancedStatsQueries _queries;
    public GetMostAnticipatedDirectorQueryHandler(IWatchlistAdvancedStatsQueries queries) => _queries = queries;

    public async Task<DirectorCountDto?> Handle(GetMostAnticipatedDirectorQuery request, CancellationToken ct)
    {
        return await _queries.GetMostAnticipatedDirectorAsync(request.UserId, ct);
    }
}

// 24. Identify the actor with the most pending movies in the user's watchlist
public record GetMostAnticipatedActorQuery(Guid UserId) : IRequest<ActorCountDto?>;
public class GetMostAnticipatedActorQueryHandler : IRequestHandler<GetMostAnticipatedActorQuery, ActorCountDto?>
{
    private readonly IWatchlistAdvancedStatsQueries _queries;
    public GetMostAnticipatedActorQueryHandler(IWatchlistAdvancedStatsQueries queries) => _queries = queries;

    public async Task<ActorCountDto?> Handle(GetMostAnticipatedActorQuery request, CancellationToken ct)
    {
        return await _queries.GetMostAnticipatedActorAsync(request.UserId, ct);
    }
}

// 25. Calculation of total accumulated watchtime hours in the pending list
public record GetTotalPendingWatchtimeQuery(Guid UserId) : IRequest<TimeInvestedDto?>;
public class GetTotalPendingWatchtimeQueryHandler : IRequestHandler<GetTotalPendingWatchtimeQuery, TimeInvestedDto?>
{
    private readonly IWatchlistAdvancedStatsQueries _queries;
    public GetTotalPendingWatchtimeQueryHandler(IWatchlistAdvancedStatsQueries queries) => _queries = queries;

    public async Task<TimeInvestedDto?> Handle(GetTotalPendingWatchtimeQuery request, CancellationToken ct)
    {
        return await _queries.GetTotalPendingWatchtimeAsync(request.UserId, ct);
    }
}

// 27. Identify the movie that has been waiting the longest since it was added to the watchlist
public record GetOldestPendingMovieQuery(Guid UserId) : IRequest<MovieSimpleDto?>;
public class GetOldestPendingMovieQueryHandler : IRequestHandler<GetOldestPendingMovieQuery, MovieSimpleDto?>
{
    private readonly IWatchlistAdvancedStatsQueries _queries;
    public GetOldestPendingMovieQueryHandler(IWatchlistAdvancedStatsQueries queries) => _queries = queries;

    public async Task<MovieSimpleDto?> Handle(GetOldestPendingMovieQuery request, CancellationToken ct)
    {
        return await _queries.GetOldestPendingMovieAsync(request.UserId, ct);
    }
}

// 28. Proportion of genres in the watchlist versus already watched movies
public record GetGenreProportionWatchlistVsWatchedQuery(Guid UserId) : IRequest<IEnumerable<GenreProportionDto>>;
public class GetGenreProportionWatchlistVsWatchedQueryHandler : IRequestHandler<GetGenreProportionWatchlistVsWatchedQuery, IEnumerable<GenreProportionDto>>
{
    private readonly IWatchlistAdvancedStatsQueries _queries;
    public GetGenreProportionWatchlistVsWatchedQueryHandler(IWatchlistAdvancedStatsQueries queries) => _queries = queries;

    public async Task<IEnumerable<GenreProportionDto>> Handle(GetGenreProportionWatchlistVsWatchedQuery request, CancellationToken ct)
    {
        return await _queries.GetGenreProportionWatchlistVsWatchedAsync(request.UserId, ct);
    }
}
