import { TestBed } from '@angular/core/testing';
import { TokenStorageService } from './token-storage.service';

describe('TokenStorageService', () => {
  let service: TokenStorageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenStorageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('access token', () => {
    it('should store and retrieve an access token', () => {
      service.setAccessToken('test-token');
      expect(service.getAccessToken()).toBe('test-token');
    });

    it('should return null when no access token is stored', () => {
      expect(service.getAccessToken()).toBeNull();
    });

    it('should overwrite an existing access token', () => {
      service.setAccessToken('first');
      service.setAccessToken('second');
      expect(service.getAccessToken()).toBe('second');
    });
  });

  describe('refresh token', () => {
    it('should store and retrieve a refresh token', () => {
      service.setRefreshToken('refresh-me');
      expect(service.getRefreshToken()).toBe('refresh-me');
    });

    it('should return null when no refresh token is stored', () => {
      expect(service.getRefreshToken()).toBeNull();
    });
  });

  describe('clear', () => {
    it('should remove all tokens', () => {
      service.setAccessToken('token');
      service.setRefreshToken('refresh');
      service.clear();
      expect(service.getAccessToken()).toBeNull();
      expect(service.getRefreshToken()).toBeNull();
    });
  });

  describe('decodePayload', () => {
    it('should decode a valid JWT payload', () => {
      const payload = { sub: '123', name: 'Test', exp: 9999999999 };
      const base64 = btoa(JSON.stringify(payload));
      const token = `header.${base64}.signature`;
      const decoded = service.decodePayload(token);
      expect(decoded?.sub).toBe('123');
      expect(decoded?.name).toBe('Test');
    });

    it('should return null for an invalid token', () => {
      expect(service.decodePayload('invalid')).toBeNull();
    });
  });

  describe('isTokenValid', () => {
    it('should return true for a non-expired token', () => {
      const futureExp = Math.floor(Date.now() / 1000) + 3600;
      const payload = { exp: futureExp };
      const base64 = btoa(JSON.stringify(payload));
      service.setAccessToken(`header.${base64}.signature`);
      expect(service.isTokenValid()).toBe(true);
    });

    it('should return false for an expired token', () => {
      const pastExp = Math.floor(Date.now() / 1000) - 3600;
      const payload = { exp: pastExp };
      const base64 = btoa(JSON.stringify(payload));
      service.setAccessToken(`header.${base64}.signature`);
      expect(service.isTokenValid()).toBe(false);
    });

    it('should return false when no token exists', () => {
      expect(service.isTokenValid()).toBe(false);
    });
  });

  describe('getCurrentUser', () => {
    it('should extract user info from a valid token', () => {
      const payload = {
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': '42',
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'johndoe',
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'john@test.com',
        exp: 9999999999
      };
      const base64 = btoa(JSON.stringify(payload));
      service.setAccessToken(`header.${base64}.signature`);
      const user = service.getCurrentUser();
      expect(user?.id).toBe('42');
      expect(user?.username).toBe('johndoe');
      expect(user?.email).toBe('john@test.com');
    });

    it('should return null when no token is set', () => {
      expect(service.getCurrentUser()).toBeNull();
    });
  });
});
