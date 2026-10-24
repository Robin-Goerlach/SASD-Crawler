# SASD-Crawler – Projektstatus

**Stichtag:** 24. Oktober 2026  
**Gesamtstatus:** 🟡 Architekturvalidierung; A2 technisch erfolgreich, A3–A5 mit belastbarer Teil-Evidence, G0 noch nicht freigegeben  
**Aktueller Arbeitsblock:** **G0-Rest-Evidence plus verifizierte Pre-G0-C#-Produktionsbasis**  
**Nächstes Gate:** **G0 – Architecture Feasibility Review**  
**Letzte verifizierte Main-Basis vor CR-2026-003:** `60f185a81582ed5723454571d65e018b4cb96ced`

## 1. Executive Snapshot

| Bereich | Status | Kommentar |
|---|---|---|
| Produktanalyse | ✅ DONE | auditierte Produkt-/Funktionsanalyse vorhanden |
| Fachliche Baseline | ✅ ACTIVE | Lastenheft 0.1 + Amendments 0.1a/0.1b; wirksam 260 Anforderungen |
| CR-2026-001 / Amendment 0.1a | ✅ ACCEPTED | WinForms/.NET-8-Rebaselining bestätigt |
| CR-2026-002 / Amendment 0.1b | ✅ ACCEPTED | C#/.NET-8-Kanon, Stopwörter, fortlaufende Dublettenerkennung |
| CR-2026-003 / ADR-0017 | ✅ ACCEPTED | Produktionscode künftig physisch in shared/frontend/indexer getrennt |
| Pflichtenheft 0.2 | ✅ ACTIVE BASELINE | WinForms/.NET 8; ersetzt widersprechende 0.1-Technikannahmen |
| A1 WinForms Host Lifecycle | 🟡 CONDITIONAL GO | 9 automatisierte Tests; vier interaktive Windows-Smokes offen |
| A2 Lucene.NET | 🟢 TECHNICAL GO | 1M-Dokument-, Realkorpus-, Crash-, Full-Disk- und Funktions-Evidence vorhanden; ADR-0005 formal weiter Proposed |
| A3 Tika Sidecar | 🟡 CONDITIONAL GO | Tika 3.3.2 Sidecar/62-MiB-Runtime/Formatprobe erfolgreich; 4.1-Vergleich, finales Packaging/SBOM/CVE/AV offen |
| A4 Media Identity | 🟡 CONDITIONAL GO | Windows-API-/Matching-Modell + 13 Tests; physische Wechselmedien-/Letter-Wechsel-Smokes offen |
| A5 Tika vs. Toxy | 🟡 CONDITIONAL / DECISION CLEAR | Tika bleibt Default; kein Toxy-Fast-Path. Real-/Legacy-DOC/PPT-/verschlüsselter Korpus offen |
| G0 | 🔴 NOT READY | technische PoCs weit fortgeschritten, aber Rest-Evidence, ADR-Reviews und .NET-8-Lifecycle-Entscheidung offen |
| C# Produktionsbasis | 🟢 FOUNDATION VERIFIED | Local Crawl + Registry + durable Multi-Stage Queue + Native TXT/HTML Extraction + produktiver Lucene-Adapter; 78 Produktions-/Architektur-/E2E-Tests grün; zählt noch nicht als G0-/0.1-Gate |
| CI | 🟢 BASELINE VERIFIED | PR #12 Code-Head: Restore/Format/Release-Build/Tests grün; 115 Tests gesamt |
| Milestone 0.1 | ⬜ BLOCKED | darf erst nach G0-GO beginnen |

## 2. Verifizierte Pre-G0-Produktionsbasis

Auf ausdrückliche Entwicklungsanweisung wurde vor dem formalen G0-GO ein **feature-begrenztes Produktionsfundament** angelegt. Das ändert den Gate-Status nicht.

Implementiert und automatisiert verifiziert:

- `src/shared/Sasd.Crawler.Domain`: `SourceId`, `SourceKind`, `SourceDefinition`;
- `src/shared/Sasd.Crawler.Application`: `ISourceCatalog`, `ISourceRepository`, Source-Use-Cases und lokale Canonical-Location-Normalisierung;
- `src/indexer/Sasd.Crawler.Persistence.Sqlite`: zentraler SQLite-Control-Store, Schema v4, WAL, Source-/Scan-/Document-Persistenz, durable Multi-Stage CrawlJob/WorkItem Queue und atomare Reconciliation;
- `src/indexer/Sasd.Crawler.Indexer.LocalFileSystem`: streamingbasierte lokale Traversierung mit Tiefenbegrenzung, Reparse-Point-Schutz und nicht-fatalen Scan-Issues;
- `src/shared/Sasd.Crawler.Application`: `LocalSourceScanService`, `IDocumentRegistry` und `ILocalFileSystemConnector` als UI-neutrale Scan-Grenzen;
- `src/shared/Sasd.Crawler.Domain`: zusätzlich `DocumentId`, `ScanRunId`, `DocumentRecord` und explizite Scan-Outcomes;
- `src/frontend/Sasd.Crawler.Frontend`: UI-neutraler MVP Presenter;
- `src/indexer/Sasd.Crawler.Indexer.Workers`: `CrawlWorkItemProcessor` + Generic-Host-`BackgroundService` für Discovery sowie durable Dokument-Index/Delete-Stufen außerhalb des UI-Threads;
- `src/indexer/Sasd.Crawler.Search.Lucene`: produktiver `ISearchIndex`-Adapter mit persistentem Upsert/Delete/Search, DE/EN-Analyzer und versionierter Stopwortkonfiguration;
- `src/indexer/Sasd.Crawler.Extraction.Native`: read-only Native-Extraction für TXT/HTML mit Größenlimit und DOM-basierter HTML-Textextraktion;
- `src/frontend/Sasd.Crawler.WinForms`: produktiver .NET-8-WinForms-Shell für Quellenliste, lokale Quelle registrieren, Aktiv/Inaktiv und `Scan now` als reinen Queue-Befehl;
- additive Developer-Views `Sasd.Crawler.Frontend.slnf` und `Sasd.Crawler.Indexer.slnf`;
- Architekturtests sichern die Frontend/shared/Indexer-Grenzen ab.

Der Lucene-Produktadapter wurde mit Post-Merge-CI #37 verifiziert: **97 Tests gesamt, 0 Fehler, 0 übersprungen** (60 Produktion/Architektur/E2E + 37 archivierte Spike-Tests). Er implementiert persistentes Upsert/Delete/Search hinter `ISearchIndex`, Source-Filter sowie symmetrische, versionierte DE/EN-Analyzer. `ja` und `nein` bleiben bewusst außerhalb der konservativen Default-Stopwortliste.

Der aktuelle TXT/HTML-Slice wurde mit CI #39 auf Commit `6ba0fcd2f1b24d18c647da64f15a6e1e14be3a9f` verifiziert: **115 Tests gesamt, 0 Fehler, 0 übersprungen** (78 Produktion/Architektur/E2E + 37 archivierte Spike-Tests). Der reale E2E-Pfad beweist TXT/HTML → durable WorkItems → Native Extraction → Lucene sowie Update und Löschung erst nach autoritativer Reconciliation.

Der erste Foundation-Slice wurde durch CI #22 verifiziert, der Local-Scan-/Registry-Slice durch CI #25. Der dritte Code-Slice wurde durch CI #28 auf Commit `dc4296caff07b70c7347bcd6e7202de20d16caf7` verifiziert: Restore, Format, Release-Build und Tests erfolgreich; **83 Tests gesamt, 0 Fehler, 0 übersprungen**. Davon gehören 46 Tests zur Produktionsstruktur und 37 zu den Architekturspikes.

Neu vorhanden sind `CrawlJob`, `WorkItem`, persistente Lease-Metadaten, eine SQLite-v3-Queue, `CrawlJobService`, `CrawlWorkItemProcessor`, ein Generic-Host-`BackgroundService` und ein WinForms-`Scan now`-Befehl, der ausschließlich durable Arbeit einreiht. Filesystem-I/O läuft nicht im UI-Thread.

Automatisches Reclaim abgelaufener Leases ist bewusst noch nicht implementiert. Ein harter Prozessabbruch kann daher ein WorkItem in `Leased` zurücklassen; das ist fail-closed und verhindert Doppelverarbeitung, erfüllt aber noch keinen vollständigen Crash-Wiederanlauf.

Die Registry-Anforderungen ID-001, ID-002, ID-003, ID-007 und ID-008 bleiben vollständig implementiert und verifiziert. Job-/Background-/Reliability-Anforderungen werden nur teilweise hochgestuft, soweit der aktuelle Slice ihre Semantik tatsächlich beweist.

Evidence:
- `docs/evidence/pre-g0-production-foundation/`
- `docs/evidence/pre-g0-local-scan-document-registry/`
- `docs/evidence/pre-g0-crawl-work-queue/`
- `docs/evidence/pre-g0-lucene-product-search-index/`
- `docs/evidence/pre-g0-native-text-html-indexing/`

## 3. Verifizierte Spike-Evidence

### A1
- WinForms + Generic Host/DI
- cancellable BackgroundService
- SQLite/WAL
- MVP/UI-Thread-Marshalling
- Tray-Code
- Single Instance + Named Pipe
- 9 automatisierte Tests

Offen: sichtbare Responsiveness, Tray-Interaktion, Second-Launch-Fokus/Restore und echter Forced-Kill/Restart.

### A2
- Lucene.NET 4.8.0-beta00018
- 100k und 1M synthetische Dokumente
- realer kleiner Repository-Korpus
- Update/Delete
- deutsche/englische Analyzer
- Phrase/Boolean/Fuzzy/Prefix, Highlighting und Filter/Facetten
- koordinierter Writer + nebenläufige Leser
- harter `Environment.FailFast` + Reopen
- simulierter Full-Disk-/Write-Failure-Fall

Messwert 1M: Query p50 173,9 ms, p95 224,5 ms; Peak Working Set 114,8 MB.

### A3
Tika 3.3.2 wurde als isolierter Sidecar mit Java 21, Loopback-Bindung, Limits, Restart, Timeout, Fehlerisolation und reduzierter 62,0-MiB-Runtime praktisch erprobt. Ein partielles Komponenten-Inventar liegt vor.

Nicht bewiesen sind insbesondere Tika 4.1 im gleichen Packaging-Modell, finales SBOM/CVE-Mapping, Signierung sowie AV/SmartScreen.

### A4
Volume-GUID/Serial/Filesystem/Capacity, `MediaId + RelativePath`, konservatives Matching und `WM_DEVICECHANGE`-Infrastruktur wurden implementiert und automatisiert getestet. Physische Wechselmedien-Smokes bleiben offen.

### A5
Der reproduzierbare Vergleich umfasst DOCX/XLSX/PPTX/PDF/HTML/RTF/TXT, Legacy-XLS, 5-MiB-TXT und ein malformed PDF. Tika bleibt Referenzparser; Toxy wird derzeit nicht in Produktcode übernommen. Reale komplexe Dokumente, Legacy DOC/PPT sowie verschlüsselte/bösartige Dateien fehlen noch.

## 4. Aktuelle Blocker vor G0

1. A1: vier interaktive Desktop-Smokes.
2. A3: Tika-4.1-vs-3.3.2-Entscheidung auf gleicher Packaging-/Security-Basis sowie finales SBOM/CVE/AV/Installer-Evidence.
3. A4: echte NTFS-/exFAT-/FAT32-Wechselmedien, Attach/Detach und Laufwerksbuchstabenwechsel.
4. A5: realer/komplexer, Legacy-DOC/PPT- und verschlüsselter Fehlerkorpus.
5. PoC-abhängige ADRs (insbesondere 0005/0006/0007/0011) formal reviewen.
6. .NET-8-Supportende am 10.11.2026: bewusstes Risiko dokumentieren und Upgradepfad definieren; C#/.NET 8 bleibt wegen Managed-PC-Kompatibilität die kanonische V1-Baseline, bis ein eigener Change Request etwas anderes beschließt.

## 5. Nächste kontrollierte Schritte

1. A1-manual-smokes nachholen.
2. A3-Rest-Evidence inklusive Tika-Produktlinie/Packaging schließen.
3. A4-Hardware-Smokes durchführen.
4. A5-Korpus-Evidence vervollständigen.
5. ADR-0005/0006/0007/0011 formal entscheiden; CR-2026-002/ADR-0016 und CR-2026-003/ADR-0017 in G0 berücksichtigen.
6. .NET-8-Support-/Upgradepfad dokumentieren, ohne die festgelegte V1-Kompatibilitätsbaseline stillschweigend zu ändern.
7. G0 final auditieren; erst nach **G0 = GO** Milestone 0.1 starten.

## 6. Fortschrittsregel

`specified ≠ implemented ≠ verified ≠ released`. Spike-Evidence beweist Architekturhypothesen, zählt aber nicht automatisch als implementierte Produktanforderung.
