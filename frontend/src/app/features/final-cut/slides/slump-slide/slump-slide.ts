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
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="slump-icon"><path d="M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z"/></svg>
          <div class="slump-info">
            <span class="slump-value" style="font-family: var(--font-mono)">{{ data.slumpMonth }}</span>
            <span class="slump-label">Your quietest month</span>
            <span class="slump-sub">Just {{ data.slumpMonthWatchCount }} films that month</span>
          </div>
        </div>

        <div class="slump-card" *ngIf="data.slumpDay">
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="slump-icon"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>
          <div class="slump-info">
            <span class="slump-value" style="font-family: var(--font-mono)">{{ data.slumpDay }}</span>
            <span class="slump-label">Your quietest day of the week</span>
            <span class="slump-sub">Just {{ data.slumpDayWatchCount }} films on that day</span>
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
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 20px;
      width: 100%;
      max-width: 640px;
    }
    .slump-card {
      display: flex;
      align-items: center;
      gap: 24px;
      padding: 24px 28px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.05);
      background: rgba(255,255,255,0.025);
      backdrop-filter: blur(12px);
    }
    .slump-icon { color: rgba(255,255,255,0.4); flex-shrink: 0; }
    .slump-info { display: flex; flex-direction: column; gap: 4px; }
    .slump-value {
      font-size: 2.2rem;
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
