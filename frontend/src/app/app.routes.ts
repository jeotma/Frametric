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
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./components/dashboard/dashboard').then(m => m.DashboardComponent)
      },
      {
        path: 'final-cut-teaser',
        loadComponent: () => import('./components/final-cut-teaser/final-cut-teaser').then(m => m.FinalCutTeaserComponent)
      },
      {
        path: 'imports',
        loadComponent: () => import('./components/import-center/import-center').then(m => m.ImportCenterComponent)
      },
      {
        path: 'final-cut/:year',
        loadComponent: () => import('./features/final-cut/final-cut').then(m => m.FinalCutComponent)
      },
      {
        path: 'stats',
        loadComponent: () => import('./features/stats/stats').then(m => m.StatsComponent)
      },
      {
        path: 'recommendations',
        loadComponent: () => import('./features/recommendations/recommendations').then(m => m.RecommendationsComponent)
      },
      {
        path: 'movies/:id',
        loadComponent: () => import('./features/movies/movie-detail/movie-detail').then(m => m.MovieDetailComponent)
      },
      {
        path: 'actors/:id',
        loadComponent: () => import('./features/actors/actor-detail/actor-detail').then(m => m.ActorDetailComponent)
      },
      {
        path: 'directors/:id',
        loadComponent: () => import('./features/directors/director-detail/director-detail').then(m => m.DirectorDetailComponent)
      }
    ]
  },
  // Fallback
  { path: '**', redirectTo: '' },
];
