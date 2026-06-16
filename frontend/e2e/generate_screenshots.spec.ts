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
    } else if (url.includes('/api/movies/123') && !url.includes('/log')) {
      await route.fulfill(json({
        id: '123',
        title: 'Inception',
        releaseYear: 2010,
        runtimeMinutes: 148,
        posterUrl: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg',
        overview: 'Cobb, a skilled thief who commits corporate espionage by infiltrating the subconscious of his targets is offered a chance to regain his old life as payment for a task considered to be impossible: \"inception\", the implantation of another person\'s idea into a target\'s subconscious.',
        tmdbRating: 8.3,
        userAverageScore: 9.0,
        genres: [{ id: 1, name: 'Sci-Fi' }, { id: 2, name: 'Action' }],
        directors: [{ id: '789', name: 'Christopher Nolan' }],
        actors: [{ id: '456', name: 'Leonardo DiCaprio' }, { id: '457', name: 'Joseph Gordon-Levitt' }],
        diaryEntries: [
          { id: 'entry-1', dateWatched: '2026-05-01', rating: 4.5, isRewatch: true }
        ],
        isWatched: true
      }));
    } else if (url.includes('/api/actors/456')) {
      await route.fulfill(json({
        id: '456',
        name: 'Ryan Gosling',
        profilePath: 'https://image.tmdb.org/t/p/w500/8qeezU4c1spIlfhpaA8m708J7IQ.jpg',
        watchCount: 14,
        averageRating: 4.2,
        movies: [
          { id: '123', title: 'Blade Runner 2049', releaseYear: 2017, posterPath: 'https://image.tmdb.org/t/p/w500/gGe2uBwogYr4O63hk4mdlCYLI7y.jpg', isWatched: true }
        ],
        directedMovies: []
      }));
    } else if (url.includes('/api/directors/789')) {
      await route.fulfill(json({
        id: '789',
        name: 'Christopher Nolan',
        profilePath: 'https://image.tmdb.org/t/p/w500/xu9iaLO8afAnZo3JIBN460clwqQ.jpg',
        watchCount: 12,
        averageRating: 4.4,
        movies: [
          { id: '123', title: 'Inception', releaseYear: 2010, posterPath: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg', isWatched: true },
          { id: '124', title: 'Interstellar', releaseYear: 2014, posterPath: 'https://image.tmdb.org/t/p/w500/yQvGrMoipbRoddT0ZR8tPoR7NfX.jpg', isWatched: true }
        ],
        actorMovies: []
      }));
    } else if (url.includes('/api/v1/recommendations/generate')) {
      await route.fulfill(json([
        {
          movieId: '123',
          title: 'La La Land',
          directorName: 'Damien Chazelle',
          releaseYear: 2016,
          matchPercentage: 98.4,
          recommendationReason: 'Shares your deep appreciation for musicals, vibrant visual styling, and romantic realism.',
          posterUrl: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg',
          runtimeMinutes: 128
        },
        {
          movieId: '124',
          title: 'Blade Runner 2049',
          directorName: 'Denis Villeneuve',
          releaseYear: 2017,
          matchPercentage: 95.1,
          recommendationReason: 'Matches your top director Denis Villeneuve and preference for philosophical Sci-Fi.',
          posterUrl: 'https://image.tmdb.org/t/p/w500/gGe2uBwogYr4O63hk4mdlCYLI7y.jpg',
          runtimeMinutes: 164
        }
      ]));
    } else if (url.includes('/api/v1/discovery/bingo')) {
      await route.fulfill(json({
        gridSize: 3,
        squares: [
          { objectiveId: 'obj-1', description: 'Watch a Sci-Fi movie', isCompleted: true, completionDate: '2026-01-10', row: 0, column: 0, movieTitle: 'Blade Runner 2049', watchedDate: '2026-01-10' },
          { objectiveId: 'obj-2', description: 'Watch a movie over 150m', isCompleted: false, completionDate: null, row: 0, column: 1 },
          { objectiveId: 'obj-3', description: 'Watch a 90s classic', isCompleted: false, completionDate: null, row: 0, column: 2 },
          { objectiveId: 'obj-4', description: 'Watch a movie by Christopher Nolan', isCompleted: true, completionDate: '2026-02-14', row: 1, column: 0, movieTitle: 'Interstellar', watchedDate: '2026-02-14' },
          { objectiveId: 'obj-5', description: 'Watch an Oscar Winner', isCompleted: false, completionDate: null, row: 1, column: 1 },
          { objectiveId: 'obj-6', description: 'Watch a Comedy', isCompleted: true, completionDate: '2026-03-01', row: 1, column: 2, movieTitle: 'Project X', watchedDate: '2026-03-01' },
          { objectiveId: 'obj-7', description: 'Watch a Horror movie', isCompleted: false, completionDate: null, row: 2, column: 0 },
          { objectiveId: 'obj-8', description: 'Watch a movie from France', isCompleted: false, completionDate: null, row: 2, column: 1 },
          { objectiveId: 'obj-9', description: 'Watch a movie under 90m', isCompleted: true, completionDate: '2026-04-05', row: 2, column: 2, movieTitle: 'Project X', watchedDate: '2026-04-05' }
        ],
        startDate: '2026-01-01',
        endDate: '2026-12-31'
      }));
    } else if (url.includes('/api/v1/discovery/roulette')) {
      await route.fulfill(json({
        winner: {
          movieId: '123',
          title: 'Inception',
          directorName: 'Christopher Nolan',
          releaseYear: 2010,
          selectionMechanismMetadata: 'Random Choice',
          posterUrl: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg',
          runtimeMinutes: 148
        },
        spinSequence: [
          {
            movieId: '124',
            title: 'Interstellar',
            directorName: 'Christopher Nolan',
            releaseYear: 2014,
            selectionMechanismMetadata: 'Random Choice',
            posterUrl: 'https://image.tmdb.org/t/p/w500/yQvGrMoipbRoddT0ZR8tPoR7NfX.jpg',
            runtimeMinutes: 169
          },
          {
            movieId: '123',
            title: 'Inception',
            directorName: 'Christopher Nolan',
            releaseYear: 2010,
            selectionMechanismMetadata: 'Random Choice',
            posterUrl: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg',
            runtimeMinutes: 148
          }
        ]
      }));
    } else if (url.includes('/api/v1/discovery/dice')) {
      await route.fulfill(json({
        movieId: '123',
        title: 'Inception',
        directorName: 'Christopher Nolan',
        releaseYear: 2010,
        posterUrl: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg',
        runtimeMinutes: 148,
        selectionMechanismMetadata: 'Crit Choice',
        diceResults: [
          { diceType: 0, rollValue: 4, label: 'Genre', description: 'Sci-Fi' },
          { diceType: 1, rollValue: 6, label: 'Decade', description: '2010s' },
          { diceType: 2, rollValue: 5, label: 'Rating', description: 'High (8+)' },
          { diceType: 3, rollValue: 2, label: 'Popularity', description: 'Mainstream' },
          { diceType: 4, rollValue: 1, label: 'Runtime', description: 'Medium (90-150m)' }
        ],
        specialEvent: null
      }));
    } else if (url.includes('/api/v1/discovery/slot-machine')) {
      await route.fulfill(json({
        movieId: '123',
        title: 'Inception',
        directorName: 'Christopher Nolan',
        releaseYear: 2010,
        posterUrl: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg',
        runtimeMinutes: 148,
        selectionMechanismMetadata: 'Perfect Slots Match',
        reelResults: [
          { label: 'Genre', value: 'Sci-Fi' },
          { label: 'Decade', value: '2010s' },
          { label: 'Rating', value: '8.5+' },
          { label: 'Popularity', value: 'Jackpot' },
          { label: 'Country', value: 'USA' }
        ],
        isJackpot: true,
        matchCount: 5,
        matchedReels: [true, true, true, true, true]
      }));
    } else if (url.includes('/api/v1/discovery/mystery-box') && !url.includes('/reveal')) {
      await route.fulfill(json({
        boxIds: ['box-1', 'box-2', 'box-3'],
        variant: 1,
        generatedAt: '2026-06-16T10:00:00Z',
        hints: [
          { boxId: 'box-1', hint: 'Directed by a 21st-century auteur' },
          { boxId: 'box-2', hint: 'Sci-Fi masterpiece with a ticking clock' },
          { boxId: 'box-3', hint: 'Mind-bending dream heist movie' }
        ]
      }));
    } else if (url.includes('/reveal')) {
      await route.fulfill(json({
        movieId: '123',
        title: 'Inception',
        directorName: 'Christopher Nolan',
        releaseYear: 2010,
        selectionMechanismMetadata: 'Mystery Box Choice',
        posterUrl: 'https://image.tmdb.org/t/p/w500/uDO8zWDhfNsPkNyHOjftVz8u22Y.jpg',
        runtimeMinutes: 148
      }));
    } else {
      await route.fulfill(json(null));
    }
  });
}

// ── Helpers ──────────────────────────────────────────────────────────────────

async function clickSlideZone(page: Page, times: number) {
  for (let i = 0; i < times; i++) {
    await page.keyboard.press('ArrowRight');
    await page.waitForTimeout(400);
  }
}

async function selectCinematicOption(page: Page, index: number, optionLabel: string) {
  const container = page.locator('app-cinematic-select').nth(index);
  await container.locator('.select-trigger').click({ force: true });
  const regex = new RegExp(`^\\s*${optionLabel.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&')}\\s*$`);
  await container.locator('.select-option').filter({ hasText: regex }).click({ force: true });
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

    // 2. LOGIN (unauthenticated)
    await page.goto('/login');
    await page.waitForSelector('.auth-card', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/login.png` });

    // 3. REGISTER (unauthenticated)
    await page.goto('/register');
    await page.waitForSelector('.auth-card', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/register.png` });

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
    await selectCinematicOption(page, 0, 'Habits & Correlations');
    await page.waitForTimeout(300);
    await selectCinematicOption(page, 1, 'Preferred Watch Day');
    await page.waitForTimeout(300);
    await page.locator('.analyze-btn').click();
    await page.waitForSelector('.chart-card', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/stats-chart.png` });

    // 5. STATS — Hero card (Total Time Invested)
    await selectCinematicOption(page, 0, 'Watched History');
    await page.waitForTimeout(300);
    await selectCinematicOption(page, 1, 'Total Time Invested');
    await page.waitForTimeout(300);
    await page.locator('.analyze-btn').click();
    await page.waitForSelector('.hero-card', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/stats-hero.png` });

    // 6. STATS — Director Ranking table
    await selectCinematicOption(page, 1, 'Watched Directors');
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

    // 13. RECOMMENDATIONS
    await page.goto('/recommendations');
    await page.waitForSelector('.recommendations-grid', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/recommendations.png` });

    // 14. DISCOVERY — Bingo
    await page.goto('/discovery');
    await page.waitForSelector('.discovery-container', { timeout: 15000 });
    await page.waitForTimeout(1000);
    // Switch to Bingo tab first
    await page.locator('.tab-btn', { hasText: 'Bingo' }).click({ force: true });
    await page.waitForTimeout(500);
    // Refresh board to display the grid
    await page.locator('.action-buttons-group button').first().click({ force: true });
    await page.waitForSelector('.bingo-grid', { timeout: 15000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/discovery-bingo.png` });

    // 15. DISCOVERY — Roulette
    await page.locator('.tab-btn', { hasText: 'Roulette' }).click({ force: true });
    await page.waitForSelector('app-roulette-wheel', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/discovery-roulette.png` });

    // 16. DISCOVERY — Mystery Box
    await page.locator('.tab-btn', { hasText: 'Mystery Box' }).click({ force: true });
    await page.waitForSelector('app-mystery-grid', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/discovery-mystery.png` });

    // 17. DISCOVERY — Dice
    await page.locator('.tab-btn', { hasText: 'Dice' }).click({ force: true });
    await page.waitForSelector('app-dice-roller', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/discovery-dice.png` });

    // 18. DISCOVERY — Slot Machine
    await page.locator('.tab-btn', { hasText: 'Slot Machine' }).click({ force: true });
    await page.waitForSelector('app-slot-reels', { timeout: 10000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/discovery-slots.png` });

    // 19. DETAILS — Movie details
    await page.goto('/movies/123/inception');
    await page.waitForSelector('.movie-detail-container', { timeout: 15000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/movie-details.png` });

    // 20. DETAILS — Actor details
    await page.goto('/actors/456/ryan-gosling');
    await page.waitForSelector('.person-detail-container', { timeout: 15000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/actor-details.png` });

    // 21. DETAILS — Director details
    await page.goto('/directors/789/christopher-nolan');
    await page.waitForSelector('.person-detail-container', { timeout: 15000 });
    await page.waitForTimeout(600);
    await page.screenshot({ path: `${IMAGES_DIR}/director-details.png` });
  });
});
