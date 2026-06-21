// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Commands.ImportData;
using Frametric.Application.Commands.Imports;
using Frametric.Application.DTOs.Imports;
using Frametric.Application.Interfaces;
using Frametric.Application.Queries.Imports;
using MediatR;

namespace Frametric.Application.Services;

public class ImportApplication : IImportApplication
{
    private readonly IMediator _mediator;

    public ImportApplication(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Guid> ImportLetterboxdAsync(Guid userId, Stream zipStream, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ImportLetterboxdArchiveCommand(userId, zipStream), cancellationToken);
    }

    public async Task<List<ImportHistoryDto>> GetImportHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new GetImportHistoryQuery(userId), cancellationToken);
    }

    public async Task<bool> DeleteImportAsync(Guid userId, Guid importId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DeleteImportCommand(userId, importId), cancellationToken);
    }
}
