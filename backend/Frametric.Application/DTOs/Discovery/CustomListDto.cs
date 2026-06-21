using Frametric.Application.DTOs.Analytics;

namespace Frametric.Application.DTOs.Discovery;

public class CustomListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<MovieSimpleDto> Movies { get; set; } = new();
}
