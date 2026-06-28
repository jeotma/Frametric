using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frametric.Infrastructure.Persistence.Configurations;

public class EntityRevisionConfiguration : IEntityTypeConfiguration<EntityRevision>
{
    public void Configure(EntityTypeBuilder<EntityRevision> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(r => r.EntityId).IsRequired();
        builder.Property(r => r.ChangedAt).IsRequired();
        builder.Property(r => r.ChangedBy).IsRequired().HasMaxLength(150);
        builder.Property(r => r.StateJson).IsRequired();

        builder.HasIndex(r => new { r.EntityType, r.EntityId });
    }
}
