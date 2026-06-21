// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Domain.Entities;

public class CustomList
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<CustomListItem> Items { get; private set; } = new List<CustomListItem>();

    private CustomList() { }

    public CustomList(Guid id, Guid userId, string name, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        Name = name;
        CreatedAt = createdAt;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }
}
