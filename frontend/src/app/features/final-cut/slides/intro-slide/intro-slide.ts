import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-intro-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content intro-bg">
      <h2 class="slide-title">The {{ username }}'s Cut.</h2>
      <p class="slide-explainer">Grab your popcorn and take a seat. The show is about to begin.</p>
      <p class="subtitle">Relive your year through the art of filmmaking.</p>
      
      <div class="stats-grid">
        <div class="stat-box">
          <span class="label">Movies Watched</span>
          <span class="slide-value">{{ data.totalWatches }}</span>
        </div>
        <div class="stat-box">
          <span class="label">Time Spent (Hours)</span>
          <span class="slide-value">{{ (data.totalWatchtimeMinutes / 60) | number:'1.0-0' }}</span>
        </div>
      </div>
      <p class="footer-note">Tap the right side of the screen to continue.</p>
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
    .intro-bg {
      background: radial-gradient(circle at center, rgba(139, 92, 246, 0.15) 0%, transparent 70%);
    }
    .subtitle {
      font-size: 1.25rem;
      color: var(--text-secondary);
      margin-bottom: 40px;
    }
    .stats-grid {
      display: flex;
      gap: 32px;
      margin-top: 20px;
    }
    .stat-box {
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.05);
      padding: 32px;
      border-radius: 24px;
      min-width: 200px;
      backdrop-filter: blur(10px);
    }
    .label {
      display: block;
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: var(--text-muted);
    }
    .slide-value {
      display: block;
      margin: 10px 0 0 0 !important;
      font-size: 4rem !important;
    }
    .footer-note {
      position: absolute;
      bottom: 40px;
      color: rgba(255, 255, 255, 0.3);
      font-size: 0.85rem;
      animation: pulse 2s infinite;
    }
    @keyframes pulse {
      0%, 100% { opacity: 0.5; }
      50% { opacity: 1; }
    }
  `]
})
export class IntroSlideComponent {
  @Input({ required: true }) data!: WrappedSummaryDto;
  @Input() year!: number | 'global';
  @Input() username!: string;
}
