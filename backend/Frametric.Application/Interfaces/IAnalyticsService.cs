using Frametric.Application.DTOs.Analytics;

namespace Frametric.Application.Interfaces;

public interface IAnalyticsService
{
    Task<WrappedSummaryDto> GetWrappedSummaryAsync(Guid userId, int year, CancellationToken cancellationToken);
    Task<MonthlyActivityResponseDto> GetMonthlyActivityAsync(Guid userId, int year, CancellationToken cancellationToken);
    Task<List<DirectorLeaderboardDto>> GetTopDirectorsAsync(Guid userId, int limit, CancellationToken cancellationToken);
}
