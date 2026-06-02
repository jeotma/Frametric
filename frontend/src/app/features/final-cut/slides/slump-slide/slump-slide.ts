import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CinematicFatigueExpandedDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-slump-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slump-bg">
      <div class="act-label">Act I · The Establishing Shot</div>
      <h2 class="slide-title">The Box Office Bomb.</h2>
      <p class="slide-subtitle">Even the biggest fans take a break from the multiplex.</p>
      <p class="slide-explainer">Even the greatest directors call 'cut'. The moments you stepped away from the screen.</p>

      <div class="slump-stats" *ngIf="data">
        <div class="slump-card">
          <span class="slump-icon">📉</span>
          <div class="slump-info">
            <span class="slump-value">{{ data.slumpDay }}</span>
            <span class="slump-label">Your slowest day of the week</span>
            <span class="slump-sub">Only {{ data.slumpDayWatchCount }} films in total</span>
          </div>
        </div>

        <div class="slump-card">
          <span class="slump-icon">🌨️</span>
          <div class="slump-info">
            <span class="slump-value">{{ data.slumpMonth }}</span>
            <span class="slump-label">Your quietest month</span>
            <span class="slump-sub">Just {{ data.slumpMonthWatchCount }} films that month</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">Not enough data for {{ year === 'global' ? 'All-Time' : year }}.</p>
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
    .slump-bg {
      background: radial-gradient(ellipse at 50% 30%, rgba(100, 116, 139, 0.12) 0%, transparent 60%);
    }
    .slump-stats {
      display: flex;
      flex-direction: column;
      gap: 20px;
      width: 100%;
      max-width: 560px;
    }
    .slump-card {
      display: flex;
      align-items: center;
      gap: 24px;
      padding: 28px 32px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.05);
      background: rgba(255,255,255,0.025);
      backdrop-filter: blur(12px);
    }
    .slump-icon { font-size: 2rem; }
    .slump-info { display: flex; flex-direction: column; gap: 4px; }
    .slump-value {
      font-size: 2rem;
      font-weight: 800;
      color: #94a3b8;
      letter-spacing: -0.02em;
    }
    .slump-label {
      font-size: 0.85rem;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }
    .slump-sub {
      font-size: 0.9rem;
      color: var(--text-secondary);
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class SlumpSlideComponent {
  @Input() data?: CinematicFatigueExpandedDto | null;
  @Input() year!: number | 'global';
}
