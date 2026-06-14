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

        string op = "";
        if (expression.Contains("==")) op = "==";
        else if (expression.Contains("!=")) op = "!=";
        else if (expression.Contains(">=")) op = ">=";
        else if (expression.Contains("<=")) op = "<=";
        else if (expression.Contains(">")) op = ">";
        else if (expression.Contains("<")) op = "<";

        if (string.IsNullOrEmpty(op)) return false;

        var parts = expression.Split(new[] { op }, StringSplitOptions.None);
        if (parts.Length != 2) return false;

        string property = parts[0].Trim();
        string valueStr = parts[1].Trim().Trim('\'');

        switch (property)
        {
            case "RuntimeMinutes":
                if (!movie.RuntimeMinutes.HasValue || !int.TryParse(valueStr, out var runtimeVal)) return false;
                return op switch {
                    "==" => movie.RuntimeMinutes.Value == runtimeVal,
                    "!=" => movie.RuntimeMinutes.Value != runtimeVal,
                    "<" => movie.RuntimeMinutes.Value < runtimeVal,
                    ">" => movie.RuntimeMinutes.Value > runtimeVal,
                    ">=" => movie.RuntimeMinutes.Value >= runtimeVal,
                    "<=" => movie.RuntimeMinutes.Value <= runtimeVal,
                    _ => false
                };

            case "ReleaseYear":
                if (!movie.ReleaseYear.HasValue || !int.TryParse(valueStr, out var yearVal)) return false;
                return op switch {
                    "==" => movie.ReleaseYear.Value == yearVal,
                    "!=" => movie.ReleaseYear.Value != yearVal,
                    "<" => movie.ReleaseYear.Value < yearVal,
                    ">" => movie.ReleaseYear.Value > yearVal,
                    ">=" => movie.ReleaseYear.Value >= yearVal,
                    "<=" => movie.ReleaseYear.Value <= yearVal,
                    _ => false
                };

            case "TmdbRating":
                if (!movie.TmdbRating.HasValue || !double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ratingVal)) return false;
                return op switch {
                    "==" => movie.TmdbRating.Value == ratingVal,
                    "!=" => movie.TmdbRating.Value != ratingVal,
                    "<" => movie.TmdbRating.Value < ratingVal,
                    ">" => movie.TmdbRating.Value > ratingVal,
                    ">=" => movie.TmdbRating.Value >= ratingVal,
                    "<=" => movie.TmdbRating.Value <= ratingVal,
                    _ => false
                };

            case "Genre":
                bool hasGen = HasGenre(movie, valueStr);
                return op switch {
                    "==" => hasGen,
                    "!=" => !hasGen,
                    _ => false
                };

            case "IsDocumentary":
                if (!bool.TryParse(valueStr, out var docVal)) return false;
                return op switch {
                    "==" => movie.IsDocumentary == docVal,
                    "!=" => movie.IsDocumentary != docVal,
                    _ => false
                };

            case "Language":
                bool langMatch = !string.IsNullOrWhiteSpace(movie.Language) && movie.Language.Contains(valueStr, StringComparison.OrdinalIgnoreCase);
                return op switch {
                    "==" => langMatch,
                    "!=" => !langMatch,
                    _ => false
                };

            case "Country":
                bool countryMatch = !string.IsNullOrWhiteSpace(movie.Country) && movie.Country.Equals(valueStr, StringComparison.OrdinalIgnoreCase);
                return op switch {
                    "==" => countryMatch,
                    "!=" => !countryMatch,
                    _ => false
                };

            default:
                return false;
        }
    }

    private static bool HasGenre(Movie movie, string genre)
    {
        return movie.Genres.Any(g => g.Name.Equals(genre, StringComparison.OrdinalIgnoreCase));
    }
}
