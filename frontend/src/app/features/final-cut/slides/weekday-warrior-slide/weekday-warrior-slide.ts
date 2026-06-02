import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { WeekendWarriorDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-weekday-warrior-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content warrior-bg">
      <div class="act-label">Act I · The Establishing Shot</div>
      <h2 class="slide-title">Weekday Warrior vs Weekend Enjoyer.</h2>
      <p class="slide-subtitle">The eternal battle of the cinema-goer's soul.</p>
      <p class="slide-explainer">Escaping reality on a Tuesday or saving the blockbusters for the weekend.</p>

      <div class="vs-container" *ngIf="data">
        <div class="vs-card weekday-side" [class.winner]="data.weekdayWatches >= data.weekendWatches">
          <div class="vs-label">WEEKDAY WARRIOR</div>
          <div class="vs-big">{{ data.weekdayWatches | number }}</div>
          <div class="vs-sub">Mon → Fri</div>
          <div class="winner-badge" *ngIf="data.weekdayWatches >= data.weekendWatches">👑 Your Jam</div>
        </div>

        <div class="vs-divider">VS</div>

        <div class="vs-card weekend-side" [class.winner]="data.weekendWatches > data.weekdayWatches">
          <div class="vs-label">WEEKEND ENJOYER</div>
          <div class="vs-big">{{ data.weekendWatches | number }}</div>
          <div class="vs-sub">Sat → Sun</div>
          <div class="winner-badge" *ngIf="data.weekendWatches > data.weekdayWatches">👑 Your Jam</div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">No data available.</p>
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
    .warrior-bg {
      background: radial-gradient(ellipse at 30% 50%, rgba(59, 130, 246, 0.1) 0%, transparent 50%),
                  radial-gradient(ellipse at 70% 50%, rgba(168, 85, 247, 0.1) 0%, transparent 50%);
    }
    .vs-container {
      display: flex;
      align-items: center;
      gap: 24px;
      width: 100%;
      max-width: 640px;
    }
    .vs-card {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      padding: 36px 24px;
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
      font-size: 0.7rem;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      color: var(--text-muted);
    }
    .vs-big {
      font-size: 3.5rem;
      font-weight: 900;
      letter-spacing: -0.04em;
      color: var(--text-primary);
    }
    .weekday-side .vs-big { color: #60a5fa; }
    .weekend-side .vs-big { color: #c084fc; }
    .vs-sub {
      font-size: 0.85rem;
      color: var(--text-muted);
    }
    .winner-badge {
      font-size: 0.8rem;
      margin-top: 8px;
      padding: 4px 12px;
      border-radius: 99px;
      background: rgba(251, 191, 36, 0.15);
      color: #fbbf24;
      font-weight: 700;
    }
    .vs-divider {
      font-size: 1.5rem;
      font-weight: 900;
      color: var(--text-muted);
      letter-spacing: 0.1em;
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class WeekdayWarriorSlideComponent {
  @Input() data?: WeekendWarriorDto | null;
}
