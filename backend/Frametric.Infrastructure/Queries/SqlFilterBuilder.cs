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

    public SqlFilterBuilder(AnalyticsFilterDto filter, DynamicParameters parameters, string movieAlias = "m", string targetAlias = "w", string targetDateColumn = "Date", bool isMoviesJoined = true)
    {
        _filter = filter ?? new AnalyticsFilterDto();
        _parameters = parameters;
        _movieAlias = movieAlias;
        _targetAlias = targetAlias;
        _targetDateColumn = targetDateColumn;
        _isMoviesJoined = isMoviesJoined;
    }

    public string BuildJoins()
    {
        var joins = new StringBuilder();

        bool needsMovieJoin = _filter.ActorId.HasValue || _filter.DirectorId.HasValue || _filter.GenreId.HasValue || _filter.ReleaseYear.HasValue;

        if (!_isMoviesJoined && needsMovieJoin)
        {
            joins.AppendLine($"JOIN \"Movies\" {_movieAlias} ON {_targetAlias}.\"MovieId\" = {_movieAlias}.\"Id\"");
        }

        if (_filter.ActorId.HasValue)
        {
            joins.AppendLine($"JOIN \"MovieActor\" f_ma ON {_movieAlias}.\"Id\" = f_ma.\"MoviesId\"");
        }
        if (_filter.DirectorId.HasValue)
        {
            joins.AppendLine($"JOIN \"MovieDirector\" f_md ON {_movieAlias}.\"Id\" = f_md.\"MoviesId\"");
        }
        if (_filter.GenreId.HasValue)
        {
            joins.AppendLine($"JOIN \"MovieGenre\" f_mg ON {_movieAlias}.\"Id\" = f_mg.\"MoviesId\"");
        }

        // Only join MovieRatings if we actually need it for filtering and it's not assumed to be joined already.
        // Usually, complex queries already join MovieRatings as 'mr'.
        // So we will assume 'mr' is the alias if they want to filter by Min/Max rating.

        return joins.ToString();
    }

    public string BuildWhereClause(string ratingAlias = "mr")
    {
        var where = new StringBuilder();

        if (_filter.WatchYear.HasValue)
        {
            where.AppendLine($"AND EXTRACT(YEAR FROM {_targetAlias}.\"{_targetDateColumn}\") = @WatchYear");
            _parameters.Add("WatchYear", _filter.WatchYear.Value);
        }
        
        if (_filter.ReleaseYear.HasValue)
        {
            where.AppendLine($"AND {_movieAlias}.\"ReleaseYear\" = @ReleaseYear");
            _parameters.Add("ReleaseYear", _filter.ReleaseYear.Value);
        }

        if (_filter.MinRating.HasValue)
        {
            where.AppendLine($"AND {ratingAlias}.\"Score\" >= @MinRating");
            _parameters.Add("MinRating", _filter.MinRating.Value);
        }

        if (_filter.MaxRating.HasValue)
        {
            where.AppendLine($"AND {ratingAlias}.\"Score\" <= @MaxRating");
            _parameters.Add("MaxRating", _filter.MaxRating.Value);
        }

        if (_filter.ActorId.HasValue)
        {
            where.AppendLine($"AND f_ma.\"ActorsId\" = @ActorId");
            _parameters.Add("ActorId", _filter.ActorId.Value);
        }

        if (_filter.DirectorId.HasValue)
        {
            where.AppendLine($"AND f_md.\"DirectorsId\" = @DirectorId");
            _parameters.Add("DirectorId", _filter.DirectorId.Value);
        }

        if (_filter.GenreId.HasValue)
        {
            where.AppendLine($"AND f_mg.\"GenresId\" = @GenreId");
            _parameters.Add("GenreId", _filter.GenreId.Value);
        }

        return where.ToString();
    }
}
