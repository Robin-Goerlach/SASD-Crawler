# Current Checklist

**Stichtag:** 23. Oktober 2026  
**Aktueller Fokus:** **G0-Rest-Evidence schließen und Baseline 0.1b konsistent halten**

## Baseline / Governance

- [x] Produktanalyse
- [x] Lastenheft 0.1
- [x] WinForms/.NET-8-Architektur
- [x] CR-2026-001 WinForms/.NET-8-Rebaselining angenommen
- [x] Lastenheft-Amendment 0.1a angenommen
- [x] Pflichtenheft 0.2 als aktive technische Baseline
- [x] Roadmap / Teststrategie / Risk Register / ADR-Baseline
- [x] CR-2026-002 + Amendment 0.1b angenommen
- [x] wirksame fachliche Baseline auf 260 Anforderungen erweitert
- [x] SEA-022 Stopwortfilterung spezifiziert
- [x] ID-011 geschärft und ID-012 fortlaufende Dublettenerkennung spezifiziert
- [x] Polyglot-Repository-Grenze mit C#/.NET-8-Kanon festgelegt
- [x] CR-2026-003 / ADR-0017: Frontend/shared/Indexer als getrennte Produktionsbereiche festgelegt
- [x] `src/frontend/`, `src/shared/`, `src/indexer/` und gespiegelte Testbereiche vorbereitet
- [x] Repository-Reconciliation 2026-10-06 nach `main` gemerged
- [x] Windows-CI auf reconciled `main` grün
- [ ] PoC-abhängige ADRs nach A2–A5 formal reviewen
- [x] C#/.NET 8 als kanonische V1 für Managed-PC-Kompatibilität bestätigt (CR-2026-002 / ADR-0016)
- [ ] .NET-8-Support-/Upgradepfad als G0-Risiko dokumentieren
- [ ] G0 final entscheiden

## Pre-G0 C#-Produktionsbasis

- [x] Domain-Projekt mit stabiler `SourceId` und `SourceDefinition`
- [x] Application-Projekt mit UI-neutralem `ISourceCatalog`
- [x] SQLite-Control-Store Schema v3 mit WAL, vorwärtskompatibler v1→v2→v3-Migration und Schema-Versionsschutz
- [x] UI-neutraler MVP Presenter
- [x] produktive .NET-8-WinForms-Shell für Quellenverwaltung
- [x] Frontend-/Indexer-Solution-Filter
- [x] Domain-, Application-, SQLite-, Presenter- und Architekturtests
- [x] `DocumentId`, `ScanRunId` und durable `DocumentRecord`
- [x] LocalFileSystemConnector mit Streaming-Traversierung und konfigurierbarer Maximaltiefe
- [x] Reparse Points werden im aktuellen Local-Scope nicht verfolgt
- [x] nicht-fatale Datei-/Verzeichnisfehler machen den Scan `Partial`, ohne den restlichen Crawl abzubrechen
- [x] Document Registry mit LastSeen/LastSeenScanRunId
- [x] Reconciliation nur nach `Succeeded`; Partial/Failed/Cancelled können keine Missing-Markierung auslösen
- [x] realer E2E-Test: Ordner → Scan → SQLite → Datei löschen → erfolgreicher Rescan → genau ein Missing-Dokument
- [x] `CrawlJobId` und `WorkItemId` als getrennte dauerhafte Identitäten
- [x] SQLite-v3-`crawl_jobs` / `work_items` mit Stage, Status, Attempts, NextAttempt, LeaseOwner, LeaseUntil
- [x] atomare Lease-Übernahme und Owner-geprüfte Complete/Fail/Release-Transitions
- [x] `CrawlJobService`: UI-neutraler Queue-Use-Case
- [x] `CrawlWorkItemProcessor` und Generic-Host-`CrawlWorkBackgroundService`
- [x] WinForms `Scan now` reiht nur durable Arbeit ein; kein Crawl auf dem UI-Thread
- [x] Background-E2E: durable Job → echter BackgroundService → Local Scan → Document Registry
- [x] CI #28: Restore / Format / Release-Build / Tests grün
- [x] 83 Tests gesamt, 0 Fehler, 0 übersprungen
- [x] ID-001/ID-002/ID-003/ID-007/ID-008 vollständig implementiert und automatisiert verifiziert
- [ ] Crash-Reclaim/Retry-Backoff für abgelaufene Leases erst mit expliziter Recovery-/Single-Instance-Policy
- [ ] nächster kontrollierter Produktionsslice: Lucene-Produktadapter und einfache TXT/HTML-Indexierung

## Architektur-Spikes

- [x] A1 WinForms + Generic Host – **CONDITIONAL GO**
  - [x] Build / automatisierte Tests / Format
  - [ ] interaktiver Tray-Smoke
  - [ ] sichtbare Responsiveness
  - [ ] Second-Launch-Fokus/Restore
  - [ ] echter Forced-Kill/Restart
- [x] A2 Lucene.NET – **TECHNICAL GO**
  - [x] 100k / 1M Benchmark
  - [x] Realkorpus
  - [x] Update/Delete + Suchfunktionen
  - [x] Crash/Reopen
  - [x] Full-Disk-/Write-Failure-Simulation
  - [ ] ADR-0005 formal reviewen
- [x] A3 Tika Sidecar – **CONDITIONAL GO**
  - [x] Tika 3.3.2 Sidecar + Java 21
  - [x] reduzierte Runtime und Formatprobe
  - [ ] Tika 4.1 auf gleicher Basis vergleichen
  - [ ] finales Packaging/SBOM/CVE/Signierung/AV
- [x] A4 Media Identity – **CONDITIONAL GO**
  - [x] Windows-API-/Matching-/RelativePath-Modell
  - [x] 13 automatisierte Tests
  - [ ] physische NTFS/exFAT/FAT32-Smokes
  - [ ] Attach/Detach + Drive-Letter-Wechsel
- [x] A5 Tika vs. Toxy – **CONDITIONAL; TIKA DEFAULT**
  - [x] synthetischer Formatvergleich inkl. Legacy-XLS + 5-MiB-TXT
  - [x] kein Toxy-Fast-Path freigegeben
  - [ ] reale/komplexe Dokumente
  - [ ] Legacy DOC/PPT
  - [ ] verschlüsselte/bösartige Dateien
- [ ] **G0 = GO**

## Repository-Hygiene / CI

- [x] MIT-Lizenz im README korrekt
- [x] `bin/`, `obj/`, `*.user` aus Git entfernt
- [x] `Sasd.Crawler.sln` kanonisch
- [x] Windows-CI für restore/format/Release-build/tests
- [x] erster CI-Lauf auf reconciled `main` erfolgreich
- [x] Recovery-PR A2–A5 erfolgreich verifiziert und als PR #4 gemerged
- [x] alte Recovery-/Codex-/Reconciliation-Branches remote/lokal entfernt; nur `main` verbleibt

## Erst nach G0

- [ ] Milestone 0.1 Local Vertical Slice starten
