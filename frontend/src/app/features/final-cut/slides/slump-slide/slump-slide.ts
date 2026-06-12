import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CinematicFatigueExpandedDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-slump-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-record">
      <div class="act-label">ACT I · SCENE 4 · THE BOX OFFICE BOMB</div>
      <h2 class="slide-title">The Box Office Bomb.</h2>
      <p class="slide-subtitle">Even the biggest fans take a break from the multiplex.</p>
      <p class="slide-explainer">Even the greatest directors call 'cut'. The moments you stepped away from the screen.</p>

      <div class="slump-stats" *ngIf="data">
        <div class="slump-card">
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="slump-icon"><polyline points="22 17 13.5 8.5 8.5 13.5 2 7"/><polyline points="16 17 22 17 22 11"/></svg>
          <div class="slump-info">
            <span class="slump-value" style="font-family: var(--font-mono)">{{ data.slumpDay }}</span>
            <span class="slump-label">Your slowest day of the week</span>
            <span class="slump-sub">Only {{ data.slumpDayWatchCount }} films in total</span>
          </div>
        </div>

        <div class="slump-card">
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="slump-icon"><path d="M20 16.2A4.5 4.5 0 0 0 17.5 8h-1.8A7 7 0 1 0 4 14.9"/><path d="M8 15v.01"/><path d="M8 19v.01"/><path d="M12 17v.01"/><path d="M12 21v.01"/><path d="M16 15v.01"/><path d="M16 19v.01"/></svg>
          <div class="slump-info">
            <span class="slump-value" style="font-family: var(--font-mono)">{{ data.slumpMonth }}</span>
            <span class="slump-label">Your quietest month</span>
            <span class="slump-sub">Just {{ data.slumpMonthWatchCount }} films that month</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!data">Not enough data for {{ year === 'global' ? 'All-Time' : year }}.</p>
      <div class="timecode">TC 00:18:42:22</div>
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
    .slump-stats {
      display: flex;
      flex-direction: column;
      gap: 20px;
      width: 100%;
      max-width: 640px;
    }
    .slump-card {
      display: flex;
      align-items: center;
      gap: 32px;
      padding: 32px 40px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.05);
      background: rgba(255,255,255,0.025);
      backdrop-filter: blur(12px);
    }
    .slump-icon { color: rgba(255,255,255,0.4); }
    .slump-info { display: flex; flex-direction: column; gap: 4px; }
    .slump-value {
      font-size: 2.5rem;
      font-weight: 800;
      color: #94a3b8;
      letter-spacing: -0.02em;
    }
    .slump-label {
      font-size: 0.95rem;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }
    .slump-sub {
      font-size: 1rem;
      color: var(--text-secondary);
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class SlumpSlideComponent {
  @Input() data?: CinematicFatigueExpandedDto | null;
  @Input() year!: number | 'global';
}
