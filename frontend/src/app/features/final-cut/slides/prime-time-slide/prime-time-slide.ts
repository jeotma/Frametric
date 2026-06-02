import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimeTimeStatsDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-prime-time-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content prime-time-bg">
      <div class="act-label">Act I · The Establishing Shot</div>
      <h2 class="slide-title">The Prime Time Blockbuster.</h2>
      <p class="slide-subtitle">When the projector lights up brightest for you.</p>
      <p class="slide-explainer">When the lights go down. Your rhythm and preferred screening times.</p>

      <div class="peak-stats" *ngIf="data">
        <div class="peak-card main-peak">
          <div class="peak-icon">📅</div>
          <div class="peak-info">
            <span class="peak-name">{{ data.peakDay }}</span>
            <span class="peak-detail">Your busiest day of the week</span>
            <span class="peak-count">{{ data.peakDayCount }} films</span>
          </div>
        </div>

        <div class="peak-card">
          <div class="peak-icon">🗓️</div>
          <div class="peak-info">
            <span class="peak-name">{{ data.peakMonth }}</span>
            <span class="peak-detail">Your biggest month of the year</span>
            <span class="peak-count">{{ data.peakMonthCount }} films</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">No activity data available for {{ year === 'global' ? 'All-Time' : year }}.</p>
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
    .prime-time-bg {
      background: radial-gradient(ellipse at 30% 40%, rgba(251, 191, 36, 0.1) 0%, transparent 60%),
                  radial-gradient(ellipse at 70% 80%, rgba(139, 92, 246, 0.08) 0%, transparent 50%);
    }
    .peak-stats {
      display: flex;
      flex-direction: column;
      gap: 20px;
      width: 100%;
      max-width: 560px;
    }
    .peak-card {
      display: flex;
      align-items: center;
      gap: 24px;
      padding: 28px 32px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.03);
      backdrop-filter: blur(12px);
    }
    .main-peak {
      border-color: rgba(251, 191, 36, 0.25);
      background: rgba(251, 191, 36, 0.05);
    }
    .peak-icon { font-size: 2rem; }
    .peak-info { display: flex; flex-direction: column; gap: 4px; }
    .peak-name {
      font-size: 2rem;
      font-weight: 800;
      color: #fbbf24;
      letter-spacing: -0.02em;
    }
    .peak-detail {
      font-size: 0.85rem;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }
    .peak-count {
      font-size: 1rem;
      color: var(--text-secondary);
      font-weight: 600;
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class PrimeTimeSlideComponent {
  @Input() data?: PrimeTimeStatsDto | null;
  @Input() year!: number | 'global';
}
