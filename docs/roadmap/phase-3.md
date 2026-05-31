# Roadmap: Phase 3 (Angular 19+ Cinematic Dashboard & "Wrapped" Client)

This phase shifts focus to the client-side experience. We will build a premium, highly responsive single-page application (SPA) using Angular 19+ that visualizes the ingested movie data, displays detailed historical stats, and presents a cinematic Spotify Wrapped-style slideshow.

## Step 1: Angular 19+ Foundation & Design System

- [x] Initialize the Angular 19+ workspace using standalone components.
- [x] Configure Tailwind CSS (or premium modern custom SCSS/CSS modules) for styling.
- [x] Build a premium unified layout (Dark Mode default, sleek gradients, Outfit/Inter typography, and glassmorphic panels).
- [x] Define the reusable UI design system tokens (buttons, cards, skeleton loaders, and interactive hover states).

## Step 2: Auto-Generated API Clients & Authentication Flow

- [x] Set up an NSwag or OpenAPI TypeScript generator script to output strongly-typed services based on the ASP.NET Core swagger definition.
- [x] Implement the client-side Auth interceptors to attach the JWT token to every request and automatically handle token refreshing.
- [x] Create sleek, interactive Login and Sign-up screens with real-time field validation.

## Step 3: Interactive Ingestion & Import Control Center

- [x] Design an interactive file-upload area supporting drag-and-drop letterboxd `.zip` files.
- [x] Implement a live import dashboard showing current and past imports, and their real-time TMDB metadata enrichment status (using Server-Sent Events or WebSockets for real-time state, or short-polling).
- [x] Provide delete/reprocess buttons allowing direct interaction with import batches.

## Step 4: Cinematic Analytics Dashboard

- [x] Implement high-performance data-visualization widgets using **Chart.js** (or `ngx-charts`):
  - **Activity Heatmap:** Calendar grid displaying movie watch patterns.
  - **Decade Radial Chart:** Visual breakdown of the user's historical preferences.
  - **Genre Radar/Bar Chart:** Sleek visualization of favorite cinematic genres.
- [x] Build detailed leaderboards for Top Directors, Top Actors, and Top Rated movies with custom posters (TMDB URL integration).

## Step 5: Spotify Wrapped-Style Interactive Experience

- [ ] Implement a mobile-first, interactive slideshow presentation with fluid micro-animations (slide-in, fade-out, zoom).
- [ ] Standardize the wrapped story pages:
  - **Intro:** Welcome cinematic screen with key achievements (e.g., "In 2025, you spent X hours in front of the screen").
  - **Genre Story:** A visual explosion of the favorite genre.
  - **Director/Actor Focus:** Card style display of the most watched filmmaker.
  - **Decade Journey:** Animation tracing the era of the movies watched.
  - **Summary Card:** A gorgeous, consolidated stats overview page.
- [ ] Add sharing functionality by rendering the Summary Card into a shareable image file (`PNG/JPG`) via `html2canvas` or a server-side rendering API.
