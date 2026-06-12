import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-hidden-gems-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-sepia">
      <div class="act-label">ACT III · SCENE 12 · HIDDEN GEMS</div>
      <h2 class="slide-title">Obscure Masterpieces.</h2>
      <p class="slide-subtitle">The hidden gems you unearthed that most people missed.</p>
      <p class="slide-explainer">The road less traveled. Your favorite obscure and niche discoveries.</p>

      <div class="gems-list" *ngIf="topGems.length">
        <div *ngFor="let g of topGems; let i = index" class="gem-card">
          <div class="poster-wrap">
            <img *ngIf="g.posterPath" [src]="posterUrl(g.posterPath)" [alt]="g.title" class="poster-img">
            <div class="poster-fallback" *ngIf="!g.posterPath">
              <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.2)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="fallback-icon"><path d="M6 3h12l4 6-10 13L2 9Z"/><path d="M11 3 8 9l4 13"/><path d="M13 3l3 6-4 13"/></svg>
            </div>
          </div>
          <div class="gem-info">
            <span class="gem-title">{{ g.title }}</span>
            <span class="gem-year" style="font-family: var(--font-mono)">{{ g.releaseYear }}</span>
            <div class="gem-meta">
              <span class="gem-rating" title="Ratings imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                {{ g.rating | number:'1.1-1' }} / 10
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="info-icon"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
              </span>
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
    .gems-list {
      display: flex;
      flex-direction: column;
      gap: 24px;
      width: 100%;
      max-width: 800px;
    }
    .gem-card {
      display: flex;
      gap: 24px;
      padding: 24px;
      border-radius: 20px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(12px);
      align-items: center;
    }
    .poster-wrap {
      width: 80px;
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
    .gem-info { display: flex; flex-direction: column; gap: 8px; }
    .gem-title { font-size: 1.25rem; font-weight: 700; color: var(--text-primary); }
    .gem-year { font-size: 1rem; color: var(--text-muted); }
    .gem-meta { display: flex; align-items: center; gap: 16px; margin-top: 8px; }
    .gem-rating { 
      font-size: 1rem; 
      color: #fbbf24; 
      font-weight: 600; 
      display: flex; 
      align-items: center; 
      gap: 6px; 
      font-family: var(--font-mono);
    }
    .info-icon { opacity: 0.5; margin-left: 2px; }
    .gem-popularity { font-size: 0.85rem; color: #2dd4bf; text-transform: uppercase; letter-spacing: 0.1em; }
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
