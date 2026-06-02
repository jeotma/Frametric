# Frametric

**Frametric** is a premium cinematic analytics platform inspired by services such as Letterboxd and Spotify Wrapped. It processes, normalizes, and enriches a user's historical viewing activity to generate advanced data intelligence and highly interactive year-in-review presentations.

The backend is built with **.NET 9** as a modular data engine that ingests bulk source archives (initially Letterboxd ZIP exports), normalizes them, asynchronously enriches them with the TMDB API's metadata (runtimes, posters, cast/crew), and exposes them via a high-performance REST API. The frontend is an **Angular 19** standalone application featuring interactive analytics dashboards and a beautiful cinematic slide experience known as **"The Final Cut"**.

---

## Core Vision

Frametric is a dedicated cinematic intelligence engine designed to:

- **Ingest & Normalize**: Safely parse, clean, and deduplicate bulk exports (e.g., Letterboxd CSVs/ZIPs) in memory without leaking provider-specific structures.
- **Asynchronously Enrich**: Auto-supplement basic CSV logs with detailed metadata (genres, runtimes, posters, directors, and top cast) from TMDB in the background.
- **Generate Advanced Insights**: Execute high-performance Dapper queries to uncover deep viewing patterns (e.g., casting repetitions, rating trends, weekend binge habits, watchlist graveyards).
- **Cinematic Presentation**: Render **"The Final Cut"**—a custom, micro-animated Spotify Wrapped-style slideshow.
- **Clean Architecture & Decoupling**: Maintain provider-agnostic domain entities and strict layer separation for long-term scalability.

---

## High-Level Architecture

ZIP Upload → Ingestion → Normalization → Persistence → API → Frontend

### Architectural Style

- Modular Monolith
- Clean Architecture
- CQRS-inspired separation

---

## Technology Stack

### Backend

- .NET 9 / ASP.NET Core
- Entity Framework Core
- Dapper
- MediatR
- FluentValidation
- Serilog
- OpenTelemetry

### Database

- PostgreSQL
- Redis (future caching layer)

### Frontend

- Angular 19+
- Signal-based state management
- Feature-based lazy loading
- OpenAPI-generated clients

---

## Project Structure

Frametric solution:

- Frametric.Api
- Frametric.Application
- Frametric.Domain
- Frametric.Infrastructure

### Layers

- API: routing, auth, controllers
- Application: CQRS handlers, DTOs, validation
- Domain: core business logic
- Infrastructure: persistence, CSV/ZIP parsing, external IO

---

## Data Ingestion Pipeline

ZIP Upload → In-memory extraction → CSV parsing → Command → Normalization → Database

### Rules

- Convert year formats (2022.0 → 2022)
- Replace missing values with defaults
- Deduplicate by external URI
- Never leak external provider structure into domain

---

## Domain Model

Core entities:

- User
- Movie
- DiaryEntry
- MovieRating
- WatchlistItem
- ExternalReference (Value Object)

---

## CQRS

### Commands

- RegisterUserCommand
- LoginUserCommand
- ImportLetterboxdArchiveCommand
- DeleteImportCommand
- EnrichPendingMoviesCommand
- MarkImportsCompletedCommand

### Queries (Dapper optimized)

- GetDashboardSummaryQuery
- GetWrappedSummaryQuery
- GetMonthlyActivityQuery
- GetTopDirectorsQuery
- GetImportHistoryQuery
- Advanced Analytics Queries (e.g., Watched & Watchlist stats, Bonus, and Final Cut stats)

---

## API & Security

- Versioned API under `/api/` (Auth, Import, Analytics, and Advanced Analytics)
- OpenAPI / Swagger integration
- JWT authentication + sliding-expiration refresh tokens
- Role-based authorization claims
- Request validation via FluentValidation pipeline

---

## Background Processing

- **TMDB Enrichment Pipeline**: Asynchronous background worker using `System.Threading.Channels` for in-memory producer-consumer queueing.
- **Batch Processing**: Enriches imported movie catalog with Director, Actor, Genre, and Poster metadata from TMDB in throttled batches to respect rate limits.
- **History Status Lifecycle**: Updates import progress dynamically and flags failed TMDB lookups to avoid redundant calls.

---

## MVP Success Criteria

- ZIP upload works correctly
- Data parses and normalizes without errors
- Analytics queries perform efficiently
- Frontend renders metrics correctly

---

## Future Vision

- OAuth integrations
- AI recommendations
- Social analytics graph
- Mobile apps
- Multi-source ingestion expansion

---

## Engineering Philosophy

- Data-first design
- Domain isolation
- Scalability over premature optimization
- Strict separation of concerns
- Analytics-first performance mindset
