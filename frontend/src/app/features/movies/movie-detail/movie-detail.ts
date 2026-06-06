import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MoviesService, MovieDetailsDto } from '../../../core/api';
import { slugify } from '../../../core/utils/slugify';

@Component({
  selector: 'app-movie-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './movie-detail.html',
  styleUrl: './movie-detail.scss'
})
export class MovieDetailComponent implements OnInit {
  protected readonly slugify = slugify;
  private route = inject(ActivatedRoute);
  private moviesService = inject(MoviesService);
  private fb = inject(FormBuilder);

  movie = signal<MovieDetailsDto | null>(null);
  isLoading = signal(true);
  isLogging = signal(false);
  showLogForm = signal(false);
  unloggingEntryId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  selectedRating = signal<number>(0);

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
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isLoading.set(true);
        this.loadMovie(id);
      }
    });
  }

  loadMovie(id: string) {
    this.moviesService.apiMoviesIdGet(id).subscribe({
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
    this.moviesService.apiMoviesIdLogPost(id, request as any).subscribe({
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

    if (!confirm('¿Eliminar este registro de visionado?')) return;

    this.unloggingEntryId.set(entryId);
    this.errorMessage.set(null);
    this.moviesService.apiMoviesIdLogEntryIdDelete(id, entryId).subscribe({
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
}

