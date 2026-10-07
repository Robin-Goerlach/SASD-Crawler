# Risk Register

**Stand:** 7. Oktober 2026

Skala: Wahrscheinlichkeit (W) und Auswirkung (A) 1–5. Score = W × A.

| ID | Risiko | W | A | Score | Maßnahme | Trigger/Review | Status |
|---|---|---:|---:|---:|---|---|---|
| R-001 | .NET 8 Support endet 10.11.2026 | 5 | 5 | 25 | aktuelles Servicing 8.0.31 / SDK 8.0.425; Architektur upgradefähig halten; G0 legt Migration vor produktiver 1.0 fest | **G0**, jedes Release | **CRITICAL / OPEN** |
| R-002 | Lucene.NET 4.8.0-beta00018 bleibt formal Beta | 3 | 5 | 15 | A2: 100k/1M, Realkorpus, Crash/Recovery, Full-Disk und Suchfunktionen erfolgreich; `ISearchIndex`-Fallback bleibt | G0/G8 | **PARTIALLY MITIGATED – A2 TECHNICAL GO** |
| R-003 | Tika 4.x ist neuer Major mit Breaking Changes; 3.3.2 Maintenance parallel | 4 | 3 | 12 | A3 beweist 3.3.2 Sidecar + reduzierte Runtime; 4.1.0 muss auf gleicher Packaging/Security/Migrationsbasis noch verglichen werden | A3/G0 | OPEN |
| R-004 | Parser-CVE/manipulierte Dokumente | 3 | 5 | 15 | Sidecar, limits, updates, sandbox | jedes Release | OPEN |
| R-005 | falsche Löschung bei NAS/USB-Ausfall | 3 | 5 | 15 | Complete-Scan-Gate, source health | G1/G2 | OPEN |
| R-006 | USB-Medien nicht eindeutig identifizierbar | 3 | 4 | 12 | konservatives Matching + MediaId/RelativePath automatisiert bewiesen; user-assisted fallback; physische Medien-Smokes offen | A4/G0 | **PARTIALLY MITIGATED** |
| R-007 | UI friert bei Hintergrundarbeit ein | 2 | 4 | 8 | A1 Architektur automatisiert bestätigt; interaktiver Smoke offen; async/MVP beibehalten | A1 manual/G1 | PARTIALLY MITIGATED |
| R-008 | SQLite/Lucene Dual-Write Inkonsistenz | 3 | 4 | 12 | idempotente WorkItems, reconciliation | G1/G8 | OPEN |
| R-009 | Webcrawler SSRF/Crawl Trap | 3 | 5 | 15 | IP policy, limits, fixtures | G3 | OPEN |
| R-010 | OCR zu langsam | 4 | 3 | 12 | low concurrency, queue, limits, cache | G-MVP/G8 | OPEN |
| R-011 | Pflichtenheft widerspricht Architektur | 1 | 3 | 3 | CR-2026-001 + Amendment + Pflichtenheft 0.2 konsolidiert/angenommen | Reconciliation | **CLOSED 2026-10-06** |
| R-012 | Scope Creep Richtung DMS | 4 | 4 | 16 | Non-goals + roadmap gates | Roadmap review | OPEN |
| R-013 | Antivirus blockiert Tika/JRE/Temp | 2 | 4 | 8 | A3 Sidecar/Runtime auf Windows technisch grün; signierter Installer + AV/SmartScreen-Evidence fehlt | A3/RC | **PARTIALLY MITIGATED** |
| R-014 | Indexgröße wächst stark | 3 | 3 | 9 | A2 misst ca. 134,3 Byte/Dokument bei 1M synthetischen Dokumenten; reale Lastprofile später erneut messen | G8 | **PARTIALLY MITIGATED** |
| R-015 | Search Ranking enttäuscht | 3 | 4 | 12 | Golden Query Set, analyzers, tuning | G-MVP/G8 | OPEN |
| R-016 | Offline Cache enthält sensible Texte | 3 | 4 | 12 | per-user ACL, cache retention, encryption evaluate | G6 | OPEN |
| R-017 | FileSystemWatcher Eventverlust | 5 | 3 | 15 | watcher only hint, periodic reconciliation | G1 | MITIGATED BY DESIGN |
| R-018 | Drittkomponentenlizenzproblem | 2 | 4 | 8 | A3 partielles 122-Komponenten-Inventar; finales SBOM/NOTICE/Lizenz-/CVE-Mapping vor Packaging | A3/RC | **PARTIALLY MITIGATED** |
| R-019 | Installer/Upgrade beschädigt DB | 2 | 5 | 10 | backup preflight, migrations, rollback tests | G-RC | OPEN |
| R-020 | Shared Mode später schwer nachrüstbar | 2 | 4 | 8 | Application abstractions, ACL metadata | G6/1.3 | OPEN |
| R-021 | Build-/IDE-Artefakte werden versehentlich versioniert | 2 | 2 | 4 | `.gitignore`, CI, Reconciliation entfernt tracked `bin/obj/*.user` | jedes PR | MITIGATED 2026-10-06 |

## Reviewregel

- Score ≥ 15: bei jedem Gate.
- Score 10–14: mindestens je Milestone.
- Score < 10: je Releaseplanung.
