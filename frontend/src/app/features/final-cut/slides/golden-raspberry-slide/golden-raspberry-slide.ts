import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { TopBottomMoviesDto } from '../../../../core/services/final-cut.service';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-golden-raspberry-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content rasp-bg">
      <div class="act-label">Act IV · The Climax</div>
      <h2 class="slide-title">The Razzies.</h2>
      <p class="slide-subtitle">We don't talk about these. The lowest rated of {{ year === 'global' ? 'All-Time' : year }}.</p>
      <p class="slide-explainer">The cutting room floor. The films that completely missed the mark.</p>

      <div class="bottom5-grid" *ngIf="data?.bottomRated?.length">
        <div *ngFor="let m of data!.bottomRated" class="rasp-card">
          <div class="rasp-poster-wrap">
            <img *ngIf="m.posterPath" [src]="posterUrl(m.posterPath)" [alt]="m.title" class="rasp-poster">
          </div>
          <div class="rasp-info">
            <span class="rasp-title">{{ m.title }}</span>
            <span class="rasp-rating">⭐ {{ m.rating | number:'1.1-1' }} <span *ngIf="m.liked" class="heart">❤️</span></span>
          </div>
        </div>
      </div>
      
      <div class="tie-breaker-note" *ngIf="data?.bottomRated?.length">
        * Films ordered by lowest rating, disfavor, and ultimately, chance.
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
    .rasp-bg {
      background: radial-gradient(ellipse at 50% 40%, rgba(244, 63, 94, 0.08) 0%, transparent 60%);
    }
    .bottom5-grid {
      display: flex;
      justify-content: center;
      gap: 16px;
      width: 100%;
      max-width: 660px;
      margin-top: 20px;
    }
    .rasp-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      flex: 1;
      opacity: 0.8;
      filter: grayscale(0.5);
      transition: all 0.3s;
    }
    .rasp-card:hover {
      opacity: 1;
      filter: grayscale(0);
      transform: translateY(-5px);
    }
    .rasp-poster-wrap {
      width: 100%;
      aspect-ratio: 2/3;
      border-radius: 8px;
      overflow: hidden;
      border: 1px solid rgba(244, 63, 94, 0.2);
    }
    .rasp-poster { width: 100%; height: 100%; object-fit: cover; }
    .rasp-info {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
    }
    .rasp-title {
      font-size: 0.85rem;
      color: var(--text-secondary);
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .rasp-rating { font-size: 0.8rem; color: #f43f5e; font-weight: 700; margin-top: 2px; }
    .heart { font-size: 0.85em; margin-left: 2px; }
    .heart { font-size: 0.85em; margin-left: 2px; }
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
export class GoldenRaspberrySlideComponent {
  @Input() data?: TopBottomMoviesDto | null;
  @Input() summary!: WrappedSummaryDto;
  @Input() year!: number | 'global';

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w154${path}`;
  }
}
