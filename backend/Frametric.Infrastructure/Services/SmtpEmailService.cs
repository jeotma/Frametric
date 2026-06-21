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
        // For development purposes, log without exposing PII. Replace with real SMTP/SendGrid logic for production.
        _logger.LogInformation("Password reset requested and email queued for sending.");
        
        // Output to local debug console only for testing
        System.Diagnostics.Debug.WriteLine($"[DEV ONLY] Password reset requested for {toEmail}. Link: {resetLink}");

        return Task.CompletedTask;
    }
}
