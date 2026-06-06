import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { AuthService as ApiAuthService } from '../api/api/auth.service';
import { TokenStorageService } from './token-storage.service';

describe('AuthService', () => {
  let service: AuthService;
  let mockApiAuth: jasmine.SpyObj<ApiAuthService>;
  let mockTokenStorage: jasmine.SpyObj<TokenStorageService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    mockApiAuth = jasmine.createSpyObj('ApiAuthService', ['apiAuthLoginPost', 'apiAuthSignupPost']);
    mockTokenStorage = jasmine.createSpyObj('TokenStorageService', [
      'isTokenValid', 'getCurrentUser',
      'setAccessToken', 'setRefreshToken', 'clear'
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: ApiAuthService, useValue: mockApiAuth },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: Router, useValue: mockRouter },
      ]
    });

    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial state', () => {
    it('should be unauthenticated when no valid token', () => {
      mockTokenStorage.isTokenValid.and.returnValue(false);
      const s = TestBed.inject(AuthService);
      expect(s.isAuthenticated()).toBeFalse();
      expect(s.currentUser()).toBeNull();
    });

    it('should restore session when token is valid', () => {
      const fakeUser = { id: '1', username: 'test', email: 'test@test.com' };
      mockTokenStorage.isTokenValid.and.returnValue(true);
      mockTokenStorage.getCurrentUser.and.returnValue(fakeUser);
      const s = TestBed.inject(AuthService);
      expect(s.isAuthenticated()).toBeTrue();
      expect(s.currentUser()).toEqual(fakeUser);
    });
  });

  describe('login', () => {
    const credentials = { email: 'user@test.com', password: 'secret' };
    const loginResponse = { accessToken: 'access-token', refreshToken: 'refresh-token' };

    it('should store tokens and set user on successful login', (done) => {
      mockApiAuth.apiAuthLoginPost.and.returnValue(of(loginResponse));
      const fakeUser = { id: '1', username: 'user', email: 'user@test.com' };
      mockTokenStorage.getCurrentUser.and.returnValue(fakeUser);

      service.login(credentials.email, credentials.password).subscribe({
        next: () => {
          expect(mockApiAuth.apiAuthLoginPost).toHaveBeenCalledWith(credentials);
          expect(mockTokenStorage.setAccessToken).toHaveBeenCalledWith('access-token');
          expect(mockTokenStorage.setRefreshToken).toHaveBeenCalledWith('refresh-token');
          expect(service.currentUser()).toEqual(fakeUser);
          expect(service.isAuthenticated()).toBeTrue();
          done();
        }
      });
    });

    it('should not store tokens on failed login', (done) => {
      const err = { status: 401, message: 'Unauthorized' };
      mockApiAuth.apiAuthLoginPost.and.returnValue(throwError(() => err));

      service.login(credentials.email, credentials.password).subscribe({
        error: (e) => {
          expect(e).toBe(err);
          expect(mockTokenStorage.setAccessToken).not.toHaveBeenCalled();
          expect(mockTokenStorage.setRefreshToken).not.toHaveBeenCalled();
          done();
        }
      });
    });
  });

  describe('register', () => {
    const regData = { username: 'newuser', email: 'new@test.com', password: 'secret' };

    it('should call signup API on register', (done) => {
      mockApiAuth.apiAuthSignupPost.and.returnValue(of({}));

      service.register(regData.username, regData.email, regData.password).subscribe({
        next: () => {
          expect(mockApiAuth.apiAuthSignupPost).toHaveBeenCalledWith(regData);
          done();
        }
      });
    });

    it('should propagate registration errors', (done) => {
      const err = { status: 400, message: 'Email taken' };
      mockApiAuth.apiAuthSignupPost.and.returnValue(throwError(() => err));

      service.register(regData.username, regData.email, regData.password).subscribe({
        error: (e) => {
          expect(e).toBe(err);
          done();
        }
      });
    });
  });

  describe('logout', () => {
    it('should clear tokens, set user to null, and navigate to login', () => {
      service.logout();
      expect(mockTokenStorage.clear).toHaveBeenCalled();
      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBeFalse();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    });
  });
});
