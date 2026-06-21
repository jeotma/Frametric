import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { TopBottomMoviesDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-hall-of-fame-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-sepia">
      <div class="act-label">ACT IV · SCENE 19 · HALL OF FAME</div>
      <h2 class="slide-title">The Hall of Fame.</h2>
      <p class="slide-subtitle">Your top rated masterpieces of {{ year === 'global' ? 'All-Time' : year }}.</p>
      <p class="slide-explainer">Five stars. Absolute cinema. The masterpieces that left a lasting mark.</p>

      <div class="top5-grid" *ngIf="data?.topRated?.length">
        <div *ngFor="let m of data!.topRated; let i = index" class="hof-card" [class.number-one]="i === 0">
          <div class="hof-rank">#{{ i + 1 }}</div>
          <div class="poster-wrap">
            <img *ngIf="m.posterPath" [src]="posterUrl(m.posterPath)" [alt]="m.title" class="hof-poster">
          </div>
          <div class="hof-info">
            <span class="hof-title">{{ m.title }}</span>
            <span class="hof-rating" data-tooltip="Your ratings, imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
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

      <p class="no-data" *ngIf="!data?.topRated?.length">No top rated movies found.</p>
      
      <div class="tie-breaker-note" *ngIf="data?.topRated?.length">
        * Films ordered by highest rating, favor, and ultimately, chance.
      </div>
      <div class="timecode">TC 02:14:45:03</div>
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
    .top5-grid {
      display: flex;
      align-items: flex-end;
      justify-content: center;
      gap: 24px;
      width: 100%;
      max-width: 1000px;
      margin-top: 50px;
    }
    .hof-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
      flex: 1;
      transition: transform 0.3s ease;
    }
    .hof-card:hover { transform: translateY(-10px); }
    .number-one {
      transform: scale(1.15) translateY(-20px);
      z-index: 10;
    }
    .number-one:hover { transform: scale(1.15) translateY(-30px); }
    
    .hof-rank {
      font-size: 1.3rem;
      font-weight: 800;
      color: var(--text-muted);
    }
    .number-one .hof-rank { color: #fbbf24; font-size: 1.6rem; }
    
    .poster-wrap {
      width: 100%;
      aspect-ratio: 2/3;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 8px 24px rgba(0,0,0,0.4);
      border: 1px solid rgba(255,255,255,0.1);
    }
    .number-one .poster-wrap {
      border-color: rgba(52, 211, 153, 0.5);
      box-shadow: 0 12px 32px rgba(52, 211, 153, 0.2);
    }
    .hof-poster { width: 100%; height: 100%; object-fit: cover; }
    
    .hof-info {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 4px;
    }
    .hof-title {
      font-size: 1.15rem;
      font-weight: 700;
      color: var(--text-primary);
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      min-height: 2.4em;
      margin-bottom: 4px;
    }
    .number-one .hof-title { font-size: 1.3rem; color: #34d399; }
    .hof-rating { 
      font-size: 0.95rem; 
      color: #fbbf24; 
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
      font-family: var(--font-mono);
    }
    .heart { display: flex; align-items: center; color: #f43f5e; }
    .info-icon { opacity: 0.5; margin-left: 2px; }
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
export class HallOfFameSlideComponent {
  @Input() data?: TopBottomMoviesDto | null;
  @Input() year!: number | 'global';

  posterUrl(path: string): string {
    if (path?.startsWith('http')) return path;
    return `https://image.tmdb.org/t/p/w342${path}`;
  }
}
