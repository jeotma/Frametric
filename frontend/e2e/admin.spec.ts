import { test, expect } from '@playwright/test';

async function loginAsSuperAdmin(page: any) {
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
    sub: 'superadmin-id',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': 'superadmin-id',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'SuperAdmin User',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'superadmin@example.com',
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'SuperAdmin',
    exp: Math.floor(Date.now() / 1000) + 3600
  });
  const token = `${header}.${payload}.signature`;
  
  await page.goto('/');
  await page.evaluate(({ token }: { token: string }) => {
    localStorage.setItem('frametric_access_token', token);
    localStorage.setItem('frametric_refresh_token', 'refresh_token');
  }, { token });
}

test.describe('Admin Control Panel and Catalog Editing', () => {

  test.beforeEach(async ({ page }) => {
    // Mock the statistics and diagnostics API requests
    await page.route('**/api/Admin/diagnostics/database', async route => {
      await route.fulfill({
        status: 200,
        json: {
          pendingMovies: 2,
          completedMovies: 120,
          failedMovies: 1,
          notFoundMovies: 0,
          permanentlyFailedMovies: 0,
          totalMovies: 123,
          totalTvShows: 5,
          totalDiaryEntries: 450,
          totalGenres: 18,
          totalDirectors: 82,
          totalActors: 320
        }
      });
    });

    await page.route('**/api/Admin/users', async route => {
      await route.fulfill({
        status: 200,
        json: [
          {
            id: 'user-1',
            username: 'alice',
            email: 'alice@example.com',
            role: 'User',
            canManageCatalog: false,
            canAddUsers: false,
            canDeleteUsers: false,
            canPromoteToAdmin: false
          },
          {
            id: 'user-2',
            username: 'bob-admin',
            email: 'bob@example.com',
            role: 'Admin',
            canManageCatalog: true,
            canAddUsers: false,
            canDeleteUsers: false,
            canPromoteToAdmin: false
          }
        ]
      });
    });

    await loginAsSuperAdmin(page);
  });

  test('should load the admin configuration page and display tab sections', async ({ page }) => {
    await page.goto('/settings');
    
    // Check if tabs exist
    await expect(page.locator('button:has-text("Database & Queue")')).toBeVisible();
    await expect(page.locator('button:has-text("User Management")')).toBeVisible();
    await expect(page.locator('button:has-text("Catalog Editing")')).toBeVisible();
  });

  test('should allow SuperAdmin to view, create users, and delegate permissions', async ({ page }) => {
    await page.goto('/settings');
    await page.locator('button:has-text("User Management")').click();

    // Verify user list loaded
    await expect(page.locator('td:has-text("alice")')).toBeVisible();
    await expect(page.locator('td:has-text("bob-admin")')).toBeVisible();

    // Verify checkboxes are active for SuperAdmin
    const catalogCheckbox = page.locator('input[type="checkbox"]').first();
    await expect(catalogCheckbox).toBeEnabled();

    // Verify Add New User button is visible and toggles form
    const addUserButton = page.locator('button:has-text("Add New User")');
    await expect(addUserButton).toBeVisible();
    await addUserButton.click();
    await expect(page.locator('h3:has-text("Create New User Account")')).toBeVisible();
  });

  test('should support catalog search, edits, and revision history', async ({ page }) => {
    // Mock the search API
    await page.route('**/api/Search?query=Inception', async route => {
      await route.fulfill({
        status: 200,
        json: [
          {
            localId: 'inception-id',
            entityType: 'Movie',
            titleOrName: 'Inception',
            releaseYear: 2010,
            isLocal: true
          }
        ]
      });
    });

    // Mock movie details API
    await page.route('**/api/movies/inception-id', async route => {
      await route.fulfill({
        status: 200,
        json: {
          id: 'inception-id',
          title: 'Inception',
          overview: 'A thief who steals corporate secrets...',
          releaseYear: 2010,
          runtimeMinutes: 148
        }
      });
    });

    // Mock revision history API
    await page.route('**/api/admin/catalog/revisions/Movie/inception-id', async route => {
      await route.fulfill({
        status: 200,
        json: [
          {
            id: 'revision-1',
            entityType: 'Movie',
            entityId: 'inception-id',
            changedAt: '2026-06-27T12:00:00Z',
            changedBy: 'SuperAdmin User',
            stateJson: '{"Title": "Inception Original"}'
          }
        ]
      });
    });

    await page.goto('/settings');
    await page.locator('button:has-text("Catalog Editing")').click();

    // Perform Search
    const searchInput = page.locator('input[placeholder="SEARCH FOR TITLES OR NAMES..."]');
    await searchInput.fill('Inception');
    await page.locator('button:has-text("Search")').click();

    // Click result item
    await page.locator('td:has-text("Inception")').click();

    // Verify edit details pre-populate
    const titleInput = page.locator('input[type="text"]').last();
    await expect(titleInput).toHaveValue('Inception');

    // Verify revision history displays
    await expect(page.locator('td:has-text("SuperAdmin User")')).toBeVisible();
  });
});
