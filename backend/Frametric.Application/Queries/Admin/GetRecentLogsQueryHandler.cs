// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.DTOs.Admin;
using Frametric.Application.Interfaces;
using MediatR;

namespace Frametric.Application.Queries.Admin;

public class GetRecentLogsQueryHandler : IRequestHandler<GetRecentLogsQuery, List<LogEntryDto>>
{
    private readonly IDiagnosticsLogContainer _logContainer;

    public GetRecentLogsQueryHandler(IDiagnosticsLogContainer logContainer)
    {
        _logContainer = logContainer;
    }

    public Task<List<LogEntryDto>> Handle(GetRecentLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = _logContainer.GetLogs();
        return Task.FromResult(logs);
    }
}
