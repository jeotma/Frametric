import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-actor-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content actor-bg" *ngIf="data.topActors && data.topActors.length > 0">
      <h2 class="slide-title" style="font-size: 2rem;">Your star of the year</h2>
      <div class="star-card">
        <div class="card-inner">
          <div class="name">{{ data.topActors[0].actorName }}</div>
          <div class="count">{{ data.topActors[0].count }} films</div>
        </div>
      </div>

      <div class="runners-up" *ngIf="data.topActors.length > 1">
        <p>Also starring:</p>
        <div class="runner-list">
          <span *ngFor="let a of data.topActors.slice(1, 4)">{{ a.actorName }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .actor-bg {
      background: radial-gradient(circle at bottom right, rgba(56, 189, 248, 0.15) 0%, transparent 60%);
    }
    .star-card {
      margin-top: 40px;
      padding: 4px;
      background: linear-gradient(135deg, var(--accent-blue), transparent);
      border-radius: 24px;
      animation: float 6s ease-in-out infinite;
    }
    .card-inner {
      background: var(--bg-primary);
      padding: 60px;
      border-radius: 20px;
      text-align: center;
    }
    .name {
      font-size: 3rem;
      font-weight: 900;
      color: var(--text-primary);
      margin-bottom: 10px;
    }
    .count {
      color: var(--accent-blue);
      font-size: 1.25rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 2px;
    }
    .runners-up {
      margin-top: 60px;
      color: var(--text-muted);
      
      p { margin-bottom: 10px; text-transform: uppercase; font-size: 0.8rem; letter-spacing: 1px;}
    }
    .runner-list {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
      justify-content: center;

      span {
        background: rgba(255, 255, 255, 0.05);
        padding: 8px 16px;
        border-radius: 99px;
        font-size: 0.9rem;
      }
    }
    @keyframes float {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-10px); }
    }
  `]
})
export class ActorSlideComponent {
  @Input({ required: true }) data!: WrappedSummaryDto;
}
