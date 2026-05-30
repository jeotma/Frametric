import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Stats {
  moviesWatched: number;
  hoursWatched: number;
  favoriteGenre: string;
  favoriteDirector: string;
  completionRate: number;
}

interface ImportItem {
  id: number;
  filename: string;
  date: string;
  status: string;
  movies: number;
  progress: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent {
  public stats = input.required<Stats>();
  public recentImports = input.required<ImportItem[]>();
  
  public viewAllImports = output<void>();

  public onViewAll() {
    this.viewAllImports.emit();
  }
}
