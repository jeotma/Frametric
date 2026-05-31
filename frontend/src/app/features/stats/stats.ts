import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from '../../core/api/api/advanced-analytics.service';
import { AnalyticsService } from '../../core/api/api/analytics.service';
import { Observable, finalize } from 'rxjs';

interface QueryDef {
  id: string;
  name: string;
  description: string;
  category: string;
  type: 'list' | 'single' | 'chart' | 'comparison';
  methodName?: keyof AdvancedAnalyticsService | keyof AnalyticsService;
  inputs?: {
    name: string;
    label: string;
    type: 'number' | 'text' | 'select';
    defaultValue?: any;
    options?: { label: string; value: any }[];
  }[];
  execute?: (advancedApi: AdvancedAnalyticsService, api: AnalyticsService, inputs: any) => Observable<any>;
}

@Component({
  selector: 'app-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stats.html',
  styleUrl: './stats.scss'
})
export class StatsComponent implements OnInit {
  private advancedApi = inject(AdvancedAnalyticsService);
  private api = inject(AnalyticsService);

  public availableYears = [2026, 2025, 2024, 2023, 2022];

  public queries: QueryDef[] = [
    // --- WATCHED BASIC ---
    {
      id: 'watched_by_year', category: 'Watched History', name: 'Movies by Release Year', description: 'Filter watched movies by their original release year.', type: 'list',
      inputs: [{ name: 'releaseYear', label: 'Release Year', type: 'number', defaultValue: 2023 }],
      execute: (adv, _, inputs) => adv.apiAnalyticsAdvancedWatchedByReleaseYearGet(inputs.releaseYear)
    },
    { id: 'watched_directors', category: 'Watched History', name: 'Watched Directors', description: 'List of all directors you have watched.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchedDirectorsGet() },
    { id: 'watched_actors', category: 'Watched History', name: 'Watched Actors', description: 'List of all actors you have watched.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchedActorsGet() },
    { id: 'watched_genres', category: 'Watched History', name: 'Watched Genres', description: 'Count of movies watched by genre.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchedGenresGet() },
    { id: 'watched_decades', category: 'Watched History', name: 'Watched Decades', description: 'Count of movies watched grouped by release decade.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchedDecadesGet() },
    
    // --- WATCHED ADVANCED ---
    { id: 'most_repeated_actor', category: 'Watched Insights', name: 'Most Repeated Actor', description: 'The actor you have seen the most.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchedMostRepeatedActorGet() },
    { id: 'most_watched_director', category: 'Watched Insights', name: 'Most Watched Director', description: 'The director you have watched the most.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchedMostWatchedDirectorGet() },
    { id: 'predominant_era', category: 'Watched Insights', name: 'Predominant Era', description: 'Your preference between classic and modern cinema.', type: 'comparison', execute: (adv) => adv.apiAnalyticsAdvancedWatchedPredominantEraGet() },
    { id: 'director_ranking', category: 'Watched Insights', name: 'Director Ranking by Rating', description: 'Directors ranked by your average rating.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchedDirectorRankingGet() },
    {
      id: 'total_time', category: 'Watched Insights', name: 'Total Time Invested', description: 'Calculate total time spent watching a specific director or genre.', type: 'single',
      inputs: [
        { name: 'filterType', label: 'Filter Type', type: 'select', defaultValue: 'Director', options: [{label: 'Director', value: 'Director'}, {label: 'Genre', value: 'Genre'}] },
        { name: 'filterName', label: 'Name', type: 'text', defaultValue: 'Christopher Nolan' }
      ],
      execute: (adv, _, inputs) => adv.apiAnalyticsAdvancedWatchedTotalTimeGet(inputs.filterType, inputs.filterName)
    },

    // --- WATCHED CORRELATIONS ---
    { id: 'preferred_day', category: 'Habits & Correlations', name: 'Preferred Watch Day', description: 'Days of the week you watch the most movies.', type: 'chart', execute: (adv) => adv.apiAnalyticsAdvancedWatchedPreferredDayGet() },
    {
      id: 'rating_evolution', category: 'Habits & Correlations', name: 'Rating Evolution', description: 'How your ratings evolve throughout the year.', type: 'chart',
      inputs: [{ name: 'year', label: 'Year', type: 'number', defaultValue: new Date().getFullYear() }],
      execute: (adv, _, inputs) => adv.apiAnalyticsAdvancedWatchedRatingEvolutionGet(inputs.year)
    },
    { id: 'genre_streaks', category: 'Habits & Correlations', name: 'Genre Streaks', description: 'Longest streaks watching the same genre consecutively.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchedGenreStreaksGet() },
    { id: 'longest_movie', category: 'Habits & Correlations', name: 'Longest Movie', description: 'The longest movie you have ever watched.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchedLongestMovieGet() },
    { id: 'casting_reps', category: 'Habits & Correlations', name: 'Casting Repetitions', description: 'Pairs of actors you have seen together the most.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchedCastingRepetitionsGet() },

    // --- WATCHLIST BASIC ---
    {
      id: 'watchlist_by_year', category: 'Watchlist', name: 'Watchlist by Release Year', description: 'Pending movies from a specific release year.', type: 'list',
      inputs: [{ name: 'releaseYear', label: 'Release Year', type: 'number', defaultValue: 2023 }],
      execute: (adv, _, inputs) => adv.apiAnalyticsAdvancedWatchlistByReleaseYearGet(inputs.releaseYear)
    },
    { id: 'watchlist_directors', category: 'Watchlist', name: 'Watchlist Directors', description: 'Directors in your watchlist.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistDirectorsGet() },
    { id: 'watchlist_actors', category: 'Watchlist', name: 'Watchlist Actors', description: 'Actors in your watchlist.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistActorsGet() },
    { id: 'watchlist_genres', category: 'Watchlist', name: 'Watchlist Genres', description: 'Genres in your watchlist.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistGenresGet() },
    { id: 'watchlist_decades', category: 'Watchlist', name: 'Watchlist Decades', description: 'Decades in your watchlist.', type: 'chart', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistDecadesGet() },

    // --- WATCHLIST ADVANCED ---
    { id: 'most_anticipated_director', category: 'Watchlist Insights', name: 'Most Anticipated Director', description: 'Director with the most pending movies.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistMostAnticipatedDirectorGet() },
    { id: 'most_anticipated_actor', category: 'Watchlist Insights', name: 'Most Anticipated Actor', description: 'Actor with the most pending movies.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistMostAnticipatedActorGet() },
    { id: 'total_pending_watchtime', category: 'Watchlist Insights', name: 'Total Pending Watchtime', description: 'Total time required to clear your watchlist.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistTotalWatchtimeGet() },
    { id: 'oldest_pending', category: 'Watchlist Insights', name: 'Oldest Pending Movie', description: 'The movie waiting the longest in your watchlist.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistOldestPendingGet() },

    // --- WATCHLIST CORRELATIONS ---
    { id: 'watchlist_by_era', category: 'Watchlist Insights', name: 'Watchlist by Era', description: 'Pending classic vs modern movies.', type: 'comparison', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistByEraGet() },
    { id: 'ghost_actor', category: 'Watchlist Insights', name: 'Ghost Actor', description: 'Actor in many pending movies but 0 watched.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistGhostActorGet() },
    { id: 'golden_director', category: 'Watchlist Insights', name: 'Golden Pending Director', description: 'Director in watchlist with the best average rating.', type: 'single', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistGoldenDirectorGet() },
    { id: 'duration_balance', category: 'Watchlist Insights', name: 'Duration Balance', description: 'Balance between short, medium, and long pending movies.', type: 'chart', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistDurationBalanceGet() },
    { id: 'genre_proportion', category: 'Watchlist Insights', name: 'Genre Proportion', description: 'Watchlist vs Watched genres.', type: 'chart', execute: (adv) => adv.apiAnalyticsAdvancedWatchlistGenreProportionGet() },

    // --- BONUS ---
    { id: 'weekend_warrior', category: 'Bonus', name: 'Weekend Warrior', description: 'Compare your weekend vs weekday watching habits.', type: 'comparison', execute: (adv) => adv.apiAnalyticsAdvancedBonusWeekendWarriorGet() },
    { id: 'hidden_gems', category: 'Bonus', name: 'Hidden Gems', description: 'Movies you rated highly that are generally unknown.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedBonusHiddenGemsGet() },
    { id: 'watchlist_graveyard', category: 'Bonus', name: 'Watchlist Graveyard', description: 'Movies in your watchlist for the longest time.', type: 'list', execute: (adv) => adv.apiAnalyticsAdvancedBonusWatchlistGraveyardGet() },
    { id: 'cinematic_fatigue', category: 'Bonus', name: 'Cinematic Fatigue', description: 'Average rating when watching 1 vs 3+ movies a day.', type: 'comparison', execute: (adv) => adv.apiAnalyticsAdvancedBonusCinematicFatigueGet() },
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
  public inputValues = signal<any>({});
  
  public resultData = signal<any>(null);
  public loading = signal<boolean>(false);

  ngOnInit() {
    this.initInputs();
    this.runQuery();
  }

  onCategoryChange(cat: string) {
    this.selectedCategory.set(cat);
    const firstQuery = this.queriesInCategory()[0];
    if (firstQuery) {
      this.selectedQueryId.set(firstQuery.id);
      this.initInputs();
      this.resultData.set(null); // Clear previous
      this.runQuery();
    }
  }

  onQueryChange() {
    this.initInputs();
    this.resultData.set(null);
    this.runQuery();
  }

  private initInputs() {
    const q = this.currentQuery();
    const vals: any = {};
    if (q && q.inputs) {
      q.inputs.forEach(i => vals[i.name] = i.defaultValue);
    }
    this.inputValues.set(vals);
  }

  updateInput(key: string, value: any) {
    this.inputValues.update(v => ({ ...v, [key]: value }));
  }

  runQuery() {
    const q = this.currentQuery();
    if (!q || !q.execute) return;

    this.loading.set(true);
    q.execute(this.advancedApi, this.api, this.inputValues()).pipe(
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

  isSingle(val: any): boolean {
    return val && !Array.isArray(val) && typeof val === 'object';
  }
}
