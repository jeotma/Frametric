# Recommendation Strategies Detail

This document explains the inner workings, logical progression, and scoring heuristics of the 8 concrete algorithms in the Frametric Cinematic Recommendation Engine.

---

## 1. RecentMood Strategy

### Concept & Goal

Aligns recommendation recommendations with the user's current watching phase. It analyzes what the user has watched recently, computes an interest vector, and matches candidates to this vector.

### Scoring Heuristics

1. **Target Selection**: Selects up to the last 15 watched movies.
2. **Exponential Weighting**: Applies a rating multiplier (user score / 10.0 or 0.7 if unrated) combined with exponential time decay:
   $$w_i = \text{RatingWeight} \cdot e^{-\lambda \cdot t_i}$$
   This ensures that more recent watches influence the recommendation vector exponentially more than older ones.
3. **Profile Construction**: Builds weighted frequency vectors for **Genres**, **Decades**, **Directors**, **Actors**, and **Keywords** using the weights.
4. **Candidate Matching**:
   - **Genre Similarity (35% weight)**: Calculates the Cosine Similarity between the candidate's genres and the user's recent genre vector.
   - **Keyword Similarity (25% weight)**: Calculates the Cosine Similarity between the candidate's keywords and the user's recent keyword vector.
   - **Pacing Match (15% weight)**: Compares candidate duration against the user's weighted average watch runtime. If within $\pm 15$ minutes, awards $+15.0$ points.
   - **Era Match (10% weight)**: If the candidate's decade matches highly watched recent decades, adds up to $+10.0$ points.
   - **Cast/Crew Overlap (18% weight)**: Grants up to $+10.0$ points for director overlap and up to $+8.0$ points for actor overlaps.
   - **Awards Bonus**: Adds up to $+5.0$ points based on Oscar wins ($+0.5$ per win) and nominations ($+0.2$ per nomination).
   - **Global Appeal**: Integrates the Bayesian rating ($10.0\%$ weight) and TMDb popularity to promote high-quality aligned candidates.
5. **Tie-Breaker**: Appends the uniqueness offset to prevent clashes.

---

## 2. OppositeMood Strategy

### Concept & Goal

Acts as a "palette cleanser" when the user wants to break out of a loop (e.g., watching a long streak of depressing dramas or intensive action films). It actively pushes against recent trends.

### Scoring Heuristics

1. **Mood Pole Detection**: Categorizes the user's recent watches into three main mood poles:
   - **Intensity**: Action, Thriller, Horror, Sci-Fi, Adventure.
   - **Lighthearted**: Comedy, Family, Animation, Fantasy.
   - **Reflective**: Drama, Romance, Documentary, History, Mystery.
2. **Opposite Mapping**: If the user's recent watch history is heavily skewed toward one pole, candidates belonging to the other two poles receive a $+15.0$ points bonus.
3. **Genre & Theme Inversion**:
   - **Genre Distance (35% weight)**: Adds $(1.0 - \text{CosineSimilarity}) \times 35.0$ points to reward films that differ completely from recent genres.
   - **Keyword Distance (20% weight)**: Adds $(1.0 - \text{CosineSimilarity}) \times 20.0$ points to reward unfamiliar thematic concepts.
4. **Pacing Inversion**: Compares candidates against the user's average watch runtime:
   - If the user's recent average watch is long ($>115$ min) and the candidate is short ($<95$ min), adds $+15.0$ points.
   - If the user's recent average watch is short ($<95$ min) and the candidate is long ($>120$ min), adds $+15.0$ points.
5. **Quality Anchor**: Incorporates the candidate's global Bayesian average rating ($10.0\%$ weight) and awards ($+0.4$ per Oscar win) to guarantee recommendations remain highly rated.

---

## 3. ComfortZoneDisruptor Strategy

### Concept & Goal

Finds movies outside of the user's typical comfort boundaries but anchors them with familiar creators (directors, actors, or writers) whom the user has historically rated highly. This makes foreign or unfamiliar territories feel approachable.

### Scoring Heuristics

1. **Comfort Zone Mapping**: Computes comfort baselines:
   - **Comfort Genres**: Genres representing $>25\%$ of overall watch history.
   - **Comfort Eras**: Decades representing $>30\%$ of overall watch history.
2. **Disruption Scoring**:
   - If the candidate's genres do not overlap with comfort genres, adds $+35.0$ points.
   - If the candidate's decade is outside of comfort eras, adds $+15.0$ points.
3. **Familiarity Anchors**:
   - **Director Anchor**: If the candidate is directed by someone whom the user previously rated highly ($\ge 7.5$), adds $+30.0$ points.
   - **Actor Anchor**: If the candidate stars actors highly rated by the user, adds $+25.0$ points.
   - **Writer Anchor**: If written by a highly rated creator, adds $+15.0$ points.
4. **Global Prestige & Diversity**:
   - Integrates the global Bayesian rating ($15.0\%$ weight) and popularity ($5.0\%$ weight).
   - If the candidate is a foreign-language film (not English), awards a $+5.0$ points diversity bonus.

---

## 4. GuiltyPleasure Strategy

### Concept & Goal

Finds entertaining, highly enjoyable films that might be ignored by critics but loved by general audiences, specifically targeted at sub-genres the user historically rates higher than average.

### Scoring Heuristics

1. **Niche Genre Selection**: Identifies genres where the user's average rating exceeds their overall average rating. Candidate matches receive $+30.0$ points.
2. **Audience-Critic Discrepancy (35% weight)**:
   - Compares TMDb/Custom rating (audience rating) against Metacritic and Rotten Tomatoes (critic rating).
   - If the discrepancy is large (audience rating is significantly higher than critic reviews), awards up to $+35.0$ points.
3. **Cult Popularity Sweet-Spot**:
   - If the movie has a moderate, non-mainstream popularity ($10.0 < \text{TMDbPopularity} < 75.0$), adds $+15.0$ points.
4. **Certifications & Awards**:
   - Favors PG-13/R rated movies ($+10.0$ points).
   - Awards a $+5.0$ points bonus if the film has no major critical award wins, cementing its guilty-pleasure status.

---

## 5. CinephileEliteStrategy

### Concept & Goal

Recommends cinematic masterpieces. It isolates high-prestige, critically acclaimed films, with a heavy bias toward Academy Awards, international cinema, and longer narratives.

### Scoring Heuristics

1. **Prestige Rating Index (45% weight)**:
   - Calculates a combined weighted critical score:
     $$\text{PrestigeRating} = (0.35 \cdot \text{Metacritic}) + (0.35 \cdot \text{RottenTomatoes}) + (0.20 \cdot \text{IMDb} \times 10) + (0.10 \cdot \text{TMDb} \times 10)$$
     Grants up to $+45.0$ points based on this index.
2. **Awards Weight (30% weight)**:
   - Parses the awards text to compute a custom score:
     $$\text{AwardsScore} = (4.0 \cdot \text{OscarWins}) + (1.5 \cdot \text{OscarNoms}) + (0.3 \cdot \text{OtherWins}) + (0.1 \cdot \text{OtherNoms})$$
     Adds up to $+30.0$ points based on this value.
3. **International Prestige**:
   - If the release country is outside of the USA, adds $+15.0$ points to highlight international art-house cinema.
4. **Epic Runtime**:
   - If the duration is $\ge 130$ minutes, adds $+5.0$ points.
5. **Theme Match**:
   - If the overview contains prestige keywords (e.g. *existential*, *philosophical*, *surreal*, *satire*), adds $+5.0$ points.

---

## 6. DirectorsTrajectory Strategy

### Concept & Goal

Helps the user fill in gaps in the filmography of their favorite directors.

### Scoring Heuristics

1. **Favorite Directors Identification**: Filters for directors whom the user has watched and rated highly (average rating $\ge 7.2$).
2. **Auteur Alignment (80% weight)**:
   - The primary score is directly driven by the user's historical rating of the director: $\text{Score} = \text{UserAvgRating} \times 8.0$.
3. **Chronological Progress**:
   - Identifies the release year of the last movie by this director watched by the user.
   - If the candidate is released after the last watched movie, adds $+15.0$ points (chronological forward movement).
   - If the candidate is released before, adds $+5.0$ points (backtracking to fill historical gaps).
4. **Auteur Alignment Bonus**:
   - If the director also wrote the screenplay, adds $+5.0$ points.
5. **Global Quality**:
   - Integrates the global Bayesian rating directly.

---

## 7. RuntimeContext Strategy

### Concept & Goal

Suggests the best possible movie that fits into the user's available time slot, adjusting story pacing expectations based on duration.

### Scoring Heuristics

1. **Pacing Match (40% weight)**:
   - Computes deviation from target runtime. Score is proportional to the match accuracy.
2. **Pacing Density Optimization (25% weight)**:
   - **Short limits ($\le 95$ mins)**: Favors high-tempo genres (Action, Thriller, Comedy, Horror) with $+25.0$ points.
   - **Long limits ($>95$ mins)**: Favors deep narrative genres (Drama, Sci-Fi, History, Biography) with $+25.0$ points.
3. **Rating Density (25% weight)**:
   - Evaluates global Bayesian rating to maximize "quality per minute".
4. **Popularity Bonus**:
   - Grants up to $+10.0$ points based on TMDb popularity.

---

## 8. PureRandom Strategy

### Concept & Goal

A baseline random selector to let chance guide discovery, but refined with tie-breakers to avoid duplicate positions.

### Scoring Heuristics

1. **Base Score**: Sets a baseline score of $50.0$.
2. **Shuffling**: Shuffles candidate array using a pseudo-random generator.
3. **Tie-Breaker**: Appends the uniqueness offset to guarantee distinct order values.
