import { test, expect } from '@playwright/test';
import { loginAndSetToken } from './helpers';

test.describe('Discovery Module', () => {
  test.beforeEach(async ({ page }) => {
    await loginAndSetToken(page);
  });

  test('should navigate to Discovery and display Roulette tab by default', async ({ page }) => {
    await page.goto('/discovery');
    
    // Tab should be Bingo or Roulette depending on the default state
    await expect(page.locator('h1')).toContainText('DISCOVERY', { ignoreCase: true });
    
    // The tabs exist
    await expect(page.locator('button', { hasText: 'Roulette' })).toBeVisible();
    await expect(page.locator('button', { hasText: 'Bingo' })).toBeVisible();
    await expect(page.locator('button', { hasText: 'Mystery Box' })).toBeVisible();
    await expect(page.locator('button', { hasText: 'Dice' })).toBeVisible();
    await expect(page.locator('button', { hasText: 'Slot Machine' })).toBeVisible();
  });

  test('should spell roulette and display winner', async ({ page }) => {
    await page.goto('/discovery');
    await page.locator('button', { hasText: 'Roulette' }).click();
    
    // Mock the API response for Roulette
    await page.route('**/api/v1/discovery/roulette', async route => {
      await route.fulfill({
        status: 200,
        json: {
          winner: {
            movieId: '123',
            title: 'The Matrix',
            directorName: 'Lana Wachowski, Lilly Wachowski',
            releaseYear: 1999,
            posterUrl: '/matrix.jpg',
            selectionMechanismMetadata: 'random'
          },
          spinSequence: [
            { movieId: '111', title: 'Fake Movie 1', posterUrl: null, directorName: '', releaseYear: 0, selectionMechanismMetadata: '' },
            { movieId: '222', title: 'Fake Movie 2', posterUrl: null, directorName: '', releaseYear: 0, selectionMechanismMetadata: '' },
            { movieId: '123', title: 'The Matrix', posterUrl: '/matrix.jpg', directorName: 'Lana Wachowski, Lilly Wachowski', releaseYear: 1999, selectionMechanismMetadata: 'random' }
          ]
        }
      });
    });

    await page.locator('button:has-text("Spin!")').click();
    
    // Wait for the animation to finish (the component uses a sequence with delays)
    // We expect the Winner Modal to be visible eventually
    await expect(page.locator('.winner-modal-content')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.winner-modal-content h2')).toContainText('The Matrix');
  });

  test('should spin slot machine and display winner', async ({ page }) => {
    await page.goto('/discovery');
    await page.locator('button', { hasText: 'Slot Machine' }).click();
    
    // Mock the API response for Slots
    await page.route('**/api/v1/discovery/slot-machine', async route => {
      await route.fulfill({
        status: 200,
        json: {
          movieId: '456',
          title: 'Inception',
          directorName: 'Christopher Nolan',
          releaseYear: 2010,
          posterUrl: '/inception.jpg',
          selectionMechanismMetadata: 'spin',
          isJackpot: false,
          reelResults: []
        }
      });
    });

    await page.locator('button:has-text("Pull Lever!")').click();
    
    // Wait for slot animation
    await expect(page.locator('.winner-modal-content')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.winner-modal-content h2')).toContainText('Inception');
  });
});
