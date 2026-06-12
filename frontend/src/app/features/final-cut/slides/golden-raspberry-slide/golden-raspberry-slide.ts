import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { TopBottomMoviesDto } from '../../../../core/services/final-cut.service';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-golden-raspberry-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-record">
      <div class="act-label">ACT IV · SCENE 20 · THE GOLDEN RASPBERRY</div>
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
            <span class="rasp-rating" data-tooltip="Your ratings, imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
              {{ m.rating | number:'1.1-1' }}
              <span *ngIf="m.liked" class="heart">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="heart-icon"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
              </span>
              <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="info-icon"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
            </span>
          </div>
        </div>
      </div>
      
      <div class="tie-breaker-note" *ngIf="data?.bottomRated?.length">
        * Films ordered by lowest rating, disfavor, and ultimately, chance.
      </div>
      <div class="timecode">TC 02:22:10:15</div>
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
    .bottom5-grid {
      display: flex;
      justify-content: center;
      gap: 24px;
      width: 100%;
      max-width: 800px;
      margin-top: 40px;
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
      font-size: 1.05rem;
      color: var(--text-secondary);
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      margin-bottom: 4px;
    }
    .rasp-rating { 
      font-size: 0.95rem; 
      color: #f43f5e; 
      font-weight: 700; 
      margin-top: 2px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
      font-family: var(--font-mono);
    }
    .heart { display: flex; align-items: center; color: #f43f5e; }
    .info-icon { opacity: 0.5; margin-left: 2px; }
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
