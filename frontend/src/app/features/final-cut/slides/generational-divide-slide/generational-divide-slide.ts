import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-generational-divide-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content gen-bg">
      <div class="act-label">Act II · The Cast & Crew</div>
      <h2 class="slide-title">The Generational Divide.</h2>
      <p class="slide-subtitle">Which era of cinema did you call home?</p>
      <p class="slide-explainer">Time traveling through cinema. How your watches spanned the decades.</p>

      <div class="decades-viz" *ngIf="topDecades.length">
        <div *ngFor="let d of topDecades; let i = index" class="decade-row">
          <span class="decade-name">{{ d.decade }}s</span>
          <div class="decade-bar-wrap">
            <div class="decade-bar" [style.width.%]="barWidth(d.count)" [style.background]="barColor(i)"></div>
          </div>
          <span class="decade-count">{{ d.count }}</span>
        </div>
      </div>

      <div class="era-badge" *ngIf="era">
        <div class="era-label">Your Predominant Era</div>
        <div class="era-name">{{ era.era }}</div>
      </div>

      <p class="no-data" *ngIf="!topDecades.length">No decade data available.</p>
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
    .gen-bg {
      background: radial-gradient(ellipse at 40% 50%, rgba(251, 191, 36, 0.08) 0%, transparent 60%);
    }
    .decades-viz {
      display: flex;
      flex-direction: column;
      gap: 10px;
      width: 100%;
      max-width: 560px;
    }
    .decade-row {
      display: grid;
      grid-template-columns: 56px 1fr 40px;
      align-items: center;
      gap: 14px;
    }
    .decade-name {
      font-size: 0.85rem;
      font-weight: 700;
      color: var(--text-muted);
      text-align: right;
    }
    .decade-bar-wrap {
      height: 10px;
      background: rgba(255,255,255,0.05);
      border-radius: 99px;
      overflow: hidden;
    }
    .decade-bar {
      height: 100%;
      border-radius: 99px;
      transition: width 1s ease;
    }
    .decade-count { font-size: 0.8rem; color: var(--text-muted); }
    .era-badge {
      margin-top: 24px;
      padding: 16px 28px;
      border-radius: 16px;
      border: 1px solid rgba(251, 191, 36, 0.25);
      background: rgba(251, 191, 36, 0.05);
      text-align: center;
    }
    .era-label {
      font-size: 0.7rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: var(--text-muted);
    }
    .era-name {
      font-size: 1.4rem;
      font-weight: 800;
      color: #fbbf24;
      margin-top: 4px;
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class GenerationalDivideSlideComponent {
  @Input() decades: any[] = [];
  @Input() era: any;

  private COLORS = ['#a78bfa','#60a5fa','#34d399','#fbbf24','#fb7185','#2dd4bf','#f472b6','#818cf8'];

  get topDecades(): any[] {
    return (this.decades ?? []).sort((a, b) => b.count - a.count).slice(0, 7);
  }

  barWidth(count: number): number {
    const max = this.topDecades[0]?.count ?? 1;
    return (count / max) * 100;
  }

  barColor(index: number): string {
    return this.COLORS[index % this.COLORS.length];
  }
}
