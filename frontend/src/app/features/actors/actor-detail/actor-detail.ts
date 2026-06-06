import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ActorsService, ActorDetailsDto } from '../../../core/api';
import { slugify } from '../../../core/utils/slugify';

@Component({
  selector: 'app-actor-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './actor-detail.html',
  styleUrl: './actor-detail.scss'
})
export class ActorDetailComponent implements OnInit {
  protected readonly slugify = slugify;
  private route = inject(ActivatedRoute);
  private actorsService = inject(ActorsService);

  actor = signal<ActorDetailsDto | null>(null);
  isLoading = signal(true);
  activeTooltip = signal<'watchlist' | 'liked' | null>(null);

  toggleTooltip(type: 'watchlist' | 'liked') {
    this.activeTooltip.update(current => current === type ? null : type);
  }

  closeTooltip() {
    this.activeTooltip.set(null);
  }

  get watchedActingMovies() {
    const act = this.actor();
    return act ? act.movies.filter(m => m.isWatched) : [];
  }

  get unwatchedActingMovies() {
    const act = this.actor();
    return act ? act.movies.filter(m => !m.isWatched) : [];
  }

  get watchedDirectedMovies() {
    const act = this.actor();
    return act && act.directedMovies ? act.directedMovies.filter(m => m.isWatched) : [];
  }

  get unwatchedDirectedMovies() {
    const act = this.actor();
    return act && act.directedMovies ? act.directedMovies.filter(m => !m.isWatched) : [];
  }

  get muralMovies() {
    const act = this.actor();
    if (!act) return [];
    const allMovies = [...act.movies];
    if (act.directedMovies) {
      allMovies.push(...act.directedMovies);
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
        this.actorsService.apiActorsIdGet(id).subscribe({
          next: (data) => {
            this.actor.set(data);
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
