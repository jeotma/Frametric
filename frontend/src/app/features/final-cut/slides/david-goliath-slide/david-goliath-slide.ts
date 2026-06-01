import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DavidAndGoliathDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-david-goliath-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content dg-bg">
      <div class="act-label">Act III · The Deep Cuts</div>
      <h2 class="slide-title">David and Goliath.</h2>
      <p class="slide-subtitle">The epic and the intimate — the extremes of your watch list.</p>
      <p class="slide-explainer">From epic sagas to brief encounters. The extremes of your runtime.</p>

      <div class="dg-container">
        <!-- The Epic -->
        <div class="dg-card goliath-card" *ngIf="data?.longest">
          <div class="poster-wrap">
            <img *ngIf="data!.longest!.posterPath" [src]="posterUrl(data!.longest!.posterPath!)" [alt]="data!.longest!.title" class="poster-img">
            <div class="poster-fallback" *ngIf="!data!.longest!.posterPath">🎬</div>
          </div>
          <div class="dg-info">
            <span class="dg-type">The Epic — Goliath</span>
            <span class="dg-title">{{ data!.longest!.title }}</span>
            <span class="dg-year">{{ data!.longest!.releaseYear }}</span>
            <span class="dg-runtime" *ngIf="data!.longest!.runtimeMinutes">{{ data!.longest!.runtimeMinutes }} min · {{ runtimeHours(data!.longest!.runtimeMinutes!) }}</span>
          </div>
        </div>

        <!-- The Intimate -->
        <div class="dg-card david-card" *ngIf="data?.shortest">
          <div class="poster-wrap">
            <img *ngIf="data!.shortest!.posterPath" [src]="posterUrl(data!.shortest!.posterPath!)" [alt]="data!.shortest!.title" class="poster-img">
            <div class="poster-fallback" *ngIf="!data!.shortest!.posterPath">🎞️</div>
          </div>
          <div class="dg-info">
            <span class="dg-type">The Intimate — David</span>
            <span class="dg-title">{{ data!.shortest!.title }}</span>
            <span class="dg-year">{{ data!.shortest!.releaseYear }}</span>
            <span class="dg-runtime" *ngIf="data!.shortest!.runtimeMinutes">{{ data!.shortest!.runtimeMinutes }} min · {{ runtimeHours(data!.shortest!.runtimeMinutes!) }}</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data?.longest && !data?.shortest">Runtime data not available.</p>
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
    .dg-bg {
      background: radial-gradient(ellipse at 25% 50%, rgba(251, 113, 133, 0.08) 0%, transparent 50%),
                  radial-gradient(ellipse at 75% 50%, rgba(96, 165, 250, 0.08) 0%, transparent 50%);
    }
    .dg-container {
      display: flex;
      gap: 20px;
      width: 100%;
      max-width: 640px;
    }
    .dg-card {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 20px;
      border-radius: 20px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(12px);
    }
    .goliath-card { border-color: rgba(251, 113, 133, 0.2); }
    .david-card { border-color: rgba(96, 165, 250, 0.2); }
    .poster-wrap {
      width: 100%;
      aspect-ratio: 2/3;
      border-radius: 12px;
      overflow: hidden;
      background: rgba(255,255,255,0.04);
      display: flex;
      align-items: center;
      justify-content: center;
      max-height: 160px;
    }
    .poster-img { width: 100%; height: 100%; object-fit: cover; }
    .poster-fallback { font-size: 2.5rem; }
    .dg-info { display: flex; flex-direction: column; gap: 4px; }
    .dg-type {
      font-size: 0.65rem;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      color: var(--text-muted);
    }
    .dg-title {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1.2;
    }
    .dg-year { font-size: 0.8rem; color: var(--text-muted); }
    .dg-runtime { font-size: 0.8rem; color: var(--text-secondary); font-weight: 600; }
    .no-data { color: var(--text-muted); }
  `]
})
export class DavidGoliathSlideComponent {
  @Input() data?: DavidAndGoliathDto | null;

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w342${path}`;
  }

  runtimeHours(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return `${h}h ${m}m`;
  }
}
