# AGENTS.md

## Purpose

This repository contains a modern cinematic analytics platform composed of:

- A centralized .NET backend platform
- Multiple frontend clients
- External provider integrations
- Analytics and wrapped generation systems

All AI agents participating in this repository must follow the standards below.

---

# Core Philosophy

The system prioritizes:

- Maintainability
- Scalability
- Architectural clarity
- Strong typing
- Observability
- Low coupling
- Long-term sustainability

This is NOT a prototype-style repository.

Avoid shortcuts that damage architecture or consistency.

---

# Architectural Rules

## Clean Architecture

Respect the layer boundaries:

- Domain
- Application
- Infrastructure
- Api

Never leak infrastructure concerns into Domain.

Business logic must never live inside controllers.

---

## Modular Monolith

The application is organized as a modular monolith.

Modules should remain isolated and communicate through application contracts,
not through direct internal coupling.

Avoid hidden dependencies.

---

## CQRS Principles

Use commands for mutations and queries for reads.

Examples:

- Commands -> SyncProfileCommand
- Queries -> GetTopGenresQuery

Do not place complex business logic inside controllers.

---

# External Providers

External providers are unstable by nature.

Rules:

- Never expose external DTOs directly
- Always normalize provider data
- Every provider must have dedicated clients and mappers
- Implement retries and rate limiting
- Log external failures properly

Preferred flow:

External DTO -> Mapper -> Internal Domain Model

---

## Backend Standards

### ASP.NET Core

- Use dependency injection
- Prefer constructor injection
- Use async/await consistently
- Avoid static mutable state
- Controllers must remain thin

---

### Entity Framework Core

Use EF Core for:

- CRUD operations
- Migrations
- Standard persistence

Avoid extremely complex LINQ queries when performance matters.

---

## Dapper

Use Dapper for:

- Analytics
- Heavy aggregations
- Performance-critical queries

Queries must remain readable and maintainable.

---

## SQL & Dapper Query Rules

AI agents must strictly adhere to these rules when creating or modifying SQL queries and Dapper integrations:

1. **Dapper Constructor Mapping (C# Records)**:
   - When mapping to C# `record` types with primary constructors, Dapper demands an EXACT match between the columns returned by the query and the constructor's parameters (case-insensitive).
   - If a query returns fewer columns than the primary constructor demands, the DTO **MUST** have a secondary constructor explicitly matching the returned columns (e.g., `public MyDto(string name, int count) : this(name, count, 0) {}`). Dapper ignores default parameter values (`= 0`) in the primary constructor.
2. **PostgreSQL Type Casting**:
   - `AVG()` in PostgreSQL returns a `numeric` type (C# `decimal`). If the DTO expects a `double`, the query **MUST** cast it: `CAST(COALESCE(AVG(...), 0) AS DOUBLE PRECISION)`.
   - `COUNT()` in PostgreSQL returns a `bigint` (C# `long`). If the DTO expects an `int`, the query **MUST** cast it: `CAST(COUNT(...) AS INTEGER)`.
3. **Unique Films vs Rewatches**:
   - For single-entity analytical groupings (e.g., "Top Directors", "Top Actors", "Top Genres"), **always** count strictly first-time watches. You must explicitly filter out rewatches (e.g., `WHERE ""IsRewatch"" = false`) and use `COUNT(DISTINCT w.""MovieId"")`.
   - Never count re-watches for these single-entity metrics, as it inflates their true unique film count.
   - **Exception for Volume and Pairings**: For global volume metrics (e.g., "Total Watches", "Total Watchtime") AND multi-entity pair groupings (e.g., "Dynamic Duos" / Director-Actor Pairings), re-watches **SHOULD** be included. These are volumetric statistics representing total engagement, so do not filter out rewatches, and use standard counting methods that include them.
4. **Rating Source Truth**:
   - Never compute user average ratings using the `DiaryEntries` table, as these are volatile.
   - Always join the `MovieRatings` table and use the `Score` column to calculate the true historical average rating a user gave to an entity.

---

## Database Rules

Preferred database:

- PostgreSQL

Rules:

- Use proper indexing
- Avoid N+1 queries
- Normalize correctly
- Avoid premature denormalization

---

# Frontend Standards

Frontend applications should:

- Use feature-based organization
- Avoid excessive global state
- Use typed API clients
- Keep components focused and reusable
- Prefer composition over inheritance

---

# Visual Identity & UI Standards

The platform has a strictly defined aesthetic and visual language. AI agents must follow these rules when working on UI:

1. **Strict Color Palette**: Always use the established CSS variables. The explicit and complete palette is:
   - **Accents**: `var(--accent-silver)` (#d4d4d8), `var(--accent-sepia)` (#e2ba64), `var(--accent-record)` (#e50914), `var(--accent-emerald)` (#10b981).
   - **Backgrounds**: `var(--bg-primary)` (#000000), `var(--bg-secondary)` (#0a0a0a), `var(--bg-tertiary)` (#111111).
   - **Surfaces**: `var(--surface-base)` (#000000), `var(--surface-elevated)` (#0a0a0a), `var(--surface-floating)` (#151515).
   - **Text**: `var(--text-primary)` (#ffffff), `var(--text-secondary)` (#a3a3a3), `var(--text-muted)` (#737373).
   - **Borders**: `var(--border-color)` (rgba(255, 255, 255, 0.15)).
   **Never** introduce new colors (like purple, orange, etc.) or arbitrary hex codes without explicit permission.
2. **Icons (SVG Only)**: Emojis and Unicode characters are **strictly forbidden** for UI icons. Always use scalable, clean SVG icons that match the platform's vector-based design language.
3. **Ask First**: If a design requires a new color, visual concept, or icon that doesn't fit the existing system, you must ask the user for guidance before improvising.
4. **Cinematic Data as Narrative**: Every piece of data should feel like part of a story, but never stop looking like data. Use the following checklist to evaluate any UI/UX design proposal:
   - Does it add narrative? ✅
   - Is it still clear and analytical? ✅ → **IN**.
   - Does it add narrative but make the data hard to understand? ❌ → **Improve it before introducing**.
   - Does it add cinematic decoration but provides no meaning? ❌ → **Improve it before introducing**.
   - Does it improve the reading of the data using cinematic language? ✅ → **IN**.

---

# Logging & Observability

Every important operation should be observable.

Required:

- Structured logging
- Correlation IDs where possible
- Meaningful error messages
- Health checks
- Metrics when appropriate

Do not swallow exceptions silently.

---

# Code Quality

Rules:

- Prefer readability over cleverness
- Avoid unnecessary abstractions
- Avoid premature optimization
- Keep methods small and focused
- Use descriptive naming
- Remove dead code

Code should feel production-ready.

---

# Testing

Required testing priorities:

- Critical business logic
- Mapping logic
- Analytics calculations
- Provider normalization

Avoid brittle tests.

Prefer deterministic tests.

---

# Security

Mandatory:

- Validate all external input
- Never trust provider payloads
- Protect secrets
- Avoid hardcoded credentials
- Use HTTPS-only communication

---

# Git Workflow

Recommended:

- Feature branches
- Pull requests
- Conventional commits
- Small incremental changes

Avoid massive unreviewable commits.

---

# Documentation

Documentation is a first-class citizen of the codebase. Keep it up-to-date and clean:

- **Mandatory Updates**: When a substantial feature, architectural boundary, contract, or infrastructure component is modified or added, the corresponding design/requirements documentation in the `docs/` folder must be updated (or a new document created if it introduces a new feature domain).
- **API Spec Maintenance**: Any modifications, additions, or deprecations of REST API endpoints must be immediately documented in [endpoints.md](file:///c:/Users/Jeotm/Documents/PersonalProjects/Frametric/docs/api/endpoints.md).
- **Changelog Logging**: Every major feature release, milestone, or significant codebase change must be logged in the root [CHANGELOG.md](file:///c:/Users/Jeotm/Documents/PersonalProjects/Frametric/CHANGELOG.md) using semantic versioning. Do not let changelogs drift.
- **Markdown Linting**: Respect `.markdownlint.json` guidelines. Ensure document headers remain sibling-specific and avoid duplicate styling where possible.

---

# AI Agent Behaviour Rules

AI agents must:

- **PowerShell Command Separation**: When running multiple commands in sequence (like `npm run download:spec` and `npm run generate:api`), always separate them with a semicolon (`;`) instead of `&&`, as PowerShell does not support `&&` by default.
- **Include License Headers**: Every newly created or highly relevant C# source file must prepend the following copyright and license header at the very top:

  ```csharp
  // Frametric — Cinematic Analytics Platform
  // Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
  //
  // This program is free software: you can redistribute it and/or modify
  // it under the terms of the GNU General Public License as published by
  // the Free Software Foundation, either version 3 of the License, or
  // (at your option) any later version.
  ```

- Preserve architecture consistency
- Avoid introducing unnecessary complexity
- Avoid rewriting unrelated code
- Respect existing patterns
- Prefer incremental changes
- Explain significant architectural decisions

AI agents should act as professional engineering contributors,
not code generators without context.

---

# Final Principle

Every contribution should improve:

- Clarity
- Stability
- Maintainability
- Consistency

The goal is to build a sustainable professional-grade platform.
