import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RecommendationsService } from '../../core/api/api/recommendations.service';
import { RecommendationRequest, RecommendedMovieDto } from '../../core/api';
import { finalize } from 'rxjs';

export enum Strategy {
  RecentMood = 0,
  OppositeMood = 1,
  ComfortZoneDisruptor = 2,
  GuiltyPleasure = 3,
  CinephileElite = 4,
  DirectorsTrajectory = 5,
  RuntimeContext = 6,
  PureRandom = 7
}

export enum Scope {
  WatchlistOnly = 0,
  DatabaseOnly = 1,
  Hybrid = 2
}

interface StrategyInfo {
  id: Strategy;
  name: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-recommendations',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './recommendations.html',
  styleUrl: './recommendations.scss'
})
export class RecommendationsComponent implements OnInit {
  private recoService = inject(RecommendationsService);

  // Form State
  public selectedStrategy = signal<Strategy>(Strategy.RecentMood);
  public selectedScope = signal<Scope>(Scope.Hybrid);
  public selectedQuantity = signal<number>(3);
  public maxRuntime = signal<number>(120);
  public filterByRuntime = signal<boolean>(false);

  // Result State
  public recommendations = signal<RecommendedMovieDto[]>([]);
  public loading = signal<boolean>(false);
  public actionLoading = signal<string | null>(null); // Movie ID of card performing action
  public error = signal<string | null>(null);

  // Strategy list with descriptions
  public strategies: StrategyInfo[] = [
    {
      id: Strategy.RecentMood,
      name: 'Recent Mood',
      icon: '🧠',
      description: 'Aligns with your recent history. Analyzes genres, runtimes, and eras of your last 10 watches to find similar matches.'
    },
    {
      id: Strategy.OppositeMood,
      name: 'Opposite Mood',
      icon: '🔄',
      description: 'The perfect palette cleanser. Suggests styles and paces that are the exact mathematical inverse of your recent watches.'
    },
    {
      id: Strategy.ComfortZoneDisruptor,
      name: 'Comfort Disruptor',
      icon: '🚀',
      description: 'Pushes your boundaries. Selects unwatched genres/eras but anchors them with directors or actors you have rated highly.'
    },
    {
      id: Strategy.CinephileElite,
      name: 'Cinephile Elite',
      icon: '🏆',
      description: 'Filters for critically acclaimed masterpieces (average score ≥ 8.2) that have low mainstream popularity.'
    },
    {
      id: Strategy.GuiltyPleasure,
      name: 'Guilty Pleasure',
      icon: '🍿',
      description: 'Finds obscure, low-popularity movies in sub-genres you historically rate higher than the global average.'
    },
    {
      id: Strategy.DirectorsTrajectory,
      name: 'Director\'s Path',
      icon: '🎬',
      description: 'Finds unseen works from filmmakers where you have watched at least 2 of their films, organizing them chronologically.'
    },
    {
      id: Strategy.RuntimeContext,
      name: 'Runtime Matcher',
      icon: '⏱️',
      description: 'Tailors picks to your exact time budget, prioritizing high-tempo pacing and genres for shorter runtimes.'
    },
    {
      id: Strategy.PureRandom,
      name: 'Pure Chance',
      icon: '🎲',
      description: 'Pure randomness within the given scope. A completely random selection to let chance guide your night.'
    }
  ];

  ngOnInit() {
    this.generateRecommendations();
  }

  selectStrategy(strat: Strategy) {
    this.selectedStrategy.set(strat);
    if (strat === Strategy.RuntimeContext) {
      this.filterByRuntime.set(true);
    }
  }

  selectScope(scope: Scope) {
    this.selectedScope.set(scope);
  }

  selectQuantity(qty: number) {
    this.selectedQuantity.set(qty);
  }

  generateRecommendations() {
    this.loading.set(true);
    this.error.set(null);

    const requestPayload: RecommendationRequest = {
      strategy: this.selectedStrategy(),
      scope: this.selectedScope(),
      quantity: this.selectedQuantity(),
      maxRuntimeMinutes: this.filterByRuntime() ? this.maxRuntime() : null
    };

    // We cast to any because angular client generator typed them as numbers, which perfectly matches the local enums
    this.recoService.apiV1RecommendationsGeneratePost(requestPayload as any)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.recommendations.set(res || []);
        },
        error: (err) => {
          console.error('Failed to generate recommendations', err);
          this.error.set('Failed to generate recommendations. Please try uploading an import first to populate your history.');
        }
      });
  }

  skipMovie(movieId: string) {
    this.actionLoading.set(movieId);
    
    this.recoService.apiV1RecommendationsSkipMovieIdPost(movieId)
      .pipe(finalize(() => this.actionLoading.set(null)))
      .subscribe({
        next: () => {
          // Remove movie from local list
          this.recommendations.update(current => current.filter(m => m.movieId !== movieId));
          
          // If list is empty, generate new ones automatically
          if (this.recommendations().length === 0) {
            this.generateRecommendations();
          }
        },
        error: (err) => {
          console.error('Failed to skip movie', err);
        }
      });
  }

  markAsWatched(movie: RecommendedMovieDto) {
    // In a full implementation this would open a log modal or send a watched entry.
    // For this client interaction, we skip the movie to cache it and show a success indicator.
    this.skipMovie(movie.movieId);
  }
}
