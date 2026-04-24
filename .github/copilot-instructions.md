# 🏋️ Gym Tracking App — Copilot Instructions (.NET MAUI)

## 🎯 Product Vision

Build a **fully offline, high-performance gym tracking app** that evolves from a simple tracker into a **personal training intelligence system**.

The app must:

* Be **100% offline-first**
* Provide **fast, low-friction workout logging**
* Deliver **meaningful analytics and smart recommendations**
* Feel **premium and fluid** with modern UI (gauges, charts, subtle animations)
* Ensure **full user data ownership** (export/import, no cloud dependency)

---

## 🧱 Architecture Principles

### 1. Offline-First Design

* Use **SQLite** as the primary datastore
* Implement:

  * Schema versioning
  * Migration system
* No network dependencies in core flows

---

### 2. Clean Architecture

```
/Core
  - Entities
  - Enums
  - ValueObjects
  - Domain Logic

/Application
  - Services
  - Use Cases
  - DTOs

/Infrastructure
  - SQLite (Repositories)
  - Import/Export
  - Settings Persistence

/UI
  - Views (XAML)
  - ViewModels (MVVM)
```

* Use **MVVM pattern**
* Keep business logic out of UI layer

---

### 3. Performance First

* Optimize for:

  * Fast startup
  * Minimal UI blocking
* Use async/await everywhere appropriate
* Batch DB writes during workouts

---

## 🧑‍💻 Core Features

### 1. Onboarding Wizard

Collect:

* Nickname
* Birthday
* Height
* Weight
* Gender
* Fitness goal:

  * Strength
  * Hypertrophy
  * Fat loss
  * Balanced

Also:

* Ask if **new user or restore from backup**
* If restore:

  * Trigger import flow immediately

---

### 2. Workout System

#### Modes:

* Predefined workout templates
* Free workout (add exercises on the fly)

#### Requirements:

* Add exercises quickly (minimize taps)
* Autofill last used weights/reps
* Inline editing (no modal-heavy UX)

#### Per Set:

* Weight
* Reps
* Tempo
* Notes (optional)

#### Additional:

* Auto rest timer after set
* Swipe gestures:

  * Duplicate set
  * Increment weight

---

### 3. Exercise Library

* Load from provided open-source dataset
* Support:

  * Search
  * Filters (muscle group, equipment)
* Allow:

  * Custom exercises

#### Each Exercise Must Have:

* Primary muscle
* Secondary muscles
* Movement type:

  * Push / Pull / Legs / Core
* Equipment type

---

### 4. Dashboard (Main Screen)

Display:

* Last workout summary
* Weekly activity
* Readiness indicator (basic heuristic)
* Suggested next workout

#### Visual Components:

* Circular progress gauges
* Mini charts (sparklines)
* Highlight:

  * Personal records
  * Progress trends

---

### 5. History Tab

* Chronological workout list
* Expand to view:

  * Exercises
  * Sets
* Filters:

  * Date range
  * Tags

---

### 6. Analytics Tab

#### Metrics:

* Volume (sets × reps × weight)
* Strength progression per exercise
* Weekly volume per muscle group
* Workout frequency

#### Visuals:

* Line charts
* Bar charts
* Radial gauges

---

### 7. Smart Suggestions Engine

Rules-based (initial version):

#### Detect:

* Muscle imbalance
* Plateau (no progress over time)
* Undertrained muscle groups
* Overtraining patterns

#### Suggest:

* Increase weight
* Increase reps
* Add/remove exercises
* Recommend deload

---

### 8. Goals System

Allow users to define:

* Strength goals (e.g. +20kg squat)
* Frequency goals (e.g. 4 workouts/week)
* Balance goals

Tie goals into:

* Dashboard
* Suggestions
* Analytics

---

### 9. Session Tracking

Track:

* Workout duration
* Rest time
* Time per exercise
* Estimated intensity score

---

### 10. Personal Records (PRs)

Track:

* Max weight
* Rep PRs
* Volume PRs
* Estimated 1RM

---

### 11. Tagging System

Allow tagging workouts:

* Bulking
* Cutting
* Deload
* Custom tags

Use in filtering + analytics

---

### 12. Notes System

Support notes:

* Per set
* Per exercise
* Per workout

---

## 💾 Data & Persistence

### SQLite Requirements

* Use repository pattern
* Tables:

  * Users
  * Exercises
  * Workouts
  * WorkoutExercises
  * Sets
  * Goals
  * Tags

---

### Import / Export

#### Export:

* JSON format
* Include:

  * Schema version
  * App version

#### Import:

* Validate version compatibility
* Migrate if needed

Optional:

* Compression support

---

## 🎨 UI / UX Guidelines

### Theme

* Light & Dark mode support
* Accent color: `#FFD700` (gold)

### Design Principles

* Minimal
* Soft shadows
* Rounded cards
* Clean spacing

---

### Interaction Design

* Reduce taps required for logging
* Avoid modal-heavy flows
* Use gestures where possible

---

### Feedback & Delight

* Subtle animations on:

  * Completing sets
  * Achieving PRs
* Optional haptic feedback

---

### Dashboard Feel

Should resemble:

* A control panel
* Status overview
* Not just static data

---

## ⚙️ Settings & Profile

User can edit:

* Profile data
* Units (kg/lb)
* Theme (light/dark)
* Rest timer defaults

---

## ⚠️ Critical Technical Decisions

### Must Handle Early:

* Unit conversion system
* Timezone-safe timestamps
* Flexible exercise model
* DB migration strategy

---

## 🚀 Non-Functional Requirements

* Fully offline operation
* Fast UI interactions (<100ms perceived delay)
* Stable data persistence
* No data loss scenarios

---

## 🧪 Testing Guidelines

* Unit test:

  * Domain logic
  * Suggestion engine
* Integration test:

  * Database operations
* UI test:

  * Core flows (start workout, log sets, save)

---

## 🧠 Future-Ready Considerations

Prepare architecture for:

* Wearable integration
* Cloud sync (optional, future)
* AI-based recommendations
* Social features (optional)

---

## 🧩 Development Priorities (Order)

1. Data model + SQLite setup
2. Workout logging flow (core experience)
3. Exercise library
4. Dashboard (basic)
5. History
6. Analytics
7. Smart suggestions
8. Polish (animations, UX)

---
