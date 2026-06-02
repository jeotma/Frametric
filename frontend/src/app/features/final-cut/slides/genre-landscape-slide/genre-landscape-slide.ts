import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { GenreWithRatingDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-genre-landscape-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content genre-bg">
      <div class="act-label">Act II · The Cast & Crew</div>
      <h2 class="slide-title">The Genre Landscape.</h2>
      <p class="slide-subtitle">The cinematic territories you explored in {{ year === 'global' ? 'All-Time' : year }}.</p>
      <p class="slide-explainer">The colors of your palette. The genres that defined your personal film festival.</p>

      <div class="genre-bars" *ngIf="genres.length">
        <div *ngFor="let g of topGenres; let i = index" class="genre-row">
          <div class="genre-rank">#{{ i + 1 }}</div>
          <div class="genre-name">{{ g.genreName }}</div>
          <div class="genre-bar-wrap">
            <div class="genre-bar" [style.width.%]="barWidth(g.count)" [style.background]="barColor(i)"></div>
          </div>
          <div class="genre-count">{{ g.count }}</div>
          <div class="genre-rating" *ngIf="g.averageRating > 0">⭐ {{ g.averageRating | number:'1.1-1' }}</div>
        </div>
      </div>

      <p class="no-data" *ngIf="!genres.length">No genre data for {{ year === 'global' ? 'All-Time' : year }}.</p>
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
    .genre-bg {
      background: radial-gradient(ellipse at 40% 30%, rgba(168, 85, 247, 0.1) 0%, transparent 55%);
    }
    .genre-bars {
      display: flex;
      flex-direction: column;
      gap: 12px;
      width: 100%;
      max-width: 640px;
    }
    .genre-row {
      display: grid;
      grid-template-columns: 28px 120px 1fr 40px 72px;
      align-items: center;
      gap: 12px;
    }
    .genre-rank {
      font-size: 0.75rem;
      color: var(--text-muted);
      font-weight: 700;
    }
    .genre-name {
      font-size: 0.9rem;
      font-weight: 600;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .genre-bar-wrap {
      height: 8px;
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
      font-size: 0.8rem;
      color: var(--text-muted);
      text-align: right;
    }
    .genre-rating {
      font-size: 0.75rem;
      color: #fbbf24;
      text-align: right;
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class GenreLandscapeSlideComponent {
  @Input() genres: GenreWithRatingDto[] = [];
  @Input() year!: number | 'global';

  private readonly COLORS = ['#a78bfa','#60a5fa','#34d399','#fbbf24','#fb7185','#f472b6','#2dd4bf','#818cf8'];

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
