import { Observable } from 'rxjs';
import { AdvancedAnalyticsService } from '../../core/api/api/advanced-analytics.service';
import { AnalyticsService } from '../../core/api/api/analytics.service';

export interface GlobalFilters {
  watchYear?: number;
  releaseYear?: number;
  minRating?: number;
  maxRating?: number;
  minCustomRating?: number;
  maxCustomRating?: number;
  actor?: string;
  director?: string;
  genre?: string;
}

export interface QueryDef {
  id: string;
  name: string;
  description: string;
  category: string;
  type: 'list' | 'single' | 'chart' | 'comparison';
  inputs?: {
    name: string;
    label: string;
    type: 'number' | 'text' | 'select';
    defaultValue?: any;
    options?: { label: string; value: any }[];
  }[];
  allowedFilters: string[];
  execute?: (advancedApi: AdvancedAnalyticsService, api: AnalyticsService, globalFilters: GlobalFilters, querySpecificInputs: any) => Observable<any>;
}

export const STATS_QUERIES: QueryDef[] = [
  // --- WATCHED BASIC ---
  {
    id: 'watched_by_year', category: 'Watched History', name: 'Movies Watched', description: 'List of all movies you have watched. You can filter them freely.', type: 'list',
    allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'],
    execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  },
  { id: 'watched_directors', category: 'Watched History', name: 'Watched Directors', description: 'List of all directors you have watched.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedDirectorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'watched_actors', category: 'Watched History', name: 'Watched Actors', description: 'List of all actors you have watched.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedActorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'watched_genres', category: 'Watched History', name: 'Watched Genres', description: 'Count of movies watched by genre.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedGenresGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'watched_decades', category: 'Watched History', name: 'Watched Decades', description: 'Count of movies watched grouped by release decade.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedDecadesGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },

  { id: 'total_time', category: 'Watched History', name: 'Total Time Invested', description: 'Calculate total time spent watching a specific director or genre.', type: 'single',
    allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'],
    inputs: [
      { name: 'filterType', label: 'Filter Type', type: 'select', defaultValue: 'Director', options: [{label: 'Director', value: 'Director'}, {label: 'Actor', value: 'Actor'}, {label: 'Genre', value: 'Genre'}] },
      { name: 'filterName', label: 'Name', type: 'text', defaultValue: '' }
    ],
    execute: (adv, _, gf, qs) => adv.apiAnalyticsAdvancedWatchedTotalTimeGet(qs.filterType, qs.filterName, gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  },

  // --- WATCHED CORRELATIONS ---
  { id: 'preferred_day', category: 'Habits & Correlations', name: 'Preferred Watch Day', description: 'Days of the week you watch the most movies.', type: 'chart', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedPreferredDayGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  {
    id: 'rating_evolution', category: 'Habits & Correlations', name: 'Rating Evolution', description: 'How your ratings evolve throughout the year.', type: 'chart', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'],
    execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedRatingEvolutionGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  },
  { id: 'genre_streaks', category: 'Habits & Correlations', name: 'Genre Streaks', description: 'Longest streaks watching the same genre consecutively.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedGenreStreaksGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'longest_movie', category: 'Habits & Correlations', name: 'Longest Movie', description: 'The longest movie you have ever watched.', type: 'single', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedLongestMovieGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'shortest_movie', category: 'Habits & Correlations', name: 'Shortest Movie', description: 'The shortest movie you have ever watched.', type: 'single', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedFinalCutShortestMovieGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'casting_reps', category: 'Habits & Correlations', name: 'Casting Repetitions', description: 'Pairs of actors you have seen together the most.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedCastingRepetitionsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },

  // --- WATCHLIST BASIC ---
  {
    id: 'watchlist_movies', category: 'Watchlist', name: 'Watchlist Movies', description: 'Pending movies in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'],
    execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  },
  { id: 'watchlist_directors', category: 'Watchlist', name: 'Watchlist Directors', description: 'Directors in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistDirectorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'watchlist_actors', category: 'Watchlist', name: 'Watchlist Actors', description: 'Actors in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'director'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistActorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'watchlist_genres', category: 'Watchlist', name: 'Watchlist Genres', description: 'Genres in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistGenresGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'watchlist_decades', category: 'Watchlist', name: 'Watchlist Decades', description: 'Decades in your watchlist.', type: 'chart', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistDecadesGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },

  // --- WATCHLIST ADVANCED ---
  { id: 'most_anticipated_director', category: 'Watchlist Insights', name: 'Most Anticipated Director', description: 'Director with the most pending movies.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistMostAnticipatedDirectorGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'most_anticipated_actor', category: 'Watchlist Insights', name: 'Most Anticipated Actor', description: 'Actor with the most pending movies.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'director'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistMostAnticipatedActorGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'total_pending_watchtime', category: 'Watchlist Insights', name: 'Total Pending Watchtime', description: 'Total time required to clear your watchlist.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistTotalWatchtimeGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'oldest_pending', category: 'Watchlist Insights', name: 'Oldest Pending Movie', description: 'The movie waiting the longest in your watchlist.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistOldestPendingGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },

  // --- WATCHLIST CORRELATIONS ---
  { id: 'watchlist_by_era', category: 'Watchlist Insights', name: 'Watchlist by Era', description: 'Pending classic vs modern movies.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistByEraGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'duration_balance', category: 'Watchlist Insights', name: 'Duration Balance', description: 'Balance between short, medium, and long pending movies.', type: 'chart', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistDurationBalanceGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  { id: 'genre_proportion', category: 'Watchlist Insights', name: 'Genre Proportion', description: 'Watchlist vs Watched genres.', type: 'list', allowedFilters: ['releaseYear', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistGenreProportionGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor) },
  // --- SPECIAL INSIGHTS (BONUS) ---
  {
    id: 'weekend_warrior', category: 'Special Insights', name: 'Weekend Warrior', description: 'Compare watches on weekends vs weekdays.', type: 'comparison',
    allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'],
    execute: (adv, _, gf) => adv.apiAnalyticsAdvancedBonusWeekendWarriorGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  },
  {
    id: 'cinematic_fatigue', category: 'Special Insights', name: 'Cinematic Fatigue', description: 'Analyze if watching multiple movies in a single day affects your ratings.', type: 'comparison',
    allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'],
    execute: (adv, _, gf) => adv.apiAnalyticsAdvancedBonusCinematicFatigueGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  },
  {
    id: 'hidden_gems', category: 'Special Insights', name: 'Hidden Gems', description: 'High rated classic movies (released >30 years ago) in your watched history.', type: 'list',
    allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'],
    execute: (adv, _, gf) => adv.apiAnalyticsAdvancedBonusHiddenGemsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  },
  {
    id: 'watchlist_graveyard', category: 'Special Insights', name: 'Watchlist Graveyard', description: 'Movies that have been pending in your watchlist the longest.', type: 'list',
    allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'minCustomRating', 'maxCustomRating', 'genre', 'director', 'actor'],
    execute: (adv, _, gf) => adv.apiAnalyticsAdvancedBonusWatchlistGraveyardGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.minCustomRating, gf.maxCustomRating, gf.genre, gf.director, gf.actor)
  }
];