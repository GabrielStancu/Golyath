# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture, tech decisions, layer boundaries | Mal | Clean architecture violations, MVVM design, migration strategy |
| Code review | Mal | Review PRs, check quality, enforce layer rules |
| Epic decomposition, scope & priorities | Mal | Break epics into tasks, trade-offs, what to build next |
| SQLite schema, repositories, migrations | Kaylee | DB design, data access, import/export, application services |
| Domain entities, use cases, DTOs | Kaylee | Core entities, application layer services |
| Import/Export | Kaylee | JSON serialization, schema versioning, backup/restore |
| XAML Views, ViewModels, UI components | Wash | Pages, bindings, commands, converters, styles |
| Animations, gestures, theme | Wash | Swipe gestures, rest timer UI, dark/light mode |
| Navigation, shell structure | Wash | App shell, tab navigation, page routing |
| Unit tests, integration tests | Zoe | Domain logic tests, SQLite integration tests, UI flow tests |
| Test coverage review | Zoe | Check coverage, identify gaps, write missing tests |
| Session logging | Scribe | Automatic — never needs routing |
| Backlog tracking, issue queue | Ralph | Monitor open work, drive issue lifecycle |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Mal |
| `squad:mal` | Pick up issue and complete the work | Mal |
| `squad:kaylee` | Pick up issue and complete the work | Kaylee |
| `squad:wash` | Pick up issue and complete the work | Wash |
| `squad:zoe` | Pick up issue and complete the work | Zoe |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Mal** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Mal's review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks. **No auto-commit** — Scribe stages but does NOT commit; Gabriel commits manually.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn Zoe to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Mal handles all `squad` (base label) triage.

