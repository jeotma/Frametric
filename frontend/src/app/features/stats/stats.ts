import { Component, OnDestroy, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, Subject } from 'rxjs';
import { finalize, first, map, switchMap, takeUntil } from 'rxjs/operators';
import { AdvancedAnalyticsService } from '../../core/api/api/advanced-analytics.service';
import { AnalyticsService } from '../../core/api/api/analytics.service';
import { SearchService } from '../../core/api/api/search.service';
import { DirectorsService } from '../../core/api/api/directors.service';
import { ActorsService } from '../../core/api/api/actors.service';
import { EasterEggPipe } from '../../core/services/easter-egg.pipe';
import { slugify } from '../../core/utils/slugify';
import { STATS_QUERIES, GlobalFilters, QueryDef } from './stats-queries';
import { GlobalSearchResultDto } from '../../core/api/model/global-search-result-dto';

@Component({
  selector: 'app-stats',
  standalone: true,
  imports: [CommonModule, FormsModule, EasterEggPipe, RouterLink],
  templateUrl: './stats.html',
  styleUrl: './stats.scss'
})
export class StatsComponent implements OnInit, OnDestroy {
  protected readonly slugify = slugify;
  private advancedApi = inject(AdvancedAnalyticsService);
  private api = inject(AnalyticsService);
  private searchService = inject(SearchService);
  private directorsService = inject(DirectorsService);
  private actorsService = inject(ActorsService);
  private router = inject(Router);

  public isPretentious = signal<boolean>(false);
  public baconMessage = signal<string | null>(null);
  public navigationError = signal<string | null>(null);
  public toxicDirectors = signal<Set<string>>(new Set<string>());

  public globalFilters: GlobalFilters = {};

  public queries = STATS_QUERIES;

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

  private destroy$ = new Subject<void>();

  private readonly STORAGE_KEY = 'frametric_stats_state';

  public resultData = signal<any>(null);
  public loading = signal<boolean>(false);

  // Layout & Pagination State
  public showPosters = signal<boolean>(true);
  public currentPage = signal<number>(1);
  public pageSize = signal<number>(25);
  public readonly pageSizeOptions = [10, 25, 50, 100];

  public totalPages = computed(() => {
    const data = this.resultData();
    if (!this.isArray(data)) return 0;
    return Math.ceil(data.length / this.pageSize());
  });

  public paginatedResultData = computed(() => {
    const data = this.sortedResultData();
    if (!this.isArray(data)) return data;
    const startIndex = (this.currentPage() - 1) * this.pageSize();
    return data.slice(startIndex, startIndex + this.pageSize());
  });

  hasPosterField(): boolean {
    const data = this.resultData();
    return this.isArray(data) && data.length > 0 && 'posterUrl' in data[0] && data[0].posterUrl !== undefined;
  }

  hasProfileField(): boolean {
    const data = this.resultData();
    return this.isArray(data) && data.length > 0 && 'profilePath' in data[0];
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  changePageSize(size: number) {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  ngOnInit() {
    if (!this.restoreState()) {
      this.initInputs();
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onCategoryChange(cat: string) {
    this.selectedCategory.set(cat);
    const firstQuery = this.queriesInCategory()[0];
    if (firstQuery) {
      this.selectedQueryId.set(firstQuery.id);
      this.initInputs();
      this.resultData.set(null); // Clear previous
      this.currentPage.set(1);
    }
  }

  onQueryChange() {
    this.initInputs();
    this.resultData.set(null);
    this.currentPage.set(1);
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
      if (!q.allowedFilters.includes('minCustomRating')) this.globalFilters.minCustomRating = undefined;
      if (!q.allowedFilters.includes('maxCustomRating')) this.globalFilters.maxCustomRating = undefined;
      if (!q.allowedFilters.includes('actor')) this.globalFilters.actor = undefined;
      if (!q.allowedFilters.includes('director')) this.globalFilters.director = undefined;
      if (!q.allowedFilters.includes('genre')) this.globalFilters.genre = undefined;
    }

    // Special case: enrich total_time for Director/Actor with profile data
    if (q.id === 'total_time' && (this.querySpecificInputs.filterType === 'Director' || this.querySpecificInputs.filterType === 'Actor') && this.querySpecificInputs.filterName) {
      this.runEnrichedTotalTimeQuery(q);
      return;
    }

    this.loading.set(true);
    q.execute(this.advancedApi, this.api, this.globalFilters, this.querySpecificInputs).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (res) => {
        this.resultData.set(res);
        this.evaluateEasterEggs(q.id, res);
      },
      error: (err) => {
        console.error('Query failed', err);
        this.resultData.set(null);
      }
    });
  }

  private runEnrichedTotalTimeQuery(q: QueryDef) {
    const name = this.querySpecificInputs.filterName;
    const filterType = this.querySpecificInputs.filterType as 'Director' | 'Actor';
    this.loading.set(true);

    this.searchService.apiSearchGet(name).pipe(
      switchMap(results => {
        const entity = filterType === 'Director'
          ? results.find(r => r.entityType === 'Director' || r.entityType === 'Director / Actor')
          : results.find(r => r.entityType === 'Actor' || r.entityType === 'Director / Actor');
        if (!entity?.localId) {
          return q.execute!(this.advancedApi, this.api, this.globalFilters, this.querySpecificInputs);
        }

        const detail$ = filterType === 'Director'
          ? this.directorsService.apiDirectorsIdGet(entity.localId)
          : this.actorsService.apiActorsIdGet(entity.localId);

        return forkJoin({
          detail: detail$,
          time: this.advancedApi.apiAnalyticsAdvancedWatchedTotalTimeGet(
            filterType, name,
            this.globalFilters.watchYear, this.globalFilters.releaseYear,
            this.globalFilters.minRating, this.globalFilters.maxRating,
            this.globalFilters.minCustomRating, this.globalFilters.maxCustomRating,
            this.globalFilters.genre, this.globalFilters.director, this.globalFilters.actor
          )
        }).pipe(
          map(({ detail, time }: { detail: any; time: any }) => {
            const movies = (detail.movies || []) as any[];
            return {
              name: detail.name,
              profilePath: detail.profilePath,
              count: detail.watchCount,
              averageRating: detail.averageRating / 2,
              totalMinutes: time.totalMinutes,
              totalHours: time.totalHours,
              entityId: detail.id,
              entityType: filterType.toLowerCase() as 'director' | 'actor',
              movies,
              watchedMovies: movies.filter((m: any) => m.isWatched),
              actorMovies: detail.actorMovies || null,
              directedMovies: detail.directedMovies || null,
              watchlistCount: detail.watchlistCount ?? detail.watchlistMovieTitles?.length ?? 0,
              watchlistMovieTitles: detail.watchlistMovieTitles || [],
              likedMovieTitles: detail.likedMovieTitles || []
            };
          })
        );
      }),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (res) => {
        this.resultData.set(res);
        this.evaluateEasterEggs(q.id, res);
      },
      error: (err) => {
        console.error('Query failed', err);
        this.resultData.set(null);
      }
    });
  }

  private evaluateEasterEggs(queryId: string, data: any) {
    if (!data || !Array.isArray(data)) return;

    // 1. Pretentiometer check
    if (queryId === 'watched_by_year') {
      const slowDirectors = ['tarkovsky', 'bergman', 'lav diaz', 'béla tarr', 'bela tarr', 'kiarostami'];
      const blockbusterKeywords = ['superhero', 'marvel', 'dc', 'action'];

      let slowCount = 0;
      let slowHighRating = 0;
      let blockbusterLowRating = 0;
      let blockbusterCount = 0;

      data.forEach((m: any) => {
        const title = (m.title || '').toLowerCase();
        const dir = (m.directorName || m.directors || '').toLowerCase();
        const keywords = (m.keywords || '').toLowerCase();
        const rating = m.userRating || m.rating || 0;

        if (slowDirectors.some(sd => dir.includes(sd))) {
          slowCount++;
          if (rating >= 4.0 || rating >= 8.0) { // Support 5-star and 10-star scales
            slowHighRating++;
          }
        }

        if (blockbusterKeywords.some(bk => keywords.includes(bk) || title.includes(bk))) {
          blockbusterCount++;
          if (rating > 0 && (rating <= 2.0 || rating <= 4.0)) {
            blockbusterLowRating++;
          }
        }
      });

      // Trigger if user watched high-rated arthouse & low-rated blockbusters
      if (slowHighRating >= 2 || (slowCount >= 2 && blockbusterLowRating >= 1)) {
        this.isPretentious.set(Math.random() * 100 < 3); // 3% probability
      } else {
        this.isPretentious.set(false);
      }
    }

    // 2. Toxic Director Check (Watched 5+ movies but average score < 2.0 / 4.0)
    if (queryId === 'watched_directors' || queryId === 'top_directors' || queryId === 'director_ranking') {
      const toxic = new Set<string>();
      data.forEach((d: any) => {
        const name = d.directorName || d.name;
        const count = d.count || d.watchCount || 0;
        const avg = d.averageRating || d.avgRating || 0;

        // Count threshold of 5 watches and avg rating <= 2.0
        if (count >= 5 && avg > 0 && avg <= 2.0) {
          if (Math.random() * 100 < 3) { // 3% probability
            toxic.add(name);
          }
        }
      });
      this.toxicDirectors.set(toxic);
    }
  }

  calculateBaconDistance(actorName: string) {
    const normalized = (actorName || '').trim().toLowerCase();
    if (normalized === 'kevin bacon') {
      this.baconMessage.set(`Degree of separation for Kevin Bacon to Kevin Bacon is 0 step(s). You are looking at the legend himself! 🥓`);
      return;
    }

    const isEE = Math.random() * 100 < 2; // 2% chance for the easter egg
    if (isEE) {
      this.baconMessage.set(`You are currently 2 steps away from Kevin Bacon. But more importantly, you are 0 steps away from avoiding your real-life responsibilities. Go watch a movie.`);
    } else {
      const degrees = Math.floor(Math.random() * 3) + 1;
      this.baconMessage.set(`Degree of separation for ${actorName} to Kevin Bacon is ${degrees} step(s) via standard co-star connections.`);
    }
  }

  closeBaconModal() {
    this.baconMessage.set(null);
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
    if ('totalMinutes' in data && data.totalMinutes === 0 && data.totalHours === 0) return true;
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

  public colNames = computed(() => {
    const data = this.resultData();
    if (!Array.isArray(data) || data.length === 0) return [];

    const allKeys = Object.keys(data[0]);

    return allKeys.filter(key =>
      key !== 'posterUrl' &&
      key !== 'profilePath' &&
      key.toLowerCase() !== 'id' &&
      !key.toLowerCase().endsWith('id') &&
      data.some((item: any) => {
        const val = item[key];
        return val !== 0 && val !== null && val !== undefined && val !== '';
      })
    );
  });

  formatColName(name: string): string {
    if (name === 'count') {
      return this.currentQuery()?.id?.includes('watchlist') ? 'Pending Count' : 'Watched Count';
    }

    if (name.toLowerCase().includes('rating')) {
      return 'Your Rating';
    }

    const result = name.replace(/([A-Z])/g, " $1");
    return result.charAt(0).toUpperCase() + result.slice(1);
  }

  isFilterAllowed(filterName: string): boolean {
    const q = this.currentQuery();
    if (!q || !q.allowedFilters) return true;
    return q.allowedFilters.includes(filterName);
  }

  private saveState() {
    const state = {
      selectedCategory: this.selectedCategory(),
      selectedQueryId: this.selectedQueryId(),
      globalFilters: this.globalFilters,
      querySpecificInputs: this.querySpecificInputs,
      currentPage: this.currentPage(),
      pageSize: this.pageSize(),
      showPosters: this.showPosters(),
      sortColumn: this.sortColumn(),
      sortDirection: this.sortDirection(),
      resultData: this.resultData()
    };
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
  }

  private restoreState(): boolean {
    const raw = sessionStorage.getItem(this.STORAGE_KEY);
    if (!raw) return false;
    try {
      const state = JSON.parse(raw);
      this.selectedCategory.set(state.selectedCategory ?? 'Watched History');
      this.selectedQueryId.set(state.selectedQueryId ?? 'watched_by_year');
      this.globalFilters = state.globalFilters ?? {};
      this.querySpecificInputs = state.querySpecificInputs ?? {};
      this.currentPage.set(state.currentPage ?? 1);
      this.pageSize.set(state.pageSize ?? 25);
      this.showPosters.set(state.showPosters ?? true);
      this.sortColumn.set(state.sortColumn ?? null);
      this.sortDirection.set(state.sortDirection ?? 'desc');
      this.resultData.set(state.resultData ?? null);
      sessionStorage.removeItem(this.STORAGE_KEY);
      return true;
    } catch {
      return false;
    }
  }

  private resolveEntityId(result: GlobalSearchResultDto, type: 'Movie' | 'Actor' | 'Director'): string | number | null {
    if (type === 'Actor') return result.actorId || result.localId || result.tmdbId;
    if (type === 'Director') return result.directorId || result.localId || result.tmdbId;
    return result.localId || result.tmdbId; // Movie
  }

  navigateToEntity(name: string, type: 'Movie' | 'Actor' | 'Director', fragment?: string) {
    if (!name) return;
    this.saveState();
    if (type === 'Movie') {
      this.navigateToMovie(name);
      return;
    }
    this.searchService.apiSearchGet(name).pipe(first(), takeUntil(this.destroy$)).subscribe({
      next: (results) => {
        const match = results.find(r => r.entityType === type || r.entityType === 'Director / Actor');
        if (match) {
          const id = this.resolveEntityId(match, type);
          if (id) {
            const slug = slugify(name);
            const route = type === 'Actor' ? '/actors' : '/directors';
            this.router.navigate([route, id, slug], fragment ? { fragment } : undefined);
            return;
          }
        }
        this.navigationError.set(`Could not find "${name}" in ${type}s.`);
        setTimeout(() => this.navigationError.set(null), 3000);
      }
    });
  }

  isToxic(name: string): boolean {
    return this.toxicDirectors().has(name);
  }

  splitEntityNames(value: string): string[] {
    if (!value) return [];
    return value.split(',').map(s => s.trim()).filter(s => s.length > 0);
  }

  getFirstEntityName(value: string): string {
    return this.splitEntityNames(value)[0] || value;
  }

  private navigateToMovie(name: string) {
    this.saveState();
    this.searchService.apiSearchGet(name).pipe(first(), takeUntil(this.destroy$)).subscribe({
      next: (results) => {
        const match = results.find(r => r.entityType === 'Movie');
        if (match) {
          const id = match.localId || match.tmdbId;
          if (id) {
            this.router.navigate(['/movies', id, slugify(name)]);
          }
        }
      }
    });
  }
}
