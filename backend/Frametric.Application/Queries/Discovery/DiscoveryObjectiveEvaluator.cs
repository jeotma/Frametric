// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Linq;
using Frametric.Domain.Discovery.Entities;
using Frametric.Domain.Entities;

namespace Frametric.Application.Queries.Discovery;

public static class DiscoveryObjectiveEvaluator
{
    public static bool Matches(string expression, DiaryEntry diaryEntry)
    {
        var movie = diaryEntry.Movie;
        if (movie == null)
        {
            return false;
        }

        expression = expression.Trim();

        return expression switch
        {
            "RuntimeMinutes < 90" => movie.RuntimeMinutes.HasValue && movie.RuntimeMinutes.Value < 90,
            "RuntimeMinutes > 120" => movie.RuntimeMinutes.HasValue && movie.RuntimeMinutes.Value > 120,
            "Genre == 'Horror'" => HasGenre(movie, "Horror"),
            "Genre == 'Animation'" => HasGenre(movie, "Animation"),
            "Genre == 'Science Fiction'" => HasGenre(movie, "Science Fiction"),
            "Genre == 'Comedy'" => HasGenre(movie, "Comedy"),
            "Genre == 'Drama'" => HasGenre(movie, "Drama"),
            "Genre == 'Action'" => HasGenre(movie, "Action"),
            "Genre == 'Romance'" => HasGenre(movie, "Romance"),
            "Genre == 'Thriller'" => HasGenre(movie, "Thriller"),
            "Genre == 'Fantasy'" => HasGenre(movie, "Fantasy"),
            "IsDocumentary == true" => movie.IsDocumentary,
            "Language != 'English'" => !string.IsNullOrWhiteSpace(movie.Language) && !movie.Language.Equals("English", StringComparison.OrdinalIgnoreCase),
            "Country != 'USA'" => !string.IsNullOrWhiteSpace(movie.Country) && !movie.Country.Equals("USA", StringComparison.OrdinalIgnoreCase),
            "Country == 'Japan'" => movie.Country?.Equals("Japan", StringComparison.OrdinalIgnoreCase) ?? false,
            "Country == 'France'" => movie.Country?.Equals("France", StringComparison.OrdinalIgnoreCase) ?? false,
            "ReleaseYear < 1980" => movie.ReleaseYear.HasValue && movie.ReleaseYear.Value < 1980,
            "TmdbRating >= 8.0" => movie.TmdbRating.HasValue && movie.TmdbRating.Value >= 8.0,
            "TmdbRating >= 7.5" => movie.TmdbRating.HasValue && movie.TmdbRating.Value >= 7.5,
            _ => false,
        };
    }

    private static bool HasGenre(Movie movie, string genre)
    {
        return movie.Genres.Any(g => g.Name.Equals(genre, StringComparison.OrdinalIgnoreCase));
    }
}
