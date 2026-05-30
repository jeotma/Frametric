import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

// Standalone Child Components Import
import { SidebarComponent } from './components/sidebar/sidebar';
import { DashboardComponent } from './components/dashboard/dashboard';
import { ImportCenterComponent } from './components/import-center/import-center';
import { WrappedTeaserComponent } from './components/wrapped-teaser/wrapped-teaser';
import { AuthService } from './core/services/auth.service';

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
  
  public auth = inject(AuthService);

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

  // UI States
  public isUploading = signal(false);

  public selectTab(tab: 'dashboard' | 'imports' | 'wrapped') {
    this.activeTab.set(tab);
  }
}
