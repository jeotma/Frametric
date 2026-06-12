import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';
import { EasterEggPipe } from '../../../../core/services/easter-egg.pipe';

@Component({
  selector: 'app-big-numbers-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe, EasterEggPipe],
  template: `
    <div class="slide-content slide-bg-record">
      <div class="act-label">ACT I · SCENE 2 · THE BOX OFFICE RECEIPTS</div>
      <h2 class="slide-title">The Box Office Receipts.</h2>
      <p class="slide-subtitle">Here's what {{ year === 'global' ? 'All-Time' : year }} looked like in raw numbers.</p>
      <p class="slide-explainer">Every frame watched is a moment lived. The raw scale of your cinematic journey.</p>

      <div class="numbers-grid">
        <div class="number-card accent-purple">
          @let watchesEE = summary.totalWatches | easterEgg:'watches';
          <span class="n-value" style="font-family: var(--font-mono)" [class]="watchesEE ? watchesEE.className : ''" [attr.data-tooltip]="watchesEE ? watchesEE.tooltip : null">
            {{ summary.totalWatches | number }} {{ watchesEE ? watchesEE.text : '' }}
          </span>
          <span class="n-label">Total Screenings</span>
        </div>
        <div class="number-card accent-gold">
          @let uniqueEE = summary.uniqueMoviesCount | easterEgg:'unique';
          <span class="n-value" style="font-family: var(--font-mono)" [class]="uniqueEE ? uniqueEE.className : ''" [attr.data-tooltip]="uniqueEE ? uniqueEE.tooltip : null">
            {{ summary.uniqueMoviesCount | number }} {{ uniqueEE ? uniqueEE.text : '' }}
          </span>
          <span class="n-label">Unique Titles</span>
        </div>
        <div class="number-card accent-teal">
          @let hoursEE = totalHours | easterEgg:'hours';
          <span class="n-value" style="font-family: var(--font-mono)" [class]="hoursEE ? hoursEE.className : ''" [attr.data-tooltip]="hoursEE ? hoursEE.tooltip : null">
            {{ totalHours | number:'1.0-0' }} {{ hoursEE ? hoursEE.text : '' }}
          </span>
          <span class="n-label">Hours Watched</span>
        </div>
        <div class="number-card accent-record">
          @let daysEE = (totalDays | number:'1.1-1') | easterEgg:'days';
          <span class="n-value" style="font-family: var(--font-mono)" [class]="daysEE ? daysEE.className : ''" [attr.data-tooltip]="daysEE ? daysEE.tooltip : null">
            {{ totalDays | number:'1.1-1' }} {{ daysEE ? daysEE.text : '' }}
          </span>
          <span class="n-label">Days of Your Life</span>
        </div>
        <div class="number-card accent-blue" *ngIf="summary.topGenres.length">
          <span class="n-value" style="font-family: var(--font-mono)">{{ summary.topGenres![0].genreName }}</span>
          <span class="n-label">Dominant Genre</span>
        </div>
        <div class="number-card accent-green">
          <span class="n-value" style="font-family: var(--font-mono)">{{ avgPerMonth | number:'1.1-1' }}</span>
          <span class="n-label">Films / Month</span>
        </div>
      </div>
      <div class="timecode">TC 00:04:12:15</div>
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
    .numbers-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 24px;
      width: 100%;
      max-width: 900px;
      margin-top: 32px;
    }
    .number-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 36px 20px;
      border-radius: 20px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.03);
      backdrop-filter: blur(12px);
      transition: transform 0.2s ease;
      gap: 8px;
    }
    .number-card:hover { transform: translateY(-4px); }
    .n-value {
      font-size: 3rem;
      font-weight: 800;
      line-height: 1;
      letter-spacing: -0.02em;
    }
    .n-label {
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: var(--text-muted);
      text-align: center;
    }
    .accent-purple .n-value { color: var(--accent-silver); }
    .accent-gold .n-value { color: #fbbf24; }
    .accent-teal .n-value { color: #2dd4bf; }
    .accent-record .n-value { color: #fca5a5; }
    .accent-blue .n-value { color: #60a5fa; }
    .accent-green .n-value { color: #4ade80; }
  `]
})
export class BigNumbersSlideComponent {
  @Input({ required: true }) summary!: WrappedSummaryDto;
  @Input() year!: number | 'global';

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
