import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { WeekendWarriorDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-weekday-warrior-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-emerald">
      <div class="act-label">ACT I · SCENE 5 · THE WEEKDAY WARRIOR</div>
      <h2 class="slide-title">Weekday Warrior vs Weekend Enjoyer.</h2>
      <p class="slide-subtitle">The eternal battle of the cinema-goer's soul.</p>
      <p class="slide-explainer">Escaping reality on a Tuesday or saving the blockbusters for the weekend.</p>

      <div class="vs-container" *ngIf="data">
        <div class="vs-card weekday-side" [class.winner]="isWeekdayWinner || isTie">
          <div class="vs-label">WEEKDAY WARRIOR</div>
          <div class="vs-big" style="font-family: var(--font-mono)">{{ weekdayAvg | number:'1.3-3' }}</div>
          <div class="vs-avg-label">Avg films / day</div>
          <div class="vs-sub">Mon → Fri</div>
          <div class="vs-total">Total: {{ data.weekdayWatches | number }} films</div>
          <div class="winner-badge" *ngIf="isWeekdayWinner || isTie">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="crown-icon"><polygon points="12 2 15 8 22 7 18 13 20 20 12 18 4 20 6 13 2 7 9 8 12 2"/></svg>
            {{ isTie ? 'Balanced' : 'Your jam' }}
          </div>
        </div>

        <div class="vs-divider">VS</div>

        <div class="vs-card weekend-side" [class.winner]="!isWeekdayWinner || isTie">
          <div class="vs-label">WEEKEND ENJOYER</div>
          <div class="vs-big" style="font-family: var(--font-mono)">{{ weekendAvg | number:'1.3-3' }}</div>
          <div class="vs-avg-label">Avg films / day</div>
          <div class="vs-sub">Sat → Sun</div>
          <div class="vs-total">Total: {{ data.weekendWatches | number }} films</div>
          <div class="winner-badge" *ngIf="!isWeekdayWinner || isTie">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="crown-icon"><polygon points="12 2 15 8 22 7 18 13 20 20 12 18 4 20 6 13 2 7 9 8 12 2"/></svg>
            {{ isTie ? 'Balanced' : 'Your Jam' }}
          </div>
        </div>
      </div>

      <p class="vs-explanation" *ngIf="data">
        <span *ngIf="isTie">
          🤝 <strong>It's a perfect match!</strong> You watched movies at the exact same daily pace on both weekdays and weekends.
        </span>
        <span *ngIf="!isTie && isWeekdayWinner">
          🏆 Winner decided by <strong>daily pace</strong>: you managed to maintain a higher watch rate during the busy workweek!
        </span>
        <span *ngIf="!isTie && !isWeekdayWinner">
          🏆 Winner decided by <strong>daily pace</strong>: weekends are much shorter, meaning you watch movies at a higher density rate!
        </span>
      </p>

      <p class="no-data" *ngIf="!data">No data available.</p>
      <div class="timecode">TC 00:24:10:05</div>
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
    .vs-container {
      display: flex;
      align-items: center;
      gap: 32px;
      width: 100%;
      max-width: 800px;
    }
    .vs-card {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 10px;
      padding: 40px 32px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.03);
      backdrop-filter: blur(12px);
      position: relative;
      transition: transform 0.3s ease;
    }
    .vs-card.winner {
      border-color: rgba(251, 191, 36, 0.3);
      background: rgba(251, 191, 36, 0.05);
      transform: scale(1.04);
    }
    .vs-label {
      font-size: 0.85rem;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      color: var(--text-muted);
    }
    .vs-big {
      font-size: 4rem;
      font-weight: 900;
      letter-spacing: -0.04em;
      color: var(--text-primary);
      line-height: 1;
    }
    .weekday-side .vs-big { color: #60a5fa; }
    .weekend-side .vs-big { color: var(--accent-silver); }
    .vs-avg-label {
      font-size: 0.85rem;
      color: var(--text-secondary);
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .vs-sub {
      font-size: 0.95rem;
      color: var(--text-muted);
    }
    .vs-total {
      font-size: 0.9rem;
      color: rgba(255, 255, 255, 0.5);
    }
    .winner-badge {
      font-size: 0.9rem;
      margin-top: 8px;
      padding: 6px 16px;
      border-radius: 99px;
      background: rgba(16, 185, 129, 0.15);
      color: var(--accent-emerald);
      font-weight: 700;
      display: flex;
      align-items: center;
      gap: 6px;
    }
    .vs-divider {
      font-size: 1.5rem;
      font-weight: 900;
      color: var(--text-muted);
      letter-spacing: 0.1em;
    }
    .vs-explanation {
      font-size: 0.95rem;
      color: var(--text-muted);
      margin-top: 24px;
      text-align: center;
      max-width: 600px;
      line-height: 1.4;
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class WeekdayWarriorSlideComponent {
  @Input() data?: WeekendWarriorDto | null;

  get weekdayAvg(): number {
    return this.data?.weekdayAverage ?? 0;
  }

  get weekendAvg(): number {
    return this.data?.weekendAverage ?? 0;
  }

  get isTie(): boolean {
    return Math.abs(this.weekdayAvg - this.weekendAvg) < 0.0001;
  }

  get isWeekdayWinner(): boolean {
    return this.weekdayAvg > this.weekendAvg;
  }
}
