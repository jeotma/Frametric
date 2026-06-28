// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System.Globalization;
using System.IO.Compression;
using CsvHelper;
using CsvHelper.Configuration;
using Frametric.Application.DTOs.Letterboxd;
using Frametric.Application.Interfaces;
using Frametric.Infrastructure.Importer.CsvModels;
using Frametric.Infrastructure.Importer.Mappers;

namespace Frametric.Infrastructure.Importer;

public class LetterboxdZipImporter : ILetterboxdImporter
{
    public async Task<LetterboxdExportData> ImportFromZipAsync(Stream zipStream, CancellationToken cancellationToken = default)
    {
        var diaryDtos = new List<ParsedDiaryDto>();
        var ratingDtos = new List<ParsedRatingDto>();
        var watchlistDtos = new List<ParsedWatchlistItemDto>();
        var likeDtos = new List<ParsedLikeDto>();
        var watchedDtos = new List<ParsedWatchedDto>();

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        // Strict Validation: Ensure the zip contains the FULL Letterboxd export structure including folders
        var requiredFiles = new[] { 
            "diary.csv", "ratings.csv", "watchlist.csv", "profile.csv", "reviews.csv",
            "watched.csv", "likes/films.csv", "likes/reviews.csv"
        };
        
        bool MatchEntry(string entryFullName, string requiredPath)
        {
            var normalized = entryFullName.Replace('\\', '/');
            return normalized.Equals(requiredPath, StringComparison.OrdinalIgnoreCase) ||
                   normalized.EndsWith("/" + requiredPath, StringComparison.OrdinalIgnoreCase);
        }

        bool isValidArchive = requiredFiles.All(reqFile => 
            archive.Entries.Any(e => MatchEntry(e.FullName, reqFile))
        );

        if (!isValidArchive)
        {
            throw new InvalidDataException("The uploaded file must be the original, unmodified Letterboxd export zip file containing its complete folder structure.");
        }

        foreach (var entry in archive.Entries)
        {
            if (MatchEntry(entry.FullName, "diary.csv"))
            {
                var diaryRecords = await ParseCsvAsync<DiaryCsvRecord, DiaryCsvRecordMap>(entry, cancellationToken);
                diaryDtos.AddRange(diaryRecords.Select(MapToDto));
            }
            else if (MatchEntry(entry.FullName, "ratings.csv"))
            {
                var ratingRecords = await ParseCsvAsync<RatingCsvRecord, RatingCsvRecordMap>(entry, cancellationToken);
                ratingDtos.AddRange(ratingRecords.Select(MapToDto));
            }
            else if (MatchEntry(entry.FullName, "watchlist.csv"))
            {
                var watchlistRecords = await ParseCsvAsync<WatchlistCsvRecord, WatchlistCsvRecordMap>(entry, cancellationToken);
                watchlistDtos.AddRange(watchlistRecords.Select(MapToDto));
            }
            else if (MatchEntry(entry.FullName, "likes/films.csv"))
            {
                var likeRecords = await ParseCsvAsync<LikeCsvRecord, LikeCsvRecordMap>(entry, cancellationToken);
                likeDtos.AddRange(likeRecords.Select(MapToDto));
            }
            else if (MatchEntry(entry.FullName, "watched.csv"))
            {
                var watchedRecords = await ParseCsvAsync<WatchedCsvRecord, WatchedCsvRecordMap>(entry, cancellationToken);
                watchedDtos.AddRange(watchedRecords.Select(MapToDto));
            }
        }

        return new LetterboxdExportData(diaryDtos, ratingDtos, watchlistDtos, likeDtos, watchedDtos);
    }

    private async Task<List<TRecord>> ParseCsvAsync<TRecord, TMap>(ZipArchiveEntry entry, CancellationToken cancellationToken) 
        where TMap : ClassMap
    {
        var records = new List<TRecord>();
        
        // We do not wrap this in a 'using' that disposes the stream, because ZipArchiveEntry streams
        // are tied to the ZipArchive. We just read them.
        await using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream);
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            BadDataFound = null // Ignore bad data for resilience
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<TMap>();

        await foreach (var record in csv.GetRecordsAsync<TRecord>(cancellationToken))
        {
            records.Add(record);
        }

        return records;
    }

    // Mapping Methods
    private ParsedDiaryDto MapToDto(DiaryCsvRecord r) => new ParsedDiaryDto
    {
        Date = r.Date,
        Name = r.Name,
        Year = r.Year,
        LetterboxdUri = r.LetterboxdUri,
        Rating = r.Rating,
        Rewatch = r.RewatchRaw?.Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) == true,
        Tags = r.Tags,
        WatchedDate = r.WatchedDate ?? r.Date // If watched date is missing, fallback to log date
    };

    private ParsedRatingDto MapToDto(RatingCsvRecord r) => new ParsedRatingDto
    {
        Date = r.Date,
        Name = r.Name,
        Year = r.Year,
        LetterboxdUri = r.LetterboxdUri,
        Rating = r.Rating
    };

    private ParsedWatchlistItemDto MapToDto(WatchlistCsvRecord r) => new ParsedWatchlistItemDto
    {
        Date = r.Date,
        Name = r.Name,
        Year = r.Year,
        LetterboxdUri = r.LetterboxdUri
    };

    private ParsedLikeDto MapToDto(LikeCsvRecord r) => new ParsedLikeDto
    {
        Date = r.Date,
        Name = r.Name,
        Year = r.Year,
        LetterboxdUri = r.LetterboxdUri
    };

    private ParsedWatchedDto MapToDto(WatchedCsvRecord r) => new ParsedWatchedDto
    {
        Date = r.Date,
        Name = r.Name,
        Year = r.Year,
        LetterboxdUri = r.LetterboxdUri
    };
}
