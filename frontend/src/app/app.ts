import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

// Standalone Child Components Import
import { SidebarComponent } from './components/sidebar/sidebar';
import { AuthService } from './core/services/auth.service';
import { SearchService, GlobalSearchResultDto } from './core/api';

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
  public isSearching = signal(false);
  public searchResults = signal<GlobalSearchResultDto[]>([]);
  public showSearchDropdown = signal(false);
  public currentSearchEasterEgg = signal<{ name: string; desc: string } | null>(null);

  private searchTimeout: any;
  private searchService = inject(SearchService);
  private router = inject(Router);

  // Search box listener for cult queries and global search
  onSearchKeyup(event: Event) {
    const input = event.target as HTMLInputElement;
    const value = (input.value || '').trim().toLowerCase();

    // 1. Handle Easter Eggs
    this.handleEasterEggs(value, input);

    // 2. Handle Global Search
    if (value.length < 2) {
      this.searchResults.set([]);
      this.showSearchDropdown.set(false);
      return;
    }

    if (this.searchTimeout) clearTimeout(this.searchTimeout);
    
    this.searchTimeout = setTimeout(() => {
      this.isSearching.set(true);
      this.showSearchDropdown.set(true);
      
      this.searchService.apiSearchGet(value).subscribe({
        next: (results) => {
          this.searchResults.set(results);
          this.isSearching.set(false);
        },
        error: () => {
          this.isSearching.set(false);
          this.searchResults.set([]);
        }
      });
    }, 400); // 400ms debounce
  }

  closeSearch() {
    this.showSearchDropdown.set(false);
  }

  navigateToResult(result: GlobalSearchResultDto) {
    this.showSearchDropdown.set(false);
    if (result.entityType === 'Movie') {
      this.router.navigate(['/movies', result.localId || result.tmdbId]);
    } else if (result.entityType === 'Actor') {
      this.router.navigate(['/actors', result.localId || result.tmdbId]);
    } else if (result.entityType === 'Director') {
      this.router.navigate(['/directors', result.localId || result.tmdbId]);
    }
  }

  private handleEasterEggs(value: string, input: HTMLInputElement) {

    // Helper to set current easter egg indicator
    const setEasterEggIndicator = (name: string, desc: string, durationMs: number) => {
      this.currentSearchEasterEgg.set({ name, desc });
      setTimeout(() => {
        if (this.currentSearchEasterEgg()?.name === name) {
          this.currentSearchEasterEgg.set(null);
        }
      }, durationMs);
    };

    // 1. "rosebud" — 5% trigger rate
    if (value === 'rosebud' && Math.random() < 0.05) {
      setEasterEggIndicator('Rosebud Sled', 'You unlocked the legendary Citizen Kane sepia sled easter egg! How cool is that?', 15000);
      const body = document.body;
      body.style.transition = 'filter 1s ease';
      body.style.filter = 'sepia(1) contrast(1.1) brightness(0.9)';
      
      // Spawn a pixelated sled element
      const sled = document.createElement('div');
      sled.className = 'rosebud-sled';
      sled.innerText = '🛷 ROSEBUD';
      document.body.appendChild(sled);

      setTimeout(() => {
        body.style.filter = '';
        sled.remove();
        input.value = '';
      }, 2000);
    }

    // 2. "malkovich malkovich" — 5% trigger rate
    if (value === 'malkovich malkovich' && Math.random() < 0.05) {
      setEasterEggIndicator('Malkovich Mode', 'Malkovich Malkovich Malkovich. Everything is Malkovich!', 15000);
      const titles = document.querySelectorAll('.movie-title, .meta-row span, .bar-label, h3');
      const originalTexts: { el: Element; text: string }[] = [];
      titles.forEach(el => {
        originalTexts.push({ el, text: el.textContent || '' });
        el.textContent = 'Malkovich';
      });

      setTimeout(() => {
        originalTexts.forEach(x => {
          x.el.textContent = x.text;
        });
        input.value = '';
      }, 2500);
    }

    // 3. "memento" or "tenet" — 5% trigger rate
    if ((value === 'memento' || value === 'tenet') && Math.random() < 0.05) {
      setEasterEggIndicator('Temporal Inversion', 'You flipped the layout! A nod to Christopher Nolan\'s backwards timeline paradoxes.', 15000);
      const body = document.body;
      body.style.transition = 'transform 1.5s ease';
      body.style.transform = 'scaleX(-1)';
      
      setTimeout(() => {
        body.style.transform = '';
        input.value = '';
      }, 2500);
    }

    // 4. "matrix" — 5% trigger rate
    if (value === 'matrix' && Math.random() < 0.05) {
      setEasterEggIndicator('Matrix Code Rain', 'Down the rabbit hole you go! Unlocked falling digital rain animation.', 15000);
      const matrixRain = document.createElement('div');
      matrixRain.className = 'matrix-rain';
      for (let i = 0; i < 50; i++) {
        const drop = document.createElement('span');
        drop.innerText = '0110100101101110011001010110110101100001';
        drop.style.left = `${Math.random() * 100}vw`;
        drop.style.animationDelay = `${Math.random() * 0.8}s`;
        drop.style.fontSize = `${Math.random() * 12 + 10}px`;
        matrixRain.appendChild(drop);
      }
      document.body.appendChild(matrixRain);

      setTimeout(() => {
        matrixRain.remove();
        input.value = '';
      }, 4000);
    }

    // 5. "clever girl" — 5% trigger rate
    if ((value === 'jurassic park' || value === 'jurassic world') && Math.random() < 0.05) {
      setEasterEggIndicator('Jurassic Surprise', 'Clever Girl! Unlocked the raptor peek-a-boo.', 15000);
      const dino = document.createElement('div');
      dino.className = 'raptor-peek';
      dino.innerText = '🦖 Clever girl...';
      document.body.appendChild(dino);

      setTimeout(() => {
        dino.remove();
        input.value = '';
      }, 2000);
    }

    // 6. "there is no spoon" — 5% trigger rate
    if (value === 'there is no spoon' && Math.random() < 0.05) {
      setEasterEggIndicator('Bending Search Box', 'You bent the search input box! Indeed, there is no spoon.', 15000);
      const searchBox = document.querySelector('.search-box') as HTMLElement;
      if (searchBox) {
        searchBox.style.transition = 'transform 0.8s ease';
        searchBox.style.transform = 'skewY(15deg) rotate(5deg) scale(0.9)';
        
        setTimeout(() => {
          searchBox.style.transform = '';
          input.value = '';
        }, 2500);
      }
    }
  }
}
