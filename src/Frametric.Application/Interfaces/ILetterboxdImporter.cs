using Frametric.Application.DTOs.Letterboxd;

namespace Frametric.Application.Interfaces;

public interface ILetterboxdImporter
{
    Task<LetterboxdExportData> ImportFromZipAsync(Stream zipStream, CancellationToken cancellationToken = default);
}
