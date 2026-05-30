# Letterboxd Data Mapping Specification

This document acts as a technical contract and schema reference for the ingestion of **Letterboxd export files**. Due to anomalies and specific formatting choices in the Letterboxd CSVs, explicit conversion rules must be followed when building the parsing infrastructure (e.g., `CsvHelper` profiles).

---

## 1. General Rules

- **Letterboxd URIs** act as the primary unique identifier for movies across all CSV files. They should be stored natively as an `ExternalReference` value object.
- **Dates** generally follow `YYYY-MM-DD` and map efficiently to `.NET DateOnly`.
- **String Parsing:** Multi-line text inside quotes (such as user reviews) is prevalent. The CSV parser **must** be configured to handle embedded newlines natively without treating them as EOF/new row events.

---

## 2. File Specifications

### 2.1. `diary.csv`

Contains the highest fidelity user activity log.

- **Date:** `DateOnly`
- **Name:** `string`
- **Year:** `int?` (Can be empty for unreleased titles)
- **Letterboxd URI:** `string` (Key identifier)
- **Rating:** `decimal?` (e.g., 3.5, 4.0; can be empty if logged without stars)
- **Rewatch:** `string` -> `bool` (Value `"Yes"` must be converted to `true`; empty string to `false`)
- **Tags:** `string` (Comma-separated text, frequently empty)
- **Watched Date:** `DateOnly` (Differs from `Date` which is the logging timestamp)

### 2.2. `ratings.csv`

Contains user-assigned ratings, uncoupled from the diary viewing sessions.

- **Date:** `DateOnly`
- **Name:** `string`
- **Year:** `int?`
- **Letterboxd URI:** `string`
- **Rating:** `decimal` (0.5 to 5.0 scale)

### 2.3. `watched.csv` & `watchlist.csv`

Minimal tracking datasets for overall completion and pending lists.

- **Date:** `DateOnly` (Added date)
- **Name:** `string`
- **Year:** `int?` (In `watchlist.csv`, this might be completely empty or include decimal strings like `2022.0` that must be explicitly cast to `int?`)
- **Letterboxd URI:** `string`

### 2.4. `reviews.csv` & `comments.csv`

- **Review/Comment Text:** The actual textual content often spans multiple lines and paragraphs, encapsulated inside standard double quotes `"..."`. The parser configuration (`HasHeaderRecord`, quote handling) is critical here to avoid misinterpreting internal newlines as broken schemas.

### 2.5. `profile.csv`

Contains a single data row of metadata.

- **Favorite Films:** Presented as a single, comma-separated string wrapped in quotes containing URLs (e.g., `"https://boxd.it/2b5u, https://boxd.it/9vEe"`).
- **Conversion Rule:** Must execute `.Split(',')` and trim whitespace to properly extract the individual references if relational normalization is required.

### 2.6. Custom Lists (`lists/*.csv`)

These represent user-curated rankings or collections.
**WARNING: Multi-Table Structure**
These files are not standard data-only CSVs. They contain contextual metadata pre-appended to the file.

**Structure:**

1. Title row (`Letterboxd list export v7`)
2. List Metadata Headers (`Date,Name,Tags,URL,Description`)
3. List Metadata Values
4. *Blank line*
5. Target Data Headers (`Position,Name,Year,URL,Description`)
6. Real list data...

**Conversion Rule:** The file stream reader must be manually advanced/skipped until the target tabular data line (`Position,Name,Year,URL,Description`) is identified. Applying a standard CSV reader from Line 1 will result in catastrophic failure.
