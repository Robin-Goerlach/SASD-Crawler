# Pre-G0 C# Production Foundation – Summary

**Datum:** 22. Oktober 2026  
**Branch:** `feat/production-source-catalog-foundation`  
**Code-Commit verifiziert:** `10a4809008b4ad4ece83dfc39dd78d443e146d55`  
**CI:** GitHub Actions run #22 – success  
**Gate-Auswirkung:** G0 bleibt NOT READY; Milestone 0.1 bleibt formal blockiert.

## Ziel

Die kanonische C#/.NET-8-Produktionsimplementierung soll nicht länger nur aus Architekturspikes bestehen. Gleichzeitig darf der noch offene G0-Status nicht durch eine falsche Milestone-Deklaration umgangen werden.

Daher wurde ein kleiner, realer Produktionspfad aufgebaut, der die akzeptierten Architekturgrenzen praktisch erzwingt, aber noch keinen vollständigen 0.1-Crawl behauptet.

## Implementierter Produktionscode

### Shared / Domain

- starke `SourceId` statt nackter GUID-Nutzung;
- `SourceKind` als fachlicher Quellentyp;
- `SourceDefinition` ohne WinForms-, SQLite- oder Lucene-Abhängigkeit;
- idempotentes Aktivieren/Deaktivieren mit stabilen Zeitstempelinvarianten.

### Shared / Application

- `ISourceCatalog` als UI-neutraler Use-Case-Vertrag;
- `ISourceRepository` als Persistence Port;
- `SourceCatalogService`;
- Canonical-Location-Normalisierung für lokale Ordner;
- idempotente Quellregistrierung und Race-safe `TryAdd`-Semantik.

### Indexer / SQLite

- Control-Store-Schema Version 1;
- WAL;
- parametrisierte Statements;
- eindeutige Canonical Location mit NOCASE;
- Schema-`user_version` mit Fail-Closed-Verhalten bei unbekannter neuerer Version;
- kurzlebige Connections; Pooling zunächst bewusst deaktiviert, damit Lifecycle und Tests deterministisch bleiben.

### Frontend

- UI-neutraler `MainPresenter`;
- `IMainView` ohne WinForms-Typen;
- echte `Sasd.Crawler.WinForms`-Produktionsexecutable;
- lokale Ordner registrieren;
- Quellen anzeigen;
- Aktiv/Inaktiv umschalten;
- Refresh;
- Generic Host als Composition Root.

### Build-/Repository-Sicht

- `Sasd.Crawler.sln` bleibt Integrations-Solution;
- `Sasd.Crawler.Frontend.slnf`;
- `Sasd.Crawler.Indexer.slnf`;
- bestehende A1–A5-Spikes bleiben unverändert als Evidence.

## Bewusst noch nicht implementiert

- Dateisystem-Crawl;
- rekursive Traversierung;
- Document Registry;
- ScanRunId;
- Reconciliation;
- Content Hash;
- Lucene-Produktadapter;
- Search UI;
- Tika/OCR-Produktintegration.

Deshalb wird keine 0.1-Anforderung in diesem Arbeitsblock auf `DONE` gesetzt.

## Architekturbeobachtung

ADR-0017 wird erstmals durch Produktionscode praktisch erzwungen:

```text
frontend  ─────► shared ◄───── indexer
```

Der Desktop Composition Root darf konkrete Indexer-Implementierungen verdrahten. Presenter und Views kennen SQLite nicht; die Indexer-Persistence kennt WinForms nicht.

## Nächster sinnvoller Produktionsslice

Nach beziehungsweise parallel zur kontrollierten G0-Schließung:

```text
LocalFileSystemConnector
→ Document Registry
→ ScanRunId
→ erfolgreiche Full-Scan-Markierung
→ Reconciliation
```

Dabei gilt weiterhin: Ein unvollständiger oder fehlgeschlagener Scan darf niemals Massendeletion auslösen.
