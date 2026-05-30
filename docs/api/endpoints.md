# API Endpoints Contract (MVP)

This document outlines the high-level REST API endpoints expected for the MVP. All endpoints are prefixed with `/api/v1/` and secured via JWT Bearer authentication (where applicable).

## 1. Authentication

* **POST** `/api/v1/auth/register`
  * Body: `Username`, `Email`, `Password`
  * Response: `Token`, `RefreshToken`
* **POST** `/api/v1/auth/login`
  * Body: `Email`, `Password`
  * Response: `Token`, `RefreshToken`

## 2. Imports

* **POST** `/api/v1/imports/letterboxd`
  * Content-Type: `multipart/form-data`
  * Body: `file` (IFormFile, .zip)
  * Action: Ingests, parses, normalizes, and saves data to the database.
  * Response: Import Summary (Total Movies, Total Diary Entries parsed).

## 3. Analytics (Read-Only / Dapper Optimized)

* **GET** `/api/v1/analytics/overview`
  * Query Params: `year` (optional)
  * Response: High-level counts (Total Movies Watched, Total Ratings, Average Rating).
* **GET** `/api/v1/analytics/top-directors`
  * Query Params: `limit` (default 5)
  * Response: List of top watched directors.
* **GET** `/api/v1/analytics/monthly-activity`
  * Query Params: `year`
  * Response: Array of 12 elements with watch counts per month.

## 4. Wrapped

* **GET** `/api/v1/wrapped/{year}`
  * Description: Aggregates heavy analytics into a single "Spotify Wrapped" style payload.
  * Response: Complex nested JSON containing Top 5s, visual statistics, and personalized cinematic insights.
