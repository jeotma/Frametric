// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Domain.Discovery.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frametric.Infrastructure.Persistence.Configurations;

public class DiscoveryObjectiveConfiguration : IEntityTypeConfiguration<DiscoveryObjective>
{
    public void Configure(EntityTypeBuilder<DiscoveryObjective> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.RequirementExpression).IsRequired();
        builder.Property(o => o.Description).IsRequired();

        builder.HasIndex(o => o.UserId);
    }
}
