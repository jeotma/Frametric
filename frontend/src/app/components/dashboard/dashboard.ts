import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AnalyticsService, DashboardSummaryDto, ImportService } from '../../core/api';
import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private analyticsService = inject(AnalyticsService);
  private importService = inject(ImportService);
  public auth = inject(AuthService);
  public modalService = inject(ModalService);
  private destroy$ = new Subject<void>();

  summary = signal<DashboardSummaryDto | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);
  hasSuccessfulImport = signal<boolean>(true); // Defaults to true to avoid initial flicker

  ngOnInit() {
    if (!this.auth.isAuthenticated()) {
      this.isLoading.set(false);
      return;
    }

    // 1. Fetch dashboard stats
    this.analyticsService.apiAnalyticsDashboardGet().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: DashboardSummaryDto) => {
        this.summary.set(data);
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Error fetching dashboard summary', err);
        this.errorMessage.set('Could not load dashboard data. Ensure the backend is running and you have imported data.');
        this.isLoading.set(false);
      }
    });

    // 2. Check if we have at least one successful import in history
    this.importService.apiImportHistoryGet().pipe(takeUntil(this.destroy$)).subscribe({
      next: (history) => {
        const hasValid = history.some(item => item.status === 'Completed' || item.status === 'Enriching');
        this.hasSuccessfulImport.set(hasValid);
      },
      error: (err) => {
        console.error('Failed to fetch import history', err);
      }
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  formatWatchtime(minutes: number | undefined): string {
    if (!minutes) return '0h';
    const hours = Math.floor(minutes / 60);
    return `${hours}h`;
  }
}
