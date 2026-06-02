using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watched;

// 1. Filter watched movies by original release year
public record GetWatchedMoviesByReleaseYearQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<MovieSimpleDto>>;
public class GetWatchedMoviesByReleaseYearQueryHandler : IRequestHandler<GetWatchedMoviesByReleaseYearQuery, IEnumerable<MovieSimpleDto>>
{
    private readonly IWatchedBasicQueries _queries;
    public GetWatchedMoviesByReleaseYearQueryHandler(IWatchedBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<MovieSimpleDto>> Handle(GetWatchedMoviesByReleaseYearQuery request, CancellationToken ct)
    {
        return await _queries.GetMoviesByReleaseYearAsync(request.UserId, request.Filter, ct);
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



