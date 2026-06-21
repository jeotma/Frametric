import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { TokenStorageService } from '../services/token-storage.service';

export const adminGuard: CanActivateFn = () => {
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);

  if (tokenStorage.isTokenValid() && tokenStorage.isAdmin()) {
    return true;
  }

  // Redirect to dashboard if authenticated but not admin, or login if unauthenticated
  if (tokenStorage.isTokenValid()) {
    return router.createUrlTree(['/dashboard']);
  }

  return router.createUrlTree(['/login']);
};
