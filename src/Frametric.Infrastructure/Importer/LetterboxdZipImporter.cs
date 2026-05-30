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

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.Equals("diary.csv", StringComparison.OrdinalIgnoreCase))
            {
                diaryDtos.AddRange(await ParseCsvAsync<DiaryCsvRecord, DiaryCsvRecordMap>(entry, cancellationToken)
                    .ContinueWith(t => t.Result.Select(MapToDto), cancellationToken));
            }
            else if (entry.FullName.Equals("ratings.csv", StringComparison.OrdinalIgnoreCase))
            {
                ratingDtos.AddRange(await ParseCsvAsync<RatingCsvRecord, RatingCsvRecordMap>(entry, cancellationToken)
                    .ContinueWith(t => t.Result.Select(MapToDto), cancellationToken));
            }
            else if (entry.FullName.Equals("watchlist.csv", StringComparison.OrdinalIgnoreCase))
            {
                watchlistDtos.AddRange(await ParseCsvAsync<WatchlistCsvRecord, WatchlistCsvRecordMap>(entry, cancellationToken)
                    .ContinueWith(t => t.Result.Select(MapToDto), cancellationToken));
            }
            else if (entry.FullName.Replace('\\', '/').Equals("likes/films.csv", StringComparison.OrdinalIgnoreCase))
            {
                likeDtos.AddRange(await ParseCsvAsync<LikeCsvRecord, LikeCsvRecordMap>(entry, cancellationToken)
                    .ContinueWith(t => t.Result.Select(MapToDto), cancellationToken));
            }
        }

        return new LetterboxdExportData(diaryDtos, ratingDtos, watchlistDtos, likeDtos);
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
}
