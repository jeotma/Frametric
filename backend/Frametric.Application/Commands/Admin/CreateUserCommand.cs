using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record CreateUserCommand(string Username, string Email, string Password) : IRequest<Guid>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        // Check if performing user has permission to add users
        if (actor.Role != UserRole.SuperAdmin && !(actor.Role == UserRole.Admin && actor.CanAddUsers))
        {
            throw new UnauthorizedAccessException("Insufficient privileges to create users.");
        }

        // Check for duplicates
        var duplicate = await _context.Users.AnyAsync(u => 
            u.Username.ToLower() == request.Username.ToLower() || 
            u.Email.ToLower() == request.Email.ToLower(), 
            cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException("Username or Email is already registered.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var newUser = new User(Guid.NewGuid(), request.Username, request.Email, passwordHash);

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        return newUser.Id;
    }
}
