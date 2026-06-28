import { test, expect, Page } from '@playwright/test';

// Helper to simulate authentication client-side
async function loginAndSetToken(page: Page) {
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

async function selectCinematicOption(page: Page, index: number, optionLabel: string) {
  const container = page.locator('app-cinematic-select').nth(index);
  await container.locator('.select-trigger').click({ force: true });
  const regex = new RegExp(`^\\s*${optionLabel.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&')}\\s*$`);
  await container.locator('.select-option').filter({ hasText: regex }).click({ force: true });
}

test.describe('Advanced Statistics Tests', () => {
  test.beforeEach(async ({ page }) => {
    await loginAndSetToken(page);
    await page.goto('/stats');
  });

  test('should verify default category and metric selection structure', async ({ page }) => {
    // Check page header
    await expect(page.locator('.page-title')).toContainText('TECHNICAL REPORT');

    // Check default category and metric values
    const categorySelect = page.locator('app-cinematic-select').first();
    const metricSelect = page.locator('app-cinematic-select').nth(1);

    await expect(categorySelect.locator('.selected-label')).toHaveText('Watched History');
    await expect(metricSelect.locator('.selected-label')).toHaveText('Movies Watched');
  });

  test('should verify input enable/disable states based on query selection', async ({ page }) => {
    // For 'watched_by_year', all filters are allowed: watchYear, releaseYear, minRating, maxRating, genre, director, actor
    const watchYearGroup = page.locator('.filter-group').filter({ has: page.locator('label', { hasText: /^Watch Year$/ }) });
    const releaseYearGroup = page.locator('.filter-group').filter({ has: page.locator('label', { hasText: /^Release Year$/ }) });
    const actorGroup = page.locator('.filter-group').filter({ has: page.locator('label', { hasText: /^Actor$/ }) });

    await expect(watchYearGroup).not.toHaveClass(/disabled-group/);
    await expect(releaseYearGroup).not.toHaveClass(/disabled-group/);
    await expect(actorGroup).not.toHaveClass(/disabled-group/);

    // Switch to category 'Watchlist' and metric 'watchlist_directors'
    await selectCinematicOption(page, 0, 'Watchlist');
    await selectCinematicOption(page, 1, 'Watchlist Directors');

    // For 'watchlist_directors', allowedFilters are: ['releaseYear', 'genre', 'actor']
    // So 'Watch Year' and 'Director' should be disabled (class disabled-group)
    const directorGroup = page.locator('.filter-group').filter({ has: page.locator('label', { hasText: /^Director$/ }) });
    await expect(watchYearGroup).toHaveClass(/disabled-group/);
    await expect(directorGroup).toHaveClass(/disabled-group/);
    await expect(actorGroup).not.toHaveClass(/disabled-group/);
  });

  test('should test LIST query execution, sorting, and tabular rendering', async ({ page }) => {
    // Intercept Watched History -> Movies Watched
    await page.route('**/api/v1/analytics/advanced/watched**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { title: 'Inception', rating: 4.5, releaseYear: 2010, liked: true },
          { title: 'The Dark Knight', rating: 5.0, releaseYear: 2008, liked: true },
          { title: 'Tenet', rating: 3.5, releaseYear: 2020, liked: false }
        ])
      });
    });

    // Run query
    await page.locator('.analyze-btn').click();

    // Verify list table headers
    const tableHeaders = page.locator('table.glass-table th');
    await expect(tableHeaders.first()).toContainText('Title');
    await expect(tableHeaders.nth(1)).toContainText('Rating');
    await expect(tableHeaders.last()).toContainText('Liked');

    // Verify data rows
    const rows = page.locator('table.glass-table tbody tr');
    await expect(rows).toHaveCount(3);
    await expect(rows.first().locator('td').first()).toContainText('Inception');

    // Verify sorting trigger (click title header)
    const titleHeader = tableHeaders.first();
    await titleHeader.click();
    // Verify sort icon presence
    await expect(titleHeader.locator('.sort-icon')).toBeVisible();
  });

  test('should test SINGLE query execution rendering card details', async ({ page }) => {
    await selectCinematicOption(page, 0, 'Watchlist Insights');
    await selectCinematicOption(page, 1, 'Oldest Pending Movie');

    // Intercept oldest pending API call
    await page.route('**/api/v1/analytics/advanced/watchlist/oldest-pending**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Modern Era',
          count: 420,
          averageRating: 2.06 // Will be multiplied by 2 in the template to become 4.12
        })
      });
    });

    // Run query
    await page.locator('.analyze-btn').click();

    // Assert custom single view container shows up
    const heroCard = page.locator('.hero-card');
    await expect(heroCard).toBeVisible();
    await expect(heroCard.locator('.hero-title')).toHaveText('Modern Era');
    await expect(heroCard.locator('.stat-box:has-text("Watches") .value')).toHaveText('420');
    await expect(heroCard.locator('.stat-box:has-text("Avg Rating") .value')).toContainText('4.12');
  });

  test('should test HABITS & CORRELATIONS queries rendering chart details', async ({ page }) => {
    await selectCinematicOption(page, 0, 'Habits & Correlations');
    await selectCinematicOption(page, 1, 'Preferred Watch Day');

    // Intercept preferred day API call
    await page.route('**/api/v1/analytics/advanced/watched/preferred-day**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { dayOfWeek: 'Friday', count: 45 },
          { dayOfWeek: 'Saturday', count: 68 },
          { dayOfWeek: 'Sunday', count: 50 }
        ])
      });
    });

    // Run query
    await page.locator('.analyze-btn').click();

    // Assert chart renders
    const chartCard = page.locator('.chart-card');
    await expect(chartCard).toBeVisible();
    const bars = chartCard.locator('.bar-col');
    await expect(bars).toHaveCount(3);
    await expect(bars.first().locator('.bar-label')).toHaveText('Friday');
    await expect(bars.nth(1).locator('.bar-value')).toHaveText('68');
  });
});
