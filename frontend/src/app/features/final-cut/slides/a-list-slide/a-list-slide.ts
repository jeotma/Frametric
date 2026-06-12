import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

interface ActorCount { actorName: string; count: number; }

@Component({
  selector: 'app-a-list-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-silver">
      <div class="act-label">ACT II · SCENE 7 · THE A-LIST</div>
      <h2 class="slide-title">The A-List.</h2>
      <p class="slide-subtitle">The faces that dominated your screens in {{ year === 'global' ? 'All-Time' : year }}.</p>
      <p class="slide-explainer">Your leading stars. The actors who captured the screen and your attention the most.</p>

      <div class="actor-list" *ngIf="topActors.length">
        <div *ngFor="let a of topActors; let i = index" class="actor-row" [class.top-actor]="i === 0">
          <span class="actor-rank" *ngIf="i !== 0">#{{ i + 1 }}</span>
          <span class="actor-rank" *ngIf="i === 0">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="currentColor" stroke="none" class="star-icon"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
          </span>
          <div class="actor-avatar">
            <img *ngIf="a.profilePath" [src]="'https://image.tmdb.org/t/p/w185' + a.profilePath" [alt]="a.actorName">
            <div *ngIf="!a.profilePath" class="actor-avatar-fallback">{{ a.actorName.charAt(0) }}</div>
          </div>
          <span class="actor-name">{{ a.actorName }}</span>
          <span class="actor-count" style="font-family: var(--font-mono)">{{ a.count }} film{{ a.count !== 1 ? 's' : '' }}</span>
        </div>
      </div>

      <p class="no-data" *ngIf="!topActors.length">No actor data for {{ year === 'global' ? 'All-Time' : year }}.</p>
      <div class="timecode">TC 00:41:20:11</div>
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
    .actor-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
      width: 100%;
      max-width: 560px;
      margin-bottom: 40px;
      flex-shrink: 0;
    }
    .actor-row {
      display: grid;
      grid-template-columns: 48px 40px 1fr auto;
      align-items: center;
      gap: 16px;
      padding: 10px 20px;
      border-radius: 16px;
      border: 1px solid rgba(255,255,255,0.04);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(8px);
      transition: background 0.2s;
    }
    .actor-row:hover { background: rgba(255,255,255,0.05); }
    .actor-row.top-actor {
      border-color: rgba(251, 191, 36, 0.3);
      background: rgba(251, 191, 36, 0.05);
    }
    .actor-rank {
      font-size: 1rem;
      font-weight: 700;
      color: var(--text-muted);
      text-align: center;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .top-actor .actor-rank { color: #fbbf24; }
    .actor-name {
      font-size: 1.15rem;
      font-weight: 600;
      color: var(--text-primary);
    }
    .actor-count {
      font-size: 0.85rem;
      color: rgba(255, 255, 255, 0.6);
      background: rgba(255, 255, 255, 0.05);
      padding: 4px 10px;
      border-radius: 8px;
      border: 1px solid rgba(255, 255, 255, 0.08);
      font-weight: 500;
    }
    .no-data { color: var(--text-muted); }
    .actor-avatar { width: 40px; height: 40px; border-radius: 50%; overflow: hidden; background: rgba(255,255,255,0.05); flex-shrink: 0; display: flex; align-items: center; justify-content: center; border: 1px solid rgba(255,255,255,0.1); }
    .actor-avatar img { width: 100%; height: 100%; object-fit: cover; }
    .actor-avatar-fallback { font-size: 1.1rem; font-weight: 700; color: var(--text-muted); }
  `]
})
export class AListSlideComponent {
  @Input() actors: any[] = [];
  @Input() year!: number | 'global';

  get topActors(): any[] {
    return (this.actors ?? []).slice(0, 7);
  }
}
