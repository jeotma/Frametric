import { Component, input, output, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImportService } from '../../core/api/api/import.service';
import { ImportHistoryDto } from '../../core/api/model/import-history-dto';

interface Stats {
  moviesWatched: number;
  hoursWatched: number;
  favoriteGenre: string;
  favoriteDirector: string;
  completionRate: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit {
  private importService = inject(ImportService);

  public stats = input.required<Stats>();
  public recentImports = signal<ImportHistoryDto[]>([]);
  
  public viewAllImports = output<void>();

  ngOnInit() {
    this.importService.apiImportHistoryGet().subscribe({
      next: (data) => {
        // Only show the top 3 most recent imports on the dashboard
        this.recentImports.set(data.slice(0, 3));
      },
      error: (err) => console.error('Failed to fetch dashboard imports', err)
    });
  }

  public onViewAll() {
    this.viewAllImports.emit();
  }
}
