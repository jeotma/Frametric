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
            <span class="pair-person">
              <div class="pair-avatar">
                <img *ngIf="p.directorProfilePath" [src]="'https://image.tmdb.org/t/p/w185' + p.directorProfilePath" [alt]="p.directorName">
                <div *ngIf="!p.directorProfilePath" class="pair-avatar-fallback">{{ p.directorName.charAt(0) }}</div>
              </div>
              <span class="pair-director">{{ p.directorName }}</span>
            </span>
            <span class="pair-connector">+</span>
            <span class="pair-person">
              <div class="pair-avatar">
                <img *ngIf="p.actorProfilePath" [src]="'https://image.tmdb.org/t/p/w185' + p.actorProfilePath" [alt]="p.actorName">
                <div *ngIf="!p.actorProfilePath" class="pair-avatar-fallback">{{ p.actorName.charAt(0) }}</div>
              </div>
              <span class="pair-actor">{{ p.actorName }}</span>
            </span>
          </div>
          <div class="pair-count custom-tooltip-container" style="font-family: var(--font-mono)">
            {{ p.collaborationCount }} shared viewings
            <span class="custom-tooltip">Total number of times you've watched a film with this pair, including rewatches.</span>
          </div>
        </div>
      </div>

      <div class="casting-pairs-section" *ngIf="castingPairs.length">
        <p class="section-note">Actor pairs that kept crossing paths:</p>
        <div *ngFor="let p of castingPairs.slice(0, 3); let i = index" class="pair-card">
          <div class="pair-badge">#{{ i + 1 }}</div>
          <div class="pair-info">
            <span class="pair-person">
              <div class="pair-avatar">
                <img *ngIf="p.actor1ProfilePath" [src]="'https://image.tmdb.org/t/p/w185' + p.actor1ProfilePath" [alt]="p.actor1Name">
                <div *ngIf="!p.actor1ProfilePath" class="pair-avatar-fallback">{{ p.actor1Name.charAt(0) }}</div>
              </div>
              <span class="pair-director">{{ p.actor1Name }}</span>
            </span>
            <span class="pair-connector">+</span>
            <span class="pair-person">
              <div class="pair-avatar">
                <img *ngIf="p.actor2ProfilePath" [src]="'https://image.tmdb.org/t/p/w185' + p.actor2ProfilePath" [alt]="p.actor2Name">
                <div *ngIf="!p.actor2ProfilePath" class="pair-avatar-fallback">{{ p.actor2Name.charAt(0) }}</div>
              </div>
              <span class="pair-actor">{{ p.actor2Name }}</span>
            </span>
          </div>
          <div class="pair-count custom-tooltip-container" style="font-family: var(--font-mono)">
            {{ p.collaborationCount }} shared viewings
            <span class="custom-tooltip">Total number of times you've watched a film with this pair, including rewatches.</span>
          </div>
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
      margin-bottom: 16px;
      font-style: italic;
      max-width: 600px;
      text-align: center;
    }
    .pairs-section, .casting-pairs-section {
      display: flex;
      flex-direction: column;
      gap: 10px;
      width: 100%;
      max-width: 800px;
    }
    .section-note {
      font-size: 0.85rem;
      color: var(--accent-sepia);
      text-transform: uppercase;
      letter-spacing: 0.15em;
      margin-top: 20px;
      margin-bottom: 12px;
      text-align: center;
      font-weight: 600;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
      padding-top: 16px;
    }
    .pair-card {
      display: grid;
      grid-template-columns: 40px 1fr auto;
      align-items: center;
      gap: 20px;
      padding: 12px 24px;
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
    .pair-person { display: flex; align-items: center; gap: 8px; }
    .pair-avatar { width: 32px; height: 32px; border-radius: 50%; overflow: hidden; background: rgba(255,255,255,0.05); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .pair-avatar img { width: 100%; height: 100%; object-fit: cover; }
    .pair-avatar-fallback { font-size: 0.9rem; font-weight: 700; color: var(--text-muted); }
    .pair-director { font-size: 1.1rem; font-weight: 600; color: #60a5fa; }
    .pair-connector { color: var(--text-muted); font-weight: 700; }
    .pair-actor { font-size: 1.1rem; font-weight: 600; color: #f472b6; }
    .pair-count {
      font-size: 0.85rem;
      color: rgba(255, 255, 255, 0.6);
      background: rgba(255, 255, 255, 0.05);
      padding: 4px 10px;
      border-radius: 8px;
      border: 1px solid rgba(255, 255, 255, 0.08);
      font-weight: 500;
      text-align: right;
    }.no-data { color: var(--text-muted); }
    .inline-icon { opacity: 0.8; }
    .custom-tooltip-container {
      position: relative;
      cursor: help;
    }
    .custom-tooltip {
      visibility: hidden;
      opacity: 0;
      width: 220px;
      background-color: rgba(10, 10, 10, 0.95);
      color: #fff;
      text-align: center;
      border-radius: 8px;
      padding: 8px 12px;
      font-size: 0.75rem;
      font-family: system-ui, sans-serif;
      white-space: normal;
      line-height: 1.4;
      border: 1px solid rgba(255, 255, 255, 0.1);
      position: absolute;
      z-index: 100;
      bottom: 125%;
      left: 50%;
      transform: translateX(-50%);
      transition: opacity 0.3s ease 0.2s, visibility 0.3s ease 0.2s;
    }
    .custom-tooltip-container:hover .custom-tooltip {
      visibility: visible;
      opacity: 1;
    }
  `]
})
export class DynamicDuosSlideComponent {
  @Input() castingPairs: any[] = [];
  @Input() directorActorPairs: DirectorActorPairDto[] = [];

  get topPairs(): DirectorActorPairDto[] {
    return (this.directorActorPairs ?? []).slice(0, 3);
  }
}
