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
