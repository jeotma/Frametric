// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Commands.Auth;
using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;
using MediatR;

namespace Frametric.Application.Services;

public class UserApplication : IUserApplication
{
    private readonly IMediator _mediator;

    public UserApplication(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Guid> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new RegisterUserCommand(username, email, password), cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new LoginUserCommand(email, password), cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
    }

    public async Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ForgotPasswordCommand(email), cancellationToken);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ResetPasswordCommand(email, token, newPassword), cancellationToken);
    }
}
