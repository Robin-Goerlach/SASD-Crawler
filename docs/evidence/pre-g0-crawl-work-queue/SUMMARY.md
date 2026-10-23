# Pre-G0 Crawl Work Queue / Background Worker – Summary

**Datum:** 23. Oktober 2026  
**Branch:** `feat/crawl-work-queue-background`  
**Code-Commit verifiziert:** `dc4296caff07b70c7347bcd6e7202de20d16caf7`  
**CI:** GitHub Actions run #28 – success  
**Gate-Auswirkung:** G0 bleibt NOT READY; Milestone 0.1 bleibt formal blockiert.

## Ziel

Den bereits implementierten Local Full Scan so ausführen, dass die WinForms-Oberfläche keine Dateisystemarbeit ausführt und angeforderte Arbeit vor der Ausführung dauerhaft gespeichert wird.

```text
WinForms "Scan now"
    ↓
MainPresenter
    ↓
ICrawlJobService
    ↓
durable CrawlJob + WorkItem (SQLite)
    ↓
lease
    ↓
CrawlWorkBackgroundService
    ↓
CrawlWorkItemProcessor
    ↓
ILocalSourceScanner
    ↓
LocalSourceScanService
    ↓
Document Registry / Reconciliation
```

## Architektur

Job und WorkItem sind getrennte Konzepte. Der Job ist die dauerhafte Benutzer-/Scheduler-Anforderung; WorkItems sind ausführbare Stufen/Versuche.

Neu sind unter anderem:

- `CrawlJobId`, `WorkItemId`;
- `CrawlJobStatus`, `WorkItemStatus`, `WorkItemStage`;
- `ICrawlJobService`, `CrawlJobService`;
- `ICrawlWorkQueue`, `CrawlJobSnapshot`, `LeasedCrawlWorkItem`;
- `ILocalSourceScanner`;
- `SqliteCrawlWorkQueue`;
- `CrawlWorkItemProcessor`;
- `CrawlWorkBackgroundService`.

## SQLite Schema v3

`crawl_jobs` speichert Source, Status, Request/Start/Ende, Scan-Zähler und Fehlerzusammenfassung.

`work_items` speichert Stage, Status, Attempts, NextAttemptUtc, LeaseOwner, LeaseUntilUtc, Zeitstempel und Fehler.

Job und initiales WorkItem werden gemeinsam in einer Transaktion erzeugt. Leasing erfolgt atomar mit `UPDATE ... RETURNING`. Complete/Fail/Release prüfen den persistierten LeaseOwner.

## Background-Ausführung

Der aktuelle Produktionsstand benutzt genau einen Consumer. Das ist absichtlich correctness-first: Parallelität wird erst nach Messungen ergänzt.

Bei kooperativem Host-Shutdown wird der aktive Scan non-authoritative abgebrochen und das WorkItem wieder `Pending`; der Job wird erneut `Queued`.

Bei echtem Verarbeitungsfehler werden WorkItem und Job terminal `Failed`.

## Fail-closed Crash-Verhalten

`LeaseUntil` wird persistent gespeichert, aber abgelaufene Leases werden noch **nicht** automatisch übernommen.

Nach einem harten Prozessabbruch kann ein WorkItem daher in `Leased` zurückbleiben. Das verhindert derzeit automatische Doppelverarbeitung, ist aber noch kein vollständiger Wiederanlauf. Die spätere Recovery muss mit der Produktions-Single-Instance-/Worker-Policy abgestimmt werden.

## WinForms

Der neue Befehl **Scan now** reiht ausschließlich durable Arbeit ein. Die Form kennt weder SQLite-Queue noch FilesystemConnector und wartet nicht auf den Full Scan.

## Nächster sinnvoller Slice

```text
Document Registry
    ↓
TXT/HTML content read
    ↓
ISearchIndex production adapter
    ↓
Lucene
    ↓
SearchService
    ↓
WinForms Search Shell
```

Lease-Recovery/Retry bleibt als eigener Reliability-Slice dokumentiert.
