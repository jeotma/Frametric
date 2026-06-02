# Frametric

**Frametric** is a modern cinematic analytics platform inspired by services such as Letterboxd and Spotify Wrapped. It acts as a centralized audiovisual data engine capable of ingesting, normalizing, and analyzing viewing activity from multiple sources.

Built with .NET 9 and a modular architecture designed for long-term scalability.

---

## Core Vision

Frametric is not a Letterboxd clone.

It is a cinematic data intelligence platform designed to:

- Aggregate viewing activity from multiple providers
- Normalize heterogeneous data structures
- Generate advanced analytics and “Wrapped”-style insights
- Support multiple clients (Web, Mobile, Dashboards)
- Maintain strict separation of concerns

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
- ImportLetterboxdZipCommand
- GenerateWrappedSummaryCommand

### Queries (Dapper optimized)

- GetTopGenresQuery
- GetTopDirectorsQuery
- GetYearSummaryQuery
- GetMonthlyActivityQuery

---

## API & Security

- Versioned API: /api/v1/
- OpenAPI / Swagger
- JWT authentication + refresh tokens
- Role-based authorization
- FluentValidation pipeline

---

## Background Processing

Future support:

- Hangfire or Quartz.NET

Use cases:

- Async analytics
- Wrapped generation
- Maintenance jobs

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
