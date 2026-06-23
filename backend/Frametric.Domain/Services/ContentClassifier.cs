// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Linq;
using Frametric.Domain.Enums;

namespace Frametric.Domain.Services;

public static class ContentClassifier
{
    public static ContentCategory DetectCategory(
        IEnumerable<string> genreNames,
        IEnumerable<int> genreIds,
        string? keywords,
        int? runtimeMinutes)
    {
        var cleanGenres = genreNames.Select(g => g.ToLowerInvariant()).ToList();
        var cleanIds = genreIds.ToList();
        var kwList = (keywords ?? "")
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLowerInvariant())
            .ToList();

        // 1. Wrestling
        var wrestlingKws = new[] { "wrestling", "professional wrestling", "sports entertainment", "wwe", "aew", "njpw", "wcw", "ecw" };
        if (kwList.Any(k => wrestlingKws.Contains(k)) || cleanGenres.Contains("wrestling"))
            return ContentCategory.Wrestling;

        // 2. Sports
        var sportsKws = new[] { "mma", "mixed martial arts", "ufc", "boxing", "combat sports", "fight card", "sport event" };
        if (kwList.Any(k => sportsKws.Contains(k)) || cleanGenres.Contains("sports"))
            return ContentCategory.Sports;

        // 3. Concerts
        var concertKws = new[] { "concert", "concert film", "live concert", "live performance", "live recording", "music concert", "tour", "world tour" };
        if (kwList.Any(k => concertKws.Contains(k)))
            return ContentCategory.Concerts;

        // 4. Stand-up
        var standupKws = new[] { "stand-up comedy", "standup comedy", "comedy special", "comedian" };
        if (kwList.Any(k => standupKws.Contains(k)))
            return ContentCategory.StandUp;

        // 5. Theater
        var theaterKws = new[] { "stage play", "theatre", "theater", "broadway", "west end", "filmed theater" };
        if (kwList.Any(k => theaterKws.Contains(k)))
            return ContentCategory.Theater;

        // 6. Opera
        var operaKws = new[] { "opera", "operetta" };
        if (kwList.Any(k => operaKws.Contains(k)))
            return ContentCategory.Opera;

        // 7. Check Ballet
        var balletKws = new[] { "ballet", "dance performance" };
        if (kwList.Any(k => balletKws.Contains(k)))
            return ContentCategory.Ballet;

        // 8. Documentary (TMDB genre ID 99)
        if (cleanIds.Contains(99) || cleanGenres.Contains("documentary"))
            return ContentCategory.Documentary;

        // 9. Short Film (TMDB has keywords for short film, or runtime < 40 minutes)
        if (kwList.Contains("short film") || kwList.Contains("short") || (runtimeMinutes.HasValue && runtimeMinutes.Value > 0 && runtimeMinutes.Value < 40))
            return ContentCategory.ShortFilm;

        // 10. Default to Movie
        return ContentCategory.Movie;
    }
}
