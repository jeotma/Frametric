import { Component, OnInit, inject, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService } from '../../core/api/api/analytics.service';
import { MonthlyActivityResponseDto } from '../../core/api/model/monthly-activity-response-dto';

@Component({
  selector: 'app-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stats.html',
  styleUrl: './stats.scss'
})
export class StatsComponent implements OnInit {
  private analytics = inject(AnalyticsService);

  public selectedYear = signal<number>(new Date().getFullYear() - 1);
  public data = signal<MonthlyActivityResponseDto | null>(null);
  public loading = signal<boolean>(false);

  public availableYears = [2026, 2025, 2024, 2023, 2022];

  public totalWatches = computed(() => {
    const d = this.data();
    if (!d || !d.monthlyActivity) return 0;
    return d.monthlyActivity.reduce((acc, curr) => acc + (curr.count || 0), 0);
  });

  constructor() {
    effect(() => {
      this.loadData(this.selectedYear());
    });
  }

  ngOnInit() {
    // Initial load handled by effect
  }

  private loadData(year: number) {
    this.loading.set(true);
    // As per the API: apiAnalyticsMonthlyActivityYearGet
    this.analytics.apiAnalyticsMonthlyActivityYearGet(year).subscribe({
      next: (res) => {
        this.data.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load stats', err);
        this.loading.set(false);
      }
    });
  }
}
