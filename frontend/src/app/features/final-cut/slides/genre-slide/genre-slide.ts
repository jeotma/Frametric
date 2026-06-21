import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-genre-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content slide-bg-amber">
      <div class="act-label">ACT II · SCENE 6 · THE GENRE LANDSCAPE</div>
      <h2 class="slide-title">You craved...</h2>
      <div class="genre-list">
        <div class="genre-item" *ngFor="let g of data.topGenres.slice(0, 5); let i = index" [style.animation-delay]="(i * 0.15) + 's'">
          <span class="rank">#{{ i + 1 }}</span>
          <span class="name">{{ g.genreName }}</span>
          <span class="count" style="font-family: var(--font-mono)">{{ g.count }} movies</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .genre-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
      margin-top: 40px;
      width: 100%;
      max-width: 400px;
    }
    .genre-item {
      display: flex;
      align-items: center;
      background: rgba(255, 255, 255, 0.05);
      border-radius: 16px;
      padding: 16px 24px;
      opacity: 0;
      transform: translateX(-20px);
      animation: slideIn 0.5s forwards;
    }
    .rank {
      font-weight: 800;
      color: var(--accent-pink);
      font-size: 1.5rem;
      width: 40px;
    }
    .name {
      font-size: 1.5rem;
      font-weight: 600;
      flex: 1;
      text-align: left;
    }
    .count {
      color: var(--text-muted);
      font-size: 0.9rem;
    }
    @keyframes slideIn {
      to { opacity: 1; transform: translateX(0); }
    }
  `]
})
export class GenreSlideComponent {
  @Input({ required: true }) data!: WrappedSummaryDto;
}
