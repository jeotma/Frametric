import { Component, signal, inject, ViewChild, ElementRef, HostListener } from '@angular/core';
import { RouterOutlet, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

// Standalone Child Components Import
import { SidebarComponent } from './components/sidebar/sidebar';
import { ToastComponent } from './components/toast/toast.component';
import { AuthService } from './core/services/auth.service';
import { SearchService, GlobalSearchResultDto } from './core/api';
import { slugify } from './core/utils/slugify';
import { ModalService } from './core/services/modal.service';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, 
    RouterLink,
    CommonModule, 
    SidebarComponent,
    ToastComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('Frametric');
  
  public auth = inject(AuthService);
  public modalService = inject(ModalService);
  public isSidebarPinned = signal<boolean>(localStorage.getItem('frametric_sidebar_state') === 'expanded');

  constructor() {
    // Listen to sidebar pinning changes
    window.addEventListener('frametric-sidebar-state-changed', (e: any) => {
      this.isSidebarPinned.set(e.detail === 'expanded');
    });

    // Defensive cleanup — reset any leftover easter egg effects from bfcache or SPA state
    document.body.style.filter = '';
    document.body.style.transform = '';
    document.body.style.transition = '';

    // bfcache: Chrome restores page state including inline body styles
    window.addEventListener('pageshow', (event) => {
      if (event.persisted) {
        document.body.style.filter = '';
        document.body.style.transform = '';
        document.body.style.transition = '';
      }
    });
  }

  get isAuthPage(): boolean {
    const url = this.router.url;
    return url.includes('/login') || url.includes('/register');
  }

  // Easter egg reactive state — replaces direct document.body.style manipulation
  protected readonly bodyFilter = signal<string | null>(null);
  protected readonly bodyTransform = signal<string | null>(null);
  protected readonly showRosebudSled = signal(false);
  protected readonly showRaptorPeek = signal(false);

  @ViewChild('easterEggContainer', { read: ElementRef }) easterEggContainer!: ElementRef<HTMLElement>;

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
  public isTopbarExpanded = signal(false);
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

  @ViewChild('searchInput') searchInputEl!: ElementRef<HTMLInputElement>;

  expandTopbar() {
    this.isTopbarExpanded.set(true);
    setTimeout(() => {
      if (this.searchInputEl) {
        this.searchInputEl.nativeElement.focus();
      }
    }, 50);
  }

  closeSearch() {
    setTimeout(() => {
      this.showSearchDropdown.set(false);
      
      const input = this.searchInputEl?.nativeElement;
      if (!input || input.value.trim().length === 0) {
        this.isTopbarExpanded.set(false);
      }
    }, 200);
  }

  navigateToResult(event: MouseEvent, result: GlobalSearchResultDto) {
    event.preventDefault();
    event.stopPropagation();
    this.showSearchDropdown.set(false);
    
    // Clear input
    const inputEl = document.querySelector('.search-box input') as HTMLInputElement;
    if (inputEl) {
      inputEl.value = '';
    }

    const slug = slugify(result.titleOrName || '');
    if (result.entityType === 'Movie') {
      this.router.navigate(['/movies', result.localId || result.tmdbId, slug]);
    } else if (result.entityType === 'Actor' || result.entityType === 'Director / Actor') {
      this.router.navigate(['/actors', result.actorId || result.localId || result.tmdbId, slug]);
    } else if (result.entityType === 'Director') {
      this.router.navigate(['/directors', result.directorId || result.localId || result.tmdbId, slug]);
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

    // 1. "rosebud" — 35% trigger rate
    if (value === 'rosebud' && Math.random() < 0.35) {
      setEasterEggIndicator('Rosebud Sled', 'You unlocked the legendary Citizen Kane sepia sled easter egg! How cool is that?', 15000);
      this.bodyFilter.set('sepia(1) contrast(1.1) brightness(0.9)');
      this.showRosebudSled.set(true);

      setTimeout(() => {
        this.bodyFilter.set(null);
        this.showRosebudSled.set(false);
        input.value = '';
      }, 2000);
    }

    // 2. "malkovich malkovich" — 35% trigger rate
    if (value === 'malkovich malkovich' && Math.random() < 0.35) {
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

    // 3. "memento" or "tenet" — 35% trigger rate
    if ((value === 'memento' || value === 'tenet') && Math.random() < 0.35) {
      setEasterEggIndicator('Temporal Inversion', 'You flipped the layout! A nod to Christopher Nolan\'s backwards timeline paradoxes.', 15000);
      this.bodyTransform.set('scaleX(-1)');

      setTimeout(() => {
        this.bodyTransform.set(null);
        input.value = '';
      }, 2500);
    }

    // 4. "matrix" — 35% trigger rate
    if (value === 'matrix' && Math.random() < 0.35) {
      setEasterEggIndicator('Matrix Code Rain', 'Down the rabbit hole you go! Unlocked falling digital rain animation.', 15000);
      const container = this.easterEggContainer?.nativeElement;
      if (container) {
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
        container.appendChild(matrixRain);

        setTimeout(() => {
          matrixRain.remove();
          input.value = '';
        }, 4000);
      }
    }

    // 5. "jurassic park" or "jurassic world" — 35% trigger rate
    if ((value === 'jurassic park' || value === 'jurassic world') && Math.random() < 0.35) {
      setEasterEggIndicator('Jurassic Surprise', 'Clever Girl! Unlocked the raptor peek-a-boo.', 15000);
      this.showRaptorPeek.set(true);

      setTimeout(() => {
        this.showRaptorPeek.set(false);
        input.value = '';
      }, 2000);
    }

    // 6. "there is no spoon" — 35% trigger rate
    if (value === 'there is no spoon' && Math.random() < 0.35) {
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

  @HostListener('window:keydown.escape', ['$event'])
  handleEscapeKey(event: any) {
    if (this.modalService.showAuthModal()) {
      this.modalService.closeAuthModal();
    }
  }
}
