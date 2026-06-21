import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-obsession-streak-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-silver">
      <div class="act-label">ACT III · SCENE 12 · THE OBSESSION STREAK</div>
      <h2 class="slide-title">The Obsession Streak.</h2>
      <p class="slide-subtitle">Getting stuck in a cinematic loop. Your longest streaks in a single genre.</p>
      <p class="slide-explainer">Binge-watching masterclass. When one genre took complete control of the narrative.</p>

      <div class="streaks-list" *ngIf="topStreaks.length">
        <div *ngFor="let s of topStreaks; let i = index" class="streak-card" [class.top-streak]="i === 0">
          <div class="streak-genre">{{ s.genreName }}</div>
          <div class="streak-meta">
            <span class="streak-count" style="font-family: var(--font-mono)">{{ s.streakLength }} consecutive films</span>
          </div>
        </div>
      </div>

      <p class="no-data" *ngIf="!topStreaks.length">No genre streaks found.</p>
      <div class="timecode">TC 01:18:22:04</div>
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
    .streaks-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
      width: 100%;
      max-width: 520px;
    }
    .streak-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      border-radius: 16px;
      border: 1px solid rgba(255,255,255,0.05);
      background: rgba(255,255,255,0.02);
      backdrop-filter: blur(8px);
    }
    .streak-card.top-streak {
      border-color: #e2ba64;
      background: rgba(229, 9, 20, 0.06);
    }
    .streak-genre {
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-primary);
    }
    .top-streak .streak-genre { color: #e2ba64; }
    .streak-count {
      font-size: 0.85rem;
      color: var(--text-muted);
    }
    .no-data { color: var(--text-muted); }
  `]
})
export class ObsessionStreakSlideComponent {
  @Input() streaks: any[] = [];

  get topStreaks(): any[] {
    return (this.streaks ?? []).sort((a, b) => b.streakLength - a.streakLength).slice(0, 5);
  }
}
