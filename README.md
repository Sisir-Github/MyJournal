# Journal Management System

A **desktop journaling application** built using **.NET MAUI Blazor**.

---

## Project Details

* **Project Type:** Individual Project
* **Developer:** Sisir Paudel
* **Platform:** Desktop (Windows / Mac)
* **Technology Stack:** .NET MAUI Blazor, C#, SQLite

---

## Overview

Journal Management System is a **secure, offline-first personal journal application**. It allows users to write **one journal entry per day**, track moods, organize entries with tags and categories, view analytics, and export entries as PDF. All data is stored **locally** to ensure privacy.

---

## Key Features

### Journal Entries

* Create, update, and delete daily journal entries
* Only **one entry per date** is allowed
* System-generated **CreatedAt** and **UpdatedAt** timestamps
* Entry includes title, content, and visible date/time

### Rich Text / Markdown Support

* Bold, italics, headings
* Bullet and numbered lists
* Hyperlinks

### Mood Tracking

* **1 required primary mood**
* **Up to 2 optional secondary moods**

**Mood Types:**

* **Positive:** Happy, Excited, Relaxed, Grateful, Confident
* **Neutral:** Calm, Thoughtful, Curious, Nostalgic, Bored
* **Negative:** Sad, Angry, Stressed, Lonely, Anxious

### Categories

* Each entry has one category
* Categories are separate from moods and tags

### Tags

* Custom user-defined tags
* Pre-built tags include:

  * Work, Career, Studies, Family, Friends, Health, Fitness
  * Travel, Finance, Self-care, Projects, Reflection, etc.

### Calendar & Navigation

* Calendar view to access past entries
* Paginated timeline/list view

### Search & Filter

* Search by title or content
* Filter by date range, mood, and tags

### Streak Tracking

* Current streak
* Longest streak
* Missed days tracking

### Analytics Dashboard

* Mood distribution (Positive / Neutral / Negative)
* Most frequent mood
* Most used tags
* Tag usage by category
* Word count trends over time

### Security & Privacy

* Password or PIN protection
* Offline-only data storage

### Export

* Export journal entries as **PDF** by date range

### Theme Support

* Light mode
* Dark mode
* Optional custom themes

---

## Architecture Overview

* **UI Layer:** Blazor components (desktop-first design)
* **Logic Layer:** C# services and business logic
* **Data Layer:** SQLite (local database)

---

## Database Overview

Main tables:

* **JournalEntry** (EntryId, Title, Content, EntryDate, CreatedAt, UpdatedAt, CategoryId)
* **Mood** (MoodId, MoodName, MoodType)
* **Tag** (TagId, TagName)
* **EntryTag** (EntryId, TagId)

---

## Installation & Run Instructions

1. Install **.NET SDK (latest version)**
2. Install **Visual Studio** with **.NET MAUI workload**
3. Clone or download this repository
4. Open the project in Visual Studio
5. Select **Windows** or **Mac Catalyst** target
6. Build and run the application

---

## Usage Guide

1. Launch the application
2. Unlock using password or PIN
3. Select a date from the calendar
4. Write or edit your journal entry
5. Choose mood, category, and tags
6. Save the entry
7. View analytics from the dashboard
8. Export entries as PDF if required

---

## Testing

* Manual testing performed for:

  * Entry creation and update
  * One-entry-per-day validation
  * Search and filter features
  * Mood and tag analytics
  * PDF export functionality

---

## Known Limitations

* No cloud synchronization
* Single-user application
* Desktop-only support

---

## Declaration

I, **Sisir Paudel**, confirm that this project is my **individual academic work**, developed using .NET MAUI Blazor and submitted for verification and assessment purposes.

---

© Sisir Paudel
