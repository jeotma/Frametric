import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WrappedSummaryDto } from '../../../../core/api/model/wrapped-summary-dto';

@Component({
  selector: 'app-director-slide',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="slide-content director-bg" *ngIf="data.topDirectors && data.topDirectors.length > 0">
      <h2 class="slide-title" style="font-size: 2rem;">Your visionary of the year</h2>
      <div class="visionary-card">
        <div class="card-inner">
          <div class="name">{{ data.topDirectors[0].directorName }}</div>
          <div class="count">{{ data.topDirectors[0].count }} films</div>
        </div>
      </div>

      <div class="runners-up" *ngIf="data.topDirectors.length > 1">
        <p>Also loved:</p>
        <div class="runner-list">
          <span *ngFor="let d of data.topDirectors.slice(1, 4)">{{ d.directorName }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .director-bg {
      background: radial-gradient(circle at bottom left, rgba(234, 179, 8, 0.15) 0%, transparent 60%);
    }
    .visionary-card {
      margin-top: 40px;
      padding: 4px;
      background: linear-gradient(135deg, var(--accent-gold), transparent);
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
      color: var(--accent-gold);
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
export class DirectorSlideComponent {
  @Input({ required: true }) data!: WrappedSummaryDto;
}
