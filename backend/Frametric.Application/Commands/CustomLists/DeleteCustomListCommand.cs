using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.CustomLists;

public record DeleteCustomListCommand(Guid UserId, Guid ListId) : IRequest<bool>;

public class DeleteCustomListCommandHandler : IRequestHandler<DeleteCustomListCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCustomListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCustomListCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.CustomLists
            .FirstOrDefaultAsync(c => c.Id == request.ListId && c.UserId == request.UserId, cancellationToken);

        if (list == null) return false;

        _context.CustomLists.Remove(list);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
