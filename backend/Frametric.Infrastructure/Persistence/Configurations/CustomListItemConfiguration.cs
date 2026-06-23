using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frametric.Infrastructure.Persistence.Configurations;

public class CustomListItemConfiguration : IEntityTypeConfiguration<CustomListItem>
{
    public void Configure(EntityTypeBuilder<CustomListItem> builder)
    {
        builder.HasKey(c => new { c.CustomListId, c.MovieId });

        builder.Property(c => c.Nickname)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.HasOne(c => c.CustomList)
            .WithMany(cl => cl.Items)
            .HasForeignKey(c => c.CustomListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Movie)
            .WithMany()
            .HasForeignKey(c => c.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
