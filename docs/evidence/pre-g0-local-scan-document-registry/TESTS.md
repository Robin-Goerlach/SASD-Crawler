# Pre-G0 Local Scan / Document Registry – Tests

**CI-Lauf:** #25  
**Code-Commit:** `4c8a4bf5a9ea0888b934c58cf6790e2482ea09da`

## CI-Schritte

- Restore: success
- `dotnet format --verify-no-changes`: success
- Release build: success
- Tests: success

## Gesamtergebnis

- Passed: **69**
- Failed: **0**
- Skipped: **0**

## Produktions-Testprojekte

| Projekt | Tests |
|---|---:|
| `Sasd.Crawler.Domain.Tests` | 3 |
| `Sasd.Crawler.Application.Tests` | 7 |
| `Sasd.Crawler.Frontend.Tests` | 2 |
| `Sasd.Crawler.Architecture.Tests` | 5 |
| `Sasd.Crawler.Indexer.LocalFileSystem.Tests` | 4 |
| `Sasd.Crawler.Persistence.Sqlite.Tests` | 10 |
| `Sasd.Crawler.LocalScan.E2E.Tests` | 1 |
| **Produktion gesamt** | **32** |

Zusätzlich bleiben 37 Spike-Tests aus A1–A4 grün.

## Neu nachgewiesene Eigenschaften

- lokale Verzeichnisse werden streamingbasiert rekursiv traversiert;
- MaximumDepth begrenzt die Rekursion;
- fehlender Root wird nicht als leerer erfolgreicher Scan behandelt;
- Cancellation bleibt non-authoritative;
- einzelne Scan-Issues verhindern nicht das Weiterarbeiten, aber verhindern Reconciliation;
- wiederholte Beobachtung behält dieselbe DocumentId;
- Dateigröße, Modified/Created und LastSeen werden aktualisiert;
- LastSeenScanRunId wird pro Fund fortgeschrieben;
- erfolgreicher vollständiger Folgescan markiert ausschließlich unseen Dokumente missing;
- Partial markiert keine unseen Dokumente missing;
- Failed markiert keine unseen Dokumente missing;
- Cancelled markiert keine unseen Dokumente missing;
- ein wieder auftauchendes Dokument verliert seinen Missing-Marker;
- Schema v1 migriert auf v2 ohne Verlust vorhandener Source-Daten;
- LocalFileSystem-Indexer bleibt frei von WinForms- und SQLite-Abhängigkeiten;
- realer Local-Scan-E2E-Pfad funktioniert.

## Noch nicht bewiesen

- tatsächliche Inhaltslesbarkeit/Content Extraction für TXT/HTML;
- Lucene-Index-Update und gezielte Index-Löschung;
- persistente CrawlJob-/WorkItem-Ausführung;
- UI-gesteuerter Hintergrundscan;
- FileSystemWatcher-Inkrementalpfad;
- physische Permission-Änderung während eines Windows-Scans als reproduzierbarer CI-Fall;
- Performance auf großen realen Dateibäumen.

Diese Punkte werden aus diesem Slice ausdrücklich nicht abgeleitet.
