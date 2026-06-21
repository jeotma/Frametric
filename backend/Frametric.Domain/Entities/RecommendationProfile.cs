// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Domain.Entities;

public class RecommendationProfile
{
    public Guid UserId { get; private set; }
    public List<string> FavoriteGenres { get; private set; } = new();
    public List<string> RecurringDirectors { get; private set; } = new();
    public int? ComfortDecade { get; private set; }

    public RecommendationProfile(Guid userId, List<string> favoriteGenres, List<string> recurringDirectors, int? comfortDecade)
    {
        UserId = userId;
        FavoriteGenres = favoriteGenres ?? new List<string>();
        RecurringDirectors = recurringDirectors ?? new List<string>();
        ComfortDecade = comfortDecade;
    }
}
