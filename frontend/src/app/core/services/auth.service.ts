import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { AuthService as ApiAuthService } from '../api/api/auth.service';
import { TokenStorageService } from './token-storage.service';

export interface CurrentUser {
  id: string;
  username: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _apiAuth = inject(ApiAuthService);
  private readonly _tokenStorage = inject(TokenStorageService);
  private readonly _router = inject(Router);

  private readonly _currentUser = signal<CurrentUser | null>(
    this._tokenStorage.isTokenValid() ? this._tokenStorage.getCurrentUser() : null
  );

  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);

  login(email: string, password: string): Observable<void> {
    return this._apiAuth.apiAuthLoginPost({ email, password }).pipe(
      tap(response => {
        this._tokenStorage.setAccessToken(response.accessToken!);
        this._tokenStorage.setRefreshToken(response.refreshToken!);
        this._currentUser.set(this._tokenStorage.getCurrentUser());
      }),
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      catchError((err: any) => throwError(() => err)),
      // Map to void
      tap({ complete: () => {} }) as never
    ) as unknown as Observable<void>;
  }

  register(username: string, email: string, password: string): Observable<void> {
    return this._apiAuth.apiAuthSignupPost({ username, email, password }).pipe(
      // After successful registration, auto-login
      tap(() => {}),
      catchError((err: any) => throwError(() => err)) // eslint-disable-line @typescript-eslint/no-explicit-any
    ) as unknown as Observable<void>;
  }

  logout(): void {
    this._tokenStorage.clear();
    this._currentUser.set(null);
    this._router.navigate(['/login']);
  }
}
