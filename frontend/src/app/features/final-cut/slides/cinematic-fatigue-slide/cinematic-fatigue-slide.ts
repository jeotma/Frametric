import { Component, Input } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { CinematicFatigueExpandedDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-cinematic-fatigue-slide',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <div class="slide-content slide-bg-silver">
      <div class="act-label">ACT III · SCENE 14 · CINEMATIC FATIGUE</div>
      <h2 class="slide-title">Cinematic Fatigue.</h2>
      <p class="slide-subtitle">The binge toll: Does watching more make you enjoy it less?</p>
      <p class="slide-explainer">Quality vs. Quantity. How your ratings shifted during your movie marathons.</p>

      <div class="fatigue-container" *ngIf="data">
        <div class="fatigue-card">
          <div class="f-row">
            <span class="f-label">On Light Days (1 film)</span>
            <span class="f-value" 
                  [ngClass]="{
                    'higher': data.avgRatingLightDays > data.avgRatingHeavyDays,
                    'lower': data.avgRatingLightDays < data.avgRatingHeavyDays,
                    'equal': data.avgRatingLightDays === data.avgRatingHeavyDays
                  }"
                  style="font-family: var(--font-mono)">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
              {{ data.avgRatingLightDays | number:'1.2-2' }}
            </span>
          </div>
          <div class="f-divider"></div>
          <div class="f-row">
            <span class="f-label">On Heavy Days (2+ films)</span>
            <span class="f-value" 
                  [ngClass]="{
                    'higher': data.avgRatingHeavyDays > data.avgRatingLightDays,
                    'lower': data.avgRatingHeavyDays < data.avgRatingLightDays,
                    'equal': data.avgRatingHeavyDays === data.avgRatingLightDays
                  }"
                  style="font-family: var(--font-mono)">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
              {{ data.avgRatingHeavyDays | number:'1.2-2' }}
            </span>
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
      <div class="timecode">TC 01:34:12:08</div>
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
    .fatigue-container {
      display: flex;
      flex-direction: column;
      gap: 32px;
      width: 100%;
      max-width: 640px;
    }
    .fatigue-card {
      display: flex;
      flex-direction: column;
      padding: 40px;
      border-radius: 24px;
      border: 1px solid rgba(255,255,255,0.06);
      background: rgba(255,255,255,0.03);
      backdrop-filter: blur(12px);
    }
    .f-row { display: flex; justify-content: space-between; align-items: center; }
    .f-divider { height: 1px; background: rgba(255,255,255,0.08); margin: 24px 0; }
    .f-label { font-size: 1.15rem; color: var(--text-secondary); }
    .f-value { 
      font-size: 1.7rem; 
      font-weight: 700; 
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .f-value.higher { color: var(--accent-emerald); }
    .f-value.lower { color: var(--accent-record); }
    .f-value.equal { color: #fbbf24; }
    .fatigue-insight {
      font-size: 1.15rem;
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
