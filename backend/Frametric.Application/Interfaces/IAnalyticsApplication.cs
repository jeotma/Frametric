using Frametric.Application.DTOs.Analytics;

namespace Frametric.Application.Interfaces;

public interface IAnalyticsApplication
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken);
    Task<WrappedSummaryDto> GetWrappedSummaryAsync(Guid userId, int? year = null, CancellationToken cancellationToken = default);
    Task<MonthlyActivityResponseDto> GetMonthlyActivityAsync(Guid userId, int year, CancellationToken cancellationToken);
    Task<List<DirectorLeaderboardDto>> GetTopDirectorsAsync(Guid userId, int limit, CancellationToken cancellationToken);
}
