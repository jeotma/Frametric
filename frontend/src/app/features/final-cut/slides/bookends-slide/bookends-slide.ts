import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookendsDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-bookends-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content bookends-bg">
      <div class="act-label">Act IV · The Climax</div>
      <h2 class="slide-title">The Bookends.</h2>
      <p class="slide-subtitle">How it started, and how it ended in {{ year }}.</p>

      <div class="bookends-container" *ngIf="data">
        
        <div class="be-card" *ngIf="data.openingScene">
          <div class="be-header">The Opening Scene</div>
          <div class="be-poster-wrap">
            <img *ngIf="data.openingScene.posterPath" [src]="posterUrl(data.openingScene.posterPath)" [alt]="data.openingScene.title" class="be-poster">
          </div>
          <div class="be-title">{{ data.openingScene.title }}</div>
          <div class="be-year">{{ data.openingScene.releaseYear }}</div>
        </div>

        <div class="be-arrow">→</div>

        <div class="be-card" *ngIf="data.fadeToBlack">
          <div class="be-header">Fade to Black</div>
          <div class="be-poster-wrap">
            <img *ngIf="data.fadeToBlack.posterPath" [src]="posterUrl(data.fadeToBlack.posterPath)" [alt]="data.fadeToBlack.title" class="be-poster">
          </div>
          <div class="be-title">{{ data.fadeToBlack.title }}</div>
          <div class="be-year">{{ data.fadeToBlack.releaseYear }}</div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data?.openingScene && !data?.fadeToBlack">No bookend data for {{ year }}.</p>
    </div>
  `,
  styles: [`
    .bookends-bg {
      background: radial-gradient(ellipse at 30% 50%, rgba(251, 191, 36, 0.08) 0%, transparent 50%),
                  radial-gradient(ellipse at 70% 50%, rgba(168, 85, 247, 0.08) 0%, transparent 50%);
    }
    .bookends-container {
      display: flex;
      align-items: center;
      gap: 32px;
      width: 100%;
      max-width: 720px;
    }
    .be-card {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 24px;
      border-radius: 20px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(12px);
    }
    .be-header {
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      color: var(--text-muted);
      margin-bottom: 20px;
      font-weight: 700;
    }
    .be-poster-wrap {
      width: 140px;
      aspect-ratio: 2/3;
      border-radius: 12px;
      overflow: hidden;
      margin-bottom: 16px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.3);
    }
    .be-poster { width: 100%; height: 100%; object-fit: cover; }
    .be-title {
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-primary);
      text-align: center;
      margin-bottom: 4px;
    }
    .be-year { font-size: 0.85rem; color: var(--text-muted); }
    .be-arrow {
      font-size: 2rem;
      color: rgba(255,255,255,0.2);
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class BookendsSlideComponent {
  @Input() data?: BookendsDto | null;
  @Input() year!: number;

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w342${path}`;
  }
}
