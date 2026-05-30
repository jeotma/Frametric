using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frametric.Infrastructure.Persistence.Configurations;

public class ImportHistoryConfiguration : IEntityTypeConfiguration<ImportHistory>
{
    public void Configure(EntityTypeBuilder<ImportHistory> builder)
    {
        builder.HasKey(ih => ih.Id);

        builder.Property(ih => ih.Status).IsRequired().HasMaxLength(50);
        builder.Property(ih => ih.ProviderSource).IsRequired().HasMaxLength(100);
        builder.Property(ih => ih.ErrorMessage).HasMaxLength(1000);

        builder.HasMany(ih => ih.DiaryEntries)
               .WithOne(de => de.ImportHistory)
               .HasForeignKey(de => de.ImportHistoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ih => ih.MovieRatings)
               .WithOne(mr => mr.ImportHistory)
               .HasForeignKey(mr => mr.ImportHistoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ih => ih.WatchlistItems)
               .WithOne(wi => wi.ImportHistory)
               .HasForeignKey(wi => wi.ImportHistoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ih => ih.MovieLikes)
               .WithOne(ml => ml.ImportHistory)
               .HasForeignKey(ml => ml.ImportHistoryId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
