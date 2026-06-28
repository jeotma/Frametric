import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrimeTimeStatsDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-prime-time-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-sepia">
      <div class="act-label">ACT I · SCENE 3 · THE PRIME TIME BLOCKBUSTER</div>
      <h2 class="slide-title">The Prime Time Blockbuster.</h2>
      <p class="slide-subtitle">When the projector lights up brightest for you.</p>
      <p class="slide-explainer">When the lights go down. Your rhythm and preferred screening times.</p>

      <div class="peak-stats" *ngIf="data">
        <div class="peak-card main-peak">
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="peak-icon"><path d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V9h14v10zm-7-4.5l3.5-3.5-1.41-1.41L12 11.17l-1.09-1.08L9.5 11.5 12 14.5z"/></svg>
          <div class="peak-info">
            <span class="peak-name">{{ data.peakMonth }}</span>
            <span class="peak-detail">Your biggest month of the year</span>
            <span class="peak-count" style="font-family: var(--font-mono)">{{ data.peakMonthCount }} films</span>
          </div>
        </div>

        <div class="peak-card main-peak" *ngIf="data.peakDay">
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="peak-icon"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
          <div class="peak-info">
            <span class="peak-name">{{ data.peakDay }}</span>
            <span class="peak-detail">Your busiest day of the week</span>
            <span class="peak-count" style="font-family: var(--font-mono)">{{ data.peakDayCount }} films</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">No activity data available for {{ year === 'global' ? 'All-Time' : year }}.</p>
      <div class="timecode">TC 00:12:35:08</div>
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
    .peak-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 20px;
      width: 100%;
      max-width: 640px;
    }
    .peak-card {
      display: flex;
      align-items: center;
      gap: 24px;
      padding: 24px 28px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.03);
      backdrop-filter: blur(12px);
    }
    .main-peak {
      border-color: rgba(251, 191, 36, 0.25);
      background: rgba(251, 191, 36, 0.05);
    }
    .peak-icon { opacity: 0.8; flex-shrink: 0; }
    .peak-info { display: flex; flex-direction: column; gap: 4px; }
    .peak-name {
      font-size: 2.2rem;
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
      font-size: 1.15rem;
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
