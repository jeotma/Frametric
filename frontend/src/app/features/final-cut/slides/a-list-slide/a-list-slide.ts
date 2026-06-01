import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

interface ActorCount { actorName: string; count: number; }

@Component({
  selector: 'app-a-list-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content alist-bg">
      <div class="act-label">Act II · The Cast & Crew</div>
      <h2 class="slide-title">The A-List.</h2>
      <p class="slide-subtitle">The faces that dominated your screens in {{ year }}.</p>

      <div class="actor-list" *ngIf="topActors.length">
        <div *ngFor="let a of topActors; let i = index" class="actor-row" [class.top-actor]="i === 0">
          <span class="actor-rank">{{ i === 0 ? '★' : '#' + (i + 1) }}</span>
          <span class="actor-name">{{ a.actorName }}</span>
          <span class="actor-count">{{ a.count }} film{{ a.count !== 1 ? 's' : '' }}</span>
        </div>
      </div>

      <p class="no-data" *ngIf="!topActors.length">No actor data for {{ year }}.</p>
    </div>
  `,
  styles: [`
    .alist-bg {
      background: radial-gradient(ellipse at 60% 30%, rgba(251, 113, 133, 0.1) 0%, transparent 55%);
    }
    .actor-list {
      display: flex;
      flex-direction: column;
      gap: 10px;
      width: 100%;
      max-width: 520px;
    }
    .actor-row {
      display: grid;
      grid-template-columns: 36px 1fr auto;
      align-items: center;
      gap: 16px;
      padding: 14px 20px;
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
      font-size: 0.85rem;
      font-weight: 700;
      color: var(--text-muted);
      text-align: center;
    }
    .top-actor .actor-rank { color: #fbbf24; font-size: 1.1rem; }
    .actor-name {
      font-size: 0.95rem;
      font-weight: 600;
      color: var(--text-primary);
    }
    .actor-count {
      font-size: 0.8rem;
      color: var(--text-muted);
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class AListSlideComponent {
  @Input() actors: any[] = [];
  @Input() year!: number;

  get topActors(): any[] {
    return (this.actors ?? []).slice(0, 7);
  }
}
