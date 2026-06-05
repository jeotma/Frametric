import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DirectorsService, DirectorDetailsDto } from '../../../core/api';
import { slugify } from '../../../core/utils/slugify';

@Component({
  selector: 'app-director-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './director-detail.html',
  styleUrl: './director-detail.scss'
})
export class DirectorDetailComponent implements OnInit {
  protected readonly slugify = slugify;
  private route = inject(ActivatedRoute);
  private directorsService = inject(DirectorsService);

  director = signal<DirectorDetailsDto | null>(null);
  isLoading = signal(true);

  get watchedMovies() {
    const dir = this.director();
    return dir ? dir.movies.filter(m => m.isWatched) : [];
  }

  get unwatchedMovies() {
    const dir = this.director();
    return dir ? dir.movies.filter(m => !m.isWatched) : [];
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.directorsService.apiDirectorsIdGet(id).subscribe({
        next: (data) => {
          this.director.set(data);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        }
      });
    }
  }
}
