import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, forkJoin, map } from 'rxjs';


// ── DTO interfaces ────────────────────────────────────────────────────────────

export interface WrappedMovie {
  id: string;
  title: string;
  releaseYear?: number;
  posterPath?: string;
  runtimeMinutes?: number;
  rating?: number;
  liked?: boolean;
}

export interface BookendsDto {
  openingScene?: WrappedMovie;
  fadeToBlack?: WrappedMovie;
}

export interface MonthlyExtremeDto {
  month: number;
  monthName: string;
  bestMovie?: WrappedMovie;
  worstMovie?: WrappedMovie;
}

export interface TopBottomMoviesDto {
  topRated: WrappedMovie[];
  bottomRated: WrappedMovie[];
}

export interface MostRewatchedDto {
  title: string;
  posterPath?: string;
  releaseYear?: number;
  rewatchCount: number;
}

export interface DavidAndGoliathDto {
  shortest?: WrappedMovie;
  longest?: WrappedMovie;
}

export interface RookieDto {
  name: string;
  moviesWatchedThisYear: number;
  averageRating: number;
}

export interface BestRookiesDto {
  newDirectors: RookieDto[];
  newActors: RookieDto[];
}

export interface GenreWithRatingDto {
  genreName: string;
  count: number;
  averageRating: number;
}

export interface DirectorActorPairDto {
  directorName: string;
  actorName: string;
  collaborationCount: number;
}

export interface PrimeTimeStatsDto {
  peakDay?: string;
  peakDayCount: number;
  peakMonth?: string;
  peakMonthCount: number;
  slumpDay?: string;
  slumpDayCount: number;
  slumpMonth?: string;
  slumpMonthCount: number;
}

export interface CinematicFatigueExpandedDto {
  avgRatingLightDays: number;
  avgRatingHeavyDays: number;
  slumpDay?: string;
  slumpDayWatchCount: number;
  slumpMonth?: string;
  slumpMonthWatchCount: number;
}

export interface WeekendWarriorDto {
  weekendWatches: number;
  weekdayWatches: number;
}

export interface FinalCutData {
  primeTime?: PrimeTimeStatsDto;
  cinemaFatigue?: CinematicFatigueExpandedDto;
  weekendWarrior?: WeekendWarriorDto;
  genreLandscape: GenreWithRatingDto[];
  castingPairs: any[];
  directorActorPairs: DirectorActorPairDto[];
  bestRookies?: BestRookiesDto;
  bookends?: BookendsDto;
  davidAndGoliath?: DavidAndGoliathDto;
  monthlyExtremes: MonthlyExtremeDto[];
  mostRewatched?: MostRewatchedDto;
  topBottom?: TopBottomMoviesDto;
  hiddenGems: any[];
  ratingEvolution: any[];
  genreStreaks: any[];
  directors: any[];
  actors: any[];
  decadeBreakdown: any[];
  eraBreakdown?: any;
}

// ─────────────────────────────────────────────────────────────────────────────

import { BASE_PATH } from '../api/variables';

@Injectable({ providedIn: 'root' })
export class FinalCutService {
  private http = inject(HttpClient);
  private basePath = inject(BASE_PATH, { optional: true });

  private get base(): string {
    return this.basePath ?? '';
  }

  private get<T>(path: string, params?: Record<string, any>): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      for (const [k, v] of Object.entries(params)) {
        if (v !== undefined && v !== null) {
          httpParams = httpParams.set(k, String(v));
        }
      }
    }
    return this.http.get<T>(`${this.base}${path}`, { params: httpParams });
  }

  loadAllData(year: number): Observable<FinalCutData> {
    return forkJoin({
      primeTime: this.get<PrimeTimeStatsDto>('/api/analytics/advanced/final-cut/prime-time', { year }),
      cinemaFatigue: this.get<CinematicFatigueExpandedDto>('/api/analytics/advanced/bonus/cinematic-fatigue', { year }),
      weekendWarrior: this.get<WeekendWarriorDto>('/api/analytics/advanced/bonus/weekend-warrior', { year }),
      genreLandscape: this.get<GenreWithRatingDto[]>('/api/analytics/advanced/final-cut/genre-landscape', { year }),
      castingPairs: this.get<any[]>('/api/analytics/advanced/watched/casting-repetitions', { year }),
      directorActorPairs: this.get<DirectorActorPairDto[]>('/api/analytics/advanced/final-cut/director-actor-pairs', { year }),
      bestRookies: this.get<BestRookiesDto>('/api/analytics/advanced/final-cut/best-rookies', { year }),
      bookends: this.get<BookendsDto>('/api/analytics/advanced/final-cut/bookends', { year }),
      longestMovie: this.get<WrappedMovie>('/api/analytics/advanced/watched/longest-movie', { year }),
      shortestMovie: this.get<WrappedMovie>('/api/analytics/advanced/final-cut/shortest-movie', { year }),
      monthlyExtremes: this.get<MonthlyExtremeDto[]>('/api/analytics/advanced/final-cut/monthly-extremes', { year }),
      mostRewatched: this.get<MostRewatchedDto>('/api/analytics/advanced/final-cut/most-rewatched', { year }),
      topBottom: this.get<TopBottomMoviesDto>('/api/analytics/advanced/final-cut/top-bottom-rated', { year }),
      hiddenGems: this.get<any[]>('/api/analytics/advanced/bonus/hidden-gems', { year }),
      ratingEvolution: this.get<any[]>('/api/analytics/advanced/watched/rating-evolution', { year }),
      genreStreaks: this.get<any[]>('/api/analytics/advanced/watched/genre-streaks', { year }),
      directors: this.get<any[]>('/api/analytics/advanced/watched/directors', { year }),
      actors: this.get<any[]>('/api/analytics/advanced/watched/actors', { year }),
      decadeBreakdown: this.get<any[]>('/api/analytics/advanced/watched/decades', { year }),
      eraBreakdown: this.get<any>('/api/analytics/advanced/watched/predominant-era', { year }),
    }).pipe(
      map(r => ({
        primeTime: r.primeTime,
        cinemaFatigue: r.cinemaFatigue,
        weekendWarrior: r.weekendWarrior,
        genreLandscape: r.genreLandscape ?? [],
        castingPairs: r.castingPairs ?? [],
        directorActorPairs: r.directorActorPairs ?? [],
        bestRookies: r.bestRookies,
        bookends: r.bookends,
        davidAndGoliath: { shortest: r.shortestMovie, longest: r.longestMovie },
        monthlyExtremes: r.monthlyExtremes ?? [],
        mostRewatched: r.mostRewatched,
        topBottom: r.topBottom,
        hiddenGems: r.hiddenGems ?? [],
        ratingEvolution: r.ratingEvolution ?? [],
        genreStreaks: r.genreStreaks ?? [],
        directors: r.directors ?? [],
        actors: r.actors ?? [],
        decadeBreakdown: r.decadeBreakdown ?? [],
        eraBreakdown: r.eraBreakdown,
      }))
    );
  }
}
