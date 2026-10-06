# Changelog

All notable project changes are recorded here. The project follows semantic-versioning intent; pre-1.0 spike milestones may still change internal APIs.

## Unreleased

### Changed
- Reconciled repository state against the SASD-Crawler strategy, requirements, architecture and merged A1 implementation.
- Promoted CR-2026-001, Lastenheft Amendment 0.1a and Pflichtenheft 0.2 as the active WinForms/.NET-8 baseline.
- Updated the roadmap/status for A1 `CONDITIONAL GO` and A2 readiness.
- Updated .NET 8 servicing SDK pin to 8.0.425.
- Updated A3 planning to explicitly evaluate current Tika 4.x versus the supported 3.3.x maintenance line.

### Added
- Windows GitHub Actions CI for restore, format verification, Release build and tests.
- Quality maturity model (`docs/QUALITY-LEVEL.md`).
- Repository reconciliation audit dated 2026-10-06.
- Non-normative Enterprise Information Discovery vision.
- A2 Lucene.NET task and autonomous Codex prompt.

### Removed
- Tracked `bin/` / `obj/` build outputs and `.csproj.user` IDE state.
- Redundant A1 `.slnx` solution in favor of canonical `Sasd.Crawler.sln`.
- Stale starter repository manifest/setup metadata.

## 0.0.1 – A1 WinForms Host Lifecycle Spike

### Added
- .NET 8 Windows Forms spike with Generic Host and dependency injection.
- Cancellable heartbeat `BackgroundService`.
- SQLite/WAL heartbeat persistence under the per-user application path.
- MVP/presenter boundary with UI-thread marshalling.
- Tray Open, Pause/Resume and Exit behavior.
- Per-user single-instance mutex and named-pipe activation.
- Automated A1 test suite: 9/9 passed in recorded evidence.

### Gate
- **A1: CONDITIONAL GO.** Automated architecture evidence is green; manual interactive Windows desktop smoke evidence remains pending.
