namespace Frametric.Application.DTOs.Letterboxd;

public class ParsedDiaryDto
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string LetterboxdUri { get; set; } = string.Empty;
    public decimal? Rating { get; set; }
    public bool Rewatch { get; set; }
    public string? Tags { get; set; }
    public DateOnly WatchedDate { get; set; }
}
