import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DirectorActorPairDto } from '../../../../core/services/final-cut.service';

@Component({
  selector: 'app-dynamic-duos-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-sepia">
      <div class="act-label">ACT II · SCENE 9 · DYNAMIC DUOS</div>
      <h2 class="slide-title">Dynamic Duos & Perfect Pairs.</h2>
      <p class="slide-subtitle">The director-actor collaborations you returned to again and again.</p>
      <p class="slide-explainer">Iconic collaborations. The director-actor pairings that sparked magic.</p>

      <div class="pairs-section" *ngIf="topPairs.length">
        <div *ngFor="let p of topPairs; let i = index" class="pair-card" [class.top-pair]="i === 0">
          <div class="pair-badge">#{{ i + 1 }}</div>
          <div class="pair-info">
            <span class="pair-director">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="inline-icon"><path d="M2 10v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-9"/><path d="M2 10l5-5"/><path d="M7 5l5 5"/><path d="M12 5l5 5"/><path d="M17 5l5 5"/><path d="M22 10H2"/></svg>
              {{ p.directorName }}
            </span>
            <span class="pair-connector">+</span>
            <span class="pair-actor">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="inline-icon"><circle cx="12" cy="12" r="10"/><path d="M8 14s1.5 2 4 2 4-2 4-2"/><line x1="9" y1="9" x2="9.01" y2="9"/><line x1="15" y1="9" x2="15.01" y2="9"/></svg>
              {{ p.actorName }}
            </span>
          </div>
          <div class="pair-count" style="font-family: var(--font-mono)">{{ p.collaborationCount }} films together</div>
        </div>
      </div>

      <div class="casting-pairs-section" *ngIf="castingPairs.length">
        <p class="section-note">Actor pairs that kept crossing paths:</p>
        <div *ngFor="let p of castingPairs.slice(0, 3); let i = index" class="pair-card">
          <div class="pair-badge">#{{ i + 1 }}</div>
          <div class="pair-info">
            <span class="pair-director">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="inline-icon"><circle cx="12" cy="12" r="10"/><path d="M8 14s1.5 2 4 2 4-2 4-2"/><line x1="9" y1="9" x2="9.01" y2="9"/><line x1="15" y1="9" x2="15.01" y2="9"/></svg>
              {{ p.actor1Name }}
            </span>
            <span class="pair-connector">+</span>
            <span class="pair-actor">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="inline-icon"><circle cx="12" cy="12" r="10"/><path d="M8 14s1.5 2 4 2 4-2 4-2"/><line x1="9" y1="9" x2="9.01" y2="9"/><line x1="15" y1="9" x2="15.01" y2="9"/></svg>
              {{ p.actor2Name }}
            </span>
          </div>
          <div class="pair-count" style="font-family: var(--font-mono)">{{ p.collaborationCount }} films together</div>
        </div>
      </div>

      <p class="no-data" *ngIf="!topPairs.length && !castingPairs.length">
        No recurring collaborations found yet.
      </p>
      <div class="timecode">TC 00:55:30:14</div>
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
    .pairs-section, .casting-pairs-section {
      display: flex;
      flex-direction: column;
      gap: 16px;
      width: 100%;
      max-width: 800px;
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
      grid-template-columns: 40px 1fr auto;
      align-items: center;
      gap: 20px;
      padding: 20px 24px;
      border-radius: 20px;
      border: 1px solid rgba(255,255,255,0.04);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(8px);
    }
    .pair-card.top-pair {
      border-color: rgba(52, 211, 153, 0.3);
      background: rgba(52, 211, 153, 0.04);
    }
    .pair-badge {
      font-size: 1rem;
      font-weight: 700;
      color: var(--text-muted);
    }
    .pair-info {
      display: flex;
      align-items: center;
      gap: 12px;
      flex-wrap: wrap;
    }
    .pair-director { font-size: 1.1rem; font-weight: 600; color: #60a5fa; display: flex; align-items: center; gap: 6px; }
    .pair-connector { color: var(--text-muted); font-weight: 700; }
    .pair-actor { font-size: 1.1rem; font-weight: 600; color: #f472b6; display: flex; align-items: center; gap: 6px; }
    .pair-count { font-size: 0.95rem; color: var(--text-muted); white-space: nowrap; }
    .no-data { color: var(--text-muted); }
    .inline-icon { opacity: 0.8; }
  `]
})
export class DynamicDuosSlideComponent {
  @Input() castingPairs: any[] = [];
  @Input() directorActorPairs: DirectorActorPairDto[] = [];

  get topPairs(): DirectorActorPairDto[] {
    return (this.directorActorPairs ?? []).slice(0, 3);
  }
}
