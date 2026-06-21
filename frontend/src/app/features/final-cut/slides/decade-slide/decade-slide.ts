import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-decade-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-silver">
      <div class="act-label">ACT II · SCENE 7 · THE DECADES</div>
      <h2 class="slide-title" style="font-size: 2rem;">A journey through time</h2>
      <p class="subtitle">Your favorite cinematic era.</p>
      
      <div class="timeline" *ngIf="sortedDecades.length > 0">
        <div class="timeline-item main-era">
          <span class="decade">{{ sortedDecades[0].decade }}s</span>
          <span class="movies" style="font-family: var(--font-mono)">{{ sortedDecades[0].count }} movies</span>
        </div>
        
        <div class="other-eras" *ngIf="sortedDecades.length > 1">
          <div class="mini-era" *ngFor="let era of sortedDecades.slice(1, 4)">
            <span class="d">{{ era.decade }}s</span>
            <div class="bar-container">
              <div class="bar" [style.width.%]="(era.count! / sortedDecades[0].count!) * 100"></div>
            </div>
            <span class="c" style="font-family: var(--font-mono)">{{ era.count }}</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .subtitle {
      color: var(--text-secondary);
      margin-bottom: 40px;
    }
    .timeline {
      display: flex;
      flex-direction: column;
      align-items: center;
      width: 100%;
      max-width: 500px;
    }
    .main-era {
      text-align: center;
      margin-bottom: 50px;
      animation: popIn 0.8s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards;
    }
    .decade {
      display: block;
      font-size: 6rem;
      font-weight: 900;
      color: var(--accent-emerald);
      line-height: 1;
      text-shadow: 0 0 40px rgba(16, 185, 129, 0.4);
    }
    .movies {
      font-size: 1.2rem;
      color: var(--text-primary);
      text-transform: uppercase;
      letter-spacing: 2px;
    }
    .other-eras {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    .mini-era {
      display: flex;
      align-items: center;
      gap: 16px;
      opacity: 0;
      animation: fadeIn 0.5s forwards;
      animation-delay: 0.5s;
    }
    .d {
      width: 50px;
      font-weight: 700;
      color: var(--text-secondary);
    }
    .bar-container {
      flex: 1;
      height: 8px;
      background: rgba(255, 255, 255, 0.05);
      border-radius: 4px;
      overflow: hidden;
    }
    .bar {
      height: 100%;
      background: var(--accent-emerald);
      border-radius: 4px;
    }
    .c {
      width: 30px;
      text-align: right;
      color: var(--text-muted);
      font-size: 0.9rem;
    }
    @keyframes popIn {
      0% { transform: scale(0.8); opacity: 0; }
      100% { transform: scale(1); opacity: 1; }
    }
    @keyframes fadeIn {
      to { opacity: 1; }
    }
  `]
})
export class DecadeSlideComponent {
  @Input({ required: true }) data!: WrappedSummaryDto;

  get sortedDecades() {
    return [...(this.data?.decadeBreakdown || [])].sort((a, b) => (b.count || 0) - (a.count || 0));
  }
}
