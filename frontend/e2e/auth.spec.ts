import { test, expect } from '@playwright/test';

test.describe('Authentication and Navigation Flow', () => {
  test('should redirect unauthenticated users from dashboard to login', async ({ page }) => {
    // Attempt to access dashboard
    await page.goto('/dashboard');

    // Should be redirected to /login because of authGuard
    await expect(page).toHaveURL(/\/login/);
    await expect(page.locator('.brand-name')).toHaveText('Frametric');
    await expect(page.locator('.auth-header h2')).toHaveText('Welcome back');
  });

  test('should show validation errors on invalid form submission', async ({ page }) => {
    await page.goto('/login');

    // Click submit immediately
    await page.locator('#login-submit').click();

    // Trigger validation by touching fields
    const emailInput = page.locator('#login-email');
    await emailInput.focus();
    await emailInput.blur();

    const passwordInput = page.locator('#login-password');
    await passwordInput.focus();
    await passwordInput.blur();

    // Check validation hint texts
    await expect(page.locator('.field-hint.error').first()).toContainText('Email is required.');
    await expect(page.locator('.field-hint.error').last()).toContainText('Password must be at least 6 characters.');
  });
});
