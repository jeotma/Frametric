using Frametric.Application.DTOs.Analytics;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Movies;

public record EnrichSpecificMovieCommand(int TmdbId) : IRequest<MovieSimpleDto?>;

public class EnrichSpecificMovieCommandHandler : IRequestHandler<EnrichSpecificMovieCommand, MovieSimpleDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITmdbService _tmdbService;
    private readonly IOmdbService _omdbService;

    public EnrichSpecificMovieCommandHandler(IApplicationDbContext context, ITmdbService tmdbService, IOmdbService omdbService)
    {
        _context = context;
        _tmdbService = tmdbService;
        _omdbService = omdbService;
    }

    public async Task<MovieSimpleDto?> Handle(EnrichSpecificMovieCommand request, CancellationToken cancellationToken)
    {
        // First check if it already exists
        var tmdbIdStr = request.TmdbId.ToString();
        var existingMovie = await _context.Movies
            .FirstOrDefaultAsync(m => m.ExternalReference.Source == "tmdb" && m.ExternalReference.ExternalId == tmdbIdStr, cancellationToken);
        
        if (existingMovie != null)
        {
            return new MovieSimpleDto(existingMovie.Id, existingMovie.Title, existingMovie.ReleaseYear, existingMovie.PosterUrl);
        }

        var tmdbData = await _tmdbService.GetMovieDetailsByIdAsync(request.TmdbId, cancellationToken);

        if (tmdbData == null || tmdbData.IsTvShow)
        {
            return null; // We only support movies for custom selections currently
        }

        var movie = new Movie(Guid.NewGuid(), tmdbData.Title ?? "Unknown Title", tmdbData.FirstAirYear ?? 0, new Frametric.Domain.ValueObjects.ExternalReference("tmdb", request.TmdbId.ToString()));
        _context.Movies.Add(movie);

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
            if (movie.ReleaseYear == 0)
            {
                // Update release year based on exact date
                var newDateMovie = new Movie(movie.Id, movie.Title, rDate.Year, movie.ExternalReference);
                _context.Movies.Remove(movie);
                movie = newDateMovie;
                _context.Movies.Add(movie);
            }
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
            overview: overview);

        await _context.SaveChangesAsync(cancellationToken);

        return new MovieSimpleDto(movie.Id, movie.Title, movie.ReleaseYear, movie.PosterUrl);
    }
}
