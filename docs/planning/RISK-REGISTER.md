# Risk Register

**Stand:** 21. August 2026

Skala: Wahrscheinlichkeit (W) und Auswirkung (A) 1–5. Score = W × A.

| ID | Risiko | W | A | Score | Maßnahme | Trigger/Review | Status |
|---|---|---:|---:|---:|---|---|---|
| R-001 | .NET 8 Support endet 10.11.2026 | 5 | 4 | 20 | Framework zentral, Upgradepfad, vor 1.0 neu bewerten | G0, G-RC | OPEN |
| R-002 | Lucene.NET formaler Beta-Status | 3 | 5 | 15 | A2 Benchmark + ISearchIndex-Fallback | A2 | OPEN |
| R-003 | Tika/JRE macht Desktoppaket schwer/komplex | 4 | 3 | 12 | A3 Packaging + Toxy Benchmark | A3/A5 | OPEN |
| R-004 | Parser-CVE/manipulierte Dokumente | 3 | 5 | 15 | Sidecar, limits, updates, sandbox | jedes Release | OPEN |
| R-005 | falsche Löschung bei NAS/USB-Ausfall | 3 | 5 | 15 | Complete-Scan-Gate, source health | G1/G2 | OPEN |
| R-006 | USB-Medien nicht eindeutig identifizierbar | 3 | 4 | 12 | mehrere Fingerprints, user-assisted fallback | A4 | OPEN |
| R-007 | UI friert bei Hintergrundarbeit ein | 3 | 4 | 12 | MVP, async, bounded workers | A1/G1 | OPEN |
| R-008 | SQLite/Lucene Dual-Write Inkonsistenz | 3 | 4 | 12 | idempotente WorkItems, reconciliation | G1/G8 | OPEN |
| R-009 | Webcrawler SSRF/Crawl Trap | 3 | 5 | 15 | IP policy, limits, fixtures | G3 | OPEN |
| R-010 | OCR zu langsam | 4 | 3 | 12 | low concurrency, queue, limits, cache | G-MVP/G8 | OPEN |
| R-011 | Pflichtenheft widerspricht Architektur | 5 | 3 | 15 | Baseline Control + Pflichtenheft 0.2 | vor G0 | OPEN |
| R-012 | Scope Creep Richtung DMS | 4 | 4 | 16 | Non-goals + roadmap gates | Roadmap review | OPEN |
| R-013 | Antivirus blockiert Tika/JRE/Temp | 2 | 4 | 8 | A3 reale Windows-Tests | A3/RC | OPEN |
| R-014 | Indexgröße wächst stark | 3 | 3 | 9 | term vectors bewusst, cache policy, benchmark | A2/G8 | OPEN |
| R-015 | Search Ranking enttäuscht | 3 | 4 | 12 | Golden Query Set, analyzers, tuning | G-MVP/G8 | OPEN |
| R-016 | Offline Cache enthält sensible Texte | 3 | 4 | 12 | per-user ACL, cache retention, encryption evaluate | G6 | OPEN |
| R-017 | FileSystemWatcher Eventverlust | 5 | 3 | 15 | watcher only hint, periodic reconciliation | G1 | MITIGATED BY DESIGN |
| R-018 | Drittkomponentenlizenzproblem | 2 | 4 | 8 | SBOM/license gate | A3/RC | OPEN |
| R-019 | Installer/Upgrade beschädigt DB | 2 | 5 | 10 | backup preflight, migrations, rollback tests | G-RC | OPEN |
| R-020 | Shared Mode später schwer nachrüstbar | 2 | 4 | 8 | Application abstractions, ACL metadata | G6/1.3 | OPEN |

## Reviewregel

- Score ≥ 15: bei jedem Gate.
- Score 10–14: mindestens je Milestone.
- Score < 10: je Releaseplanung.
