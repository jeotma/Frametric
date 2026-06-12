import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MostRewatchedDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-return-of-the-king-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-sepia">
      <div class="act-label">ACT IV · SCENE 18 · RETURN OF THE KING</div>
      <h2 class="slide-title">The Return of the King.</h2>
      <p class="slide-subtitle">The movie you just couldn't stay away from.</p>
      <p class="slide-explainer">Encore performances. The stories you just had to experience one more time.</p>

      <div class="rotk-container" *ngIf="data">
        <div class="rotk-poster-wrap">
          <img *ngIf="data.posterPath" [src]="posterUrl(data.posterPath)" [alt]="data.title" class="rotk-poster">
          <div class="rotk-fallback" *ngIf="!data.posterPath">
            <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="var(--accent-sepia)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="fallback-icon"><polygon points="12 2 15 8 22 7 18 13 20 20 12 18 4 20 6 13 2 7 9 8 12 2"/></svg>
          </div>
        </div>
        
        <div class="rotk-info">
          <div class="rotk-title">{{ data.title }}</div>
          <div class="rotk-year" style="font-family: var(--font-mono)">{{ data.releaseYear }}</div>
          <div class="rotk-meta">
            <span class="rotk-count" style="font-family: var(--font-mono)">{{ data.rewatchCount }}</span>
            <span class="rotk-label">{{ year === 'global' ? 'all-time rewatches' : 'rewatches this year' }}</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">No rewatches found {{ year === 'global' ? 'overall' : 'this year' }}. Always moving forward!</p>
      
      <div class="tie-breaker-note" *ngIf="data">
        * Films ordered by highest rating, favor, and ultimately, chance.
      </div>
      <div class="timecode">TC 02:05:20:11</div>
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
    .rotk-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 24px;
      margin-top: 24px;
    }
    .rotk-poster-wrap {
      height: min(340px, 45vh);
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
      gap: 12px;
    }
    .rotk-title {
      font-size: 2.5rem;
      font-weight: 800;
      color: var(--text-primary);
      text-align: center;
    }
    .rotk-year { font-size: 1.25rem; color: var(--text-muted); }
    .rotk-meta {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-top: 20px;
    }
    .rotk-count {
      font-size: 4rem;
      font-weight: 900;
      color: var(--accent-sepia);
      line-height: 1;
    }
    .rotk-label {
      font-size: 1rem;
      text-transform: uppercase;
      letter-spacing: 0.15em;
      color: var(--text-muted);
      margin-top: 12px;
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
  @Input() year!: number | 'global';

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w500${path}`;
  }
}
