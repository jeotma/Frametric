using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frametric.Infrastructure.Importer;
using Xunit;

namespace Frametric.UnitTests;

public class LetterboxdZipImporterTests
{
    [Fact]
    public async Task ImportFromZipAsync_ShouldParseValidZipCorrectly()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Create diary.csv
            var diaryEntry = archive.CreateEntry("diary.csv");
            using (var writer = new StreamWriter(diaryEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI,Rating,Rewatch,Tags,Watched Date");
                writer.WriteLine("2026-05-30,Inception,2010.0,https://letterboxd.com/film/inception/,4.5,Yes,sci-fi,2026-05-29");
            }

            // Create ratings.csv
            var ratingsEntry = archive.CreateEntry("ratings.csv");
            using (var writer = new StreamWriter(ratingsEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI,Rating");
                writer.WriteLine("2026-05-30,Interstellar,2014,https://letterboxd.com/film/interstellar/,5.0");
            }

            // Create watchlist.csv
            var watchlistEntry = archive.CreateEntry("watchlist.csv");
            using (var writer = new StreamWriter(watchlistEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI");
                writer.WriteLine("2026-05-30,Dunkirk,2017,https://letterboxd.com/film/dunkirk/");
            }

            // Create profile.csv (required for strict validation)
            var profileEntry = archive.CreateEntry("profile.csv");
            using (var writer = new StreamWriter(profileEntry.Open()))
            {
                writer.WriteLine("Favorite Films");
                writer.WriteLine("\"https://letterboxd.com/film/inception/\"");
            }

            // Create reviews.csv (required for strict validation)
            var reviewsEntry = archive.CreateEntry("reviews.csv");
            using (var writer = new StreamWriter(reviewsEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI,Rating,Rewatch,Review");
                writer.WriteLine("2026-05-30,Inception,2010,https://letterboxd.com/film/inception/,4.5,No,Amazing!");
            }

            // Create watched.csv (required for strict validation)
            var watchedEntry = archive.CreateEntry("watched.csv");
            using (var writer = new StreamWriter(watchedEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI");
                writer.WriteLine("2026-05-30,Inception,2010,https://letterboxd.com/film/inception/");
            }

            // Create likes/films.csv
            var likesEntry = archive.CreateEntry("likes/films.csv");
            using (var writer = new StreamWriter(likesEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI");
                writer.WriteLine("2026-05-30,Memento,2000,https://letterboxd.com/film/memento/");
            }

            // Create likes/reviews.csv (required for strict validation)
            var likesReviewsEntry = archive.CreateEntry("likes/reviews.csv");
            using (var writer = new StreamWriter(likesReviewsEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI");
            }
        }

        memoryStream.Position = 0;
        var importer = new LetterboxdZipImporter();

        // Act
        var result = await importer.ImportFromZipAsync(memoryStream);

        // Assert
        Assert.NotNull(result);

        // Assert Diary
        Assert.Single(result.DiaryEntries);
        var diary = result.DiaryEntries.First();
        Assert.Equal("Inception", diary.Name);
        Assert.Equal(2010, diary.Year); // Tests the YearNullableIntConverter converting "2010.0" -> 2010
        Assert.Equal(4.5m, diary.Rating);
        Assert.True(diary.Rewatch);
        Assert.Equal("sci-fi", diary.Tags);
        Assert.Equal(new DateOnly(2026, 5, 30), diary.Date);
        Assert.Equal(new DateOnly(2026, 5, 29), diary.WatchedDate);

        // Assert Ratings
        Assert.Single(result.Ratings);
        var rating = result.Ratings.First();
        Assert.Equal("Interstellar", rating.Name);
        Assert.Equal(2014, rating.Year);
        Assert.Equal(5.0m, rating.Rating);

        // Assert Watchlist
        Assert.Single(result.Watchlist);
        var watchlist = result.Watchlist.First();
        Assert.Equal("Dunkirk", watchlist.Name);
        Assert.Equal(2017, watchlist.Year);

        // Assert Likes
        Assert.Single(result.Likes);
        var like = result.Likes.First();
        Assert.Equal("Memento", like.Name);
        Assert.Equal(2000, like.Year);
    }

    [Fact]
    public async Task ImportFromZipAsync_ShouldThrowInvalidDataException_WhenFilesAreMissing()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Only create diary.csv, leave others missing to trigger strict validation error
            var diaryEntry = archive.CreateEntry("diary.csv");
            using (var writer = new StreamWriter(diaryEntry.Open()))
            {
                writer.WriteLine("Date,Name,Year,Letterboxd URI,Rating,Rewatch,Tags,Watched Date");
                writer.WriteLine("2026-05-30,Inception,2010,https://letterboxd.com/film/inception/,4.5,No,,");
            }
        }

        memoryStream.Position = 0;
        var importer = new LetterboxdZipImporter();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => importer.ImportFromZipAsync(memoryStream));
    }
}
