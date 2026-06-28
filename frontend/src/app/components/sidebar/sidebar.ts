import { Component, inject, computed, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class SidebarComponent {
  public auth = inject(AuthService);
  public showUserMenu = signal(false);
  public isHovered = signal(false);
  public sidebarState = signal<'expanded' | 'collapsed' | 'hoverable'>(
    (localStorage.getItem('frametric_sidebar_state') as any) || 'hoverable'
  );

  public isLocked = computed(() => this.sidebarState() !== 'hoverable');
  public isPinned = computed(() => this.sidebarState() === 'expanded');
  public isLockedCollapsed = computed(() => this.sidebarState() === 'collapsed');

  public toggleLock() {
    if (this.isLocked()) {
      this.sidebarState.set('hoverable');
    } else {
      if (this.isHovered() || this.isPinned()) {
        this.sidebarState.set('expanded');
      } else {
        this.sidebarState.set('collapsed');
      }
    }
    localStorage.setItem('frametric_sidebar_state', this.sidebarState());
    window.dispatchEvent(new CustomEvent('frametric-sidebar-state-changed', { detail: this.sidebarState() }));
  }

  public toggleSize() {
    const newState = this.isPinned() ? 'collapsed' : 'expanded';
    this.sidebarState.set(newState);
    localStorage.setItem('frametric_sidebar_state', newState);
    window.dispatchEvent(new CustomEvent('frametric-sidebar-state-changed', { detail: newState }));
  }

  public initials = computed(() => {
    const user = this.auth.currentUser();
    if (!user || !user.username) return 'U';
    const parts = user.username.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return user.username.substring(0, 2).toUpperCase();
  });

  public toggleUserMenu() {
    this.showUserMenu.update(v => !v);
  }

  public logout() {
    this.auth.logout();
  }
}
