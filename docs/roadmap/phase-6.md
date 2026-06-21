# Roadmap: Phase 6 (Heuristic Decision & Interactive Selection Systems)

This phase expands the cinematic platform by implementing a gamified interactive selection engine and a decision-fatigue mitigation suite. All subsystems operate under a clean architecture approach, utilizing strong typing and granular control over data scopes.

Each system answers a distinct question within the user's decision process:

* What movie comes out? → **Roulette**
* What kind of experience am I risking? → **Dice**
* How is the search combination built? → **Slot Machine**
* Which option do I choose among unknowns? → **Mystery Box**
* What long-term progress do I have as a cinephile? → **Bingo**

---

## Step 1: Data Context & Discovery Scopes Definition

To ensure system extensibility, all discovery services process a context object that defines the candidate source, incorporating native support for custom movie collections uploaded dynamically by the user.

```csharp
// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace Frametric.Domain.Discovery;

public enum DiscoveryDataSourceScope
{
    WatchlistOnly,     // Restricted to the user's local watchlist
    DatabaseOnly,      // Queries the global enriched catalog (TMDB)
    CustomCollection,  // Based on a list of IDs (Custom List) extracted from a provided list of titles for movies on-demand
    Hybrid             // Weighted combination of local and global sources
}

```

---

## Step 2: Selection Roulette — Absolute Randomness

The roulette is the purest and most direct selection system on the platform. There is no advanced configuration or user influence beyond an optional initial filter.

### Functionality

1. A pool of valid movies is generated based on general availability or minimal filters if they exist.
2. The roulette spins through a visual set of real movie posters.
3. The system selects a single winner in a completely random fashion.

### Key Property: Non-Manipulable Randomness

* No weighting based on user taste.
* No "hidden preference" systems.
* No probability adjustments.
* All movies in the pool have the same real probability of being picked.

### Application Layer Contract

```csharp
// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>

using MediatR;

namespace Frametric.Application.Discovery.Queries;

public record RouletteSelectionQuery(
    Guid UserId,
    DiscoveryDataSourceScope Scope,
    IEnumerable<Guid>? CustomSourceIds
) : IRequest<SelectionResultDto>;

public record SelectionResultDto(
    Guid MovieId,
    string Title,
    string DirectorName,
    int ReleaseYear,
    string SelectionMechanismMetadata
);
```

### Advanced Mode: Persistence Threshold

The user may enable an optional rule that no movie becomes the winner until it has appeared a configurable number of times within the session.

#### Execution Cycle

1. Spin → result A (counter A++)
2. Brief pause
3. Spin → result B (counter B++)
4. Repeat…

When a movie reaches the defined threshold (e.g., 3 appearances):

* It becomes the winner.
* The system automatically stops the roulette.

This transforms the result into a **statistical consensus of randomness** rather than a single draw, mitigating single-draw bias. Each appearance is registered per movie ID through an internal session counter.

```csharp
// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>

public record IterativeSelectionQuery(
    Guid UserId,
    DiscoveryDataSourceScope Scope,
    int? PersistenceThreshold,            // Threshold for hit validation
    IEnumerable<Guid>? CustomSourceIds    // Support for Custom Lists
) : IRequest<SelectionResultDto>;
```

### Universal Exclusion Logic (SQL Dapper Layer)

Every discovery query must apply an exclusion filter against the `DiaryEntries` table to omit previously consumed content. Explicit PostgreSQL type casting is enforced to match the DTO primary constructors.

```sql
-- Dapper-executed query for the discovery pool
WITH ValidDiscoveryPool AS (
    SELECT m."Id" AS MovieId, m."Title", m."Director", m."ReleaseYear"
    FROM "Movies" m
    WHERE m."Id" NOT IN (
        SELECT CAST(d."MovieId" AS UUID) 
        FROM "DiaryEntries" d 
        WHERE d."UserId" = @UserId
    ) -- Strict exclusion of watched films
)
SELECT 
    vdp.MovieId,
    vdp."Title",
    vdp."Director" AS DirectorName,
    CAST(vdp."ReleaseYear" AS INTEGER) AS ReleaseYear,
    'Iterative_Consensus' AS SelectionMechanismMetadata
FROM ValidDiscoveryPool vdp
ORDER BY RANDOM()
LIMIT 1;
```

---

## Step 3: Cinematic Dice — Risk & Reward (D&D-Style System)

The dice do not select movies directly. They determine the **quality, rarity, and abstract characteristics** of the final recommendation.

Each die represents a dimension of the outcome. The user decides which dice to use before rolling, and modifiers accumulate through streaks, bingo achievements, and user activity — introducing cinephile character progression.

### Die Types (Attributes)

* **Quality:** Defines the overall quality of the recommendation (Low → entertaining, Medium → good film, High → very good, Critical → potential masterpiece).
* **Rarity:** Defines how well-known the film is (Popular, Known, Lesser-known, Hidden gem, Extreme discovery).
* **Risk:** Defines how far it strays from the user's usual taste (Safe, Slight shift, Moderate, Risky, Total chaos).
* **Complexity (optional):** Narrative or thematic demandingness (Easy, Conventional, Deep, Complex, Challenging).
* **Exploration (optional):** Cultural or geographical openness (Habitual, International, Uncommon, Global cinema, Extreme discovery).

### Roll Mechanics

The user selects dice and rolls. Instead of immediately generating a movie, the system defines a **search zone**:

> "Very good film, little-known, and fairly outside the user's usual taste."

From there:

* The catalog is filtered.
* A movie matching the profile is selected.
* A single final recommendation is returned.

### Heuristic Attribute Matrix

Weighted coefficients or numeric inputs map to specific analytical constraints over the catalog:

* **Minimum (Critical Fail):** Maps to `m."GlobalRating" <= 4.0` and triggers an active user affinity inversion (Chaos Mode).
* **Intermediate:** Maps to `m."GlobalRating" BETWEEN 6.5 AND 7.8` alongside mid-range popularity index constraints.
* **Maximum (Critical Success):** Maps to `m."GlobalRating" >= 8.2` and targets specific "Hidden Gems" (< 15th percentile in global visibility index).

### Criticals & Fumbles

* **Critical (maximum values):** Activate special events — legendary film, exceptional hidden gem, special rewards in other modes (bingo, mystery boxes, etc.).
* **Fumble (minimum values):** Activate unexpected results — intentionally amusing absurd/bad film, "Chaos Mode," completely out-of-context recommendation.

---

## Step 4: Cinematic Slot Machine — Visual Filter Combination

The slot machine is the system for randomly constructing search criteria. Unlike dice, it does not measure quality or risk — it builds a **search equation** from independent reels configured by the user.

### Pre-Configuration

The user defines what each reel represents:

* Reel 1 → Genre
* Reel 2 → Decade
* Reel 3 → Director
* Reel 4 → Duration
* Reel 5 → Country

### Activation

* The machine does not spin until the user pulls the lever.
* Each reel spins independently and stops sequentially, building tension.

### Result

The final combination defines the exact search filters:

🎰 Horror
🎰 2000s
🎰 James Wan
🎰 <120 min

→ A film matching those criteria is retrieved.

### Dimensional Search Contract

If a criterion is defined as null, the engine assumes heuristic resolution, filling the dimension through randomness based on user trends.

```csharp
// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>

namespace Frametric.Application.Discovery.Queries;

public record DimensionalSearchRequest(
    string? Genre,
    int? Decade,
    string? Director,
    DiscoveryDataSourceScope Scope
);
```

### Key Distinction

* Slot Machine = "what movie am I looking for"
* Dice = "how good / rare / risky will it be"

### Jackpot

Special combinations may activate cult films, premium recommendations, unusual experiences, or bonuses for other modes.

---

## Step 5: Mystery Box — Choosing Among Hidden Options

The mystery box is a controlled decision system within randomness. Before being displayed:

1. Five random movies are selected.
2. They are assigned to five boxes.
3. The assignment is locked and hidden.

### Interaction

The user chooses a box without knowing its contents:

* Opening animation.
* Poster reveal.
* Basic information display.

### Variants

* 🎭 **Thematic:** All options share a common theme.
* 💎 **Premium:** Higher probability of a standout film.
* 🔍 **Full Reveal:** All boxes are shown at the end.
* ⚖️ **Strategy:** Some clues are provided before choosing.

```csharp
// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>

namespace Frametric.Application.Discovery.Queries;

public record MysteryBoxGenerationQuery(
    Guid UserId,
    DiscoveryDataSourceScope Scope,
    MysteryBoxVariant Variant,    // Thematic, Premium, FullReveal, Strategy
    int BoxCount                  // Number of boxes (default: 5)
) : IRequest<MysteryBoxDto>;

public record MysteryBoxDto(
    IReadOnlyList<Guid> BoxIds,
    MysteryBoxVariant Variant,
    DateTime GeneratedAt
);
```

---

## Step 6: Cinematic Bingo — Long-Term Progression & Retention

Bingo is the long-term system that does not select movies directly but defines **cinephile consumption objectives**. It establishes a persistence infrastructure to asynchronously evaluate user consumption behavior, validating logs against predefined requirement expressions.

### Configuration

* Duration: 7 / 30 / 90 days
* Grid: 3x3 / 4x4 / 5x5

### Squares

Each square is a challenge:

* Watch an animated film
* Watch a foreign film
* Watch a film under 90 minutes
* Watch a classic
* Watch an award-winning film
* Watch a horror film

### Automatic Progress

* The system detects watched films.
* Squares are completed automatically.
* A single film may fulfill multiple squares.

### Rewards

* Completed line → bonus
* Full bingo → major rewards
* Unlock special modes in roulette / dice / mystery box

```csharp
// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Frametric.Domain.Discovery.Entities;

[Table("DiscoveryObjectives")]
public class DiscoveryObjective
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string RequirementExpression { get; set; } // e.g., "ReleaseYear < 1980 && Genre == 'Sci-Fi'"

    public bool IsAchieved { get; set; }

    public DateTime? CompletionDate { get; set; }

    public Guid? FulfillingDiaryEntryId { get; set; }
}
```

---

## Step 7: Integration Architecture (Angular 19+ Client)

The frontend completely decouples the logical state from the interactive visual animations, ensuring an architecture based on highly cohesive standalone components.

* **Scope Isolation Provider:** A service that injects and distributes the selected scope state (`WatchlistOnly`, `DatabaseOnly`, `CustomCollection`, or `Hybrid`) to the auto-generated OpenAPI strongly-typed clients.
* **Dynamic Collection Preprocessor:** Client-side logic to intercept ad-hoc collections (processed local custom files), extracting relational maps to transfer them as a structured array of identifiers (`CustomSourceIds`) to the API.
* **State Splicing:** Angular UI components consume asynchronous data streams and execute synchronized transitions with the backend response, shielding the system from local data manipulation.

---

## System Relationship Matrix

| System          | Role                                          |
| --------------- | --------------------------------------------- |
| 🎡 Roulette     | Absolute random final selection               |
| 🎲 Dice         | Quality, rarity, and risk of the result       |
| 🎰 Slot Machine | Search filter construction                    |
| 📦 Mystery Box  | Choice among hidden options                   |
| 🎯 Bingo        | Long-term progression and objectives          |
