import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { TopBottomMoviesDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-hall-of-fame-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content hof-bg">
      <div class="act-label">Act IV · The Climax</div>
      <h2 class="slide-title">The Hall of Fame.</h2>
      <p class="slide-subtitle">Your top rated masterpieces of {{ year }}.</p>

      <div class="top5-grid" *ngIf="data?.topRated?.length">
        <div *ngFor="let m of data!.topRated; let i = index" class="hof-card" [class.number-one]="i === 0">
          <div class="hof-rank">#{{ i + 1 }}</div>
          <div class="poster-wrap">
            <img *ngIf="m.posterPath" [src]="posterUrl(m.posterPath)" [alt]="m.title" class="hof-poster">
          </div>
          <div class="hof-info">
            <span class="hof-title">{{ m.title }}</span>
            <span class="hof-rating">⭐ {{ m.rating | number:'1.1-1' }} <span *ngIf="m.liked" class="heart">❤️</span></span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data?.topRated?.length">No top rated movies found.</p>
      
      <div class="tie-breaker-note" *ngIf="data?.topRated?.length">
        * Films ordered by highest rating, favor, and ultimately, chance.
      </div>
    </div>
  `,
  styles: [`
    .hof-bg {
      background: radial-gradient(ellipse at 50% 40%, rgba(52, 211, 153, 0.1) 0%, transparent 60%);
    }
    .top5-grid {
      display: flex;
      align-items: flex-end;
      justify-content: center;
      gap: 16px;
      width: 100%;
      max-width: 880px;
      margin-top: 40px;
    }
    .hof-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
      flex: 1;
      transition: transform 0.3s ease;
    }
    .hof-card:hover { transform: translateY(-10px); }
    .number-one {
      transform: scale(1.15) translateY(-20px);
      z-index: 10;
    }
    .number-one:hover { transform: scale(1.15) translateY(-30px); }
    
    .hof-rank {
      font-size: 1.1rem;
      font-weight: 800;
      color: var(--text-muted);
    }
    .number-one .hof-rank { color: #fbbf24; font-size: 1.3rem; }
    
    .poster-wrap {
      width: 100%;
      aspect-ratio: 2/3;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 8px 24px rgba(0,0,0,0.4);
      border: 1px solid rgba(255,255,255,0.1);
    }
    .number-one .poster-wrap {
      border-color: rgba(52, 211, 153, 0.5);
      box-shadow: 0 12px 32px rgba(52, 211, 153, 0.2);
    }
    .hof-poster { width: 100%; height: 100%; object-fit: cover; }
    
    .hof-info {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 4px;
    }
    .hof-title {
      font-size: 0.9rem;
      font-weight: 700;
      color: var(--text-primary);
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      min-height: 2.4em;
    }
    .number-one .hof-title { font-size: 1rem; color: #34d399; }
    .hof-rating { font-size: 0.85rem; color: #fbbf24; }
    .heart { font-size: 0.85em; margin-left: 2px; }
    .no-data { color: var(--text-muted); }
    .tie-breaker-note {
      font-size: 0.75rem;
      color: rgba(255, 255, 255, 0.3);
      margin-top: 16px;
      font-style: italic;
      text-align: center;
      letter-spacing: 0.05em;
    }
  `]
})
export class HallOfFameSlideComponent {
  @Input() data?: TopBottomMoviesDto | null;
  @Input() year!: number;

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w342${path}`;
  }
}
