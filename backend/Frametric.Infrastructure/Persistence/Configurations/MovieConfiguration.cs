using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frametric.Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(500);
        builder.Property(m => m.EnrichmentStatus).HasConversion<string>().HasMaxLength(50);

        // Value Object configuration (Owned Entity pattern)
        builder.OwnsOne(m => m.ExternalReference, er =>
        {
            er.Property(e => e.Source).IsRequired().HasMaxLength(50).HasColumnName("ExternalSource");
            er.Property(e => e.ExternalId).IsRequired().HasMaxLength(200).HasColumnName("ExternalId");
            
            // Create a unique index to allow deduplication (e.g. "Letterboxd", "https://boxd.it/123")
            er.HasIndex(e => new { e.Source, e.ExternalId }).IsUnique();
        });

        builder.Property(m => m.TmdbRating);
        builder.Property(m => m.TmdbPopularity);
        builder.Property(m => m.ImdbRating);
        builder.Property(m => m.RottenTomatoesRating);
        builder.Property(m => m.MetacriticRating);
        builder.Property(m => m.CustomAverageRating);
        builder.Property(m => m.ReleaseDate);
        builder.Property(m => m.Keywords).HasMaxLength(4000);
        builder.Property(m => m.Awards).HasMaxLength(1000);
        builder.Property(m => m.Writers).HasMaxLength(1000);
        builder.Property(m => m.Language).HasMaxLength(100);
        builder.Property(m => m.Country).HasMaxLength(200);
        builder.Property(m => m.BoxOffice).HasMaxLength(100);
        builder.Property(m => m.Certification).HasMaxLength(50);
        builder.Property(m => m.StreamingProviders).HasMaxLength(1000);
        builder.Property(m => m.Overview).HasMaxLength(4000);

        // Many-to-Many relationships
        builder.HasMany(m => m.Genres)
               .WithMany(g => g.Movies)
               .UsingEntity("MovieGenre");

        builder.HasMany(m => m.Directors)
               .WithMany(d => d.Movies)
               .UsingEntity("MovieDirector");

        builder.HasMany(m => m.Actors)
               .WithMany(a => a.Movies)
               .UsingEntity("MovieActor");
    }
}
