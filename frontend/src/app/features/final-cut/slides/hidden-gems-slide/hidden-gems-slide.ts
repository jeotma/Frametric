import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-hidden-gems-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content gems-bg">
      <div class="act-label">Act III · The Deep Cuts</div>
      <h2 class="slide-title">Obscure Masterpieces.</h2>
      <p class="slide-subtitle">The hidden gems you unearthed that most people missed.</p>
      <p class="slide-explainer">The road less traveled. Your favorite obscure and niche discoveries.</p>

      <div class="gems-list" *ngIf="topGems.length">
        <div *ngFor="let g of topGems; let i = index" class="gem-card">
          <div class="poster-wrap">
            <img *ngIf="g.posterPath" [src]="posterUrl(g.posterPath)" [alt]="g.title" class="poster-img">
            <div class="poster-fallback" *ngIf="!g.posterPath">💎</div>
          </div>
          <div class="gem-info">
            <span class="gem-title">{{ g.title }}</span>
            <span class="gem-year">{{ g.releaseYear }}</span>
            <div class="gem-meta">
              <span class="gem-rating" title="Ratings imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">⭐ {{ g.rating | number:'1.1-1' }} / 10 ℹ️</span>
              <span class="gem-popularity">Low Popularity Index</span>
            </div>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!topGems.length">No hidden gems found for {{ year === 'global' ? 'All-Time' : year }}.</p>
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
    .gems-bg {
      background: radial-gradient(ellipse at 40% 70%, rgba(45, 212, 191, 0.08) 0%, transparent 55%);
    }
    .gems-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
      width: 100%;
      max-width: 600px;
    }
    .gem-card {
      display: flex;
      gap: 20px;
      padding: 16px;
      border-radius: 20px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(12px);
      align-items: center;
    }
    .poster-wrap {
      width: 64px;
      aspect-ratio: 2/3;
      border-radius: 8px;
      overflow: hidden;
      background: rgba(255,255,255,0.04);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .poster-img { width: 100%; height: 100%; object-fit: cover; }
    .poster-fallback { font-size: 1.8rem; }
    .gem-info { display: flex; flex-direction: column; gap: 4px; }
    .gem-title { font-size: 1.1rem; font-weight: 700; color: var(--text-primary); }
    .gem-year { font-size: 0.85rem; color: var(--text-muted); }
    .gem-meta { display: flex; align-items: center; gap: 12px; margin-top: 4px; }
    .gem-rating { font-size: 0.85rem; color: #fbbf24; font-weight: 600; }
    .gem-popularity { font-size: 0.75rem; color: #2dd4bf; text-transform: uppercase; letter-spacing: 0.1em; }
    .no-data { color: var(--text-muted); }
  `]
})
export class HiddenGemsSlideComponent {
  @Input() gems: any[] = [];
  @Input() year!: number | 'global';

  get topGems(): any[] {
    return (this.gems ?? []).slice(0, 3);
  }

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w154${path}`;
  }
}
