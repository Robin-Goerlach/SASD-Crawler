# Pre-G0 Crawl Work Queue / Background Worker – Tests

**CI-Lauf:** #28  
**Code-Commit:** `dc4296caff07b70c7347bcd6e7202de20d16caf7`

## CI

- Restore: success
- `dotnet format --verify-no-changes`: success
- Release build: success
- Tests: success

## Ergebnis

- Passed: **83**
- Failed: **0**
- Skipped: **0**
- Produktion/Architektur/E2E: **46**
- A1–A4-Spikes: **37**

| Produktions-Testprojekt | Tests |
|---|---:|
| `Sasd.Crawler.Domain.Tests` | 3 |
| `Sasd.Crawler.Application.Tests` | 9 |
| `Sasd.Crawler.Frontend.Tests` | 3 |
| `Sasd.Crawler.Architecture.Tests` | 6 |
| `Sasd.Crawler.Indexer.LocalFileSystem.Tests` | 4 |
| `Sasd.Crawler.Indexer.Workers.Tests` | 4 |
| `Sasd.Crawler.Persistence.Sqlite.Tests` | 15 |
| `Sasd.Crawler.LocalScan.E2E.Tests` | 2 |
| **Summe** | **46** |

## Nachgewiesen

- aktivierte LocalDirectory-Source kann durable Full-Scan-Arbeit erzeugen;
- deaktivierte Source wird vor Queue-Insertion abgewiesen;
- Job + WorkItem werden persistent angelegt;
- Leasing ist atomar und ein zweiter Worker erhält dasselbe Leased-Item nicht;
- Attempts erhöht sich nach Release und erneutem Lease;
- falscher LeaseOwner kann nicht abschließen;
- Succeeded/Partial/Failed werden dauerhaft unterschieden;
- kooperative Cancellation gibt den Lease zurück;
- Presenter queued nur und crawlt nicht;
- Worker-Assembly hängt weder von WinForms noch SQLite ab;
- echter BackgroundService-E2E verarbeitet einen durable Job und füllt die Document Registry;
- Schema v1 migriert bis v3 ohne Verlust vorhandener Source-Daten.

## Offen

- automatisches Reclaim nach hartem Prozesscrash;
- Retry-/Backoff-Policy;
- Pause/Resume;
- Scheduler;
- parallele Worker;
- Job-History/Failure-Queue-UI;
- Lucene-/Content-Pipeline;
- interaktive Suche unter Hintergrundlast.
