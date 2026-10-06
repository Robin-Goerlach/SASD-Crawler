# Decision Log

**Stand:** 6. Oktober 2026

| ADR | Entscheidung | Status |
|---|---|---|
| ADR-0001 | Windows Forms + .NET 8 | **Accepted**; Lifecycle-Risiko R-001 bleibt |
| ADR-0002 | Modularer Desktop-Monolith + Clean Architecture + MVP | **Accepted**, A1 bestätigt Grundschnitt |
| ADR-0003 | Per-user Desktopbetrieb als 1.0-Standard | **Accepted**, A1 nutzt per-user Datenpfad/Mutex |
| ADR-0004 | SQLite als Control Store | **Accepted in principle**, A1 bestätigt SQLite-Betrieb |
| ADR-0005 | Lucene.NET hinter `ISearchIndex` | **Proposed – A2 required** |
| ADR-0006 | Apache Tika als isolierter Referenzparser | **Proposed – A3 required**; 4.1 vs. 3.3 Maintenance prüfen |
| ADR-0007 | Toxy nur als Benchmark/Fast Path | **Proposed – A5 required** |
| ADR-0008 | Tesseract OCR | Proposed – OCR PoC später |
| ADR-0009 | Durable SQLite Queue + bounded Channels | Proposed – Produktionsslice noch offen |
| ADR-0010 | Reconciliation entscheidet Löschungen; Watcher nur Hint | **Accepted product invariant** |
| ADR-0011 | MediaId + RelativePath für Wechseldatenträger | Proposed – A4 required |
| ADR-0012 | SMB über Windows/UNC im Benutzerkontext | Accepted architectural direction; implementation later |
| ADR-0013 | Index und Cache sind rekonstruierbar | **Accepted product invariant** |
| ADR-0014 | Kein Windows Service im Desktop-MVP | **Accepted** |
| ADR-0015 | Shared Mode nach 1.0 als separater Host | **Accepted planning boundary** |

PoC-abhängige ADR-Dateien bleiben bis zum jeweiligen Gate in `Proposed`; dieser Log ist die konsolidierte Statusübersicht.
