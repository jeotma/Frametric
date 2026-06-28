import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MoviesService, MovieDetailsDto, TmdbCollectionResultDto } from '../../../core/api';
import { slugify } from '../../../core/utils/slugify';

@Component({
  selector: 'app-movie-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './movie-detail.html',
  styleUrl: './movie-detail.scss'
})
export class MovieDetailComponent implements OnInit, OnDestroy {
  public isTogglingWatchlist = signal<boolean>(false);
  protected readonly slugify = slugify;
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private moviesService = inject(MoviesService);
  private fb = inject(FormBuilder);
  private destroy$ = new Subject<void>();

  movie = signal<MovieDetailsDto | null>(null);
  isLoading = signal(true);
  isLogging = signal(false);
  showLogForm = signal(false);
  unloggingEntryId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  selectedRating = signal<number>(0);
  collection = signal<TmdbCollectionResultDto | null>(null);
  collectionLoading = signal(false);
  collectionExpanded = signal(false);

  logForm = this.fb.group({
    dateWatched: [new Date().toISOString().split('T')[0], Validators.required],
    isRewatch: [false]
  });

  setRating(stars: number) {
    const current = this.selectedRating();
    if (current === stars) {
      this.selectedRating.set(stars - 0.5);
    } else {
      this.selectedRating.set(stars);
    }
  }

  ngOnInit() {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isLoading.set(true);
        this.loadMovie(id);
      }
    });
  }

  loadMovie(id: string) {
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    if (!guidRegex.test(id)) {
      const tmdbId = parseInt(id, 10);
      if (isNaN(tmdbId)) {
        this.isLoading.set(false);
        this.errorMessage.set('Invalid movie identifier.');
        return;
      }
      this.moviesService.apiMoviesEnrichFromTmdbPost({ tmdbId }).pipe(takeUntil(this.destroy$)).subscribe({
        next: (enrichedMovie) => {
          if (enrichedMovie && enrichedMovie.id) {
            this.router.navigate(['/movies', enrichedMovie.id], { replaceUrl: true }).then(() => {
              this.loadMovie(enrichedMovie.id!);
            });
          } else {
            this.isLoading.set(false);
            this.errorMessage.set('Failed to enrich movie from TMDB.');
          }
        },
        error: (err) => {
          this.isLoading.set(false);
          this.errorMessage.set(err?.error?.title || err?.message || 'Failed to enrich movie details.');
        }
      });
      return;
    }

    this.moviesService.apiMoviesIdGet(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.movie.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err?.error?.title || err?.message || 'Failed to load movie details.');
      }
    });
  }

  loadCollection(): void {
    const movieId = this.movie()?.id;
    if (!movieId) return;
    this.collectionLoading.set(true);
    this.collectionExpanded.set(true);
    this.moviesService.apiMoviesGetCollection(movieId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.collection.set(data);
        this.collectionLoading.set(false);
      },
      error: () => {
        this.collectionLoading.set(false);
        this.collection.set(null);
      }
    });
  }

  toggleCollection(): void {
    if (this.collectionExpanded()) {
      this.collectionExpanded.set(false);
      return;
    }
    if (!this.collection()) {
      this.loadCollection();
    } else {
      this.collectionExpanded.set(true);
    }
  }

  toggleLogForm() {
    this.showLogForm.update(v => !v);
  }

  submitLog() {
    if (this.logForm.invalid) return;

    const val = this.logForm.value;
    const request = {
      dateWatched: val.dateWatched,
      rating: this.selectedRating() > 0 ? this.selectedRating() / 2 : null,
      isRewatch: val.isRewatch
    };

    const id = this.movie()?.id;
    if (!id) return;

    this.isLogging.set(true);
    this.errorMessage.set(null);
    this.moviesService.apiMoviesIdLogPost(id, request as any).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isLogging.set(false);
        this.showLogForm.set(false);
        this.loadMovie(id);
      },
      error: (err) => {
        this.isLogging.set(false);
        this.errorMessage.set(err?.error?.title || err?.message || 'Failed to log watch. Please try again.');
      }
    });
  }

  unlogWatch(entryId: string) {
    const id = this.movie()?.id;
    if (!id) return;

    if (!confirm('Â¿Eliminar este registro de visionado?')) return;

    this.unloggingEntryId.set(entryId);
    this.errorMessage.set(null);
    this.moviesService.apiMoviesIdLogEntryIdDelete(id, entryId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.unloggingEntryId.set(null);
        this.loadMovie(id);
      },
      error: (err) => {
        this.unloggingEntryId.set(null);
        this.errorMessage.set(err?.error?.title || err?.message || 'Failed to delete entry. Please try again.');
      }
    });
  }

  isLogged(m: MovieDetailsDto): boolean {
    return m.isWatched || (m.diaryEntries?.length > 0);
  }
  public toggleWatchlist() {
    if (!this.movie() || !this.movie()!.id) return;
    
    const movieId = this.movie()!.id!;
    const isCurrentlyInWatchlist = this.movie()!.isInWatchlist;
    this.isTogglingWatchlist.set(true);

    if (isCurrentlyInWatchlist) {
      this.moviesService.apiMoviesIdWatchlistDelete(movieId)
        .pipe(takeUntil(this.destroy$), finalize(() => this.isTogglingWatchlist.set(false)))
        .subscribe({
          next: () => {
            this.movie.update(m => m ? { ...m, isInWatchlist: false } : null);
          },
          error: (err) => {
            console.error('Failed to remove from watchlist', err);
            this.errorMessage.set('Could not remove from watchlist.');
          }
        });
    } else {
      this.moviesService.apiMoviesIdWatchlistPost(movieId)
        .pipe(takeUntil(this.destroy$), finalize(() => this.isTogglingWatchlist.set(false)))
        .subscribe({
          next: () => {
            this.movie.update(m => m ? { ...m, isInWatchlist: true } : null);
          },
          error: (err) => {
            console.error('Failed to add to watchlist', err);
            this.errorMessage.set('Could not add to watchlist.');
          }
        });
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}


