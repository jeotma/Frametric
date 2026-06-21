import { Injectable } from '@angular/core';

const ACCESS_TOKEN_KEY = 'frametric_access_token';
const REFRESH_TOKEN_KEY = 'frametric_refresh_token';

export interface JwtPayload {
  sub?: string;
  name?: string;
  email?: string;
  exp?: number;
  [key: string]: unknown;
}

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  setAccessToken(token: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, token);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  setRefreshToken(token: string): void {
    localStorage.setItem(REFRESH_TOKEN_KEY, token);
  }

  clear(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }

  isTokenValid(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;
    const payload = this.decodePayload(token);
    if (!payload?.exp) return false;
    return payload.exp * 1000 > Date.now();
  }

  decodePayload(token: string): JwtPayload | null {
    try {
      const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(json) as JwtPayload;
    } catch {
      return null;
    }
  }

  getCurrentUser(): { id: string; username: string; email: string } | null {
    const token = this.getAccessToken();
    if (!token) return null;
    const payload = this.decodePayload(token);
    if (!payload) return null;

    // ASP.NET Core standard claim URIs
    const id =
      (payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] as string) ??
      payload['sub'] as string ?? '';
    const username =
      (payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] as string) ?? '';
    const email =
      (payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] as string) ?? '';

    return { id, username, email };
  }

  getUserRole(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;
    const payload = this.decodePayload(token);
    if (!payload) return null;

    return (
      (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as string) ??
      (payload['role'] as string) ??
      null
    );
  }

  isAdmin(): boolean {
    return this.getUserRole() === 'Admin';
  }
}

