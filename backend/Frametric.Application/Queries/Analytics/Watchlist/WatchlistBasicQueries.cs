using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watchlist;

// 18. Filter pending movies by original release year
public record GetWatchlistByYearQuery(Guid UserId, int ReleaseYear) : IRequest<IEnumerable<MovieSimpleDto>>;
public class GetWatchlistByYearQueryHandler : IRequestHandler<GetWatchlistByYearQuery, IEnumerable<MovieSimpleDto>>
{
    private readonly IWatchlistBasicQueries _queries;
    public GetWatchlistByYearQueryHandler(IWatchlistBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<MovieSimpleDto>> Handle(GetWatchlistByYearQuery request, CancellationToken ct)
    {
        return await _queries.GetWatchlistByYearAsync(request.UserId, request.ReleaseYear, ct);
    }
}

// 19. List directors that the user has pending to watch
public record GetWatchlistDirectorsQuery(Guid UserId) : IRequest<IEnumerable<DirectorCountDto>>;
public class GetWatchlistDirectorsQueryHandler : IRequestHandler<GetWatchlistDirectorsQuery, IEnumerable<DirectorCountDto>>
{
    private readonly IWatchlistBasicQueries _queries;
    public GetWatchlistDirectorsQueryHandler(IWatchlistBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<DirectorCountDto>> Handle(GetWatchlistDirectorsQuery request, CancellationToken ct)
    {
        return await _queries.GetWatchlistDirectorsAsync(request.UserId, ct);
    }
}

// 20. List actors present in the pending movies list
public record GetWatchlistActorsQuery(Guid UserId) : IRequest<IEnumerable<ActorCountDto>>;
public class GetWatchlistActorsQueryHandler : IRequestHandler<GetWatchlistActorsQuery, IEnumerable<ActorCountDto>>
{
    private readonly IWatchlistBasicQueries _queries;
    public GetWatchlistActorsQueryHandler(IWatchlistBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<ActorCountDto>> Handle(GetWatchlistActorsQuery request, CancellationToken ct)
    {
        return await _queries.GetWatchlistActorsAsync(request.UserId, ct);
    }
}

// 21. Group pending list by cinematic genre
public record GetWatchlistByGenreQuery(Guid UserId) : IRequest<IEnumerable<GenreCountDto>>;
public class GetWatchlistByGenreQueryHandler : IRequestHandler<GetWatchlistByGenreQuery, IEnumerable<GenreCountDto>>
{
    private readonly IWatchlistBasicQueries _queries;
    public GetWatchlistByGenreQueryHandler(IWatchlistBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<GenreCountDto>> Handle(GetWatchlistByGenreQuery request, CancellationToken ct)
    {
        return await _queries.GetWatchlistByGenreAsync(request.UserId, ct);
    }
}

// 22. Count of pending movies grouped by their release decade
public record GetWatchlistByDecadeQuery(Guid UserId) : IRequest<IEnumerable<DecadeCountDto>>;
public class GetWatchlistByDecadeQueryHandler : IRequestHandler<GetWatchlistByDecadeQuery, IEnumerable<DecadeCountDto>>
{
    private readonly IWatchlistBasicQueries _queries;
    public GetWatchlistByDecadeQueryHandler(IWatchlistBasicQueries queries) => _queries = queries;

    public async Task<IEnumerable<DecadeCountDto>> Handle(GetWatchlistByDecadeQuery request, CancellationToken ct)
    {
        return await _queries.GetWatchlistByDecadeAsync(request.UserId, ct);
    }
}
