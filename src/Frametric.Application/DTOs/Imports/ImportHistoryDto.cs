namespace Frametric.Application.DTOs.Imports;

public record ImportHistoryDto(Guid Id, DateTime ImportDate, int RowCount, string Status, string ProviderSource, string? ErrorMessage);
