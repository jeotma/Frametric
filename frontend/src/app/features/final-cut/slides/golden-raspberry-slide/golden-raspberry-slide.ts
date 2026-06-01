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
      <p class="slide-subtitle">We don't talk about these. The lowest rated of {{ year }}.</p>

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

      <!-- Final Summary Note -->
      <div class="final-note" *ngIf="summary">
        And that's a wrap on {{ year }}.<br>
        <span class="fn-sub">You watched {{ summary.totalWatches }} films and spent {{ summary.totalWatchtimeMinutes / 60 | number:'1.0-0' }} hours in front of the screen.</span>
      </div>
      <p class="exit-note">Press ESC or click the exit button to return.</p>
    </div>
  `,
  styles: [`
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
    
    .final-note {
      margin-top: 60px;
      text-align: center;
      font-size: 1.5rem;
      font-weight: 800;
      color: var(--text-primary);
    }
    .fn-sub {
      display: block;
      font-size: 0.9rem;
      font-weight: 400;
      color: var(--text-muted);
      margin-top: 8px;
    }
    .exit-note {
      position: absolute;
      bottom: 30px;
      font-size: 0.8rem;
      color: rgba(255,255,255,0.3);
      animation: pulse 2s infinite;
    }
    @keyframes pulse {
      0%, 100% { opacity: 0.4; }
      50% { opacity: 1; }
    }
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
  @Input() year!: number;

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w154${path}`;
  }
}
