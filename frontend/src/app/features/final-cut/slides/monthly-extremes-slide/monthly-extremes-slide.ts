import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { MonthlyExtremeDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-monthly-extremes-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content mx-bg">
      <div class="act-label">Act IV · The Climax</div>
      <h2 class="slide-title">{{ year === 'global' ? 'The Monthly Anthology.' : 'A Year of Extremes.' }}</h2>
      <p class="slide-subtitle">{{ year === 'global' ? 'The all-time highest highs and lowest lows, aggregated month by month.' : 'The highest highs and lowest lows of ' + year + ', month by month.' }}</p>
      <p class="slide-explainer">{{ year === 'global' ? 'A lifetime of logging. The absolute triumphs and the colossal misfires you\\'ve witnessed across the calendar.' : 'The soaring highs and the crushing lows. Your peak cinematic triumphs and your biggest misfires, month by month.' }}</p>

      <div class="mx-grid" *ngIf="topMonths.length">
        <div *ngFor="let m of topMonths" class="mx-month-row">
          <div class="mx-month-name">{{ m.monthName }}</div>
          
          <div class="mx-movie best-movie" *ngIf="m.bestMovie">
            <span class="mx-icon">🏆</span>
            <div class="mx-info">
              <span class="mx-title">{{ m.bestMovie.title }}</span>
              <span class="mx-rating">⭐ {{ m.bestMovie.rating | number:'1.1-1' }}</span>
            </div>
          </div>
          <div class="mx-movie no-data-box" *ngIf="!m.bestMovie">No entries</div>

          <div class="mx-movie worst-movie" *ngIf="m.worstMovie">
            <span class="mx-icon">📉</span>
            <div class="mx-info">
              <span class="mx-title">{{ m.worstMovie.title }}</span>
              <span class="mx-rating">⭐ {{ m.worstMovie.rating | number:'1.1-1' }}</span>
            </div>
          </div>
          <div class="mx-movie no-data-box" *ngIf="!m.worstMovie">No entries</div>
        </div>
      </div>

      <p class="no-data" *ngIf="!topMonths.length">No monthly data available.</p>
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
    .mx-bg {
      background: radial-gradient(ellipse at 50% 50%, rgba(168, 85, 247, 0.08) 0%, transparent 60%);
    }
    .mx-grid {
      display: flex;
      flex-direction: column;
      gap: 12px;
      width: 100%;
      max-width: 800px;
    }
    .mx-month-row {
      display: grid;
      grid-template-columns: 100px minmax(0, 1fr) minmax(0, 1fr);
      gap: 16px;
      align-items: center;
    }
    .mx-month-name {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.1em;
      text-align: right;
    }
    .mx-movie {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px 14px;
      border-radius: 12px;
      background: rgba(255,255,255,0.02);
      border: 1px solid rgba(255,255,255,0.04);
      min-width: 0;
    }
    .mx-movie.best-movie { border-color: rgba(52, 211, 153, 0.15); }
    .mx-movie.worst-movie { border-color: rgba(244, 63, 94, 0.15); }
    .mx-icon { font-size: 1.2rem; flex-shrink: 0; }
    .mx-info { display: flex; flex-direction: column; min-width: 0; align-items: flex-start; justify-content: center; }
    .mx-title {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .mx-rating { font-size: 0.75rem; color: var(--text-muted); }
    .no-data-box {
      justify-content: center;
      color: var(--text-muted);
      font-size: 0.8rem;
      background: transparent;
      border-color: transparent;
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class MonthlyExtremesSlideComponent {
  @Input() months: MonthlyExtremeDto[] = [];
  @Input() year!: number | 'global';
  @Input() part: number = 1;

  get topMonths(): MonthlyExtremeDto[] {
    const start = this.part === 1 ? 1 : 7;
    const end = this.part === 1 ? 6 : 12;
    const monthNames = [
      "January", "February", "March", "April", "May", "June",
      "July", "August", "September", "October", "November", "December"
    ];
    
    const result: MonthlyExtremeDto[] = [];
    for (let i = start; i <= end; i++) {
      const existing = (this.months ?? []).find(m => m.month === i);
      if (existing) {
        result.push(existing);
      } else {
        // Pad with empty entries to keep the layout consistent
        result.push({
          month: i,
          monthName: monthNames[i - 1],
          bestMovie: null as any,
          worstMovie: null as any
        });
      }
    }
    return result;
  }
}
