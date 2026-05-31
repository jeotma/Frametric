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
