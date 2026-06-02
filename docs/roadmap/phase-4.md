# Roadmap: Phase 4 (Proprietary Recommendation Engine)

This phase introduces the Frametric cinematic intelligence engine. The objective is to transform the user's historical data into an active discovery tool, using proprietary algorithms that operate on highly customizable scopes: the user's local Watchlist, the global TMDB-enriched database, or a combined hybrid pool.

## Step 1: Recommendation Domain & Data Contracts

- [ ] Implement `RecommendationProfile` in the Domain layer to track real-time preference vectors (Favorite Genres, Recurring Directors, Comfort Decade).
- [ ] Create Enums for recommendation strategies and source boundaries:
  - `RecommendationStrategy`: `RecentMood`, `OppositeMood`, `ComfortZoneDisruptor`, `GuiltyPleasure`, `CinephileElite`, `DirectorsTrajectory`, `RuntimeContext`.
  - `RecommendationScope`: `WatchlistOnly`, `DatabaseOnly`, `Hybrid`.
- [ ] Define the strict request contract DTO: `RecommendationRequest`.
  - **Validation Constraints:** Quantity must be explicitly bounded to a localized array: `[1, 2, 3, 5, 10]`.

## Step 2: Granular Scope & Source Filtering (SQL Dapper Layer)

To respect the request-scoped boundaries without destroying performance, the query engine dynamically builds the target media pool using optimized indexing on `WatchlistItems` and `Movies`.

- [ ] Implement a unified Dapper data repository that dynamically alters the source CTE (*Common Table Expression*) based on the `RecommendationScope` parameter.
- [ ] **Strict Exclusion Logic:** Every query must cross-reference `DiaryEntries` to completely omit films the user has already watched.
- [ ] Enforce PostgreSQL explicit type casting within the aggregation engine to match primary record constructors:
  - `CAST(COUNT(...) AS INTEGER)` and `CAST(COALESCE(AVG(...), 0) AS DOUBLE PRECISION)`.

## Step 3: Core Algorithmic Matrix

Every strategy calculates a matching percentage (`MatchPercentage`) using in-memory vector calculations over the designated scope pool:

- **Recent Mood (Vector Alignment):** Pulls the last 10 logged films from `DiaryEntries` sorted by logging date. Analyzes Genre, Duration ($\pm$ 20 min), Release Decade, Director, and Key Actors to find the closest statistical neighbor.
- **Opposite Mood (Vector Inversion):** Finds the mathematical inverse of the Recent Mood vector. Ideal for breaking sudden viewing cycles (e.g., suggesting fast-paced indie horror after a streak of slow-burn historical dramas).
- **The Comfort Zone Disruptor:** Evaluates the historical "comfort baseline" (genres/eras representing $>35\%$ of total watch history) and purposefully forces a vector shift toward a completely unmapped genre, anchored by a director or actor the user has previously rated highly ($\ge 4.0$ stars).
- **The "Guilty Pleasure" vs "Cinephile Elite" Index:** * *Guilty Pleasure:* Prioritizes low TMDB popularity/global score movies that precisely match sub-genres the user historically rates higher than the global average.
  - *Cinephile Elite:* Filters for critically acclaimed Masterpieces (TMDB Rating $\ge 8.2$) that have low mainstream popularity traits.
- **The Director's Trajectory:** Cross-checks if a filmmaker has $\ge 2$ entries in the user's history but still has unseen works remaining inside the requested scope, organizing the remaining works chronologically.
- **Runtime & Context Matcher:** Restricts candidate runtimes based on explicit user availability input (e.g., "Exactly 90 minutes") and shifts structural pacing expectations (e.g., higher weight to high-tempo genres for shorter limits).

## Step 4: CQRS Queries & Dynamic Endpoints

- [ ] Implement `GetCinematicRecommendationsQuery` and its corresponding MediatR Handler within `Frametric.Application`.
- [ ] Expose the pipeline via `RecommendationsController` under `POST /api/v1/recommendations/generate`.

  ```json
  {
    "strategy": "RecentMood",
    "scope": "WatchlistOnly", 
    "quantity": 3,
    "maxRuntimeMinutes": 120
  }
  ```

- [ ] Implement Exclusion Rule Middleware to temporarily cache generated recommendations in Redis, preventing the exact same unwatched film from exhausting the user's feed if skipped multiple times.

## Step 5: Angular 19+ Command & Control Center

- [ ] Design an interactive, high-fidelity control dashboard in the frontend application.
- [ ] UI Component Architecture:
  - **Scope Toggle:** A premium three-way glassmorphic selector switcher (Watchlist, Discover Pool, or All Combined).
  - **Quantity Chip Matrix:** Micro-animated selectable chips specifically constrained to 1, 2, 3, 5, or 10 slots.
  - **Strategy Wheel:** A modern radial carousel or card deck showcasing the interactive modes (Mood, Disruptor, Elite, etc.) accompanied by clear micro-copy explaining the mathematical reasoning.
  - [ ] Integrate generated results directly into the Phase 1 persistence actions, enabling instant "Mark as Watched" or "Drop from Watchlist" interactions directly from the suggestion block.

## 🛠️ Backend Architectural Proof (C# Implementation Snippet)

To keep the system fully compliant with the AI Agent Behavior Rules and ensure it is not treated like a prototype, here is the formal design of the query request and handler contract:

```csharp
// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using FluentValidation;
using MediatR;

namespace Frametric.Application.Recommendations.Queries;

public enum RecommendationStrategy
{
    RecentMood,
    OppositeMood,
    ComfortZoneDisruptor,
    GuiltyPleasure,
    CinephileElite,
    DirectorsTrajectory,
    RuntimeContext
}

public enum RecommendationScope
{
    WatchlistOnly,
    DatabaseOnly,
    Hybrid
}

public record GetCinematicRecommendationsQuery(
    Guid UserId,
    RecommendationStrategy Strategy,
    RecommendationScope Scope,
    int Quantity
) : IRequest<IEnumerable<RecommendedMovieDto>>;

public record RecommendedMovieDto(
    Guid MovieId,
    string Title,
    string DirectorName,
    int ReleaseYear,
    double MatchPercentage,
    string RecommendationReason
);

public class GetCinematicRecommendationsValidator : AbstractValidator<GetCinematicRecommendationsQuery>
{
    private static readonly int[] AllowedQuantities = [1, 2, 3, 5, 10];

    public GetCinematicRecommendationsValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Strategy).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Quantity)
            .Must(q => AllowedQuantities.Contains(q))
            .WithMessage("You can only request 1, 2, 3, 5, or 10 recommendations at a time.");
    }
}
```
