// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Frametric.Application.Commands.Auth;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
        {
            // We return true even if user doesn't exist to prevent email enumeration attacks
            return true;
        }

        // Generate a secure random token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes);
        var expiry = DateTime.UtcNow.AddHours(1);

        user.SetPasswordResetToken(token, expiry);
        await _context.SaveChangesAsync(cancellationToken);

        // Ideally this should come from a frontend config
        var resetLink = $"https://frametric.pages.dev/reset-password?token={token}&email={user.Email}";
        
        await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, cancellationToken);

        return true;
    }
}
