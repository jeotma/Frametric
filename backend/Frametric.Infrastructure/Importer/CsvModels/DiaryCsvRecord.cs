using CsvHelper.Configuration.Attributes;

namespace Frametric.Infrastructure.Importer.CsvModels;

public class DiaryCsvRecord
{
    [Name("Date")]
    public DateOnly Date { get; set; }

    [Name("Name")]
    public string Name { get; set; } = string.Empty;

    [Name("Year")]
    public int? Year { get; set; }

    [Name("Letterboxd URI")]
    public string LetterboxdUri { get; set; } = string.Empty;

    [Name("Rating")]
    public decimal? Rating { get; set; }

    [Name("Rewatch")]
    public string RewatchRaw { get; set; } = string.Empty;

    [Name("Tags")]
    public string? Tags { get; set; }

    [Name("Watched Date")]
    public DateOnly? WatchedDate { get; set; }
}
