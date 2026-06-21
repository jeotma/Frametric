using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watched;

// 1. List watched movies
public record GetWatchedMoviesQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<WatchedMovieStatsDto>>;
public class GetWatchedMoviesQueryHandler : IRequestHandler<GetWatchedMoviesQuery, IEnumerable<WatchedMovieStatsDto>>
{
    private readonly IWatchedBasicQueries _queries;
    private readonly ICacheService _cacheService;

    public GetWatchedMoviesQueryHandler(IWatchedBasicQueries queries, ICacheService cacheService)
    {
        _queries = queries;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<WatchedMovieStatsDto>> Handle(GetWatchedMoviesQuery request, CancellationToken ct)
    {
        string cacheKey = $"WatchedMovies_{request.UserId}_{request.Filter.GetHashCode()}";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await _queries.GetMoviesAsync(request.UserId, request.Filter, ct),
            TimeSpan.FromMinutes(10)
        );
    }
}

// 2. List directors present in the watched history
public record GetWatchedDirectorsQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<DirectorCountDto>>;
public class GetWatchedDirectorsQueryHandler : IRequestHandler<GetWatchedDirectorsQuery, IEnumerable<DirectorCountDto>>
{
    private readonly IWatchedBasicQueries _queries;
    public GetWatchedDirectorsQueryHandler(IWatchedBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<DirectorCountDto>> Handle(GetWatchedDirectorsQuery request, CancellationToken ct)
    {
        return await _queries.GetDirectorsAsync(request.UserId, request.Filter, ct);
    }
}

// 3. List actors that appear in watched movies
public record GetWatchedActorsQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<ActorCountDto>>;
public class GetWatchedActorsQueryHandler : IRequestHandler<GetWatchedActorsQuery, IEnumerable<ActorCountDto>>
{
    private readonly IWatchedBasicQueries _queries;
    public GetWatchedActorsQueryHandler(IWatchedBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<ActorCountDto>> Handle(GetWatchedActorsQuery request, CancellationToken ct)
    {
        return await _queries.GetActorsAsync(request.UserId, request.Filter, ct);
    }
}

// 4. Group watched movies by cinematic genre
public record GetWatchedMoviesByGenreQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<GenreCountDto>>;
public class GetWatchedMoviesByGenreQueryHandler : IRequestHandler<GetWatchedMoviesByGenreQuery, IEnumerable<GenreCountDto>>
{
    private readonly IWatchedBasicQueries _queries;
    public GetWatchedMoviesByGenreQueryHandler(IWatchedBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<GenreCountDto>> Handle(GetWatchedMoviesByGenreQuery request, CancellationToken ct)
    {
        return await _queries.GetMoviesByGenreAsync(request.UserId, request.Filter, ct);
    }
}

// 5. Count of watched movies by release decade
public record GetWatchedMoviesByDecadeQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<DecadeCountDto>>;
public class GetWatchedMoviesByDecadeQueryHandler : IRequestHandler<GetWatchedMoviesByDecadeQuery, IEnumerable<DecadeCountDto>>
{
    private readonly IWatchedBasicQueries _queries;
    public GetWatchedMoviesByDecadeQueryHandler(IWatchedBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<DecadeCountDto>> Handle(GetWatchedMoviesByDecadeQuery request, CancellationToken ct)
    {
        return await _queries.GetMoviesByDecadeAsync(request.UserId, request.Filter, ct);
    }
}



