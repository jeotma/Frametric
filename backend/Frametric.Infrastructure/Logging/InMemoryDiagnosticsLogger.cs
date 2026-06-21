// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Frametric.Application.DTOs.Admin;
using Frametric.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Frametric.Infrastructure.Logging;

public class InMemoryDiagnosticsLogContainer : IDiagnosticsLogContainer
{
    private readonly ConcurrentQueue<LogEntryDto> _logs = new();
    private const int MaxLogs = 50;

    public void AddLog(string level, string category, string message, Exception? exception = null)
    {
        _logs.Enqueue(new LogEntryDto(DateTime.UtcNow, level, category, message, exception?.ToString()));
        while (_logs.Count > MaxLogs)
        {
            _logs.TryDequeue(out _);
        }
    }

    public List<LogEntryDto> GetLogs() => _logs.ToList();
}

public class InMemoryDiagnosticsLoggerProvider : ILoggerProvider
{
    private readonly IDiagnosticsLogContainer _container;

    public InMemoryDiagnosticsLoggerProvider(IDiagnosticsLogContainer container)
    {
        _container = container;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new InMemoryDiagnosticsLogger(categoryName, _container);
    }

    public void Dispose()
    {
    }
}

public class InMemoryDiagnosticsLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IDiagnosticsLogContainer _container;

    public InMemoryDiagnosticsLogger(string categoryName, IDiagnosticsLogContainer container)
    {
        _categoryName = categoryName;
        _container = container;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning; // Only keep warnings and errors to avoid spamming the ring buffer

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        _container.AddLog(logLevel.ToString(), _categoryName, message, exception);
    }
}
