// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Frametric.Infrastructure.Importer.CsvModels;

public class WatchedCsvRecord
{
    [Name("Date")]
    public DateOnly Date { get; set; }

    [Name("Name")]
    public string Name { get; set; } = string.Empty;

    [Name("Year")]
    public int? Year { get; set; }

    [Name("Letterboxd URI")]
    public string LetterboxdUri { get; set; } = string.Empty;
}

public class WatchedCsvRecordMap : ClassMap<WatchedCsvRecord>
{
    public WatchedCsvRecordMap()
    {
        Map(m => m.Date).Name("Date");
        Map(m => m.Name).Name("Name");
        Map(m => m.Year).Name("Year").Optional();
        Map(m => m.LetterboxdUri).Name("Letterboxd URI");
    }
}
