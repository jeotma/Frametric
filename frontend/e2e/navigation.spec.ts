import { test, expect, Page } from '@playwright/test';

declare const Buffer: any;

// Helper to simulate authentication client-side
async function loginAndSetToken(page: Page) {
  const b64 = (obj: any) => Buffer.from(JSON.stringify(obj)).toString('base64url');
  
  const header = b64({ alg: 'HS256', typ: 'JWT' });
  const payload = b64({
    sub: '12345',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': '12345',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'Test User',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'test@example.com',
    exp: Math.floor(Date.now() / 1000) + 3600 // expires in 1 hour
  });
  const token = `${header}.${payload}.signature`;
  
  // Go to root to set localstorage
  await page.goto('/');
  await page.evaluate(({ token }) => {
    localStorage.setItem('frametric_access_token', token);
    localStorage.setItem('frametric_refresh_token', 'refresh_token');
  }, { token });
}

test.describe('Global Navigation and Shell Layout', () => {
  test.beforeEach(async ({ page }) => {
    // Intercept essential APIs to avoid backend reliance
    await page.route('**/api/analytics/dashboard', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalWatchtimeMinutes: 120000,
          totalWatches: 1050,
          uniqueMoviesCount: 950,
          topGenres: [{ genreName: 'Sci-Fi', count: 120 }, { genreName: 'Drama', count: 90 }],
          decadeBreakdown: [{ decade: 2020, count: 50 }, { decade: 2010, count: 80 }],
          topDirectors: [{ directorName: 'Christopher Nolan', count: 15, averageRating: 4.8 }],
          topActors: [{ actorName: 'Leonardo DiCaprio', count: 22, averageRating: 4.5 }]
        })
      });
    });

    await page.route('**/api/import/history', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { id: '1', fileName: 'letterboxd_data.zip', status: 'Completed', timestamp: new Date().toISOString() }
        ])
      });
    });
  });

  test('should login, display logo, topbar search input, external LinkedIn links, and sidebar pages', async ({ page }) => {
    await loginAndSetToken(page);
    await page.goto('/dashboard');

    // 1. Verify layout shell elements
    await expect(page.locator('.logo-text')).toHaveText('Frametric');
    await expect(page.locator('.logo-version')).toHaveText('v1.2');
    
    // Topbar Search
    const searchInput = page.locator('.search-box input');
    await expect(searchInput).toBeVisible();
    await expect(searchInput).toHaveAttribute('placeholder', 'Search movies, directors, genres...');
    await searchInput.fill('Inception');

    // Topbar external Link
    const linkedinLink = page.locator('.header-linkedin-link');
    await expect(linkedinLink).toBeVisible();
    await expect(linkedinLink).toHaveAttribute('href', 'https://www.linkedin.com/in/jesus-otero-dev');

    // 2. Test Sidebar navigation links
    const dashboardLink = page.locator('nav.nav-menu a[routerLink="/dashboard"]');
    const importsLink = page.locator('nav.nav-menu a[routerLink="/imports"]');
    const finalCutLink = page.locator('nav.nav-menu a[routerLink="/final-cut-teaser"]');
    const statsLink = page.locator('nav.nav-menu a[routerLink="/stats"]');
    const recommendationsLink = page.locator('nav.nav-menu a[routerLink="/recommendations"]');

    await expect(dashboardLink).toBeVisible();
    await expect(importsLink).toBeVisible();
    await expect(finalCutLink).toBeVisible();
    await expect(statsLink).toBeVisible();
    await expect(recommendationsLink).toBeVisible();

    // Verify Active State transitions
    await expect(dashboardLink).toHaveClass(/active/);

    await importsLink.click();
    await expect(page).toHaveURL(/\/imports/);
    await expect(importsLink).toHaveClass(/active/);

    await statsLink.click();
    await expect(page).toHaveURL(/\/stats/);
    await expect(statsLink).toHaveClass(/active/);

    await recommendationsLink.click();
    await expect(page).toHaveURL(/\/recommendations/);
    await expect(recommendationsLink).toHaveClass(/active/);

    await finalCutLink.click();
    await expect(page).toHaveURL(/\/final-cut-teaser/);
    await expect(finalCutLink).toHaveClass(/active/);
  });

  test('should verify Dashboard buttons and redirect behaviors', async ({ page }) => {
    await loginAndSetToken(page);
    await page.goto('/dashboard');

    // Click "Advanced Stats" button in dashboard header
    const advStatsButton = page.locator('.header-actions a.btn-secondary:has-text("Advanced Stats")');
    await expect(advStatsButton).toBeVisible();
    await advStatsButton.click();
    await expect(page).toHaveURL(/\/stats/);

    // Go back and click "Final Cut 2025"
    await page.goto('/dashboard');
    const finalCutTeaserButton = page.locator('.header-actions a.btn-primary:has-text("Final Cut 2025")');
    await expect(finalCutTeaserButton).toBeVisible();
    await finalCutTeaserButton.click();
    await expect(page).toHaveURL(/\/final-cut-teaser/);
  });

  test('should test user profile menu and logging out', async ({ page }) => {
    await loginAndSetToken(page);
    await page.goto('/dashboard');

    // Click profile section
    const profilePanel = page.locator('.user-profile');
    await expect(profilePanel).toBeVisible();
    await profilePanel.click();

    // Sign out menu should be visible
    const signOutBtn = page.locator('.user-menu button:has-text("Sign Out")');
    await expect(signOutBtn).toBeVisible();
    await signOutBtn.click();

    // User should be redirected to login page and localStorage cleared
    await expect(page).toHaveURL(/\/login/);
    const token = await page.evaluate(() => localStorage.getItem('frametric_access_token'));
    expect(token).toBeNull();
  });
});
