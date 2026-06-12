import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { MonthlyExtremeDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-monthly-extremes-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-record">
      <div class="act-label">{{ part === 1 ? 'ACT IV · SCENE 16 · MONTHLY EXTREMES — #1' : 'ACT IV · SCENE 17 · MONTHLY EXTREMES — #2' }}</div>
      <h2 class="slide-title">{{ year === 'global' ? 'The Monthly Anthology.' : 'A Year of Extremes.' }}</h2>
      <p class="slide-subtitle">{{ year === 'global' ? 'The all-time highest highs and lowest lows, aggregated month by month.' : 'The highest highs and lowest lows of ' + year + ', month by month.' }}</p>
      <p class="slide-explainer">{{ year === 'global' ? 'A lifetime of logging. The absolute triumphs and the colossal misfires you\\'ve witnessed across the calendar.' : 'The soaring highs and the crushing lows. Your peak cinematic triumphs and your biggest misfires, month by month.' }}</p>

      <div class="mx-grid" *ngIf="topMonths.length">
        <div *ngFor="let m of topMonths" class="mx-month-row">
          <div class="mx-month-name">{{ m.monthName }}</div>
          
          <div class="mx-movie best-movie" *ngIf="m.bestMovie">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--accent-emerald)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="mx-icon"><circle cx="12" cy="8" r="7"/><polyline points="8.21 13.89 7 23 12 20 17 23 15.79 13.88"/></svg>
            <div class="mx-info">
              <span class="mx-title">{{ m.bestMovie.title }}</span>
              <span class="mx-rating" title="Ratings imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                {{ m.bestMovie.rating | number:'1.1-1' }}
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="info-icon"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
              </span>
            </div>
          </div>
          <div class="mx-movie no-data-box" *ngIf="!m.bestMovie">No entries</div>

          <div class="mx-movie worst-movie" *ngIf="m.worstMovie">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--accent-record)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="mx-icon"><polyline points="22 17 13.5 8.5 8.5 13.5 2 7"/><polyline points="16 17 22 17 22 11"/></svg>
            <div class="mx-info">
              <span class="mx-title">{{ m.worstMovie.title }}</span>
              <span class="mx-rating" title="Ratings imported from Letterboxd (scale 1-5) have been multiplied by 2 to align with the application's 10-point scale.">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                {{ m.worstMovie.rating | number:'1.1-1' }}
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="info-icon"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
              </span>
            </div>
          </div>
          <div class="mx-movie no-data-box" *ngIf="!m.worstMovie">No entries</div>
        </div>
      </div>

      <p class="no-data" *ngIf="!topMonths.length">No monthly data available.</p>
      <div class="timecode">{{ part === 1 ? 'TC 01:50:30:05' : 'TC 01:58:14:22' }}</div>
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
    .mx-grid {
      display: flex;
      flex-direction: column;
      gap: 16px;
      width: 100%;
      max-width: 900px;
    }
    .mx-month-row {
      display: grid;
      grid-template-columns: 120px minmax(0, 1fr) minmax(0, 1fr);
      gap: 24px;
      align-items: center;
    }
    .mx-month-name {
      font-size: 1rem;
      font-weight: 700;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.1em;
      text-align: right;
    }
    .mx-movie {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px 20px;
      border-radius: 16px;
      background: rgba(255,255,255,0.02);
      border: 1px solid rgba(255,255,255,0.04);
      min-width: 0;
    }
    .mx-movie.best-movie { border-color: rgba(52, 211, 153, 0.15); }
    .mx-movie.worst-movie { border-color: rgba(244, 63, 94, 0.15); }
    .mx-icon { font-size: 1.2rem; flex-shrink: 0; }
    .mx-info { 
      display: flex; 
      flex-direction: column; 
      min-width: 0; 
      align-items: flex-start; 
      justify-content: center;
      overflow: hidden;
      flex: 1;
    }
    .mx-title {
      font-size: 1rem;
      font-weight: 600;
      color: var(--text-primary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      max-width: 100%;
    }
    .mx-rating { 
      font-size: 0.85rem; 
      color: #fbbf24; 
      display: flex; 
      align-items: center; 
      gap: 4px; 
      font-family: var(--font-mono);
    }
    .info-icon { opacity: 0.5; margin-left: 2px; }
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
