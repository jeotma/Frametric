import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DirectorActorPairDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-dynamic-duos-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content duos-bg">
      <div class="act-label">Act II · The Cast & Crew</div>
      <h2 class="slide-title">Dynamic Duos & Perfect Pairs.</h2>
      <p class="slide-subtitle">The director-actor collaborations you returned to again and again.</p>
      <p class="slide-explainer">Iconic collaborations. The director-actor pairings that sparked magic.</p>

      <div class="pairs-section" *ngIf="topPairs.length">
        <div *ngFor="let p of topPairs; let i = index" class="pair-card" [class.top-pair]="i === 0">
          <div class="pair-badge">#{{ i + 1 }}</div>
          <div class="pair-info">
            <span class="pair-director">🎬 {{ p.directorName }}</span>
            <span class="pair-connector">+</span>
            <span class="pair-actor">🎭 {{ p.actorName }}</span>
          </div>
          <div class="pair-count">{{ p.collaborationCount }} films together</div>
        </div>
      </div>

      <div class="casting-pairs-section" *ngIf="castingPairs.length">
        <p class="section-note">Actor pairs that kept crossing paths:</p>
        <div *ngFor="let p of castingPairs.slice(0, 3); let i = index" class="pair-card">
          <div class="pair-badge">#{{ i + 1 }}</div>
          <div class="pair-info">
            <span class="pair-director">🎭 {{ p.actor1Name }}</span>
            <span class="pair-connector">+</span>
            <span class="pair-actor">🎭 {{ p.actor2Name }}</span>
          </div>
          <div class="pair-count">{{ p.collaborationCount }} films together</div>
        </div>
      </div>

      <p class="no-data" *ngIf="!topPairs.length && !castingPairs.length">
        No recurring collaborations found yet.
      </p>
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
    .duos-bg {
      background: radial-gradient(ellipse at 50% 40%, rgba(52, 211, 153, 0.08) 0%, transparent 55%);
    }
    .pairs-section, .casting-pairs-section {
      display: flex;
      flex-direction: column;
      gap: 10px;
      width: 100%;
      max-width: 660px;
    }
    .section-note {
      font-size: 0.8rem;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.08em;
      margin-bottom: 4px;
    }
    .pair-card {
      display: grid;
      grid-template-columns: 36px 1fr auto;
      align-items: center;
      gap: 16px;
      padding: 14px 20px;
      border-radius: 16px;
      border: 1px solid rgba(255,255,255,0.04);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(8px);
    }
    .pair-card.top-pair {
      border-color: rgba(52, 211, 153, 0.3);
      background: rgba(52, 211, 153, 0.04);
    }
    .pair-badge {
      font-size: 0.9rem;
      font-weight: 700;
      color: var(--text-muted);
    }
    .pair-info {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-wrap: wrap;
    }
    .pair-director { font-size: 1rem; font-weight: 600; color: #60a5fa; }
    .pair-connector { color: var(--text-muted); font-weight: 700; }
    .pair-actor { font-size: 1rem; font-weight: 600; color: #f472b6; }
    .pair-count { font-size: 0.85rem; color: var(--text-muted); white-space: nowrap; }
    .no-data { color: var(--text-muted); }
  `]
})
export class DynamicDuosSlideComponent {
  @Input() castingPairs: any[] = [];
  @Input() directorActorPairs: DirectorActorPairDto[] = [];

  get topPairs(): DirectorActorPairDto[] {
    return (this.directorActorPairs ?? []).slice(0, 3);
  }
}
