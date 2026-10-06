# Repository Reconciliation – 2026-10-06

**Repository:** `Robin-Goerlach/SASD-Crawler`  
**Baseline main audited:** `d58ed6ecf87b5a71d2003b2485362c649e6a695f`  
**Scope:** prior SASD-Crawler chats, current documentation, A1 code/evidence, repository hygiene and current upstream technology state

## 1. Confirmed alignment

### A1 code vs. architecture

The merged code matches the intended A1 slice:

- single-instance guard is acquired before Host/SQLite startup,
- Generic Host/DI starts Heartbeat and Activation hosted services,
- SQLite is injected behind `IHeartbeatStore`,
- the WinForms Form does not execute SQL or worker logic,
- Presenter marshals state to the UI context,
- tray and activation plumbing exist,
- A1 evidence records 9/9 tests and Conditional Go.

No production crawler/search feature is falsely counted as implemented.

### Product invariants retained

The repository documents the core decisions from strategy discussions:

- index-in-place rather than DMS import,
- local/USB/SMB/Web sources,
- offline media identity and retained results,
- successful full reconciliation as delete authority,
- Tika/Tesseract extraction/OCR direction,
- Lucene backend abstraction with OpenSearch fallback path,
- classical search before semantic/AI features.

## 2. Drift/findings found

### F-001 README license mismatch – fixed
README said no license was selected while `LICENSE` already contains MIT.

### F-002 Roadmap/status drift after A1 – fixed
A1 had merged, but some roadmap lines still listed Repository/Solution and WinForms shell as not implemented and baseline docs as unaccepted.

### F-003 Tracked build/IDE artifacts – fixed
Tracked `bin/`, `obj/` and `.csproj.user` files were present despite `.gitignore`. They are removed from Git in this reconciliation; future generated copies remain ignored.

### F-004 Duplicate solution root – fixed
Both `Sasd.Crawler.sln` and spike-specific `Sasd.Crawler.Spike.A1.slnx` described the same A1 projects. `Sasd.Crawler.sln` remains the canonical Visual Studio 2022 solution.

### F-005 CI missing – fixed
A Windows GitHub Actions baseline is added for restore, format verification, Release build and tests.

### F-006 A2 task missing – fixed
A2 Lucene.NET task and autonomous prompt are added.

### F-007 Quality maturity implicit – fixed
`docs/QUALITY-LEVEL.md` explicitly separates architecture validation, vertical slice, MVP, RC and stable quality.

### F-008 .NET 8 lifecycle became urgent – recorded
As of this review .NET 8 is in maintenance, latest servicing is 8.0.31 / SDK 8.0.425, and support ends 10 November 2026. The current spikes remain on .NET 8 as explicitly chosen, but G0 must set an upgrade point before production 1.0.

### F-009 Tika baseline changed upstream – recorded
Apache Tika 4.1.0 is current while 3.3.2 remains a supported maintenance line. A3 is now an explicit 4.1-vs-3.3 packaging/migration/security decision.

### F-010 old A1 remote branch
The remote `codex/0.0.1-winforms-host-spike` was still visible during this audit even though PR #1 is merged. It is safe to delete after this reconciliation; branch deletion is intentionally not hidden inside documentation cleanup.

## 3. Code changes deliberately not made

A1 source is not refactored into production namespaces yet. It is still a spike and should remain evidence of the architecture experiment until G0 determines the production skeleton.

No Lucene/Tika/OCR/real crawler implementation is added by this reconciliation. Those belong to their planned spikes/slices.

## 4. Current truth after this reconciliation

- A1: **CONDITIONAL GO**, implementation/automated verification complete; manual desktop evidence pending.
- A2: **READY / NEXT**.
- Product quality maturity: **Q0 Architecture Validation**.
- Active UI/platform baseline: WinForms / .NET 8 / Windows-first / per-user Desktop.
- MIT license is active.
- CI baseline exists.
- 0.1 production slice must still wait for G0.
