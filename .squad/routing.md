# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|---------|
| Architecture, scope decisions, PRD decomposition | Rocky | Clean arch layers, DB schema design, tech decisions, epics breakdown |
| Code review, PR review | Rocky | Review correctness, patterns, architecture compliance |
| .NET MAUI views, XAML, MVVM | Apollo | Pages, ViewModels, bindings, navigation, Shell |
| Animations, gestures, UX flows | Apollo | Rest timer UI, swipe gestures, transitions, haptics |
| Dashboard, charts, gauges | Apollo | SkiaSharp/LiveCharts, circular gauges, sparklines |
| Onboarding wizard UI | Apollo | Onboarding pages, profile form |
| Exercise library UI | Apollo | Browse/search/filter views, exercise detail |
| Workout logging UI | Apollo | Active workout page, set logging, inline editing |
| History & analytics UI | Apollo | History list, analytics charts |
| SQLite / repositories | Mickey | Repository implementations, migrations, schema versioning |
| Business logic / services | Mickey | Workout service, exercise service, suggestion engine |
| Clean arch layers (Core/Application/Infrastructure) | Mickey | Entities, use cases, DTOs, repository interfaces |
| Import / export | Mickey | JSON serialization, backup/restore, version migration |
| Smart suggestions engine | Mickey | Plateau detection, imbalance detection, rules engine |
| Goals & personal records | Mickey | PR tracking, 1RM calculations, goal progress |
| Session tracking, analytics data | Mickey | Volume calculations, frequency aggregations |
| Unit tests | Duke | Domain logic tests, service tests, suggestion engine tests |
| Integration tests | Duke | SQLite repository tests, migration tests |
| UI/flow tests | Duke | Core flow tests (start workout, log sets, save) |
| Test strategy, edge cases | Duke | Test planning, coverage, quality gates |
| Session logging | Scribe | Automatic — never needs routing |
| Work queue monitoring | Ralph | Issue scanning, PR triage, backlog keep-alive |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Rocky |
| `squad:rocky` | Architecture, decisions, review tasks | Rocky |
| `squad:apollo` | UI, MAUI, XAML, UX work | Apollo |
| `squad:mickey` | Backend, data, services, infrastructure | Mickey |
| `squad:duke` | Testing, QA, coverage | Duke |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Rocky** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Rocky to review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** When a feature is being built, spawn Duke to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Rocky handles all `squad` (base label) triage.
8. **Commit discipline** — agents do NOT auto-commit. Gabriel approves and commits after testing. (User directive: 2026-04-26)
