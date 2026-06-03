import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { CinematicFatigueExpandedDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-cinematic-fatigue-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content fatigue-bg">
      <div class="act-label">Act III · The Deep Cuts</div>
      <h2 class="slide-title">Cinematic Fatigue.</h2>
      <p class="slide-subtitle">The binge toll: Does watching more make you enjoy it less?</p>
      <p class="slide-explainer">Quality vs. Quantity. How your ratings shifted during your movie marathons.</p>

      <div class="fatigue-container" *ngIf="data">
        <div class="fatigue-card">
          <div class="f-row">
            <span class="f-label">On Light Days (1 film)</span>
            <span class="f-value">⭐ {{ data.avgRatingLightDays | number:'1.2-2' }}</span>
          </div>
          <div class="f-divider"></div>
          <div class="f-row">
            <span class="f-label">On Heavy Days (2+ films)</span>
            <span class="f-value" [class.lower]="data.avgRatingHeavyDays < data.avgRatingLightDays">⭐ {{ data.avgRatingHeavyDays | number:'1.2-2' }}</span>
          </div>
        </div>

        <p class="fatigue-insight" *ngIf="data.avgRatingHeavyDays < data.avgRatingLightDays">
          You tend to be <strong>harsher</strong> when binge-watching. Take a break!
        </p>
        <p class="fatigue-insight" *ngIf="data.avgRatingHeavyDays >= data.avgRatingLightDays">
          Your critical eye remains <strong>sharp</strong> no matter how many films you watch.
        </p>
      </div>

      <p class="no-data" *ngIf="!data">No cinematic fatigue data.</p>
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
    .fatigue-bg {
      background: radial-gradient(ellipse at 70% 30%, rgba(244, 63, 94, 0.08) 0%, transparent 60%);
    }
    .fatigue-container {
      display: flex;
      flex-direction: column;
      gap: 24px;
      width: 100%;
      max-width: 520px;
    }
    .fatigue-card {
      display: flex;
      flex-direction: column;
      padding: 32px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.03);
      backdrop-filter: blur(12px);
    }
    .f-row { display: flex; justify-content: space-between; align-items: center; }
    .f-divider { height: 1px; background: rgba(255,255,255,0.08); margin: 20px 0; }
    .f-label { font-size: 1rem; color: var(--text-secondary); }
    .f-value { font-size: 1.4rem; font-weight: 700; color: #fbbf24; }
    .f-value.lower { color: #f43f5e; }
    .fatigue-insight {
      font-size: 1rem;
      color: var(--text-muted);
      text-align: center;
      line-height: 1.5;
    }
    .fatigue-insight strong { color: var(--text-primary); }
    .no-data { color: var(--text-muted); }
  `]
})
export class CinematicFatigueSlideComponent {
  @Input() data?: CinematicFatigueExpandedDto | null;
  @Input() ratingEvolution: any[] = [];
}
