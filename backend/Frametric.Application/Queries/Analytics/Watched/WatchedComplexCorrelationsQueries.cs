using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Watched;

// 12. Preferred day of the week for watching movies
public record GetPreferredWatchDayOfWeekQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<PreferredDayDto>>;
public class GetPreferredWatchDayOfWeekQueryHandler : IRequestHandler<GetPreferredWatchDayOfWeekQuery, IEnumerable<PreferredDayDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetPreferredWatchDayOfWeekQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<IEnumerable<PreferredDayDto>> Handle(GetPreferredWatchDayOfWeekQuery request, CancellationToken ct)
    {
        return await _queries.GetPreferredWatchDayOfWeekAsync(request.UserId, request.Filter, ct);
    }
}

// 13. Genre streaks
public record GetGenreStreakQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<GenreStreakDto>>;
public class GetGenreStreakQueryHandler : IRequestHandler<GetGenreStreakQuery, IEnumerable<GenreStreakDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetGenreStreakQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<IEnumerable<GenreStreakDto>> Handle(GetGenreStreakQuery request, CancellationToken ct)
    {
        return await _queries.GetGenreStreakAsync(request.UserId, request.Filter, ct);
    }
}

// 14. Longest watched movie ("El Ladrillo")
public record GetLongestWatchedMovieQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<WrappedMovieDto?>;
public class GetLongestWatchedMovieQueryHandler : IRequestHandler<GetLongestWatchedMovieQuery, WrappedMovieDto?>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetLongestWatchedMovieQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<WrappedMovieDto?> Handle(GetLongestWatchedMovieQuery request, CancellationToken ct)
    {
        return await _queries.GetLongestWatchedMovieAsync(request.UserId, request.Filter, ct);
    }
}

// 15. Evolution of ratings throughout the year
public record GetRatingEvolutionQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<RatingEvolutionDto>>;
public class GetRatingEvolutionQueryHandler : IRequestHandler<GetRatingEvolutionQuery, IEnumerable<RatingEvolutionDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetRatingEvolutionQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<IEnumerable<RatingEvolutionDto>> Handle(GetRatingEvolutionQuery request, CancellationToken ct)
    {
        return await _queries.GetRatingEvolutionAsync(request.UserId, request.Filter, ct);
    }
}

// 17. Casting repetitions (actor pairs)
public record GetCastingRepetitionsQuery(Guid UserId, AnalyticsFilterDto Filter) : IRequest<IEnumerable<CastingPairDto>>;
public class GetCastingRepetitionsQueryHandler : IRequestHandler<GetCastingRepetitionsQuery, IEnumerable<CastingPairDto>>
{
    private readonly IWatchedComplexCorrelationsQueries _queries;
    public GetCastingRepetitionsQueryHandler(IWatchedComplexCorrelationsQueries queries) => _queries = queries;

    public async Task<IEnumerable<CastingPairDto>> Handle(GetCastingRepetitionsQuery request, CancellationToken ct)
    {
        return await _queries.GetCastingRepetitionsAsync(request.UserId, request.Filter, ct);
    }
}



