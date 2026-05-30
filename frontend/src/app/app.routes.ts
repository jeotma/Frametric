import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Public routes
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register').then(m => m.RegisterComponent),
  },
  // Protected shell
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./app').then(m => m.App),
  },
  // Fallback
  { path: '**', redirectTo: '' },
];
