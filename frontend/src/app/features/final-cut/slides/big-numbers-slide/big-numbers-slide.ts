import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-big-numbers-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content big-numbers-bg">
      <div class="act-label">Act I · The Establishing Shot</div>
      <h2 class="slide-title">The Box Office Receipts.</h2>
      <p class="slide-subtitle">Here's what {{ year }} looked like in raw numbers.</p>

      <div class="numbers-grid">
        <div class="number-card accent-purple">
          <span class="n-value">{{ summary.totalWatches | number }}</span>
          <span class="n-label">Total Screenings</span>
        </div>
        <div class="number-card accent-gold">
          <span class="n-value">{{ summary.uniqueMoviesCount | number }}</span>
          <span class="n-label">Unique Titles</span>
        </div>
        <div class="number-card accent-teal">
          <span class="n-value">{{ totalHours | number:'1.0-0' }}</span>
          <span class="n-label">Hours Watched</span>
        </div>
        <div class="number-card accent-rose">
          <span class="n-value">{{ totalDays | number:'1.1-1' }}</span>
          <span class="n-label">Days of Your Life</span>
        </div>
        <div class="number-card accent-blue" *ngIf="summary.topGenres.length">
          <span class="n-value">{{ summary.topGenres![0].genreName }}</span>
          <span class="n-label">Dominant Genre</span>
        </div>
        <div class="number-card accent-green">
          <span class="n-value">{{ avgPerMonth | number:'1.1-1' }}</span>
          <span class="n-label">Films / Month</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .big-numbers-bg {
      background: radial-gradient(ellipse at 20% 50%, rgba(139, 92, 246, 0.12) 0%, transparent 60%),
                  radial-gradient(ellipse at 80% 20%, rgba(234, 179, 8, 0.08) 0%, transparent 50%);
    }
    .numbers-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      width: 100%;
      max-width: 720px;
      margin-top: 32px;
    }
    .number-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 28px 16px;
      border-radius: 20px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.03);
      backdrop-filter: blur(12px);
      transition: transform 0.2s ease;
      gap: 8px;
    }
    .number-card:hover { transform: translateY(-4px); }
    .n-value {
      font-size: 2.4rem;
      font-weight: 800;
      line-height: 1;
      letter-spacing: -0.02em;
    }
    .n-label {
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: var(--text-muted);
      text-align: center;
    }
    .accent-purple .n-value { color: #a78bfa; }
    .accent-gold .n-value { color: #fbbf24; }
    .accent-teal .n-value { color: #2dd4bf; }
    .accent-rose .n-value { color: #fb7185; }
    .accent-blue .n-value { color: #60a5fa; }
    .accent-green .n-value { color: #4ade80; }
  `]
})
export class BigNumbersSlideComponent {
  @Input({ required: true }) summary!: WrappedSummaryDto;
  @Input() year!: number;

  get totalHours(): number {
    return (this.summary.totalWatchtimeMinutes ?? 0) / 60;
  }
  get totalDays(): number {
    return this.totalHours / 24;
  }
  get avgPerMonth(): number {
    return (this.summary.totalWatches ?? 0) / 12;
  }
}
