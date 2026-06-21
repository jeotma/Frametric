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
  activeTooltip = signal<'watchlist' | 'liked' | null>(null);

  toggleTooltip(type: 'watchlist' | 'liked') {
    this.activeTooltip.update(current => current === type ? null : type);
  }

  closeTooltip() {
    this.activeTooltip.set(null);
  }

  get watchedDirectedMovies() {
    const dir = this.director();
    return dir && dir.movies ? dir.movies.filter(m => m.isWatched) : [];
  }

  get unwatchedDirectedMovies() {
    const dir = this.director();
    return dir && dir.movies ? dir.movies.filter(m => !m.isWatched) : [];
  }

  get watchedActingMovies() {
    const dir = this.director();
    return dir && dir.actorMovies ? dir.actorMovies.filter(m => m.isWatched) : [];
  }

  get unwatchedActingMovies() {
    const dir = this.director();
    return dir && dir.actorMovies ? dir.actorMovies.filter(m => !m.isWatched) : [];
  }

  get muralMovies() {
    const dir = this.director();
    if (!dir) return [];
    const allMovies = [...(dir.movies || [])];
    if (dir.actorMovies) {
      allMovies.push(...dir.actorMovies);
    }
    const moviesWithPosters = allMovies.filter(m => m.posterPath && m.posterPath.trim() !== '' && m.posterPath !== 'null');
    if (moviesWithPosters.length === 0) return [];
    
    const uniqueMovies = Array.from(new Map(moviesWithPosters.map(m => [m.id, m])).values());
    const result = [...uniqueMovies];
    while (result.length < 200) {
      result.push(...uniqueMovies);
    }
    return result;
  }

  scrollToSection(sectionId: string) {
    const el = document.getElementById(sectionId);
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isLoading.set(true);
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
    });
  }
}
