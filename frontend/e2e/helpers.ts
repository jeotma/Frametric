import { Page } from '@playwright/test';

// Helper to simulate authentication client-side
export async function loginAndSetToken(page: Page) {
  const b64 = (obj: any) => {
    const str = JSON.stringify(obj);
    const base64 = btoa(unescape(encodeURIComponent(str)));
    return base64
      .replace(/=/g, '')
      .replace(/\+/g, '-')
      .replace(/\//g, '_');
  };
  
  const header = b64({ alg: 'HS256', typ: 'JWT' });
  const payload = b64({
    sub: '12345',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': '12345',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'Test User',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'test@example.com',
    exp: Math.floor(Date.now() / 1000) + 3600
  });
  const token = `${header}.${payload}.signature`;
  
  await page.goto('/');
  await page.evaluate(({ token }) => {
    localStorage.setItem('frametric_access_token', token);
    localStorage.setItem('frametric_refresh_token', 'refresh_token');
  }, { token });
}
