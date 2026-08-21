# Quality Gates

**Stand:** 21. August 2026

## G0 – Architecture Feasibility
- A1–A5 abgeschlossen.
- kein offener Architekturblocker.
- Pflichtenheft/Baseline synchron.
- Stackentscheidung dokumentiert.

## G1 – Local Vertical Slice
- local crawl E2E.
- update/delete/reconciliation.
- responsive UI.
- no critical defects.
- recovery smoke.

## G2 – Storage Sources
- USB offline/reattach.
- SMB disconnect without deletion.
- media identity.
- source health.

## G3 – Web
- robots/sitemap.
- rate limits.
- 404/410 policy.
- 429.
- SSRF protection.
- crawl trap tests.

## G4 – Rich Documents
- mandatory formats pass golden corpus.
- parser isolation.
- archive limits.
- last-known-good.

## G-MVP – 0.5
- all MVP acceptance scenarios.
- OCR.
- search syntax/facets/snippets.
- preview.
- no open Critical/High.

## G6 – Security
- per-user protection.
- secrets.
- cache/index ACLs.
- threat model review.
- shared-mode abstractions tested.

## G7 – Operations
- durable queue.
- retry/dead letter.
- scheduler.
- diagnostics.
- audit/logging.

## G8 – Hardening
- 1M benchmark.
- consistency checker.
- recovery suite.
- telemetry.
- performance targets.

## G-RC – 0.9
- installer.
- backup/restore.
- migration.
- accessibility.
- SBOM/license.
- security test.
- clean-machine install.

## G-1.0
- 169 MUSS or formally changed.
- acceptance run.
- no Critical/High.
- artifacts checksummed/signed as available.
- release notes.
- support matrix.
