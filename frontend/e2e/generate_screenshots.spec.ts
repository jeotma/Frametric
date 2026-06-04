// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

import { test, Page } from '@playwright/test';

const IMAGES_DIR = '../../portfolio/images';

// Helper to generate a mock JWT dynamically to avoid hardcoding secrets and triggering security scanners
function generateMockJwt(): string {
  const base64url = (obj: any) => {
    const str = JSON.stringify(obj);
    const base64 = btoa(unescape(encodeURIComponent(str)));
    return base64
      .replace(/=/g, '')
      .replace(/\+/g, '-')
      .replace(/\//g, '_');
  };

  const header = base64url({ alg: 'HS256', typ: 'JWT' });
  const payload = base64url({
    sub: '12345',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': '12345',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'Jesús J. Otero',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'jesusoteromartinez@outlook.com',
    exp: Math.floor(Date.now() / 1000) + 3600
  });

  return `${header}.${payload}.mock_signature_to_prevent_scanner_alerts`;
}

async function loginAndSetToken(page: Page) {
  const token = generateMockJwt();
  await page.goto('/');
  await page.evaluate((t) => {
    localStorage.setItem('frametric_access_token', t);
    localStorage.setItem('frametric_refresh_token', 'refresh_token');
  }, token);
}

// ── Mock data ────────────────────────────────────────────────────────────────

const POSTERS = {
  anora: 'https://image.tmdb.org/t/p/w500/oN0o3owobFjePDc5vMdLRAd0jkd.jpg',
  shutter: 'https://image.tmdb.org/t/p/w500/nrmXQ0zcZUL8jFLrakWc90IR8z9.jpg',
  starwars: 'https://image.tmdb.org/t/p/w500/6FfCtAuVAW8XJjZ7eWeLibRLWTw.jpg',
  projectx: 'https://image.tmdb.org/t/p/w500/lUPDGT3lyRrq8SvWuNWG2DP64bR.jpg',
  social: 'https://image.tmdb.org/t/p/w500/n0ybibhJtQ5icDqTp8eRytcIHJx.jpg',
  donnie: 'https://image.tmdb.org/t/p/w500/sv7D4vlfIH25lNjQYoXzoOFCYaz.jpg',
  midsommar: 'https://image.tmdb.org/t/p/w500/7LEI8ulZzO5gy9Ww2NVCrKmHeDZ.jpg',
  interstellar: 'https://image.tmdb.org/t/p/w500/yQvGrMoipbRoddT0ZR8tPoR7NfX.jpg',
};

const GUID = () => crypto.randomUUID();

const importHistory = [
  { id: GUID(), importDate: '2026-01-15T10:30:00Z', rowCount: 245, status: 'Completed', providerSource: 'Letterboxd', errorMessage: null },
  { id: GUID(), importDate: '2025-03-20T14:00:00Z', rowCount: 198, status: 'Completed', providerSource: 'Letterboxd', errorMessage: null },
];

const dashboardData = {
  totalWatchtimeMinutes: 28450,
  totalWatches: 245,
  uniqueMoviesCount: 228,
  topGenres: [
    { genreName: 'Sci-Fi', count: 48 },
    { genreName: 'Thriller', count: 42 },
    { genreName: 'Drama', count: 38 },
    { genreName: 'Action', count: 30 },
  ],
  decadeBreakdown: [
    { decade: 1980, count: 15 },
    { decade: 1990, count: 28 },
    { decade: 2000, count: 64 },
    { decade: 2010, count: 98 },
    { decade: 2020, count: 43 },
  ],
  topDirectors: [
    { directorName: 'Denis Villeneuve', count: 9, averageRating: 4.6 },
    { directorName: 'Christopher Nolan', count: 12, averageRating: 4.4 },
    { directorName: 'Quentin Tarantino', count: 8, averageRating: 4.3 },
    { directorName: 'Bong Joon-ho', count: 6, averageRating: 4.5 },
    { directorName: 'Greta Gerwig', count: 5, averageRating: 4.1 },
  ],
  topActors: [
    { actorName: 'Ryan Gosling', count: 14, averageRating: 4.2 },
    { actorName: 'Christian Bale', count: 11, averageRating: 4.3 },
    { actorName: 'Leonardo DiCaprio', count: 10, averageRating: 4.5 },
    { actorName: 'Cillian Murphy', count: 8, averageRating: 4.6 },
    { actorName: 'Florence Pugh', count: 7, averageRating: 4.0 },
  ],
};

const watchedMovies = [
  { title: 'The Social Network', releaseYear: 2010, director: 'David Fincher', rating: 4.5, liked: true },
  { title: 'Midsommar', releaseYear: 2019, director: 'Ari Aster', rating: 4.0, liked: true },
  { title: 'Donnie Darko', releaseYear: 2001, director: 'Richard Kelly', rating: 4.5, liked: true },
  { title: 'Star Wars', releaseYear: 1977, director: 'George Lucas', rating: 5.0, liked: true },
  { title: 'Anora', releaseYear: 2024, director: 'Sean Baker', rating: 4.5, liked: true },
  { title: 'Shutter Island', releaseYear: 2010, director: 'Martin Scorsese', rating: 4.5, liked: true },
  { title: 'Project X', releaseYear: 2012, director: 'Nima Nourizadeh', rating: 2.5, liked: false },
  { title: 'Interstellar', releaseYear: 2014, director: 'Christopher Nolan', rating: 5.0, liked: true },
  { title: 'Blade Runner 2049', releaseYear: 2017, director: 'Denis Villeneuve', rating: 4.5, liked: true },
  { title: 'Parasite', releaseYear: 2019, director: 'Bong Joon-ho', rating: 4.5, liked: true },
  { title: 'Dune', releaseYear: 2021, director: 'Denis Villeneuve', rating: 4.5, liked: true },
  { title: 'Oppenheimer', releaseYear: 2023, director: 'Christopher Nolan', rating: 5.0, liked: true },
];

const preferredDay = [
  { dayOfWeek: 'Monday', watchCount: 20 },
  { dayOfWeek: 'Tuesday', watchCount: 18 },
  { dayOfWeek: 'Wednesday', watchCount: 22 },
  { dayOfWeek: 'Thursday', watchCount: 25 },
  { dayOfWeek: 'Friday', watchCount: 52 },
  { dayOfWeek: 'Saturday', watchCount: 74 },
  { dayOfWeek: 'Sunday', watchCount: 58 },
];

const totalTimeInvested = {
  name: 'Christopher Nolan',
  totalMinutes: 2845,
  totalHours: 47,
};

const directorRanking = [
  { directorId: GUID(), name: 'Christopher Nolan', watchCount: 12, averageRating: 4.7, highestRatedMovieTitle: 'Interstellar' },
  { directorId: GUID(), name: 'Denis Villeneuve', watchCount: 9, averageRating: 4.6, highestRatedMovieTitle: 'Blade Runner 2049' },
  { directorId: GUID(), name: 'Bong Joon-ho', watchCount: 6, averageRating: 4.5, highestRatedMovieTitle: 'Parasite' },
  { directorId: GUID(), name: 'Quentin Tarantino', watchCount: 8, averageRating: 4.3, highestRatedMovieTitle: 'Pulp Fiction' },
  { directorId: GUID(), name: 'David Fincher', watchCount: 7, averageRating: 4.2, highestRatedMovieTitle: 'The Social Network' },
];

const wrappedSummaryData = {
  year: 2025,
  totalWatchtimeMinutes: 28450,
  totalWatches: 245,
  uniqueMoviesCount: 228,
  topGenres: dashboardData.topGenres,
  topDirectors: dashboardData.topDirectors,
  topActors: dashboardData.topActors,
  decadeBreakdown: dashboardData.decadeBreakdown,
  monthlyActivity: [
    { month: 1, count: 18 }, { month: 2, count: 14 }, { month: 3, count: 22 },
    { month: 4, count: 20 }, { month: 5, count: 25 }, { month: 6, count: 30 },
    { month: 7, count: 28 }, { month: 8, count: 15 }, { month: 9, count: 19 },
    { month: 10, count: 23 }, { month: 11, count: 12 }, { month: 12, count: 19 },
  ],
};

const bookendsData = {
  openingScene: {
    id: GUID(), title: 'Anora', releaseYear: 2024,
    posterPath: POSTERS.anora, runtimeMinutes: 139, rating: 4.5, liked: true,
  },
  fadeToBlack: {
    id: GUID(), title: 'Shutter Island', releaseYear: 2010,
    posterPath: POSTERS.shutter, runtimeMinutes: 138, rating: 4.5, liked: true,
  },
};

const primeTimeData = {
  peakDay: 'Saturday', peakDayCount: 74,
  peakMonth: 'December', peakMonthCount: 28,
  slumpDay: 'Monday', slumpDayCount: 12,
  slumpMonth: 'February', slumpMonthCount: 14,
};

const genreLandscape = [
  { genreName: 'Sci-Fi', count: 48, averageRating: 4.2 },
  { genreName: 'Thriller', count: 42, averageRating: 3.9 },
  { genreName: 'Drama', count: 38, averageRating: 4.1 },
  { genreName: 'Action', count: 30, averageRating: 3.7 },
  { genreName: 'Horror', count: 25, averageRating: 3.5 },
  { genreName: 'Comedy', count: 22, averageRating: 3.3 },
];

const castingPairs = [
  { actorName: 'Leonardo DiCaprio', pairedActorName: 'Brad Pitt', collaborationCount: 2 },
  { actorName: 'Christian Bale', pairedActorName: 'Cillian Murphy', collaborationCount: 2 },
];

const directorActorPairs = [
  { directorName: 'Christopher Nolan', actorName: 'Cillian Murphy', collaborationCount: 4 },
  { directorName: 'Denis Villeneuve', actorName: 'Ryan Gosling', collaborationCount: 2 },
  { directorName: 'Quentin Tarantino', actorName: 'Leonardo DiCaprio', collaborationCount: 2 },
];

const bestRookiesData = {
  newDirectors: [],
  newActors: [{ name: 'Mikey Madison', moviesWatchedThisYear: 1, averageRating: 4.5 }],
};

const longestMovie = { id: GUID(), title: 'Midsommar', releaseYear: 2019, posterPath: POSTERS.midsommar, runtimeMinutes: 147, rating: 4.0, liked: true };
const shortestMovie = { id: GUID(), title: 'Project X', releaseYear: 2012, posterPath: POSTERS.projectx, runtimeMinutes: 88, rating: 2.5, liked: false };

const monthlyExtremes = [
  { month: 1, monthName: 'January', bestMovie: null, worstMovie: null },
  { month: 6, monthName: 'June', bestMovie: { id: GUID(), title: 'The Social Network', releaseYear: 2010, posterPath: POSTERS.social, runtimeMinutes: 120, rating: 4.5, liked: true }, worstMovie: null },
  { month: 12, monthName: 'December', bestMovie: { id: GUID(), title: 'Star Wars', releaseYear: 1977, posterPath: POSTERS.starwars, runtimeMinutes: 121, rating: 5.0, liked: true }, worstMovie: null },
];

const mostRewatchedData = {
  title: 'Project X', posterPath: POSTERS.projectx, releaseYear: 2012, rewatchCount: 4,
};

const topBottomData = {
  topRated: [
    { id: GUID(), title: 'Star Wars', releaseYear: 1977, posterPath: POSTERS.starwars, runtimeMinutes: 121, rating: 5.0, liked: true },
    { id: GUID(), title: 'Interstellar', releaseYear: 2014, posterPath: POSTERS.interstellar, runtimeMinutes: 169, rating: 5.0, liked: true },
    { id: GUID(), title: 'The Social Network', releaseYear: 2010, posterPath: POSTERS.social, runtimeMinutes: 120, rating: 4.5, liked: true },
    { id: GUID(), title: 'Donnie Darko', releaseYear: 2001, posterPath: POSTERS.donnie, runtimeMinutes: 113, rating: 4.5, liked: true },
    { id: GUID(), title: 'Midsommar', releaseYear: 2019, posterPath: POSTERS.midsommar, runtimeMinutes: 147, rating: 4.0, liked: true },
  ],
  bottomRated: [
    { id: GUID(), title: 'Project X', releaseYear: 2012, posterPath: POSTERS.projectx, runtimeMinutes: 88, rating: 2.5, liked: false },
  ],
};

const hiddenGems = [
  { id: GUID(), title: 'Donnie Darko', releaseYear: 2001, posterPath: POSTERS.donnie, runtimeMinutes: 113, rating: 4.5 },
];

const ratingEvolution = [
  { month: 1, averageRating: 4.0 }, { month: 3, averageRating: 4.2 },
  { month: 6, averageRating: 4.1 }, { month: 9, averageRating: 4.3 }, { month: 12, averageRating: 4.5 },
];

const genreStreaks = [
  { genreName: 'Sci-Fi', streakLength: 3 },
  { genreName: 'Thriller', streakLength: 2 },
];

const watchedDirectors = [
  { directorName: 'Christopher Nolan', count: 12, averageRating: 4.4 },
  { directorName: 'Denis Villeneuve', count: 9, averageRating: 4.6 },
  { directorName: 'Quentin Tarantino', count: 8, averageRating: 4.3 },
  { directorName: 'David Fincher', count: 7, averageRating: 4.2 },
  { directorName: 'Bong Joon-ho', count: 6, averageRating: 4.5 },
  { directorName: 'Martin Scorsese', count: 5, averageRating: 4.3 },
  { directorName: 'Greta Gerwig', count: 5, averageRating: 4.1 },
];

const watchedActors = [
  { actorName: 'Ryan Gosling', count: 14, averageRating: 4.2 },
  { actorName: 'Christian Bale', count: 11, averageRating: 4.3 },
  { actorName: 'Leonardo DiCaprio', count: 10, averageRating: 4.5 },
  { actorName: 'Cillian Murphy', count: 8, averageRating: 4.6 },
  { actorName: 'Florence Pugh', count: 7, averageRating: 4.0 },
];

const decadeBreakdown = dashboardData.decadeBreakdown;

const eraBreakdown = { classicCount: 43, modernCount: 185 };

const cinematicFatigueData = {
  avgRatingLightDays: 4.1,
  avgRatingHeavyDays: 3.6,
  slumpDay: 'Monday', slumpDayWatchCount: 12,
  slumpMonth: 'February', slumpMonthWatchCount: 14,
};

const weekendWarriorData = { weekendWatches: 146, weekdayWatches: 99 };

// ── API route setup ──────────────────────────────────────────────────────────

async function setupApiMocks(page: Page) {
  const json = (data: unknown) => ({
    status: 200 as const,
    contentType: 'application/json',
    body: JSON.stringify(data),
  });

  await page.route(/\/api\//, async (route) => {
    const url = route.request().url().toLowerCase();

    if (url.includes('/api/import/history')) {
      await route.fulfill(json(importHistory));
    } else if (url.includes('/api/analytics/dashboard')) {
      await route.fulfill(json(dashboardData));
    } else if (url.includes('/api/analytics/advanced/final-cut/bookends')) {
      await route.fulfill(json(bookendsData));
    } else if (url.includes('/api/analytics/advanced/final-cut/prime-time')) {
      await route.fulfill(json(primeTimeData));
    } else if (url.includes('/api/analytics/advanced/final-cut/genre-landscape')) {
      await route.fulfill(json(genreLandscape));
    } else if (url.includes('/api/analytics/advanced/final-cut/director-actor-pairs')) {
      await route.fulfill(json(directorActorPairs));
    } else if (url.includes('/api/analytics/advanced/final-cut/best-rookies')) {
      await route.fulfill(json(bestRookiesData));
    } else if (url.includes('/api/analytics/advanced/final-cut/shortest-movie')) {
      await route.fulfill(json(shortestMovie));
    } else if (url.includes('/api/analytics/advanced/final-cut/monthly-extremes')) {
      await route.fulfill(json(monthlyExtremes));
    } else if (url.includes('/api/analytics/advanced/final-cut/most-rewatched')) {
      await route.fulfill(json(mostRewatchedData));
    } else if (url.includes('/api/analytics/advanced/final-cut/top-bottom-rated')) {
      await route.fulfill(json(topBottomData));
    } else if (url.includes('/api/analytics/advanced/bonus/cinematic-fatigue')) {
      await route.fulfill(json(cinematicFatigueData));
    } else if (url.includes('/api/analytics/advanced/bonus/weekend-warrior')) {
      await route.fulfill(json(weekendWarriorData));
    } else if (url.includes('/api/analytics/advanced/bonus/hidden-gems')) {
      await route.fulfill(json(hiddenGems));
    } else if (url.includes('/api/analytics/advanced/watched/preferred-day')) {
      await route.fulfill(json(preferredDay));
    } else if (url.includes('/api/analytics/advanced/watched/total-time')) {
      await route.fulfill(json(totalTimeInvested));
    } else if (url.includes('/api/analytics/advanced/watched/director-ranking')) {
      await route.fulfill(json(directorRanking));
    } else if (url.includes('/api/analytics/advanced/watched/longest-movie')) {
      await route.fulfill(json(longestMovie));
    } else if (url.includes('/api/analytics/advanced/watched/casting-repetitions')) {
      await route.fulfill(json(castingPairs));
    } else if (url.includes('/api/analytics/advanced/watched/rating-evolution')) {
      await route.fulfill(json(ratingEvolution));
    } else if (url.includes('/api/analytics/advanced/watched/genre-streaks')) {
      await route.fulfill(json(genreStreaks));
    } else if (url.includes('/api/analytics/advanced/watched/directors')) {
      await route.fulfill(json(watchedDirectors));
    } else if (url.includes('/api/analytics/advanced/watched/actors')) {
      await route.fulfill(json(watchedActors));
    } else if (url.includes('/api/analytics/advanced/watched/decades')) {
      await route.fulfill(json(decadeBreakdown));
    } else if (url.includes('/api/analytics/advanced/watched/predominant-era')) {
      await route.fulfill(json(eraBreakdown));
    } else if (url.includes('/api/analytics/advanced/watched')) {
      await route.fulfill(json(watchedMovies));
    } else if (url.includes('/api/analytics/wrapped')) {
      await route.fulfill(json(wrappedSummaryData));
    } else {
      await route.fulfill(json(null));
    }
  });
}

// ── Helpers ──────────────────────────────────────────────────────────────────

async function clickSlideZone(page: Page, times: number) {
  for (let i = 0; i < times; i++) {
    await page.locator('.right-zone').click({ force: true, timeout: 5000 });
    await page.waitForTimeout(400);
  }
}

// ── Test ─────────────────────────────────────────────────────────────────────

test.describe('Portfolio Screenshot Generator', () => {
  test.setTimeout(180000);
  test('Capture full showcase screenshots', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 800 });
    await setupApiMocks(page);

    // 1. LANDING (unauthenticated)
    await page.goto('/');
    await page.waitForTimeout(1500);
    await page.screenshot({ path: `${IMAGES_DIR}/landing.jpg`, quality: 90 });

    // ── Authenticate ──
    await loginAndSetToken(page);

    // 2. DASHBOARD
    await page.goto('/dashboard');
    await page.waitForSelector('.stat-card', { timeout: 15000 });
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/dashboard.png` });

    // 3. STATS — Movies table
    await page.goto('/stats');
    await page.waitForSelector('.topbar-filters', { timeout: 10000 });
    await page.waitForTimeout(500);
    await page.locator('.analyze-btn').click();
    await page.waitForSelector('.glass-table', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/stats-movies.png` });

    // 4. STATS — Bar chart (Preferred Watch Day)
    await page.locator('select').first().selectOption('Habits & Correlations');
    await page.waitForTimeout(300);
    await page.locator('select').nth(1).selectOption('preferred_day');
    await page.waitForTimeout(300);
    await page.locator('.analyze-btn').click();
    await page.waitForSelector('.chart-card', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/stats-chart.png` });

    // 5. STATS — Hero card (Total Time Invested)
    await page.locator('select').first().selectOption('Watched Insights');
    await page.waitForTimeout(300);
    await page.locator('select').nth(1).selectOption('total_time');
    await page.waitForTimeout(300);
    await page.locator('.analyze-btn').click();
    await page.waitForSelector('.hero-card', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/stats-hero.png` });

    // 6. STATS — Director Ranking table
    await page.locator('select').nth(1).selectOption('director_ranking');
    await page.waitForTimeout(300);
    await page.locator('.analyze-btn').click();
    await page.waitForSelector('.glass-table', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/stats-directors.png` });

    // 7. FINAL CUT TEASER
    await page.goto('/final-cut-teaser');
    await page.waitForSelector('.cinematic-teaser-card', { timeout: 10000 });
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-teaser.png` });

    // 8–11. FINAL CUT slideshow
    await page.goto('/final-cut/2025');
    await page.waitForSelector('.final-cut-container', { timeout: 15000 });
    await page.waitForTimeout(2000);

    // Slide 0: Intro
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-intro.png` });

    // Slide 1: Big Numbers
    await clickSlideZone(page, 1);
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-big-numbers.png` });

    // Slide 2: Prime Time
    await clickSlideZone(page, 1);
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-prime-time.png` });

    // Slide 5: Genre Landscape (3 more clicks)
    await clickSlideZone(page, 3);
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-genres.png` });

    // Slide 10: Generational Divide (5 more clicks)
    await clickSlideZone(page, 5);
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-generations.png` });

    // Slide 14: Bookends (4 more clicks)
    await clickSlideZone(page, 4);
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-bookends.png` });

    // Slide 17: Return of the King (3 more clicks)
    await clickSlideZone(page, 3);
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-king.png` });

    // Slide 18: Hall of Fame (1 more click)
    await clickSlideZone(page, 1);
    await page.waitForTimeout(800);
    await page.screenshot({ path: `${IMAGES_DIR}/final-cut-hall-of-fame.png` });

    // 12. IMPORT CENTER
    await page.goto('/imports');
    await page.waitForSelector('.imports-dashboard', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/imports.png` });
  });
});
