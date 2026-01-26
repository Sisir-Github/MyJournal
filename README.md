# Journal Management

Desktop journaling application built with .NET MAUI Blazor.

Project Type
- Individual project.

Scope Summary
- Secure, offline, feature-rich desktop journal with one entry per day, rich text/Markdown, mood tracking, tagging, analytics, and PDF export.

Functional Requirements
- Journal entry management: create, update, and delete the daily entry; enforce only one entry per date; system-generated CreatedAt and UpdatedAt timestamps.
- Entry structure: title + rich-text or Markdown content; date/time tied to the entry and visible to the user.
- Formatting support: bold, italics, lists, headings, and links.
- Mood tracking: one required primary mood and up to two optional secondary moods.
- Mood taxonomy:
  - Positive: Happy, Excited, Relaxed, Grateful, Confident
  - Neutral: Calm, Thoughtful, Curious, Nostalgic, Bored
  - Negative: Sad, Angry, Stressed, Lonely, Anxious
- Categories: assign a category to each entry (separate from moods/tags).
- Tags: custom tags plus pre-built tags:
  - Work, Career, Studies, Family, Friends, Relationships, Health, Fitness, Personal Growth, Self-care,
    Hobbies, Travel, Nature, Finance, Spirituality, Birthday, Holiday, Vacation, Celebration, Exercise,
    Reading, Writing, Cooking, Meditation, Yoga, Music, Shopping, Parenting, Projects, Planning, Reflection
- Calendar navigation: view and open past entries via a calendar view.
- Paginated journal view: timeline/list view with pagination.
- Search and filter: search by title/content; filter by date range, moods, and tags.
- Streak tracking: current streak, longest streak, and missed days.
- Dashboard analytics (filterable by date range):
  - Mood distribution (Positive/Neutral/Negative)
  - Most frequent mood
  - Most used tags
  - Tag breakdown by category
  - Word count trends over time
- Security and privacy: password or PIN protection for the journal.
- Export: export entries as PDF by date range.
- Local storage: all data stored locally in SQLite.
- Theme customization: light/dark themes (and optional custom themes).

Non-Functional/UI Requirements
- Unique, enhanced, professional, and attractive UI design (not default template styling).
- Desktop-first layout with responsive behavior for different window sizes.
- Offline-first; no external data dependencies for journal content.
- Efficient search/filtering on local data.

Deliverables/Documentation (per marking scheme)
- Requirements summary and mapping to features.
- Architecture and database schema overview.
- Installation/run instructions for MAUI Blazor desktop.
- User guide with screenshots of key workflows.
- Testing evidence and known limitations.
