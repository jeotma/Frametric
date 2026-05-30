# Roadmap: Phase 1 (MVP Foundation & Ingestion)

This phase focuses on the absolute core of the backend system: building the pipeline that takes a Letterboxd `.zip` export and safely persists it into normalized relational tables.

## Step 1: Solution Setup & Clean Architecture Skeleton

- [ ] Initialize `.NET 9` solution.
- [ ] Create the 4 core projects: `Frametric.Domain`, `Frametric.Application`, `Frametric.Infrastructure`, `Frametric.Api`.
- [ ] Set up project references to enforce dependency direction (API -> Infrastructure -> Application -> Domain).

## Step 2: Domain Entities & Database Context

- [ ] Implement Domain Entities (`User`, `Movie`, `DiaryEntry`, `MovieRating`, `ExternalReference`).
- [ ] Set up `FrametricDbContext` using EF Core in the Infrastructure layer.
- [ ] Configure Entity configurations (Fluent API) to handle composite keys, required fields, and index optimization.
- [ ] Create the initial PostgreSQL Migration.

## Step 3: Importer Infrastructure (CsvHelper)

- [ ] Implement `LetterboxdCsvImporter` handling the extraction of `.zip` in memory via `ZipArchive`.
- [ ] Set up `CsvHelper` ClassMaps to safely parse `diary.csv`, `ratings.csv`, `watched.csv` and `watchlist.csv`.
- [ ] Implement custom TypeConverters for "Yes" -> Boolean and nullable decimals/integers.

## Step 4: Application Layer (CQRS)

- [ ] Setup `MediatR` and `FluentValidation` pipelines.
- [ ] Create `ImportLetterboxdZipCommand` and its associated Handler.
- [ ] Implement the Deduplication logic (checking `ExternalReference` before inserting new `Movie` records).

## Step 5: API Exposure & Swagger

- [ ] Create `ImportsController` with a `POST /api/v1/imports/letterboxd` endpoint accepting `IFormFile`.
- [ ] Configure global Exception Handling middleware to standardize error responses.
- [ ] Verify that Swagger UI successfully displays the file upload endpoint.
