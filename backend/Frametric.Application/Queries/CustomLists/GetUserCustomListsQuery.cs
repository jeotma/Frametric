using Frametric.Application.DTOs.Discovery;
using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.CustomLists;

public record GetUserCustomListsQuery(Guid UserId) : IRequest<List<CustomListDto>>;

public class GetUserCustomListsQueryHandler : IRequestHandler<GetUserCustomListsQuery, List<CustomListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserCustomListsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomListDto>> Handle(GetUserCustomListsQuery request, CancellationToken cancellationToken)
    {
        var lists = await _context.CustomLists
            .Include(c => c.Items)
            .ThenInclude(i => i.Movie)
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return lists.Select(createdList => new CustomListDto
        {
            Id = createdList.Id,
            Name = createdList.Name,
            CreatedAt = createdList.CreatedAt,
            Movies = createdList.Items.Select(i => new Frametric.Application.DTOs.Analytics.MovieSimpleDto(
                i.Movie.Id, 
                i.Movie.Title, 
                i.Movie.ReleaseYear, 
                i.Movie.PosterUrl,
                false,
                i.Nickname)).ToList()
        }).ToList();
    }
}
