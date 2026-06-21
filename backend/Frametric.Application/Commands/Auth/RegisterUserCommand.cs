using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Frametric.Application.Commands.Auth;

public record RegisterUserCommand(string Username, string Email, string Password) : IRequest<Guid>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$"))
        {
            throw new ArgumentException("Password must be at least 8 characters long, contain an uppercase letter, a lowercase letter, a number, and a special character.");
        }

        var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username, cancellationToken);
        if (existingUser)
        {
            throw new ArgumentException("A user with this username or email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(Guid.NewGuid(), request.Username, request.Email, passwordHash);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
