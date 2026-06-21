// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Domain.Entities;

public class WatchedMovie
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MovieId { get; private set; }
    public DateOnly Date { get; private set; }
    public Guid? ImportHistoryId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;
    public ImportHistory? ImportHistory { get; private set; }

    public WatchedMovie(Guid id, Guid userId, Guid movieId, DateOnly date, Guid? importHistoryId = null)
    {
        Id = id;
        UserId = userId;
        MovieId = movieId;
        Date = date;
        ImportHistoryId = importHistoryId;
    }

    private WatchedMovie() { } // EF Core
}
