import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auteurs-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content auteurs-bg">
      <div class="act-label">Act II · The Cast & Crew</div>
      <h2 class="slide-title">The Auteurs.</h2>
      <p class="slide-subtitle">The visionaries who shaped your cinematic year in {{ year }}.</p>

      <div class="director-list" *ngIf="topDirectors.length">
        <div *ngFor="let d of topDirectors; let i = index" class="director-row" [class.top-dir]="i === 0">
          <span class="dir-rank">{{ i === 0 ? '🎬' : '#' + (i + 1) }}</span>
          <div class="dir-info">
            <span class="dir-name">{{ d.directorName }}</span>
          </div>
          <span class="dir-count">{{ d.count }} film{{ d.count !== 1 ? 's' : '' }}</span>
        </div>
      </div>

      <p class="no-data" *ngIf="!topDirectors.length">No director data for {{ year }}.</p>
    </div>
  `,
  styles: [`
    .auteurs-bg {
      background: radial-gradient(ellipse at 30% 60%, rgba(96, 165, 250, 0.1) 0%, transparent 55%);
    }
    .director-list {
      display: flex;
      flex-direction: column;
      gap: 10px;
      width: 100%;
      max-width: 520px;
    }
    .director-row {
      display: grid;
      grid-template-columns: 40px 1fr auto;
      align-items: center;
      gap: 16px;
      padding: 14px 20px;
      border-radius: 16px;
      border: 1px solid rgba(255,255,255,0.04);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(8px);
    }
    .director-row.top-dir {
      border-color: rgba(96, 165, 250, 0.3);
      background: rgba(96, 165, 250, 0.05);
    }
    .dir-rank { font-size: 1rem; text-align: center; color: var(--text-muted); font-weight: 700; }
    .top-dir .dir-rank { font-size: 1.2rem; }
    .dir-info { display: flex; flex-direction: column; }
    .dir-name { font-size: 0.95rem; font-weight: 600; color: var(--text-primary); }
    .dir-count { font-size: 0.8rem; color: var(--text-muted); }
    .no-data { color: var(--text-muted); }
  `]
})
export class AutoeursSlideComponent {
  @Input() directors: any[] = [];
  @Input() year!: number;

  get topDirectors(): any[] {
    return (this.directors ?? []).slice(0, 7);
  }
}
