import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

// Standalone Child Components Import
import { SidebarComponent } from './components/sidebar/sidebar';
import { DashboardComponent } from './components/dashboard/dashboard';
import { ImportCenterComponent } from './components/import-center/import-center';
import { WrappedTeaserComponent } from './components/wrapped-teaser/wrapped-teaser';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, 
    CommonModule, 
    SidebarComponent, 
    DashboardComponent, 
    ImportCenterComponent, 
    WrappedTeaserComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('Frametric');
  
  // Navigation active tab simulator
  public activeTab = signal<'dashboard' | 'imports' | 'wrapped'>('dashboard');

  // Dashboard mock stats
  public stats = signal({
    moviesWatched: 342,
    hoursWatched: 684,
    favoriteGenre: 'Sci-Fi',
    favoriteDirector: 'Denis Villeneuve',
    completionRate: 94
  });

  // Recent imports simulation data
  public recentImports = signal([
    { id: 1, filename: 'letterboxd_diary_2026.zip', date: '2026-05-30', status: 'Completed', movies: 124, progress: 100 },
    { id: 2, filename: 'letterboxd_history_all.zip', date: '2026-05-28', status: 'Enriching', movies: 218, progress: 65 },
    { id: 3, filename: 'watchlist_export.zip', date: '2026-05-15', status: 'Failed', movies: 0, progress: 0 }
  ]);

  // UI States
  public isUploading = signal(false);
  public uploadProgress = signal(0);

  public selectTab(tab: 'dashboard' | 'imports' | 'wrapped') {
    this.activeTab.set(tab);
  }

  public simulateUpload() {
    if (this.isUploading()) return;
    this.isUploading.set(true);
    this.uploadProgress.set(0);
    
    const interval = setInterval(() => {
      if (this.uploadProgress() >= 100) {
        clearInterval(interval);
        setTimeout(() => {
          this.recentImports.update(prev => [
            {
              id: Date.now(),
              filename: 'uploaded_letterboxd_archive.zip',
              date: new Date().toISOString().split('T')[0],
              status: 'Completed',
              movies: 87,
              progress: 100
            },
            ...prev
          ]);
          this.isUploading.set(false);
        }, 800);
      } else {
        this.uploadProgress.update(p => p + 10);
      }
    }, 150);
  }
}
