import { test, expect } from '@playwright/test';

test.describe('Authentication and Navigation Flow', () => {

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

  test('should navigate to forgot-password, submit email and get confirmation', async ({ page }) => {
    await page.goto('/login');
    await page.locator('text=Forgot password?').click();
    await expect(page).toHaveURL(/\/forgot-password/);
    
    // Check initial state
    await expect(page.locator('h2')).toContainText('PASSWORD RECOVERY');
    
    // Submit email
    await page.locator('#fp-email').fill('test@example.com');
    
    // Mock the api
    await page.route('**/forgot-password', async route => {
      await route.fulfill({ status: 200, json: {} });
    });
    
    await page.locator('button[type="submit"]').click();
    
    // Should show success state
    await expect(page.locator('.success-banner')).toBeVisible();
  });

  test('should navigate to reset-password with token, submit new password and redirect', async ({ page }) => {
    await page.goto('/reset-password?token=dummy-token&email=test@example.com');
    
    // Mock the api
    await page.route('**/reset-password', async route => {
      await route.fulfill({ status: 200, json: {} });
    });
    
    await page.locator('#rp-password').fill('SecureP@ss1!');
    await page.locator('button[type="submit"]').click();
    
    // Check success toast and redirect to login
    await expect(page.locator('.success-banner')).toContainText('Password has been successfully reset');
    await expect(page).toHaveURL(/\/login/);
  });
});
