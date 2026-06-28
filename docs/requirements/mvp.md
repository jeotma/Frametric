# MVP — Cinematic Analytics Platform (Import-Based Architecture)

## Purpose

The purpose of the MVP is to build a modern cinematic analytics platform capable of:

* importing cinematic activity data from external providers through exported files
* normalizing heterogeneous datasets
* generating analytics and wrapped-style summaries
* exposing a stable API for multiple frontend applications
* following production-grade backend engineering practices

The system is intentionally designed around:

* file ingestion pipelines
* provider abstraction
* normalized internal data
* long-term maintainability

The MVP does NOT depend on:

* direct provider APIs
* OAuth integrations
* scraping
* unofficial unstable endpoints

---

# Core Philosophy

The platform is NOT a Letterboxd clone.

The platform is:

* a cinematic data processing system
* an analytics platform
* a wrapped-generation engine
* a centralized normalized data backend

External providers are treated as:

* import sources
* not core dependencies

This keeps the architecture:

* stable
* extensible
* provider-agnostic

---

# High-Level Workflow

```text id="5n0g4z"
User exports data from Letterboxd
        ↓
User uploads CSV/JSON
        ↓
API validates file
        ↓
Parsing pipeline
        ↓
Normalization layer
        ↓
Persistence
        ↓
Analytics generation
        ↓
Frontend visualization
```

---

# Primary MVP Goals

The MVP must prove that the system can:

1. Import external cinematic data
2. Normalize heterogeneous formats
3. Persist a clean internal model
4. Generate analytics and wrapped summaries
5. Expose stable APIs
6. Support multiple frontend clients
7. Remain scalable and maintainable

---

# Initial Supported Providers

## Phase 1

### Letterboxd Export Files

Supported formats:

* CSV
* JSON (optional internal transformed format)

The MVP assumes manual user export from Letterboxd.

---

# MUST HAVE FEATURES

These features are mandatory.

---

# Authentication

## Requirements

* user registration
* login
* JWT authentication
* refresh tokens
* authorization

---

# User Profiles

## Requirements

Store:

* internal user information
* uploaded datasets
* import history
* synchronization metadata

---

# File Upload System

## Requirements

Endpoints capable of receiving:

* CSV files
* JSON files

Example:

```http id="9c0dpx"
POST /api/import/letterboxd
```

---

## Validation Requirements

Validate:

* file type
* file size
* malformed files
* invalid schema
* duplicate uploads
* corrupted rows

The system must fail gracefully.

---

# Import Pipeline Architecture

## Goal

The import system must remain completely decoupled from the core domain.

Recommended flow:

```text id="k9cg6x"
File Upload
    ↓
Parser
    ↓
Import DTO
    ↓
Normalization Layer
    ↓
Domain Entities
    ↓
Persistence
```

---

# Provider Abstraction

The platform must support future providers easily.

Recommended abstraction:

```csharp id="1q6e2l"
IImportProvider
```

Example implementations:

```text id="qv1wr2"
LetterboxdCsvImporter
ImdbCsvImporter
JsonImporter
```

---

# Parsing Layer

## Requirements

Each provider should contain:

* parser
* validator
* mapping layer
* normalization rules

Example:

```text id="1e95qq"
Infrastructure/
 └── Importers/
      ├── Letterboxd/
      ├── Imdb/
      └── Shared/
```

---

# Domain Model

The internal domain model must remain provider-agnostic.

The domain must NEVER know:

* CSV
* JSON
* Letterboxd-specific structures

Example domain entities:

```text id="n7jxmv"
User
Movie
WatchEntry
Review
Rating
Genre
Director
WrappedSummary
```

---

# Persistence

## Requirements

Persist normalized:

* movies
* genres
* ratings
* watch dates
* runtime
* reviews
* analytics metadata

The database becomes the source of truth.

---

# Wrapped Generation

## Requirements

Generate:

* most watched genres
* most watched directors
* monthly activity
* yearly statistics
* total runtime watched
* favorite decades
* top rated movies

---

# Analytics API

## Example Endpoints

```http id="0pq5u8"
GET /api/analytics/wrapped?year=2025
GET /api/analytics/advanced/watched/genres
GET /api/analytics/dashboard
GET /api/movies/{id}
```

---

# Angular Frontend

## Requirements

Frontend capable of:

* authentication
* uploading files
* displaying analytics
* wrapped visualization
* charts
* statistics dashboards

---

## Frontend Architecture

Recommended:

* standalone Angular
* feature-based structure
* typed API clients
* signals
* lazy loading

---

# Swagger/OpenAPI

## Requirements

* enabled from day one
* documented endpoints
* typed client generation

---

# Logging & Observability

## Requirements

* structured logging
* request logging
* import pipeline logging
* failure monitoring
* health checks

Suggested stack:

* Serilog
* Seq
* OpenTelemetry

---

# Docker Support

## Requirements

Containers for:

* API
* PostgreSQL

Optional future:

* Redis
* monitoring stack

---

# SHOULD HAVE FEATURES

These are important but not required for MVP completion.

---

# Background Jobs

## Goals

* analytics recalculation
* wrapped regeneration
* cleanup tasks

Suggested tools:

* System.Threading.Channels (in-memory producer-consumer)
* Hosted Services (BackgroundService)

---

# Import History

Users should be able to:

* see previous imports
* reprocess imports
* delete imports

---

# Advanced Analytics

Examples:

* viewing streaks
* heatmaps
* genre evolution
* actor/director trends

---

# Export Features

Examples:

* PNG wrapped export
* shareable summaries
* downloadable reports

---

# FUTURE FEATURES

These are intentionally out of scope for MVP.

---

# Direct Provider Integrations

Future possibilities:

* OAuth
* official APIs
* automated synchronization

Not required for V1.

---

# Scraping Support

Possible future support:

* scraping pipelines
* unofficial integrations

Must remain isolated from core architecture.

---

# Multi-Provider Ecosystem

Future providers:

* IMDb
* TMDB
* Trakt
* Rotten Tomatoes

The architecture must support adding providers incrementally.

---

# Mobile Applications

Possible future clients:

* mobile app
* tablet app
* TV dashboard

---

# Non-Functional Requirements

# Maintainability

The project must:

* follow Clean Architecture
* maintain strict boundaries
* avoid provider coupling
* separate infrastructure from domain

---

# Scalability

The system should:

* support multiple providers
* support large datasets
* scale analytics progressively

---

# Observability

The platform must expose:

* logs
* metrics
* health checks
* tracing

---

# Performance

The system should:

* avoid unnecessary recomputation
* optimize heavy analytics queries
* cache expensive operations later

---

# Technical Stack

## Backend

* ASP.NET Core
* PostgreSQL
* EF Core
* Dapper
* MediatR
* FluentValidation

---

## Frontend

* Angular
* Signals
* Vanilla CSS / SCSS (No Tailwind)
* Typed API clients

---

## Infrastructure

* Docker
* GitHub Actions
* Azure-ready

---

# Success Criteria

The MVP is successful if:

* users can upload export files
* files are validated correctly
* provider data is normalized
* analytics are generated successfully
* wrapped summaries work
* frontend visualizes data correctly
* architecture remains clean and extensible

---

# Explicit Non-Goals

The MVP does NOT aim to:

* integrate directly with provider APIs
* depend on scraping
* recreate all Letterboxd functionality
* support every provider immediately
* become a social network

The priority is:

* architecture quality
* maintainability
* provider abstraction
* sustainable long-term growth
