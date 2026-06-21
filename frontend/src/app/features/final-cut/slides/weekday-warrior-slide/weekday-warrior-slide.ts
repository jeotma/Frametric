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
        <div class="vs-card weekday-side" [class.winner]="data.weekdayWatches >= data.weekendWatches">
          <div class="vs-label">WEEKDAY WARRIOR</div>
          <div class="vs-big" style="font-family: var(--font-mono)">{{ data.weekdayWatches | number }}</div>
          <div class="vs-sub">Mon → Fri</div>
          <div class="winner-badge" *ngIf="data.weekdayWatches >= data.weekendWatches">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="crown-icon"><polygon points="12 2 15 8 22 7 18 13 20 20 12 18 4 20 6 13 2 7 9 8 12 2"/></svg>
            Your Jam
          </div>
        </div>

        <div class="vs-divider">VS</div>

        <div class="vs-card weekend-side" [class.winner]="data.weekendWatches > data.weekdayWatches">
          <div class="vs-label">WEEKEND ENJOYER</div>
          <div class="vs-big" style="font-family: var(--font-mono)">{{ data.weekendWatches | number }}</div>
          <div class="vs-sub">Sat → Sun</div>
          <div class="winner-badge" *ngIf="data.weekendWatches > data.weekdayWatches">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="crown-icon"><polygon points="12 2 15 8 22 7 18 13 20 20 12 18 4 20 6 13 2 7 9 8 12 2"/></svg>
            Your Jam
          </div>
        </div>
      </div>

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
      gap: 12px;
      padding: 48px 32px;
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
      font-size: 4.5rem;
      font-weight: 900;
      letter-spacing: -0.04em;
      color: var(--text-primary);
    }
    .weekday-side .vs-big { color: #60a5fa; }
    .weekend-side .vs-big { color: var(--accent-silver); }
    .vs-sub {
      font-size: 1rem;
      color: var(--text-muted);
    }
    .winner-badge {
      font-size: 0.9rem;
      margin-top: 12px;
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
    .no-data { color: var(--text-muted); }
  `]
})
export class WeekdayWarriorSlideComponent {
  @Input() data?: WeekendWarriorDto | null;
}
