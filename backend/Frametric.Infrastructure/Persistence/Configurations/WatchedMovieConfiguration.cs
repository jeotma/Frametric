// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frametric.Infrastructure.Persistence.Configurations;

public class WatchedMovieConfiguration : IEntityTypeConfiguration<WatchedMovie>
{
    public void Configure(EntityTypeBuilder<WatchedMovie> builder)
    {
        builder.HasKey(wm => wm.Id);

        // ImportHistoryId is optional — manually logged watches have no associated import
        builder.HasOne(wm => wm.ImportHistory)
               .WithMany(ih => ih.WatchedMovies)
               .HasForeignKey(wm => wm.ImportHistoryId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
