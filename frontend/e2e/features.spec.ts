import { test, expect, Page } from '@playwright/test';

// Helper to simulate authentication client-side
async function loginAndSetToken(page: Page) {
  const b64 = (obj: any) => Buffer.from(JSON.stringify(obj)).toString('base64url');
  
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

test.describe('Dashboard, Import, and Final Cut Tests', () => {

  // --- DASHBOARD TESTS ---
  test.describe('Dashboard Feature', () => {
    test('should show warning banner when there is no successful import', async ({ page }) => {
      // 1. Mock empty import history and empty dashboard BEFORE login navigation (using case-insensitive RegExp)
      await page.route(/\/api\/import\/history/i, async (route) => {
        await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify([]) });
      });
      await page.route(/\/api\/analytics\/dashboard/i, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ totalWatchtimeMinutes: 0, totalWatches: 0, uniqueMoviesCount: 0 })
        });
      });

      // 2. Perform authentication and page loading
      await loginAndSetToken(page);
      await page.goto('/dashboard');

      // Assert banner is visible
      const banner = page.locator('.no-imports-banner');
      await expect(banner).toBeVisible();
      await expect(banner.locator('h3')).toContainText('No active film imports found');
      
      const link = banner.locator('a[routerLink="/imports"]');
      await expect(link).toBeVisible();
      await expect(link).toContainText('Go to Import Center');
    });

    test('should show full stats grid when dashboard has data', async ({ page }) => {
      // 1. Mock valid history and stats BEFORE login navigation (using case-insensitive RegExp)
      await page.route(/\/api\/import\/history/i, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([{ id: '1', fileName: 'data.zip', status: 'Completed' }])
        });
      });
      await page.route(/\/api\/analytics\/dashboard/i, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            totalWatchtimeMinutes: 24000,
            totalWatches: 200,
            uniqueMoviesCount: 190,
            topGenres: [{ genreName: 'Thriller', count: 35 }],
            decadeBreakdown: [{ decade: 2000, count: 42 }],
            topDirectors: [{ directorName: 'Denis Villeneuve', count: 8, averageRating: 4.6 }],
            topActors: [{ actorName: 'Ryan Gosling', count: 12, averageRating: 4.2 }]
          })
        });
      });

      // 2. Perform authentication and page loading
      await loginAndSetToken(page);
      await page.goto('/dashboard');

      // Assert stats
      await expect(page.locator('.stat-card:has-text("Total Watchtime") .stat-value')).toHaveText('400h');
      await expect(page.locator('.stat-card:has-text("Total Watches") .stat-value')).toHaveText('200');
      await expect(page.locator('.stat-card:has-text("Unique Films") .stat-value')).toHaveText('190');

      // Leaderboard
      await expect(page.locator('.leaderboard-panel:has-text("Top Directors") .name').first()).toHaveText('Denis Villeneuve');
      await expect(page.locator('.leaderboard-panel:has-text("Top Actors") .name').first()).toHaveText('Ryan Gosling');
    });
  });

  // --- IMPORT CENTER TESTS ---
  test.describe('Import Center Feature', () => {
    test('should list history and handle file upload flows', async ({ page }) => {
      let uploadCalled = false;
      
      // 1. Setup route mocks before page loading (using case-insensitive RegExp)
      await page.route(/\/api\/import\/history/i, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            { id: 'import-123', providerSource: 'Letterboxd', status: 'Completed', timestamp: new Date().toISOString() }
          ])
        });
      });
      await page.route(/\/api\/import\/letterboxd/i, async (route) => {
        uploadCalled = true;
        await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ success: true }) });
      });
      await page.route(/\/api\/analytics\/dashboard/i, async (route) => {
        await route.fulfill({ status: 200, contentType: 'application/json', body: '{}' });
      });

      // 2. Log in and load target page
      await loginAndSetToken(page);
      await page.goto('/imports');

      // Check current import item in table
      const fileCell = page.locator('.file-name-cell');
      await expect(fileCell).toBeVisible();
      await expect(fileCell).toContainText('Letterboxd Archive');
      
      const badge = page.locator('.badge');
      await expect(badge).toHaveText('Completed');

      // Trigger upload via clicking file dropzone button
      const fileChooserPromise = page.waitForEvent('filechooser');
      const responsePromise = page.waitForResponse(/\/api\/import\/letterboxd/i);
      await page.locator('.upload-dropzone button').click();
      const fileChooser = await fileChooserPromise;
      
      // Upload fake zip
      await fileChooser.setFiles({
        name: 'test_export.zip',
        mimeType: 'application/zip',
        buffer: Buffer.from('fake zip binary data')
      });

      // Wait for the mock API response to resolve
      const response = await responsePromise;
      expect(response.status()).toBe(200);
    });

    test('should show error banner when uploading non-zip file', async ({ page }) => {
      // 1. Setup route mocks before page loading (using case-insensitive RegExp)
      await page.route(/\/api\/import\/history/i, async (route) => {
        await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify([]) });
      });
      await page.route(/\/api\/analytics\/dashboard/i, async (route) => {
        await route.fulfill({ status: 200, contentType: 'application/json', body: '{}' });
      });

      // 2. Log in and load target page
      await loginAndSetToken(page);
      await page.goto('/imports');

      const fileChooserPromise = page.waitForEvent('filechooser');
      await page.locator('.upload-dropzone button').click();
      const fileChooser = await fileChooserPromise;

      // Upload text file instead of zip
      await fileChooser.setFiles({
        name: 'test_export.txt',
        mimeType: 'text/plain',
        buffer: Buffer.from('hello world')
      });

      // Assert alert
      await expect(page.locator('.error-banner')).toBeVisible();
      await expect(page.locator('.error-banner')).toContainText('Only .zip files are supported.');
    });
  });

  // --- FINAL CUT TESTS ---
  test.describe('Final Cut Feature', () => {
    test('should load presentation slides, control slide progression, and close on ESC key', async ({ page }) => {
      // 1. Setup route mocks before page loading (using case-insensitive RegExp)
      await page.route(/\/api\/analytics\/wrapped/i, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            watchYear: 2025,
            totalWatchtimeMinutes: 10000,
            totalWatches: 85,
            uniqueMoviesCount: 80
          })
        });
      });
      
      const mockEmptyArray = async (route: any) => route.fulfill({ status: 200, contentType: 'application/json', body: '[]' });
      const mockEmptyObj = async (route: any) => route.fulfill({ status: 200, contentType: 'application/json', body: '{}' });

      await page.route(/\/api\/analytics\/advanced\/final-cut/i, mockEmptyObj);
      await page.route(/\/api\/analytics\/advanced\/bonus/i, mockEmptyObj);
      await page.route(/\/api\/analytics\/advanced\/watched/i, mockEmptyArray);
      await page.route(/\/api\/analytics\/dashboard/i, async (route) => {
        await route.fulfill({ status: 200, contentType: 'application/json', body: '{}' });
      });
      await page.route(/\/api\/import\/history/i, async (route) => {
        await route.fulfill({ status: 200, contentType: 'application/json', body: '[]' });
      });

      // 2. Log in and load target page
      await loginAndSetToken(page);
      await page.goto('/final-cut/2025');

      // Check loader transitions to slide container
      const presentation = page.locator('.final-cut-container');
      await expect(presentation).toBeVisible();

      // Check current active slide is Intro (Slide index 0)
      const introSlide = page.locator('app-intro-slide');
      await expect(introSlide).toBeVisible();

      // Check page controls (prev / next slide triggers)
      const nextZone = page.locator('.right-zone');
      await nextZone.click();

      // Active slide should advance to index 1 (app-big-numbers-slide)
      const bigNumbersSlide = page.locator('app-big-numbers-slide');
      await expect(bigNumbersSlide).toBeVisible();

      const prevZone = page.locator('.left-zone');
      await prevZone.click();

      // Active slide should return to index 0 (app-intro-slide)
      await expect(introSlide).toBeVisible();

      // Pressing Escape should navigate back to dashboard
      await page.keyboard.press('Escape');
      await expect(page).toHaveURL(/\/dashboard/);
    });
  });
});
