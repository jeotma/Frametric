import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService, DashboardSummaryDto } from '../../core/api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {
  private analyticsService = inject(AnalyticsService);

  summary = signal<DashboardSummaryDto | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  ngOnInit() {
    this.analyticsService.apiAnalyticsDashboardGet().subscribe({
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
  }

  formatWatchtime(minutes: number | undefined): string {
    if (!minutes) return '0h';
    const hours = Math.floor(minutes / 60);
    return `${hours}h`;
  }
}
