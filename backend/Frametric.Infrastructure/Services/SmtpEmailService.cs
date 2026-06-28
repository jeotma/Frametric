// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Frametric.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(ILogger<SmtpEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SendPromotionNotificationAsync(string promotedUsername, string promotedEmail, string newRole, string promotedBy)
    {
        _logger.LogInformation("[DEV ONLY AUDIT] User {Username} promoted to {Role} by {PromotedBy}.", promotedUsername, newRole, promotedBy);
        System.Diagnostics.Debug.WriteLine($"[DEV ONLY AUDIT] User {promotedUsername} ({promotedEmail}) promoted to {newRole} by {promotedBy}.");
        return Task.CompletedTask;
    }
}
