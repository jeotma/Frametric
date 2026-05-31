namespace Frametric.Application.DTOs.Analytics;

public record MonthlyActivityResponseDto(
    List<MonthlyWatchesDto> MonthlyActivity,
    List<WeeklyWatchesDto> WeeklyActivity
);

