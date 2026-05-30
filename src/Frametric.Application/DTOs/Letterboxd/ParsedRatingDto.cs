namespace Frametric.Application.DTOs.Letterboxd;

public class ParsedRatingDto
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string LetterboxdUri { get; set; } = string.Empty;
    public decimal Rating { get; set; }
}
