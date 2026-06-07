// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections.Generic;
using System.Reflection;
using Frametric.Application.Queries.Discovery;
using Frametric.Domain.Entities;
using Frametric.Domain.ValueObjects;
using Xunit;

namespace Frametric.UnitTests;

public class DiscoveryObjectiveEvaluatorTests
{
    private static Movie CreateMovie(Action<Movie>? configure = null)
    {
        var movie = new Movie(Guid.NewGuid(), "Test Movie", 2000, new ExternalReference("tmdb", "123"));
        movie.EnrichMetadata(
            runtimeMinutes: 100,
            posterUrl: null,
            genres: new List<Genre>(),
            directors: new List<Director>(),
            actors: new List<Actor>(),
            isDocumentary: false,
            tmdbRating: 7.0,
            language: "English",
            country: "USA");
        configure?.Invoke(movie);
        return movie;
    }

    private static DiaryEntry CreateEntry(Movie movie)
    {
        var entry = new DiaryEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            movie.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            8.0m,
            false,
            null);

        var movieField = typeof(DiaryEntry).GetField("<Movie>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        movieField?.SetValue(entry, movie);

        return entry;
    }

    [Fact]
    public void Matches_RuntimeMinutesLessThan90_ShouldMatch()
    {
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 85, posterUrl: null, genres: new List<Genre>(),
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: false, tmdbRating: 7.0, language: "English", country: "USA"));
        var entry = CreateEntry(movie);

        Assert.True(DiscoveryObjectiveEvaluator.Matches("RuntimeMinutes < 90", entry));
    }

    [Fact]
    public void Matches_RuntimeMinutesLessThan90_ShouldNotMatch_WhenLonger()
    {
        var movie = CreateMovie();
        var entry = CreateEntry(movie);

        Assert.False(DiscoveryObjectiveEvaluator.Matches("RuntimeMinutes < 90", entry));
    }

    [Fact]
    public void Matches_RuntimeMinutesMoreThan120_ShouldMatch()
    {
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 150, posterUrl: null, genres: new List<Genre>(),
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: false, tmdbRating: 7.0, language: "English", country: "USA"));
        var entry = CreateEntry(movie);

        Assert.True(DiscoveryObjectiveEvaluator.Matches("RuntimeMinutes > 120", entry));
    }

    [Fact]
    public void Matches_GenreHorror_ShouldMatch()
    {
        var genre = new Genre(Guid.NewGuid(), 0, "Horror");
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 100, posterUrl: null, genres: new List<Genre> { genre },
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: false, tmdbRating: 7.0, language: "English", country: "USA"));
        var entry = CreateEntry(movie);

        Assert.True(DiscoveryObjectiveEvaluator.Matches("Genre == 'Horror'", entry));
    }

    [Fact]
    public void Matches_GenreHorror_ShouldNotMatch_WhenDifferent()
    {
        var genre = new Genre(Guid.NewGuid(), 0, "Comedy");
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 100, posterUrl: null, genres: new List<Genre> { genre },
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: false, tmdbRating: 7.0, language: "English", country: "USA"));
        var entry = CreateEntry(movie);

        Assert.False(DiscoveryObjectiveEvaluator.Matches("Genre == 'Horror'", entry));
    }

    [Fact]
    public void Matches_IsDocumentary_ShouldMatch()
    {
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 100, posterUrl: null, genres: new List<Genre>(),
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: true, tmdbRating: 7.0, language: "English", country: "USA"));
        var entry = CreateEntry(movie);

        Assert.True(DiscoveryObjectiveEvaluator.Matches("IsDocumentary == true", entry));
    }

    [Fact]
    public void Matches_LanguageNotEnglish_ShouldMatch()
    {
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 100, posterUrl: null, genres: new List<Genre>(),
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: false, tmdbRating: 7.0, language: "French", country: "France"));
        var entry = CreateEntry(movie);

        Assert.True(DiscoveryObjectiveEvaluator.Matches("Language != 'English'", entry));
    }

    [Fact]
    public void Matches_CountryNotUSA_ShouldMatch()
    {
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 100, posterUrl: null, genres: new List<Genre>(),
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: false, tmdbRating: 7.0, language: "French", country: "France"));
        var entry = CreateEntry(movie);

        Assert.True(DiscoveryObjectiveEvaluator.Matches("Country != 'USA'", entry));
    }

    [Fact]
    public void Matches_ReleaseYearBefore1980_ShouldNotMatch_When2000()
    {
        var movie = CreateMovie();
        var entry = CreateEntry(movie);

        Assert.False(DiscoveryObjectiveEvaluator.Matches("ReleaseYear < 1980", entry));
    }

    [Fact]
    public void Matches_TmdbRatingAtLeast8_ShouldMatch()
    {
        var movie = CreateMovie(m => m.EnrichMetadata(
            runtimeMinutes: 100, posterUrl: null, genres: new List<Genre>(),
            directors: new List<Director>(), actors: new List<Actor>(),
            isDocumentary: false, tmdbRating: 8.5, language: "English", country: "USA"));
        var entry = CreateEntry(movie);

        Assert.True(DiscoveryObjectiveEvaluator.Matches("TmdbRating >= 8.0", entry));
    }

    [Fact]
    public void Matches_UnknownExpression_ShouldNotMatch()
    {
        var movie = CreateMovie();
        var entry = CreateEntry(movie);

        Assert.False(DiscoveryObjectiveEvaluator.Matches("UnknownExpression", entry));
    }
}
