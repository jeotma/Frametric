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

  logForm = this.fb.group({
    dateWatched: [new Date().toISOString().split('T')[0], Validators.required],
    rating: [null as number | null, [Validators.min(0), Validators.max(10)]],
    isRewatch: [false]
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadMovie(id);
    }
  }

  loadMovie(id: string) {
    this.moviesService.apiMoviesIdGet(id).subscribe({
      next: (data) => {
        this.movie.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
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
      rating: val.rating,
      isRewatch: val.isRewatch
    };

    const id = this.movie()?.id;
    if (!id) return;

    this.isLogging.set(true);
    this.moviesService.apiMoviesIdLogPost(id, request as any).subscribe({
      next: () => {
        this.isLogging.set(false);
        this.showLogForm.set(false);
        this.loadMovie(id); // Reload to show new diary entry
      },
      error: () => {
        this.isLogging.set(false);
      }
    });
  }

  unlogWatch(entryId: string) {
    const id = this.movie()?.id;
    if (!id) return;

    if (!confirm('¿Eliminar este registro de visionado?')) return;

    this.unloggingEntryId.set(entryId);
    this.moviesService.apiMoviesIdLogEntryIdDelete(id, entryId).subscribe({
      next: () => {
        this.unloggingEntryId.set(null);
        this.loadMovie(id);
      },
      error: () => {
        this.unloggingEntryId.set(null);
      }
    });
  }

  getRatingArray(score: number): number[] {
    const arr = [];
    for (let i = 1; i <= 5; i++) {
      arr.push(i <= score / 2 ? 1 : 0);
    }
    return arr;
  }
}

