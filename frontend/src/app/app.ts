import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

// Standalone Child Components Import
import { SidebarComponent } from './components/sidebar/sidebar';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, 
    CommonModule, 
    SidebarComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('Frametric');
  
  public auth = inject(AuthService);

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
}
