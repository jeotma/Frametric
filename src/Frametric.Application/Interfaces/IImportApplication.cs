using Frametric.Application.DTOs.Imports;

namespace Frametric.Application.Interfaces;

public interface IImportApplication
{
    Task<Guid> ImportLetterboxdAsync(Guid userId, Stream zipStream, CancellationToken cancellationToken);
    Task<List<ImportHistoryDto>> GetImportHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> DeleteImportAsync(Guid userId, Guid importId, CancellationToken cancellationToken);
}
