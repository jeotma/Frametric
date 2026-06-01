import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { BestRookiesDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-best-rookies-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content rookies-bg">
      <div class="act-label">Act II · The Cast & Crew</div>
      <h2 class="slide-title">The Best Rookies.</h2>
      <p class="slide-subtitle">First-time encounters that blew you away in {{ year }}.</p>

      <div class="rookies-grid" *ngIf="data">
        <div class="rookie-column">
          <div class="rookie-col-label">🎬 New Directors</div>
          <div *ngFor="let d of data.newDirectors.slice(0, 4); let i = index" class="rookie-row" [class.star]="i === 0">
            <span class="rookie-name">{{ d.name }}</span>
            <span class="rookie-meta">{{ d.moviesWatchedThisYear }} film{{ d.moviesWatchedThisYear !== 1 ? 's' : '' }}</span>
            <span class="rookie-rating" *ngIf="d.averageRating > 0">⭐ {{ d.averageRating | number:'1.1-1' }}</span>
          </div>
          <p class="no-data-sm" *ngIf="!data.newDirectors.length">No new directors this year.</p>
        </div>

        <div class="rookie-divider"></div>

        <div class="rookie-column">
          <div class="rookie-col-label">🎭 New Actors</div>
          <div *ngFor="let a of data.newActors.slice(0, 4); let i = index" class="rookie-row" [class.star]="i === 0">
            <span class="rookie-name">{{ a.name }}</span>
            <span class="rookie-meta">{{ a.moviesWatchedThisYear }} film{{ a.moviesWatchedThisYear !== 1 ? 's' : '' }}</span>
            <span class="rookie-rating" *ngIf="a.averageRating > 0">⭐ {{ a.averageRating | number:'1.1-1' }}</span>
          </div>
          <p class="no-data-sm" *ngIf="!data.newActors.length">No new actors this year.</p>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">No rookies data for {{ year }}.</p>
    </div>
  `,
  styles: [`
    .rookies-bg {
      background: radial-gradient(ellipse at 50% 40%, rgba(45, 212, 191, 0.08) 0%, transparent 55%);
    }
    .rookies-grid {
      display: grid;
      grid-template-columns: 1fr 1px 1fr;
      gap: 24px;
      width: 100%;
      max-width: 680px;
    }
    .rookie-col-label {
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: var(--text-muted);
      margin-bottom: 12px;
      font-weight: 700;
    }
    .rookie-column { display: flex; flex-direction: column; gap: 8px; }
    .rookie-divider {
      background: rgba(255,255,255,0.06);
      border-radius: 99px;
    }
    .rookie-row {
      display: flex;
      flex-direction: column;
      gap: 2px;
      padding: 10px 14px;
      border-radius: 12px;
      background: rgba(255,255,255,0.02);
      border: 1px solid rgba(255,255,255,0.04);
    }
    .rookie-row.star {
      background: rgba(45, 212, 191, 0.06);
      border-color: rgba(45, 212, 191, 0.2);
    }
    .rookie-name { font-size: 0.9rem; font-weight: 600; color: var(--text-primary); }
    .rookie-meta { font-size: 0.75rem; color: var(--text-muted); }
    .rookie-rating { font-size: 0.75rem; color: #fbbf24; }
    .no-data, .no-data-sm { color: var(--text-muted); font-size: 0.85rem; }
  `]
})
export class BestRookiesSlideComponent {
  @Input() data?: BestRookiesDto | null;
  @Input() year!: number;
}
