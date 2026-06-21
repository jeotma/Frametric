using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.CustomLists;

public record CreateCustomListCommand(Guid UserId, string Name, List<Guid> MovieIds) : IRequest<CustomListDto>;

public class CreateCustomListCommandHandler : IRequestHandler<CreateCustomListCommand, CustomListDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomListDto> Handle(CreateCustomListCommand request, CancellationToken cancellationToken)
    {
        var list = new CustomList(Guid.NewGuid(), request.UserId, request.Name, DateTime.UtcNow);
        
        foreach (var movieId in request.MovieIds.Distinct())
        {
            var exists = await _context.Movies.AnyAsync(m => m.Id == movieId, cancellationToken);
            if (exists)
            {
                list.Items.Add(new CustomListItem(list.Id, movieId, DateTime.UtcNow));
            }
        }

        _context.CustomLists.Add(list);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch to map correctly to DTO
        var createdList = await _context.CustomLists
            .Include(c => c.Items)
            .ThenInclude(i => i.Movie)
            .FirstAsync(c => c.Id == list.Id, cancellationToken);

        return new CustomListDto
        {
            Id = createdList.Id,
            Name = createdList.Name,
            CreatedAt = createdList.CreatedAt,
            Movies = createdList.Items.Select(i => new Frametric.Application.DTOs.Analytics.MovieSimpleDto(
                i.Movie.Id, 
                i.Movie.Title, 
                i.Movie.ReleaseYear, 
                i.Movie.PosterUrl)).ToList()
        };
    }
}
