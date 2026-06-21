// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Domain.Entities;

public class CustomListItem
{
    public Guid CustomListId { get; private set; }
    public Guid MovieId { get; private set; }
    public DateTime AddedAt { get; private set; }

    // Navigation properties
    public CustomList CustomList { get; private set; } = null!;
    public Movie Movie { get; private set; } = null!;

    private CustomListItem() { }

    public CustomListItem(Guid customListId, Guid movieId, DateTime addedAt)
    {
        CustomListId = customListId;
        MovieId = movieId;
        AddedAt = addedAt;
    }
}
