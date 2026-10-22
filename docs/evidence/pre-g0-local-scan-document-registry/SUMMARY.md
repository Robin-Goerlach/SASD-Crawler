# Pre-G0 Local Scan / Document Registry – Summary

**Datum:** 22. Oktober 2026  
**Branch:** `feat/local-scan-document-registry`  
**Code-Commit verifiziert:** `4c8a4bf5a9ea0888b934c58cf6790e2482ea09da`  
**CI:** GitHub Actions run #25 – success  
**Gate-Auswirkung:** G0 bleibt NOT READY; Milestone 0.1 bleibt formal blockiert.

## Ziel

Der zweite Produktionsslice setzt die sicherheitskritische Basis für lokale Full Scans um:

```text
Local source
  -> LocalFileSystemConnector
  -> LocalSourceScanService
  -> ScanRunId
  -> Document Registry
  -> successful authoritative completion
  -> Reconciliation
```

Der wichtigste Invariant lautet weiterhin:

> Ein unvollständiger Scan ist niemals Löschautorität.

## Implementierte Bausteine

### Domain

- starke `DocumentId`;
- starke `ScanRunId`;
- `DocumentRecord` mit SourceId, CanonicalLocator, Größe, Erstell-/Änderungszeit, LastSeen und LastSeenScanRunId;
- Scan-Outcomes `Running`, `Succeeded`, `Partial`, `Failed`, `Cancelled`.

### Application

- `ILocalFileSystemConnector`;
- `IDocumentRegistry`;
- `LocalSourceScanService`;
- `LocalScanOptions.MaximumDepth`;
- nicht-fatale Scan-Issues;
- erfolgreiche und partielle Scanabschlüsse werden fachlich getrennt.

### LocalFileSystem Indexer

- streamingbasierte explizite Traversierung statt vollständiger Materialisierung;
- rekursive Unterverzeichnisse;
- konfigurierbare Maximaltiefe;
- Reparse Points werden im aktuellen Local-Scope nicht verfolgt;
- Root nicht verfügbar -> fatal/non-authoritative;
- Datei-/Unterverzeichnis-I/O-Probleme -> Issue + Weiterlauf, aber gesamter Run wird Partial.

### SQLite

- zentraler `SqliteControlStore`;
- Schema-Version 2;
- forward-only Migration v1 -> v2 unter Erhalt vorhandener Sources;
- `scan_runs`;
- `documents`;
- stabile DocumentId bei wiederholter Beobachtung desselben Source+CanonicalLocator;
- LastSeen/LastSeenScanRunId;
- MissingSinceUtc als Registry-Reconciliation-Marker;
- atomare Kopplung von `Succeeded` und Missing-Reconciliation.

## Fail-closed Reconciliation

Die Implementierung besitzt absichtlich keinen globalen Missing-UPDATE in diesen Zuständen:

- Partial;
- Failed;
- Cancelled;
- laufender/stale Running-Scan.

Nur `CompleteFullScanAsync(..., Succeeded, issueCount: 0, ...)` kann unseen Dokumente als missing markieren.

Wenn ein Prozess vor dem Abschluss beendet wird, kann ein ScanRun als `Running` zurückbleiben. Das ist sicher: ein Running-Run besitzt keine Reconciliation-Autorität. Eine spätere Recovery-/Cleanup-Funktion kann solche Runs lediglich administrativ schließen.

## End-to-End-Nachweis

Ein Test verwendet reale temporäre Dateien und eine reale SQLite-Datenbank:

1. zwei Dateien anlegen;
2. Source registrieren;
3. vollständigen Scan ausführen;
4. beide Dokumente im Registry-Store sehen;
5. eine Datei physisch löschen;
6. zweiten erfolgreichen Full Scan ausführen;
7. genau ein Dokument wird missing;
8. das weiterhin vorhandene Dokument bleibt current.

## Requirement-Status

Vollständig implementiert und automatisiert verifiziert für den Local-0.1-Scope:

- ID-001 DocumentId;
- ID-002 SourceId-Zuordnung;
- ID-003 Canonical URI/Locator;
- ID-007 Zeitstempel und Größe;
- ID-008 LastSeen.

Teilimplementiert/teilverifiziert:

- SRC-001;
- SRC-003;
- LOC-001;
- LOC-002;
- LOC-003;
- LOC-005.

Diese bleiben partial, weil der vollständige 0.1-End-to-End-Pfad mit Content Extraction, Lucene, CrawlJob/WorkItem und Search noch nicht vorhanden ist.
