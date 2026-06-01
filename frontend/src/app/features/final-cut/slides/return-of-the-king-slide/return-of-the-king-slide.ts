import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MostRewatchedDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-return-of-the-king-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content rotk-bg">
      <div class="act-label">Act IV · The Climax</div>
      <h2 class="slide-title">The Return of the King.</h2>
      <p class="slide-subtitle">The movie you just couldn't stay away from.</p>
      <p class="slide-explainer">Encore performances. The stories you just had to experience one more time.</p>

      <div class="rotk-container" *ngIf="data">
        <div class="rotk-poster-wrap">
          <img *ngIf="data.posterPath" [src]="posterUrl(data.posterPath)" [alt]="data.title" class="rotk-poster">
          <div class="rotk-fallback" *ngIf="!data.posterPath">👑</div>
        </div>
        
        <div class="rotk-info">
          <div class="rotk-title">{{ data.title }}</div>
          <div class="rotk-year">{{ data.releaseYear }}</div>
          <div class="rotk-meta">
            <span class="rotk-count">{{ data.rewatchCount }}</span>
            <span class="rotk-label">rewatches this year</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">No rewatches found this year. Always moving forward!</p>
      
      <div class="tie-breaker-note" *ngIf="data">
        * Films ordered by highest rating, favor, and ultimately, chance.
      </div>
    </div>
  `,
  styles: [`
    .slide-explainer {
      font-size: 0.95rem;
      color: rgba(255,255,255,0.7);
      margin-bottom: 32px;
      font-style: italic;
      max-width: 600px;
      text-align: center;
    }
    .rotk-bg {
      background: radial-gradient(circle at center, rgba(251, 191, 36, 0.12) 0%, transparent 60%);
    }
    .rotk-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
      margin-top: 20px;
    }
    .rotk-poster-wrap {
      height: min(265px, 35vh);
      aspect-ratio: 2/3;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 16px 48px rgba(251, 191, 36, 0.2);
      border: 1px solid rgba(251, 191, 36, 0.3);
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(255,255,255,0.05);
    }
    .rotk-poster { width: 100%; height: 100%; object-fit: cover; }
    .rotk-fallback { font-size: 4rem; }
    .rotk-info {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
    }
    .rotk-title {
      font-size: 1.8rem;
      font-weight: 800;
      color: var(--text-primary);
      text-align: center;
    }
    .rotk-year { font-size: 1rem; color: var(--text-muted); }
    .rotk-meta {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-top: 12px;
    }
    .rotk-count {
      font-size: 3rem;
      font-weight: 900;
      color: #fbbf24;
      line-height: 1;
    }
    .rotk-label {
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.15em;
      color: var(--text-muted);
      margin-top: 8px;
    }
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
export class ReturnOfTheKingSlideComponent {
  @Input() data?: MostRewatchedDto | null;

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w500${path}`;
  }
}
