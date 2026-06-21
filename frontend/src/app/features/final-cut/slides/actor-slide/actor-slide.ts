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
          <div class="star-avatar" *ngIf="data.topActors[0].profilePath">
            <img [src]="'https://image.tmdb.org/t/p/w185' + data.topActors[0].profilePath" [alt]="data.topActors[0].actorName">
          </div>
          <div class="star-avatar" *ngIf="!data.topActors[0].profilePath">
            <div class="star-avatar-fallback">{{ data.topActors[0].actorName?.charAt(0) }}</div>
          </div>
          <div class="name">{{ data.topActors[0].actorName }}</div>
          <div class="count">{{ data.topActors[0].count }} films</div>
        </div>
      </div>

      <div class="runners-up" *ngIf="data.topActors.length > 1">
        <p>Also starring:</p>
        <div class="runner-list">
          <span *ngFor="let a of data.topActors.slice(1, 4)" class="runner-badge">
            <div class="runner-avatar">
              <img *ngIf="a.profilePath" [src]="'https://image.tmdb.org/t/p/w185' + a.profilePath" [alt]="a.actorName">
              <div *ngIf="!a.profilePath" class="runner-avatar-fallback">{{ a.actorName?.charAt(0) }}</div>
            </div>
            {{ a.actorName }}
          </span>
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
      padding: 40px;
      border-radius: 20px;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
    }
    .star-avatar { width: 100px; height: 100px; border-radius: 50%; overflow: hidden; margin: 0 auto 16px auto; background: rgba(255,255,255,0.05); display: flex; align-items: center; justify-content: center; }
    .star-avatar img { width: 100%; height: 100%; object-fit: cover; }
    .star-avatar-fallback { font-size: 2.5rem; font-weight: 800; color: var(--text-muted); }
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

      .runner-badge {
        display: flex;
        align-items: center;
        gap: 8px;
        background: rgba(255, 255, 255, 0.05);
        padding: 6px 16px 6px 6px;
        border-radius: 99px;
        font-size: 0.9rem;
      }
    }
    .runner-avatar { width: 24px; height: 24px; border-radius: 50%; overflow: hidden; background: rgba(255,255,255,0.1); display: flex; align-items: center; justify-content: center; }
    .runner-avatar img { width: 100%; height: 100%; object-fit: cover; }
    .runner-avatar-fallback { font-size: 0.8rem; font-weight: 700; color: var(--text-muted); }
    @keyframes float {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-10px); }
    }
  `]
})
export class ActorSlideComponent {
  @Input({ required: true }) data!: WrappedSummaryDto;
}
