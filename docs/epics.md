# 🏋️ Gym Tracking App — Epics & User Stories

## 🧱 Epic 1: Project Foundation & Architecture

### Goal

Establish a scalable, offline-first architecture with clean separation of concerns.

#### User Stories

* **E1-US1**
  As a developer, I want a clean architecture structure so that the codebase is maintainable and scalable.

* **E1-US2**
  As a developer, I want SQLite integrated so that all app data is stored locally.

* **E1-US3**
  As a developer, I want a database migration system so that future updates do not break existing user data.

* **E1-US4**
  As a developer, I want repository abstractions so that data access is decoupled from business logic.

* **E1-US5**
  As a developer, I want async data access patterns so that the UI remains responsive.

---

## 👤 Epic 2: Onboarding & User Profile

### Goal

Capture user identity, goals, and preferences at first launch and allow future edits.

#### User Stories

* **E2-US1**
  As a user, I want to choose between creating a new profile or restoring from backup so that I can continue previous progress.

* **E2-US2**
  As a user, I want to input my nickname, birthday, height, weight, and gender so that the app personalizes my experience.

* **E2-US3**
  As a user, I want to define my fitness goal so that recommendations are tailored to me.

* **E2-US4**
  As a user, I want to edit my profile later so that I can update my data.

* **E2-US5**
  As a user, I want to switch between light and dark mode so that I can use the app comfortably.

---

## 🏋️ Epic 3: Workout Logging System

### Goal

Enable fast, flexible, and intuitive workout tracking.

#### User Stories

* **E3-US1**
  As a user, I want to start a workout with or without a predefined template so that I have flexibility.

* **E3-US2**
  As a user, I want to quickly add exercises so that logging is fast and effortless.

* **E3-US3**
  As a user, I want previous weights and reps to autofill so that I don’t need to re-enter data.

* **E3-US4**
  As a user, I want to log sets with weight, reps, tempo, and notes so that my workouts are detailed.

* **E3-US5**
  As a user, I want to edit sets inline so that I don’t lose flow during workouts.

* **E3-US6**
  As a user, I want swipe gestures to duplicate or adjust sets so that logging is faster.

* **E3-US7**
  As a user, I want an automatic rest timer so that I manage recovery between sets.

* **E3-US8**
  As a user, I want to save and complete workouts so that they are stored in history.

---

## 🧩 Epic 4: Exercise Library

### Goal

Provide a rich, searchable, and extensible exercise database.

#### User Stories

* **E4-US1**
  As a user, I want to browse exercises so that I can select them for workouts.

* **E4-US2**
  As a user, I want to search exercises so that I can find them quickly.

* **E4-US3**
  As a user, I want to filter exercises by muscle group and equipment so that I find relevant ones.

* **E4-US4**
  As a user, I want to add custom exercises so that I can track unique movements.

* **E4-US5**
  As a system, exercises should include metadata (muscles, movement type) so that analytics can use them.

---

## 📊 Epic 5: Dashboard

### Goal

Provide a high-level overview of progress and current status.

#### User Stories

* **E5-US1**
  As a user, I want to see a summary of my last workout so that I can recall recent activity.

* **E5-US2**
  As a user, I want to see my weekly activity so that I understand consistency.

* **E5-US3**
  As a user, I want a readiness indicator so that I know if I should train hard or rest.

* **E5-US4**
  As a user, I want suggested next workouts so that I don’t need to plan manually.

* **E5-US5**
  As a user, I want visually engaging charts and gauges so that insights are easy to understand.

---

## 📜 Epic 6: Workout History

### Goal

Allow users to review and analyze past workouts.

#### User Stories

* **E6-US1**
  As a user, I want to see a list of past workouts so that I can track my history.

* **E6-US2**
  As a user, I want to view detailed workout breakdowns so that I can analyze performance.

* **E6-US3**
  As a user, I want to filter workouts by date and tags so that I can find specific sessions.

---

## 📈 Epic 7: Analytics

### Goal

Deliver meaningful insights into performance and progress.

#### User Stories

* **E7-US1**
  As a user, I want to see strength progression per exercise so that I can track improvement.

* **E7-US2**
  As a user, I want to see volume trends so that I understand workload.

* **E7-US3**
  As a user, I want to see muscle group distribution so that I can detect imbalances.

* **E7-US4**
  As a user, I want charts and graphs so that data is easy to interpret.

---

## 🤖 Epic 8: Smart Suggestions Engine

### Goal

Provide actionable, data-driven training recommendations.

#### User Stories

* **E8-US1**
  As a user, I want the app to detect plateaus so that I can adjust training.

* **E8-US2**
  As a user, I want suggestions on increasing weight or reps so that I progress.

* **E8-US3**
  As a user, I want imbalance detection so that I train all muscle groups evenly.

* **E8-US4**
  As a user, I want deload recommendations so that I avoid overtraining.

---

## 🎯 Epic 9: Goals System

### Goal

Allow users to define and track fitness goals.

#### User Stories

* **E9-US1**
  As a user, I want to set strength goals so that I have clear targets.

* **E9-US2**
  As a user, I want to set workout frequency goals so that I stay consistent.

* **E9-US3**
  As a user, I want to track goal progress so that I stay motivated.

---

## 🏅 Epic 10: Personal Records (PRs)

### Goal

Track and celebrate performance milestones.

#### User Stories

* **E10-US1**
  As a user, I want to see my max weight per exercise so that I know my limits.

* **E10-US2**
  As a user, I want to track rep and volume PRs so that I see progress.

* **E10-US3**
  As a user, I want estimated 1RM calculations so that I measure strength.

---

## 🏷️ Epic 11: Tagging & Notes

### Goal

Add qualitative context to workouts.

#### User Stories

* **E11-US1**
  As a user, I want to tag workouts so that I can categorize them.

* **E11-US2**
  As a user, I want to add notes to sets, exercises, and workouts so that I can record context.

---

## 💾 Epic 12: Data Import & Export

### Goal

Ensure full data ownership and portability.

#### User Stories

* **E12-US1**
  As a user, I want to export my data so that I can back it up.

* **E12-US2**
  As a user, I want to import my data so that I can restore progress.

* **E12-US3**
  As a system, exported data should include versioning so that compatibility is maintained.

---

## ⚙️ Epic 13: Settings

### Goal

Allow user customization and preferences.

#### User Stories

* **E13-US1**
  As a user, I want to switch between kg and lb so that I use my preferred unit.

* **E13-US2**
  As a user, I want to configure rest timers so that they match my training style.

---

## ✨ Epic 14: UI/UX Polish & Delight

### Goal

Create a premium, engaging experience.

#### User Stories

* **E14-US1**
  As a user, I want smooth animations so that the app feels responsive.

* **E14-US2**
  As a user, I want visual feedback on achievements so that progress feels rewarding.

* **E14-US3**
  As a user, I want an intuitive interface so that I can use the app without friction.

---

## 🧪 Epic 15: Testing & Stability

### Goal

Ensure reliability and performance.

#### User Stories

* **E15-US1**
  As a developer, I want unit tests for core logic so that calculations are correct.

* **E15-US2**
  As a developer, I want integration tests for database operations so that data integrity is maintained.

* **E15-US3**
  As a user, I want the app to never lose my data so that I trust it.

---

## 🚀 Suggested Delivery Order

1. Foundation & Architecture
2. Onboarding & Profile
3. Workout Logging (Core Loop)
4. Exercise Library
5. Dashboard (Basic)
6. History
7. Analytics
8. Smart Suggestions
9. Goals & PRs
10. Import/Export
11. Polish & Delight
12. Testing & Stability