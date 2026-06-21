import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { GenreWithRatingDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-genre-landscape-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-emerald">
      <div class="act-label">ACT II · SCENE 6 · THE GENRE LANDSCAPE</div>
      <h2 class="slide-title">The Genre Landscape.</h2>
      <p class="slide-subtitle">The cinematic territories you explored in {{ year === 'global' ? 'All-Time' : year }}.</p>
      <p class="slide-explainer">The colors of your palette. The genres that defined your personal film festival.</p>

      <div class="genre-bars" *ngIf="genres.length">
        <div *ngFor="let g of topGenres; let i = index" class="genre-row">
          <div class="genre-rank" style="font-family: var(--font-mono)">#{{ i + 1 }}</div>
          <div class="genre-name">{{ g.genreName }}</div>
          <div class="genre-bar-wrap">
            <div class="genre-bar" [style.width.%]="barWidth(g.count)" [style.background]="barColor(i)"></div>
          </div>
          <div class="genre-count" style="font-family: var(--font-mono)">{{ g.count }}</div>
          <div class="genre-rating" *ngIf="g.averageRating > 0" data-tooltip="Your ratings, imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
            {{ g.averageRating | number:'1.1-1' }}
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="info-icon"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!genres.length">No genre data for {{ year === 'global' ? 'All-Time' : year }}.</p>
      <div class="timecode">TC 00:32:55:18</div>
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
    .genre-bars {
      display: flex;
      flex-direction: column;
      gap: 16px;
      width: 100%;
      max-width: 800px;
    }
    .genre-row {
      display: grid;
      grid-template-columns: 32px 140px 1fr 48px 84px;
      align-items: center;
      gap: 16px;
    }
    .genre-rank {
      font-size: 0.85rem;
      color: var(--text-muted);
      font-weight: 700;
    }
    .genre-name {
      font-size: 1.05rem;
      font-weight: 600;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .genre-bar-wrap {
      height: 12px;
      background: rgba(255,255,255,0.06);
      border-radius: 99px;
      overflow: hidden;
    }
    .genre-bar {
      height: 100%;
      border-radius: 99px;
      transition: width 0.8s ease;
    }
    .genre-count {
      font-size: 0.9rem;
      color: var(--text-muted);
      text-align: right;
    }
    .genre-rating {
      font-size: 0.85rem;
      color: #fbbf24;
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 4px;
      font-family: var(--font-mono);
    }
    .info-icon { opacity: 0.5; margin-left: 2px; }
    .no-data { color: var(--text-muted); }
  `]
})
export class GenreLandscapeSlideComponent {
  @Input() genres: GenreWithRatingDto[] = [];
  @Input() year!: number | 'global';

  private readonly COLORS = ['var(--accent-silver)','#60a5fa','#34d399','#fbbf24','#fb7185','#f472b6','#2dd4bf','#818cf8'];

  get topGenres(): GenreWithRatingDto[] {
    return (this.genres ?? []).slice(0, 8);
  }

  barWidth(count: number): number {
    const max = this.topGenres[0]?.count ?? 1;
    return (count / max) * 100;
  }

  barColor(index: number): string {
    return this.COLORS[index % this.COLORS.length];
  }
}
