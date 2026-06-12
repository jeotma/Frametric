import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError, firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { AuthService as ApiAuthService } from '../api/api/auth.service';
import { TokenStorageService } from './token-storage.service';

function createMocks() {
  return {
    apiAuth: { apiAuthLoginPost: vi.fn(), apiAuthSignupPost: vi.fn() },
    tokenStorage: { isTokenValid: vi.fn(), getCurrentUser: vi.fn(), setAccessToken: vi.fn(), setRefreshToken: vi.fn(), clear: vi.fn() },
    router: { navigate: vi.fn() },
  };
}

function setupModule(mocks: ReturnType<typeof createMocks>) {
  TestBed.configureTestingModule({
    providers: [
      AuthService,
      { provide: ApiAuthService, useValue: mocks.apiAuth },
      { provide: TokenStorageService, useValue: mocks.tokenStorage },
      { provide: Router, useValue: mocks.router },
    ]
  });
  return TestBed.inject(AuthService);
}

describe('AuthService', () => {
  let service: AuthService;
  let mocks: ReturnType<typeof createMocks>;

  beforeEach(() => {
    mocks = createMocks();
    service = setupModule(mocks);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial state', () => {
    beforeEach(() => {
      TestBed.resetTestingModule();
    });

    it('should be unauthenticated when no valid token', () => {
      const m = createMocks();
      m.tokenStorage.isTokenValid.mockReturnValue(false);
      const s = setupModule(m);
      expect(s.isAuthenticated()).toBe(false);
      expect(s.currentUser()).toBeNull();
    });

    it('should restore session when token is valid', () => {
      const m = createMocks();
      const fakeUser = { id: '1', username: 'test', email: 'test@test.com' };
      m.tokenStorage.isTokenValid.mockReturnValue(true);
      m.tokenStorage.getCurrentUser.mockReturnValue(fakeUser);
      const s = setupModule(m);
      expect(s.isAuthenticated()).toBe(true);
      expect(s.currentUser()).toEqual(fakeUser);
    });
  });

  describe('login', () => {
    const credentials = { email: 'user@test.com', password: 'secret' };
    const loginResponse = { accessToken: 'access-token', refreshToken: 'refresh-token' };

    it('should store tokens and set user on successful login', async () => {
      mocks.apiAuth.apiAuthLoginPost.mockReturnValue(of(loginResponse));
      const fakeUser = { id: '1', username: 'user', email: 'user@test.com' };
      mocks.tokenStorage.getCurrentUser.mockReturnValue(fakeUser);

      await firstValueFrom(service.login(credentials.email, credentials.password));

      expect(mocks.apiAuth.apiAuthLoginPost).toHaveBeenCalledWith(credentials);
      expect(mocks.tokenStorage.setAccessToken).toHaveBeenCalledWith('access-token');
      expect(mocks.tokenStorage.setRefreshToken).toHaveBeenCalledWith('refresh-token');
      expect(service.currentUser()).toEqual(fakeUser);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should not store tokens on failed login', async () => {
      const err = { status: 401, message: 'Unauthorized' };
      mocks.apiAuth.apiAuthLoginPost.mockReturnValue(throwError(() => err));

      await expect(firstValueFrom(service.login(credentials.email, credentials.password))).rejects.toBe(err);

      expect(mocks.tokenStorage.setAccessToken).not.toHaveBeenCalled();
      expect(mocks.tokenStorage.setRefreshToken).not.toHaveBeenCalled();
    });
  });

  describe('register', () => {
    const regData = { username: 'newuser', email: 'new@test.com', password: 'secret' };

    it('should call signup API on register', async () => {
      mocks.apiAuth.apiAuthSignupPost.mockReturnValue(of({}));

      await firstValueFrom(service.register(regData.username, regData.email, regData.password));

      expect(mocks.apiAuth.apiAuthSignupPost).toHaveBeenCalledWith(regData);
    });

    it('should propagate registration errors', async () => {
      const err = { status: 400, message: 'Email taken' };
      mocks.apiAuth.apiAuthSignupPost.mockReturnValue(throwError(() => err));

      await expect(firstValueFrom(service.register(regData.username, regData.email, regData.password))).rejects.toBe(err);
    });
  });

  describe('logout', () => {
    it('should clear tokens, set user to null, and navigate to login', () => {
      service.logout();
      expect(mocks.tokenStorage.clear).toHaveBeenCalled();
      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
      expect(mocks.router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });
});
