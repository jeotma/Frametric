import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { BestRookiesDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-best-rookies-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-emerald">
      <div class="act-label">ACT II · SCENE 10 · THE BEST ROOKIES</div>
      <h2 class="slide-title">The Best Rookies.</h2>
      <p class="slide-subtitle">First-time encounters that blew you away in {{ year === 'global' ? 'All-Time' : year }}.</p>
      <p class="slide-explainer">Fresh faces and new voices. The talents you recently discovered for the very first time.</p>

      <div class="rookies-grid" *ngIf="data">
        <div class="rookie-column">
          <div class="rookie-col-label">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="var(--accent-emerald)" stroke="none" class="rookie-icon"><path d="M18 4l2 4h-3l-2-4h-2l2 4h-3l-2-4H8l2 4H7L5 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V4h-4z"/></svg>
            New Directors
          </div>
          <div *ngFor="let d of data.newDirectors.slice(0, 4); let i = index" class="rookie-row" [class.star]="i === 0">
            <div class="rookie-info-wrap">
              <div class="rookie-avatar">
                <img *ngIf="d.profilePath" [src]="'https://image.tmdb.org/t/p/w185' + d.profilePath" [alt]="d.name">
                <div *ngIf="!d.profilePath" class="rookie-avatar-fallback">{{ d.name.charAt(0) }}</div>
              </div>
              <span class="rookie-name">{{ d.name }}</span>
            </div>
            <div class="rookie-bottom">
              <span class="rookie-rating" *ngIf="d.averageRating > 0" data-tooltip="Your ratings, imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                {{ d.averageRating | number:'1.1-1' }}
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="info-icon"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
              </span>
              <span class="rookie-meta" style="font-family: var(--font-mono)">{{ d.moviesWatchedThisYear }} film{{ d.moviesWatchedThisYear !== 1 ? 's' : '' }}</span>
            </div>
          </div>
          <p class="no-data-sm" *ngIf="!data.newDirectors.length">No new directors this year.</p>
        </div>

        <div class="rookie-divider"></div>

        <div class="rookie-column">
          <div class="rookie-col-label">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--accent-emerald)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="rookie-icon"><circle cx="12" cy="12" r="10"/><path d="M8 14s1.5 2 4 2 4-2 4-2"/><line x1="9" y1="9" x2="9.01" y2="9"/><line x1="15" y1="9" x2="15.01" y2="9"/></svg>
            New Actors
          </div>
          <div *ngFor="let a of data.newActors.slice(0, 4); let i = index" class="rookie-row" [class.star]="i === 0">
            <div class="rookie-info-wrap">
              <div class="rookie-avatar">
                <img *ngIf="a.profilePath" [src]="'https://image.tmdb.org/t/p/w185' + a.profilePath" [alt]="a.name">
                <div *ngIf="!a.profilePath" class="rookie-avatar-fallback">{{ a.name.charAt(0) }}</div>
              </div>
              <span class="rookie-name">{{ a.name }}</span>
            </div>
            <div class="rookie-bottom">
              <span class="rookie-rating" *ngIf="a.averageRating > 0" data-tooltip="Your ratings, imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                {{ a.averageRating | number:'1.1-1' }}
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="info-icon"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
              </span>
              <span class="rookie-meta" style="font-family: var(--font-mono)">{{ a.moviesWatchedThisYear }} film{{ a.moviesWatchedThisYear !== 1 ? 's' : '' }}</span>
            </div>
          </div>
          <p class="no-data-sm" *ngIf="!data.newActors.length">No new actors this year.</p>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">No rookies data for {{ year === 'global' ? 'All-Time' : year }}.</p>
      <div class="timecode">TC 01:02:15:07</div>
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
    .rookies-grid {
      display: grid;
      grid-template-columns: 1fr 1px 1fr;
      gap: 40px;
      width: 100%;
      max-width: 800px;
    }
    .rookie-col-label {
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: var(--text-muted);
      margin-bottom: 16px;
      font-weight: 700;
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .rookie-column { display: flex; flex-direction: column; gap: 12px; }
    .rookie-divider {
      background: rgba(255,255,255,0.06);
      border-radius: 99px;
    }
    .rookie-row {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 12px 16px;
      border-radius: 16px;
      background: rgba(255,255,255,0.02);
      border: 1px solid rgba(255,255,255,0.04);
    }
    .rookie-info-wrap { display: flex; align-items: center; gap: 10px; margin-bottom: 2px; }
    .rookie-avatar { width: 32px; height: 32px; border-radius: 50%; overflow: hidden; background: rgba(255,255,255,0.05); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .rookie-avatar img { width: 100%; height: 100%; object-fit: cover; }
    .rookie-avatar-fallback { font-size: 0.9rem; font-weight: 700; color: var(--text-muted); }
    .rookie-row.star {
      background: rgba(16, 185, 129, 0.08);
      border-color: rgba(16, 185, 129, 0.3);
    }
    .rookie-name { font-size: 1.1rem; font-weight: 600; color: var(--text-primary); }
    .rookie-bottom {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 2px;
      padding-left: 42px;
    }
    .rookie-meta {
      font-size: 0.8rem;
      color: rgba(255, 255, 255, 0.6);
      background: rgba(255, 255, 255, 0.05);
      padding: 2px 8px;
      border-radius: 6px;
      border: 1px solid rgba(255, 255, 255, 0.08);
      font-weight: 500;
    }
    .rookie-rating { 
      font-size: 0.9rem; 
      color: #fbbf24; 
      display: flex; 
      align-items: center; 
      gap: 4px; 
    }
    .info-icon { opacity: 0.5; margin-left: 2px; }
    .no-data, .no-data-sm { color: var(--text-muted); font-size: 0.95rem; }
  `]
})
export class BestRookiesSlideComponent {
  @Input() data?: BestRookiesDto | null;
  @Input() year!: number | 'global';
}
