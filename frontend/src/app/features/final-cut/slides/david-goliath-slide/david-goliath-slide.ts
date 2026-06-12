import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DavidAndGoliathDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-david-goliath-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-silver">
      <div class="act-label">ACT III · SCENE 13 · DAVID AND GOLIATH</div>
      <h2 class="slide-title">David and Goliath.</h2>
      <p class="slide-subtitle">The epic and the intimate — the extremes of your watch list.</p>
      <p class="slide-explainer">From epic sagas to brief encounters. The extremes of your runtime.</p>

      <div class="dg-container">
        <!-- The Epic -->
        <div class="dg-card goliath-card" *ngIf="data?.longest">
          <div class="poster-wrap">
            <img *ngIf="data!.longest!.posterPath" [src]="posterUrl(data!.longest!.posterPath!)" [alt]="data!.longest!.title" class="poster-img">
            <div class="poster-fallback" *ngIf="!data!.longest!.posterPath">
              <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.2)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="fallback-icon"><rect x="2" y="2" width="20" height="20" rx="2.18" ry="2.18"/><line x1="7" y1="2" x2="7" y2="22"/><line x1="17" y1="2" x2="17" y2="22"/><line x1="2" y1="12" x2="22" y2="12"/><line x1="2" y1="7" x2="7" y2="7"/><line x1="2" y1="17" x2="7" y2="17"/><line x1="17" y1="17" x2="22" y2="17"/><line x1="17" y1="7" x2="22" y2="7"/></svg>
            </div>
          </div>
          <div class="dg-info">
            <span class="dg-type">The Epic — Goliath</span>
            <span class="dg-title">{{ data!.longest!.title }}</span>
            <span class="dg-year" style="font-family: var(--font-mono)">{{ data!.longest!.releaseYear }}</span>
            <span class="dg-runtime" *ngIf="data!.longest!.runtimeMinutes" style="font-family: var(--font-mono)">{{ data!.longest!.runtimeMinutes }} min · {{ runtimeHours(data!.longest!.runtimeMinutes!) }}</span>
          </div>
        </div>

        <!-- The Intimate -->
        <div class="dg-card david-card" *ngIf="data?.shortest">
          <div class="poster-wrap">
            <img *ngIf="data!.shortest!.posterPath" [src]="posterUrl(data!.shortest!.posterPath!)" [alt]="data!.shortest!.title" class="poster-img">
            <div class="poster-fallback" *ngIf="!data!.shortest!.posterPath">
              <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.2)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="fallback-icon"><rect x="2" y="2" width="20" height="20" rx="2.18" ry="2.18"/><line x1="7" y1="2" x2="7" y2="22"/><line x1="17" y1="2" x2="17" y2="22"/><line x1="2" y1="12" x2="22" y2="12"/><line x1="2" y1="7" x2="7" y2="7"/><line x1="2" y1="17" x2="7" y2="17"/><line x1="17" y1="17" x2="22" y2="17"/><line x1="17" y1="7" x2="22" y2="7"/></svg>
            </div>
          </div>
          <div class="dg-info">
            <span class="dg-type">The Intimate — David</span>
            <span class="dg-title">{{ data!.shortest!.title }}</span>
            <span class="dg-year" style="font-family: var(--font-mono)">{{ data!.shortest!.releaseYear }}</span>
            <span class="dg-runtime" *ngIf="data!.shortest!.runtimeMinutes" style="font-family: var(--font-mono)">{{ data!.shortest!.runtimeMinutes }} min · {{ runtimeHours(data!.shortest!.runtimeMinutes!) }}</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data?.longest && !data?.shortest">Runtime data not available.</p>
      <div class="timecode">TC 01:25:40:19</div>
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
    .dg-container {
      display: flex;
      gap: 32px;
      width: 100%;
      max-width: 800px;
    }
    .dg-card {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 32px;
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
      max-height: 220px;
    }
    .poster-img { width: 100%; height: 100%; object-fit: cover; }
    .poster-fallback { font-size: 2.5rem; }
    .dg-info { display: flex; flex-direction: column; gap: 4px; }
    .dg-type {
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      color: var(--text-muted);
    }
    .dg-title {
      font-size: 1.15rem;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1.2;
    }
    .dg-year { font-size: 0.95rem; color: var(--text-muted); }
    .dg-runtime { font-size: 1rem; color: var(--text-secondary); font-weight: 600; }
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
