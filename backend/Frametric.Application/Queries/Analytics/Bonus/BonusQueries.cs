using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces.Analytics;
using MediatR;

namespace Frametric.Application.Queries.Analytics.Bonus;

// 33. Weekend Warrior vs Weekday Watcher
public record GetWeekendWarriorStatsQuery(Guid UserId) : IRequest<WeekendWarriorDto?>;
public class GetWeekendWarriorStatsQueryHandler : IRequestHandler<GetWeekendWarriorStatsQuery, WeekendWarriorDto?>
{
    private readonly IBonusQueries _queries;
    public GetWeekendWarriorStatsQueryHandler(IBonusQueries queries) => _queries = queries;

    public async Task<WeekendWarriorDto?> Handle(GetWeekendWarriorStatsQuery request, CancellationToken ct)
    {
        return await _queries.GetWeekendWarriorStatsAsync(request.UserId, ct);
    }
}

// 34. Hidden Gems
public record GetHiddenGemsQuery(Guid UserId) : IRequest<IEnumerable<MovieSimpleDto>>;
public class GetHiddenGemsQueryHandler : IRequestHandler<GetHiddenGemsQuery, IEnumerable<MovieSimpleDto>>
{
    private readonly IBonusQueries _queries;
    public GetHiddenGemsQueryHandler(IBonusQueries queries) => _queries = queries;

    public async Task<IEnumerable<MovieSimpleDto>> Handle(GetHiddenGemsQuery request, CancellationToken ct)
    {
        return await _queries.GetHiddenGemsAsync(request.UserId, ct);
    }
}

// 35. Watchlist Graveyard (Movies in watchlist for the longest time that haven't been watched)
public record GetWatchlistGraveyardQuery(Guid UserId) : IRequest<IEnumerable<MovieSimpleDto>>;
public class GetWatchlistGraveyardQueryHandler : IRequestHandler<GetWatchlistGraveyardQuery, IEnumerable<MovieSimpleDto>>
{
    private readonly IBonusQueries _queries;
    public GetWatchlistGraveyardQueryHandler(IBonusQueries queries) => _queries = queries;

    public async Task<IEnumerable<MovieSimpleDto>> Handle(GetWatchlistGraveyardQuery request, CancellationToken ct)
    {
        return await _queries.GetWatchlistGraveyardAsync(request.UserId, ct);
    }
}

// 36. Cinematic Fatigue (Cinematic Fatigue - average rating when watching 1 movie a day vs 3+ movies a day)
public record GetCinematicFatigueQuery(Guid UserId) : IRequest<CinematicFatigueDto?>;
public class GetCinematicFatigueQueryHandler : IRequestHandler<GetCinematicFatigueQuery, CinematicFatigueDto?>
{
    private readonly IBonusQueries _queries;
    public GetCinematicFatigueQueryHandler(IBonusQueries queries) => _queries = queries;

    public async Task<CinematicFatigueDto?> Handle(GetCinematicFatigueQuery request, CancellationToken ct)
    {
        return await _queries.GetCinematicFatigueAsync(request.UserId, ct);
    }
}
