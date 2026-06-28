// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// (at your option) any later version.

using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public class PromoteUserCommandHandler : IRequestHandler<PromoteUserCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public PromoteUserCommandHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<bool> Handle(PromoteUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        // Check if performing user has permission to promote
        if (actor.Role != UserRole.SuperAdmin && !(actor.Role == UserRole.Admin && actor.CanPromoteToAdmin))
        {
            throw new UnauthorizedAccessException("Insufficient permissions to promote users.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) return false;

        // Cannot promote someone who is already Admin or SuperAdmin
        if (user.Role == UserRole.Admin || user.Role == UserRole.SuperAdmin) return true;

        user.PromoteToAdmin();
        await _context.SaveChangesAsync(cancellationToken);

        // Send email
        await _emailService.SendPromotionNotificationAsync(user.Username, user.Email, "Admin", actor.Username);

        return true;
    }
}
