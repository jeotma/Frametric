import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from '../../core/api/api/advanced-analytics.service';
import { AnalyticsService } from '../../core/api/api/analytics.service';
import { Observable, finalize } from 'rxjs';

interface GlobalFilters {
  watchYear?: number;
  releaseYear?: number;
  minRating?: number;
  maxRating?: number;
  actor?: string;
  director?: string;
  genre?: string;
}

interface QueryDef {
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

import { EasterEggPipe } from '../../core/services/easter-egg.pipe';

@Component({
  selector: 'app-stats',
  standalone: true,
  imports: [CommonModule, FormsModule, EasterEggPipe],
  templateUrl: './stats.html',
  styleUrl: './stats.scss'
})
export class StatsComponent implements OnInit {
  private advancedApi = inject(AdvancedAnalyticsService);
  private api = inject(AnalyticsService);

  public globalFilters: GlobalFilters = {};

  public queries: QueryDef[] = [
    // --- WATCHED BASIC ---
    {
      id: 'watched_by_year', category: 'Watched History', name: 'Movies Watched', description: 'List of all movies you have watched. You can filter them freely.', type: 'list',
      allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director', 'actor'],
      execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor)
    },
    { id: 'watched_directors', category: 'Watched History', name: 'Watched Directors', description: 'List of all directors you have watched.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedDirectorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'watched_actors', category: 'Watched History', name: 'Watched Actors', description: 'List of all actors you have watched.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedActorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'watched_genres', category: 'Watched History', name: 'Watched Genres', description: 'Count of movies watched by genre.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedGenresGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'watched_decades', category: 'Watched History', name: 'Watched Decades', description: 'Count of movies watched grouped by release decade.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedDecadesGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    
    // --- WATCHED ADVANCED ---
    { id: 'top_actors', category: 'Watched Insights', name: 'Top Actors', description: 'Your most watched actors.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedActorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'top_directors', category: 'Watched Insights', name: 'Top Directors', description: 'Your most watched directors.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedDirectorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'predominant_era', category: 'Watched Insights', name: 'Predominant Era', description: 'Your preference between classic and modern cinema.', type: 'single', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedPredominantEraGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'director_ranking', category: 'Watched Insights', name: 'Director Ranking by Rating', description: 'Directors ranked by your average rating.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedDirectorRankingGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    {
      id: 'total_time', category: 'Watched Insights', name: 'Total Time Invested', description: 'Calculate total time spent watching a specific director or genre.', type: 'single',
      allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'actor'],
      inputs: [
        { name: 'filterType', label: 'Filter Type', type: 'select', defaultValue: 'Director', options: [{label: 'Director', value: 'Director'}, {label: 'Genre', value: 'Genre'}] },
        { name: 'filterName', label: 'Name', type: 'text', defaultValue: 'Christopher Nolan' }
      ],
      execute: (adv, _, gf, qs) => adv.apiAnalyticsAdvancedWatchedTotalTimeGet(qs.filterType, qs.filterName, gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor)
    },

    // --- WATCHED CORRELATIONS ---
    { id: 'preferred_day', category: 'Habits & Correlations', name: 'Preferred Watch Day', description: 'Days of the week you watch the most movies.', type: 'chart', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedPreferredDayGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    {
      id: 'rating_evolution', category: 'Habits & Correlations', name: 'Rating Evolution', description: 'How your ratings evolve throughout the year.', type: 'chart', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director', 'actor'],
      execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedRatingEvolutionGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor)
    },
    { id: 'genre_streaks', category: 'Habits & Correlations', name: 'Genre Streaks', description: 'Longest streaks watching the same genre consecutively.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedGenreStreaksGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'longest_movie', category: 'Habits & Correlations', name: 'Longest Movie', description: 'The longest movie you have ever watched.', type: 'single', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedLongestMovieGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'casting_reps', category: 'Habits & Correlations', name: 'Casting Repetitions', description: 'Pairs of actors you have seen together the most.', type: 'list', allowedFilters: ['watchYear', 'releaseYear', 'minRating', 'maxRating', 'genre', 'director'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchedCastingRepetitionsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },

    // --- WATCHLIST BASIC ---
    {
      id: 'watchlist_movies', category: 'Watchlist', name: 'Watchlist Movies', description: 'Pending movies in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'],
      execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor)
    },
    { id: 'watchlist_directors', category: 'Watchlist', name: 'Watchlist Directors', description: 'Directors in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistDirectorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'watchlist_actors', category: 'Watchlist', name: 'Watchlist Actors', description: 'Actors in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'director'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistActorsGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'watchlist_genres', category: 'Watchlist', name: 'Watchlist Genres', description: 'Genres in your watchlist.', type: 'list', allowedFilters: ['releaseYear', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistGenresGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'watchlist_decades', category: 'Watchlist', name: 'Watchlist Decades', description: 'Decades in your watchlist.', type: 'chart', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistDecadesGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },

    // --- WATCHLIST ADVANCED ---
    { id: 'most_anticipated_director', category: 'Watchlist Insights', name: 'Most Anticipated Director', description: 'Director with the most pending movies.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistMostAnticipatedDirectorGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'most_anticipated_actor', category: 'Watchlist Insights', name: 'Most Anticipated Actor', description: 'Actor with the most pending movies.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'director'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistMostAnticipatedActorGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'total_pending_watchtime', category: 'Watchlist Insights', name: 'Total Pending Watchtime', description: 'Total time required to clear your watchlist.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistTotalWatchtimeGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'oldest_pending', category: 'Watchlist Insights', name: 'Oldest Pending Movie', description: 'The movie waiting the longest in your watchlist.', type: 'single', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistOldestPendingGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },

    // --- WATCHLIST CORRELATIONS ---
    { id: 'watchlist_by_era', category: 'Watchlist Insights', name: 'Watchlist by Era', description: 'Pending classic vs modern movies.', type: 'list', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistByEraGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'duration_balance', category: 'Watchlist Insights', name: 'Duration Balance', description: 'Balance between short, medium, and long pending movies.', type: 'chart', allowedFilters: ['releaseYear', 'genre', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistDurationBalanceGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
    { id: 'genre_proportion', category: 'Watchlist Insights', name: 'Genre Proportion', description: 'Watchlist vs Watched genres.', type: 'list', allowedFilters: ['releaseYear', 'director', 'actor'], execute: (adv, _, gf) => adv.apiAnalyticsAdvancedWatchlistGenreProportionGet(gf.watchYear, gf.releaseYear, gf.minRating, gf.maxRating, gf.genre, gf.director, gf.actor) },
  ];

  public categoriesList = computed(() => {
    const cats = new Set(this.queries.map(q => q.category));
    return Array.from(cats);
  });

  public selectedCategory = signal<string>('Watched History');
  
  public queriesInCategory = computed(() => {
    return this.queries.filter(q => q.category === this.selectedCategory());
  });

  public selectedQueryId = signal<string>('watched_by_year');
  
  public currentQuery = computed(() => {
    return this.queries.find(q => q.id === this.selectedQueryId()) || this.queries[0];
  });

  // Dynamic form state
  public querySpecificInputs: any = {};
  
  public resultData = signal<any>(null);
  public loading = signal<boolean>(false);

  ngOnInit() {
    this.initInputs();
  }

  onCategoryChange(cat: string) {
    this.selectedCategory.set(cat);
    const firstQuery = this.queriesInCategory()[0];
    if (firstQuery) {
      this.selectedQueryId.set(firstQuery.id);
      this.initInputs();
      this.resultData.set(null); // Clear previous
    }
  }

  onQueryChange() {
    this.initInputs();
    this.resultData.set(null);
  }

  private initInputs() {
    const q = this.currentQuery();
    this.querySpecificInputs = {};
    if (q && q.inputs) {
      q.inputs.forEach(i => this.querySpecificInputs[i.name] = i.defaultValue);
    }
  }

  runQuery() {
    const q = this.currentQuery();
    if (!q || !q.execute) return;

    // Reset global filters that are not allowed for this query
    // so they are not accidentally sent to the backend
    if (q.allowedFilters) {
      if (!q.allowedFilters.includes('watchYear')) this.globalFilters.watchYear = undefined;
      if (!q.allowedFilters.includes('releaseYear')) this.globalFilters.releaseYear = undefined;
      if (!q.allowedFilters.includes('minRating')) this.globalFilters.minRating = undefined;
      if (!q.allowedFilters.includes('maxRating')) this.globalFilters.maxRating = undefined;
      if (!q.allowedFilters.includes('actor')) this.globalFilters.actor = undefined;
      if (!q.allowedFilters.includes('director')) this.globalFilters.director = undefined;
      if (!q.allowedFilters.includes('genre')) this.globalFilters.genre = undefined;
    }

    this.loading.set(true);
    q.execute(this.advancedApi, this.api, this.globalFilters, this.querySpecificInputs).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (res) => this.resultData.set(res),
      error: (err) => {
        console.error('Query failed', err);
        this.resultData.set(null);
      }
    });
  }

  // Type Guards for Template
  isArray(val: any): boolean {
    return Array.isArray(val);
  }

  getChartMaxValue(): number {
    const data = this.resultData();
    if (!this.isArray(data) || data.length === 0) return 10;
    const max = Math.max(...data.map((item: any) => item.count || item.averageRating || item.totalWatches || item.shortCount || 0));
    return max > 0 ? max : 10;
  }

  isSingle(data: any): boolean {
    return data && !Array.isArray(data);
  }

  hasNoData(data: any): boolean {
    if (!data) return true;
    if (data.totalMinutes === 0 && data.totalHours === 0) return true;
    if (data.count === 0 && data.averageRating === undefined && data.totalMinutes === undefined) return true;
    return false;
  }

  // Sorting logic for the unified table
  public sortColumn = signal<string | null>(null);
  public sortDirection = signal<'asc' | 'desc'>('desc');

  public sortedResultData = computed(() => {
    const data = this.resultData();
    if (!this.isArray(data)) return data;

    const col = this.sortColumn();
    if (!col) return data;

    const dirMultiplier = this.sortDirection() === 'asc' ? 1 : -1;

    return [...data].sort((a, b) => {
      let valA = a[col];
      let valB = b[col];

      // Handle nulls/undefined
      if (valA == null) valA = '';
      if (valB == null) valB = '';

      if (typeof valA === 'string' && typeof valB === 'string') {
        return valA.localeCompare(valB) * dirMultiplier;
      }
      if (valA < valB) return -1 * dirMultiplier;
      if (valA > valB) return 1 * dirMultiplier;
      return 0;
    });
  });

  sortBy(column: string) {
    if (this.sortColumn() === column) {
      // Toggle direction
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('desc');
    }
  }

  getColNames(): string[] {
    const data = this.resultData();
    if (!this.isArray(data) || data.length === 0) return [];
    
    // We get the keys of the first item
    const allKeys = Object.keys(data[0]);

    // Filter out keys that are always 0 or null/undefined across ALL rows
    return allKeys.filter(key => {
      return data.some((item: any) => {
        const val = item[key];
        return val !== 0 && val !== null && val !== undefined && val !== '';
      });
    });
  }

  formatColName(name: string): string {
    if (name === 'count') {
      return this.selectedCategory().toLowerCase().includes('watchlist') ? 'Pending Count' : 'Watched Count';
    }

    // Camel case to words: 'averageRating' -> 'Average Rating'
    const result = name.replace(/([A-Z])/g, " $1");
    return result.charAt(0).toUpperCase() + result.slice(1);
  }

  isFilterAllowed(filterName: string): boolean {
    const q = this.currentQuery();
    if (!q || !q.allowedFilters) return true;
    return q.allowedFilters.includes(filterName);
  }
}
