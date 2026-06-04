// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Text;
using Dapper;
using Frametric.Application.DTOs.Analytics;

namespace Frametric.Infrastructure.Queries;

public class SqlFilterBuilder
{
    private readonly AnalyticsFilterDto _filter;
    private readonly DynamicParameters _parameters;
    private readonly string _movieAlias;
    private readonly string _targetAlias;
    private readonly string _targetDateColumn;
    private readonly bool _isMoviesJoined;
    private readonly bool _isRatingsJoined;
    private readonly string _ratingAlias;

    public SqlFilterBuilder(
        AnalyticsFilterDto filter, 
        DynamicParameters parameters, 
        string movieAlias = "m", 
        string targetAlias = "w", 
        string targetDateColumn = "Date", 
        bool isMoviesJoined = true,
        bool isRatingsJoined = false,
        string ratingAlias = "mr")
    {
        _filter = filter ?? new AnalyticsFilterDto();
        _parameters = parameters;
        _movieAlias = movieAlias;
        _targetAlias = targetAlias;
        _targetDateColumn = targetDateColumn;
        _isMoviesJoined = isMoviesJoined;
        _isRatingsJoined = isRatingsJoined;
        _ratingAlias = ratingAlias;
    }

    public string BuildJoins()
    {
        var joins = new StringBuilder();

        bool needsMovieJoin = !string.IsNullOrWhiteSpace(_filter.Actor) || !string.IsNullOrWhiteSpace(_filter.Director) || !string.IsNullOrWhiteSpace(_filter.Genre) || _filter.ReleaseYear.HasValue || _filter.MinCustomRating.HasValue || _filter.MaxCustomRating.HasValue;

        if (!_isMoviesJoined && needsMovieJoin)
        {
            joins.AppendLine($"JOIN \"Movies\" {_movieAlias} ON {_targetAlias}.\"MovieId\" = {_movieAlias}.\"Id\"");
        }

        if (!string.IsNullOrWhiteSpace(_filter.Actor))
        {
            joins.AppendLine($"JOIN \"MovieActor\" f_ma ON {_movieAlias}.\"Id\" = f_ma.\"MoviesId\"");
            joins.AppendLine($"JOIN \"Actors\" f_actor ON f_ma.\"ActorsId\" = f_actor.\"Id\"");
        }
        if (!string.IsNullOrWhiteSpace(_filter.Director))
        {
            joins.AppendLine($"JOIN \"MovieDirector\" f_md ON {_movieAlias}.\"Id\" = f_md.\"MoviesId\"");
            joins.AppendLine($"JOIN \"Directors\" f_director ON f_md.\"DirectorsId\" = f_director.\"Id\"");
        }
        if (!string.IsNullOrWhiteSpace(_filter.Genre))
        {
            joins.AppendLine($"JOIN \"MovieGenre\" f_mg ON {_movieAlias}.\"Id\" = f_mg.\"MoviesId\"");
            joins.AppendLine($"JOIN \"Genres\" f_genre ON f_mg.\"GenresId\" = f_genre.\"Id\"");
        }

        // Only join MovieRatings if we actually need it for filtering and it's not assumed to be joined already.
        if (!_isRatingsJoined && (_filter.MinRating.HasValue || _filter.MaxRating.HasValue))
        {
            joins.AppendLine($"LEFT JOIN \"MovieRatings\" {_ratingAlias} ON {_movieAlias}.\"Id\" = {_ratingAlias}.\"MovieId\" AND {_ratingAlias}.\"UserId\" = @userId");
        }

        return joins.ToString();
    }

    public string BuildWhereClause(string? ratingAlias = null)
    {
        var where = new StringBuilder();
        var rAlias = ratingAlias ?? _ratingAlias;

        if (_filter.WatchYear.HasValue)
        {
            string dateExpr;
            if (_targetAlias == "w" && _targetDateColumn == "Date")
            {
                dateExpr = $"COALESCE((SELECT MIN(de.\"WatchedDate\") FROM \"DiaryEntries\" de WHERE de.\"MovieId\" = {_targetAlias}.\"MovieId\" AND de.\"UserId\" = {_targetAlias}.\"UserId\"), {_targetAlias}.\"Date\")";
            }
            else
            {
                dateExpr = $"{_targetAlias}.\"{_targetDateColumn}\"";
            }
            where.AppendLine($"AND EXTRACT(YEAR FROM {dateExpr}) = @WatchYear");
            _parameters.Add("WatchYear", _filter.WatchYear.Value);
        }
        
        if (_filter.ReleaseYear.HasValue)
        {
            where.AppendLine($"AND {_movieAlias}.\"ReleaseYear\" = @ReleaseYear");
            _parameters.Add("ReleaseYear", _filter.ReleaseYear.Value);
        }

        if (_filter.MinRating.HasValue)
        {
            where.AppendLine($"AND {rAlias}.\"Score\" >= @MinRating");
            _parameters.Add("MinRating", _filter.MinRating.Value);
        }

        if (_filter.MaxRating.HasValue)
        {
            where.AppendLine($"AND {rAlias}.\"Score\" <= @MaxRating");
            _parameters.Add("MaxRating", _filter.MaxRating.Value);
        }

        if (_filter.MinCustomRating.HasValue)
        {
            where.AppendLine($"AND {_movieAlias}.\"CustomAverageRating\" >= @MinCustomRating");
            _parameters.Add("MinCustomRating", _filter.MinCustomRating.Value);
        }

        if (_filter.MaxCustomRating.HasValue)
        {
            where.AppendLine($"AND {_movieAlias}.\"CustomAverageRating\" <= @MaxCustomRating");
            _parameters.Add("MaxCustomRating", _filter.MaxCustomRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(_filter.Actor))
        {
            where.AppendLine($"AND f_actor.\"Name\" ILIKE @Actor");
            _parameters.Add("Actor", $"%{_filter.Actor}%");
        }

        if (!string.IsNullOrWhiteSpace(_filter.Director))
        {
            where.AppendLine($"AND f_director.\"Name\" ILIKE @Director");
            _parameters.Add("Director", $"%{_filter.Director}%");
        }

        if (!string.IsNullOrWhiteSpace(_filter.Genre))
        {
            where.AppendLine($"AND f_genre.\"Name\" ILIKE @Genre");
            _parameters.Add("Genre", $"%{_filter.Genre}%");
        }

        return where.ToString();
    }
}
