import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auteurs-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-silver">
      <div class="act-label">ACT II · SCENE 8 · THE AUTEURS</div>
      <h2 class="slide-title">The Auteurs.</h2>
      <p class="slide-subtitle">The visionaries who shaped your cinematic year in {{ year === 'global' ? 'All-Time' : year }}.</p>
      <p class="slide-explainer">The visionaries behind the lens. The directors whose craft you couldn't look away from.</p>

      <div class="director-list" *ngIf="topDirectors.length">
        <div *ngFor="let d of topDirectors; let i = index" class="director-row" [class.top-dir]="i === 0">
          <span class="dir-rank" *ngIf="i !== 0">#{{ i + 1 }}</span>
          <span class="dir-rank" *ngIf="i === 0">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="dir-icon"><path d="M2 10v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-9"/><path d="M2 10l5-5"/><path d="M7 5l5 5"/><path d="M12 5l5 5"/><path d="M17 5l5 5"/><path d="M22 10H2"/></svg>
          </span>
          <div class="dir-info">
            <span class="dir-name">{{ d.directorName }}</span>
          </div>
          <span class="dir-count" style="font-family: var(--font-mono)">{{ d.count }} film{{ d.count !== 1 ? 's' : '' }}</span>
        </div>
      </div>

      <p class="no-data" *ngIf="!topDirectors.length">No director data for {{ year === 'global' ? 'All-Time' : year }}.</p>
      <div class="timecode">TC 00:48:05:02</div>
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
    .director-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
      width: 100%;
      max-width: 560px;
    }
    .director-row {
      display: grid;
      grid-template-columns: 48px 1fr auto;
      align-items: center;
      gap: 16px;
      padding: 16px 20px;
      border-radius: 16px;
      border: 1px solid rgba(255,255,255,0.04);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(8px);
    }
    .director-row.top-dir {
      border-color: rgba(96, 165, 250, 0.3);
      background: rgba(96, 165, 250, 0.05);
    }
    .dir-rank { 
      font-size: 1.15rem; 
      text-align: center; 
      color: var(--text-muted); 
      font-weight: 700; 
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .top-dir .dir-rank { color: #60a5fa; }
    .dir-info { display: flex; flex-direction: column; }
    .dir-name { font-size: 1.15rem; font-weight: 600; color: var(--text-primary); }
    .dir-count { font-size: 0.95rem; color: var(--text-muted); }
    .no-data { color: var(--text-muted); }
  `]
})
export class AutoeursSlideComponent {
  @Input() directors: any[] = [];
  @Input() year!: number | 'global';

  get topDirectors(): any[] {
    return (this.directors ?? []).slice(0, 7);
  }
}
