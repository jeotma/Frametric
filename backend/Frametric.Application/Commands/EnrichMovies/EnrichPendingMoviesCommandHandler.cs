// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.EnrichMovies;

public class EnrichPendingMoviesCommandHandler : IRequestHandler<EnrichPendingMoviesCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ITmdbService _tmdbService;
    private readonly IOmdbService _omdbService;

    public EnrichPendingMoviesCommandHandler(IApplicationDbContext context, ITmdbService tmdbService, IOmdbService omdbService)
    {
        _context = context;
        _tmdbService = tmdbService;
        _omdbService = omdbService;
    }

    public async Task<int> Handle(EnrichPendingMoviesCommand request, CancellationToken cancellationToken)
    {
        var pendingMovies = await _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Directors)
            .Include(m => m.Actors)
            .Where(m => m.EnrichmentStatus == EnrichmentStatus.Pending)
            .OrderBy(m => m.Id)
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        if (!pendingMovies.Any()) return 0;

        int enrichedCount = 0;

        foreach (var movie in pendingMovies)
        {
            try
            {
                var tmdbData = await _tmdbService.SearchAndGetMovieDetailsAsync(movie.Title, movie.ReleaseYear, cancellationToken);

                if (tmdbData == null)
                {
                    // All 6 search strategies exhausted — mark as NotFound so it is never retried
                    movie.MarkAsNotFound();
                    continue;
                }

                if (tmdbData.IsTvShow)
                {
                    // It's a TV show/miniseries: check if it already exists in TvShows
                    var exists = await _context.TvShows.AnyAsync(t => t.TmdbId == tmdbData.TmdbId, cancellationToken);
                    if (!exists)
                    {
                        var tvShowTitle = tmdbData.Title ?? movie.Title;
                        var tvShowYear = tmdbData.FirstAirYear ?? movie.ReleaseYear;
                        var tvShow = new TvShow(Guid.NewGuid(), tvShowTitle, tvShowYear, tmdbData.TmdbId, tmdbData.PosterUrl, tmdbData.IsDocumentary);
                        _context.TvShows.Add(tvShow);
                    }
                    
                    _context.Movies.Remove(movie);
                    enrichedCount++;
                    continue;
                }

                // Categorize content and apply whitelist filter (prevent stand-up, wrestling, concerts, sports, etc.)
                var category = Frametric.Domain.Services.ContentClassifier.DetectCategory(
                    tmdbData.Genres.Select(g => g.Name),
                    tmdbData.Genres.Select(g => g.Id),
                    tmdbData.Keywords,
                    tmdbData.RuntimeMinutes
                );

                bool includeInMovieStats = 
                    category == Frametric.Domain.Enums.ContentCategory.Movie ||
                    category == Frametric.Domain.Enums.ContentCategory.Documentary ||
                    category == Frametric.Domain.Enums.ContentCategory.ShortFilm;

                if (!includeInMovieStats)
                {
                    _context.Movies.Remove(movie);
                    enrichedCount++;
                    continue;
                }

                // Standard movie enrichment
                var genres = new List<Genre>();
                foreach (var gDto in tmdbData.Genres)
                {
                    var genre = _context.Genres.Local.FirstOrDefault(g => g.TmdbId == gDto.Id)
                                ?? await _context.Genres.FirstOrDefaultAsync(g => g.TmdbId == gDto.Id, cancellationToken);
                    if (genre == null)
                    {
                        genre = new Genre(Guid.NewGuid(), gDto.Id, gDto.Name);
                        _context.Genres.Add(genre);
                    }
                    genres.Add(genre);
                }

                var directors = new List<Director>();
                foreach (var dDto in tmdbData.Directors)
                {
                    var director = _context.Directors.Local.FirstOrDefault(d => d.TmdbId == dDto.Id)
                                   ?? await _context.Directors.FirstOrDefaultAsync(d => d.TmdbId == dDto.Id, cancellationToken);
                    if (director == null)
                    {
                        director = new Director(Guid.NewGuid(), dDto.Id, dDto.Name, dDto.ProfilePath);
                        _context.Directors.Add(director);
                    }
                    else
                    {
                        director.UpdateProfilePath(dDto.ProfilePath);
                    }
                    directors.Add(director);
                }

                var actors = new List<Actor>();
                foreach (var aDto in tmdbData.Actors)
                {
                    var actor = _context.Actors.Local.FirstOrDefault(a => a.TmdbId == aDto.Id)
                                ?? await _context.Actors.FirstOrDefaultAsync(a => a.TmdbId == aDto.Id, cancellationToken);
                    if (actor == null)
                    {
                        actor = new Actor(Guid.NewGuid(), aDto.Id, aDto.Name, aDto.ProfilePath);
                        _context.Actors.Add(actor);
                    }
                    else
                    {
                        actor.UpdateProfilePath(aDto.ProfilePath);
                    }
                    actors.Add(actor);
                }

                double? tmdbRating = tmdbData.TmdbRating;
                double? tmdbPopularity = tmdbData.TmdbPopularity;
                double? imdbRating = null;
                double? rottenTomatoesRating = null;
                double? metacriticRating = null;
                double? customAverageRating = null;

                OmdbRatingsDto? omdbRatings = null;
                if (!string.IsNullOrEmpty(tmdbData.ImdbId))
                {
                    omdbRatings = await _omdbService.GetMovieRatingsAsync(tmdbData.ImdbId, cancellationToken);
                    if (omdbRatings != null)
                    {
                        imdbRating = omdbRatings.ImdbRating;
                        rottenTomatoesRating = omdbRatings.RottenTomatoesRating;
                        metacriticRating = omdbRatings.MetacriticRating;
                    }
                }

                var ratingsList = new List<double>();
                if (tmdbRating.HasValue) ratingsList.Add(tmdbRating.Value);
                if (imdbRating.HasValue) ratingsList.Add(imdbRating.Value);
                if (rottenTomatoesRating.HasValue) ratingsList.Add(rottenTomatoesRating.Value);
                if (metacriticRating.HasValue) ratingsList.Add(metacriticRating.Value);

                if (ratingsList.Any())
                {
                    customAverageRating = ratingsList.Average();
                }

                DateOnly? parsedReleaseDate = null;
                if (!string.IsNullOrEmpty(tmdbData.ReleaseDate) && DateOnly.TryParse(tmdbData.ReleaseDate, out var rDate))
                {
                    parsedReleaseDate = rDate;
                }

                string? writers = null;
                string? awards = null;
                string? boxOffice = null;
                string? language = null;
                string? country = null;
                string? rated = null;

                if (omdbRatings != null)
                {
                    writers = omdbRatings.Writers;
                    if (writers != null && writers.Length > 1000) writers = writers.Substring(0, 1000);

                    awards = omdbRatings.Awards;
                    if (awards != null && awards.Length > 1000) awards = awards.Substring(0, 1000);

                    boxOffice = omdbRatings.BoxOffice;
                    if (boxOffice != null && boxOffice.Length > 100) boxOffice = boxOffice.Substring(0, 100);

                    language = omdbRatings.Language;
                    if (language != null && language.Length > 100) language = language.Substring(0, 100);

                    country = omdbRatings.Country;
                    if (country != null && country.Length > 200) country = country.Substring(0, 200);

                    rated = omdbRatings.Rated;
                    if (rated != null && rated.Length > 50) rated = rated.Substring(0, 50);
                }

                var keywords = tmdbData.Keywords;
                if (keywords != null && keywords.Length > 4000) keywords = keywords.Substring(0, 4000);

                var providers = tmdbData.StreamingProviders;
                if (providers != null && providers.Length > 1000) providers = providers.Substring(0, 1000);

                var overview = tmdbData.Overview;
                if (overview != null && overview.Length > 4000) overview = overview.Substring(0, 4000);

                movie.EnrichMetadata(
                    tmdbData.RuntimeMinutes ?? 0, 
                    tmdbData.PosterUrl ?? string.Empty, 
                    genres, 
                    directors, 
                    actors, 
                    tmdbData.IsDocumentary,
                    tmdbRating,
                    tmdbPopularity,
                    imdbRating,
                    rottenTomatoesRating,
                    metacriticRating,
                    customAverageRating,
                    parsedReleaseDate,
                    keywords: keywords,
                    awards: awards,
                    writers: writers,
                    language: language,
                    country: country,
                    boxOffice: boxOffice,
                    certification: rated,
                    streamingProviders: providers,
                    overview: overview,
                    tmdbCollectionId: tmdbData.TmdbCollectionId,
                    tmdbCollectionName: tmdbData.TmdbCollectionName);
                enrichedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Enrichment Error] Failed to enrich movie: '{movie.Title}' (ID: {movie.Id}). Exception: {ex.Message}");
                movie.MarkEnrichmentFailed();
            }

            // Space out requests slightly to respect API rate limits
            await Task.Delay(1000, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return pendingMovies.Count;
    }
}
