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
  public currentSearchEasterEgg = signal<{ name: string; desc: string } | null>(null);

  // Search box listener for cult queries
  onSearchKeyup(event: Event) {
    if (event instanceof KeyboardEvent && event.key !== 'Enter') return;

    const input = event.target as HTMLInputElement;
    const value = (input.value || '').trim().toLowerCase();

    // Helper to set current easter egg indicator
    const setEasterEggIndicator = (name: string, desc: string, durationMs: number) => {
      this.currentSearchEasterEgg.set({ name, desc });
      setTimeout(() => {
        if (this.currentSearchEasterEgg()?.name === name) {
          this.currentSearchEasterEgg.set(null);
        }
      }, durationMs);
    };

    // 1. "rosebud" — 75% trigger rate
    if (value === 'rosebud' && Math.random() < 0.75) {
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

    // 2. "malkovich malkovich" — 75% trigger rate
    if (value === 'malkovich malkovich' && Math.random() < 0.75) {
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

    // 3. "memento" or "tenet" — 75% trigger rate
    if ((value === 'memento' || value === 'tenet') && Math.random() < 0.75) {
      setEasterEggIndicator('Temporal Inversion', 'You flipped the layout! A nod to Christopher Nolan\'s backwards timeline paradoxes.', 15000);
      const body = document.body;
      body.style.transition = 'transform 1.5s ease';
      body.style.transform = 'scaleX(-1)';
      
      setTimeout(() => {
        body.style.transform = '';
        input.value = '';
      }, 2500);
    }

    // 4. "matrix" — 75% trigger rate
    if (value === 'matrix' && Math.random() < 0.75) {
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

    // 5. "clever girl" — 75% trigger rate
    if (value === 'clever girl' && Math.random() < 0.75) {
      setEasterEggIndicator('Jurassic Surprise', 'Clever Girl! Unlocked Jurassic Park raptor peek-a-boo.', 15000);
      const dino = document.createElement('div');
      dino.className = 'raptor-peek';
      dino.innerText = '🦖 Clever girl...';
      document.body.appendChild(dino);

      setTimeout(() => {
        dino.remove();
        input.value = '';
      }, 2000);
    }

    // 6. "there is no spoon" — 75% trigger rate
    if (value === 'there is no spoon' && Math.random() < 0.75) {
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
