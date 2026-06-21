import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ToastCustomListWinner {
  movieId: string;
  movieTitle: string;
  listId: string;
  listName: string;
}

@Component({
  selector: 'app-custom-list-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container animate-slide-in">
      <div class="toast-header">
        <svg class="heading-svg-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
          <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
        </svg>
        <span>CUSTOM LIST WINNER</span>
      </div>
      <div class="toast-body">
        <p>
          <a class="entity-link" [href]="'/movies/' + data.movieId" target="_blank">{{ data.movieTitle }}</a>
          was selected from your list: <strong>{{ data.listName }}</strong>.
        </p>
      </div>
      <div class="toast-actions">
        <button class="btn-remove" (click)="onRemove()" [disabled]="removing">
          {{ removing ? 'Removing...' : 'Remove from list' }}
        </button>
        <button class="btn-dismiss" (click)="onDismiss()">&times;</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      background: var(--surface-floating);
      border: 1px solid var(--accent-amber);
      border-left: 4px solid var(--accent-amber);
      box-shadow: 0 10px 30px rgba(0,0,0,0.8);
      padding: 1rem;
      border-radius: 4px;
      width: 350px;
      margin-top: 1rem;
      pointer-events: auto;
    }

    .toast-header {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      color: var(--accent-amber);
      font-weight: bold;
      font-size: 0.85rem;
      letter-spacing: 1px;
      margin-bottom: 0.5rem;

      .heading-svg-icon {
        width: 16px;
        height: 16px;
      }
    }

    .toast-body {
      color: var(--text-secondary);
      font-size: 0.9rem;
      line-height: 1.4;
      margin-bottom: 1rem;

      .entity-link {
        color: var(--text-primary);
        font-weight: 600;
        text-decoration: underline;
        text-decoration-color: var(--accent-amber);
        text-underline-offset: 2px;
        transition: color 0.2s;

        &:hover {
          color: var(--accent-amber);
        }
      }
    }

    .toast-actions {
      display: flex;
      justify-content: flex-end;
      align-items: center;
      gap: 0.5rem;

      button {
        cursor: pointer;
        background: transparent;
        border: none;
        transition: all 0.2s;
      }

      .btn-remove {
        color: var(--accent-rose);
        border: 1px solid rgba(244, 63, 94, 0.3);
        padding: 0.25rem 0.5rem;
        border-radius: 4px;
        font-size: 0.8rem;

        &:hover:not(:disabled) {
          background: rgba(244, 63, 94, 0.1);
          border-color: var(--accent-rose);
        }

        &:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }
      }

      .btn-dismiss {
        color: var(--text-muted);
        font-size: 1.2rem;
        padding: 0 0.5rem;

        &:hover {
          color: var(--text-primary);
        }
      }
    }

    .animate-slide-in {
      animation: slideIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards;
    }

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class CustomListToastComponent {
  @Input() data!: ToastCustomListWinner;
  @Input() removing: boolean = false;
  
  @Output() remove = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  onRemove(): void {
    this.remove.emit();
  }

  onDismiss(): void {
    this.dismiss.emit();
  }
}
