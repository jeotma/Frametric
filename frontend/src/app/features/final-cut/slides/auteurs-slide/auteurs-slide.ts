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
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="dir-icon"><path d="M18 4l2 4h-3l-2-4h-2l2 4h-3l-2-4H8l2 4H7L5 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V4h-4z"/></svg>
          </span>
          <div class="dir-info">
            <div class="dir-avatar">
              <img *ngIf="d.profilePath" [src]="'https://image.tmdb.org/t/p/w185' + d.profilePath" [alt]="d.directorName">
              <div *ngIf="!d.profilePath" class="dir-avatar-fallback">{{ d.directorName?.charAt(0) }}</div>
            </div>
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
      margin-bottom: 20px;
      font-style: italic;
      max-width: 600px;
      text-align: center;
    }
    .director-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
      width: 100%;
      max-width: 560px;
      margin-bottom: 40px;
      flex-shrink: 0;
    }
    .director-row {
      display: grid;
      grid-template-columns: 48px 1fr auto;
      align-items: center;
      gap: 16px;
      padding: 10px 20px;
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
    .dir-info { display: flex; flex-direction: row; align-items: center; gap: 12px; }
    .dir-avatar { width: 40px; height: 40px; border-radius: 50%; overflow: hidden; background: rgba(255,255,255,0.05); flex-shrink: 0; display: flex; align-items: center; justify-content: center; border: 1px solid rgba(255,255,255,0.1); }
    .dir-avatar img { width: 100%; height: 100%; object-fit: cover; }
    .dir-avatar-fallback { font-size: 1.1rem; font-weight: 700; color: var(--text-muted); }
    .dir-name { font-size: 1.15rem; font-weight: 600; color: var(--text-primary); }
    .director-count {
      font-size: 0.85rem;
      color: rgba(255, 255, 255, 0.6);
      background: rgba(255, 255, 255, 0.05);
      padding: 4px 10px;
      border-radius: 8px;
      border: 1px solid rgba(255, 255, 255, 0.08);
      font-weight: 500;
    }.no-data { color: var(--text-muted); }
  `]
})
export class AutoeursSlideComponent {
  @Input() directors: any[] = [];
  @Input() year!: number | 'global';

  get topDirectors(): any[] {
    return (this.directors ?? []).slice(0, 7);
  }
}
