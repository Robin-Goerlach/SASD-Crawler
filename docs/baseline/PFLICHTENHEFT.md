# Pflichtenheft 0.2 – SASD-Crawler WinForms/.NET 8

**Status:** DRAFT – technische Rebaselining-Fassung  
**Stand:** 21. August 2026  
**Supersedes:** Pflichtenheft 0.1 in allen widersprechenden technischen Aussagen  
**Bezug:** Lastenheft 0.1 + Amendment 0.1a + CR-2026-001 + Architektur 0.1

---

# 1. Ziel

Dieses Pflichtenheft überführt die 258 fachlichen Anforderungen in die aktuelle Desktoparchitektur:

```text
Windows Forms / .NET 8
+ Clean Architecture / MVP
+ per-user Desktopbetrieb
+ SQLite Control Store
+ Lucene.NET über ISearchIndex
+ Apache Tika Sidecar
+ Tesseract OCR
+ Local/USB/UNC/Web Connectoren
+ Durable Work Queue
+ Reconciliation als Löschautorität
```

Es ersetzt die frühere Annahme eines Blazor-/Linux-first-Hosts.

# 2. Normative Architektur

## 2.1 Primäranwendung

```text
Sasd.Crawler.WinForms.exe
```

Target:

```text
net8.0-windows
win-x64
```

## 2.2 Schichten

```text
WinForms Presentation
        ↓
Application
        ↓
Domain

Infrastructure → Application/Domain Contracts
```

## 2.3 UI-Muster

Model-View-Presenter.

Forms/UserControls dürfen keine direkte Datenbank-, Lucene-, Crawler- oder Parserlogik enthalten.

## 2.4 Daten

Führend:
- SQLite,
- Originalquellen.

Rekonstruierbar:
- Lucene,
- Extraction Cache.

## 2.5 Hintergrundarbeit

Generic Host + BackgroundServices + durable SQLite Work Queue + bounded Channels.

## 2.6 Suche

Lucene.NET ist v1-Referenzbackend, sofern PoC A2 bestanden wird.

## 2.7 Parsing

Tika Sidecar ist Referenzparser. Toxy bleibt optionaler, getesteter Fast Path.

## 2.8 OCR

Tesseract.

# 3. Desktop-Sicherheitsmodell

Die Standardinstanz läuft mit dem Windows-Benutzer.

Daraus folgt:

```text
Crawler-Leserechte = Benutzer-Leserechte
Index/Cache = Benutzerprofil
```

Keine zusätzliche lokale Benutzerverwaltung ist im Desktop-1.0 erforderlich.

Shared Mode ist ein späterer Host und nutzt dann die vorbereiteten ACL-Modelle.

# 4. Quellenumsetzung

## Local
`WindowsFileSystemConnector`

## USB
`WindowsVolumeMonitor` + `MediaRegistry`

## SMB
UNC/gemappte Windows-Pfade unter aktuellem Security Context.

## Web
persistente Frontier, robots, sitemap, rate limit, SSRF guard.

# 5. Document Registry

Jedes Dokument besitzt eine interne `DocumentId`.

Wichtige Felder:

- SourceId,
- MediaId optional,
- RelativePath,
- CanonicalLocator,
- ContentHash,
- LastSeenScanRunId,
- Availability,
- ProcessingRevision,
- Parser/OCR-Version.

# 6. Reconciliation

Nur ein vollständig erfolgreicher Scan darf fehlende Dokumente als Missing Candidates klassifizieren.

Source unavailable, USB offline oder Scan aborted:

> keine Massendeletion.

# 7. Queue

Persistente WorkItems:

- Stage,
- Status,
- Attempts,
- NextAttempt,
- LeaseOwner,
- LeaseUntil.

# 8. Extraction/OCR

Pipeline:

```text
Open Stream
→ MIME
→ Limits
→ Tika/Toxy
→ Archive/Embedded
→ OCR Decision
→ Metadata
→ Cache
→ Search Document
→ Lucene
```

# 9. WinForms Workspaces

- Suche
- Quellen
- Medien
- Indexierung
- Fehler
- Einstellungen
- Diagnose
- Hilfe

# 10. Search UI

- zentrales Suchfeld,
- Trefferliste,
- Preview Pane,
- Quelle/Typ/Datum/Verfügbarkeit,
- Offline-Medium-Anzeige,
- Original öffnen.

# 11. Scheduler

Im Desktopprozess:
- periodic jobs,
- tray,
- autostart optional,
- pause/resume.

Kein Windows Service für MVP erforderlich.

# 12. Backup/Recovery

SQLite/Config sichern.

Lucene/Cache rebuildfähig.

# 13. PoC-Vorbehalt

Die technische Baseline wird vor Milestone 0.1 durch G0 bestätigt:

- A1 WinForms Host,
- A2 Lucene,
- A3 Tika,
- A4 Media Identity,
- A5 Parservergleich.

# 14. Requirements Mapping

Die folgende Matrix enthält alle 258 Anforderungen und deren technische Zielkomponente.

| Requirement | Titel | Prio | Release | Umsetzungskomponente | Disposition 0.2 | Anmerkung |
|---|---|---|---|---|---|---|
| SRC-001 | Quellen als eigenständige Objekte | MUSS | 0.1.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-002 | Quellentypen erweiterbar | MUSS | 0.1.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-003 | Quelle aktivieren/deaktivieren | MUSS | 0.1.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-004 | Vollständiger Neuaufbau pro Quelle | MUSS | 0.1.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-005 | Inkrementelle Aktualisierung | MUSS | 0.1.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-006 | Include-Regeln | MUSS | 0.1.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-007 | Exclude-Regeln | MUSS | 0.1.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-008 | Quellspezifische Größenlimits | SOLL | 0.4.0 | Connector Framework / Source Application Services | UNCHANGED | – |
| SRC-009 | Quellspezifische Priorität | KANN | 1.4.x | Connector Framework / Source Application Services | UNCHANGED | – |
| LOC-001 | Lokale Verzeichnisse | MUSS | 0.1.0 | WindowsFileSystemConnector | UNCHANGED | – |
| LOC-002 | Rekursive Traversierung | MUSS | 0.1.0 | WindowsFileSystemConnector | UNCHANGED | – |
| LOC-003 | Rekursion begrenzbar | MUSS | 0.1.0 | WindowsFileSystemConnector | UNCHANGED | – |
| LOC-004 | Symbolische Links/Junctions | MUSS | 0.4.0 | WindowsFileSystemConnector | UNCHANGED | – |
| LOC-005 | Dateisystemfehler | MUSS | 0.1.0 | WindowsFileSystemConnector | UNCHANGED | – |
| LOC-006 | Pfadänderungen | MUSS | 0.4.0 | WindowsFileSystemConnector | UNCHANGED | – |
| USB-001 | Wechseldatenträger als eigene Quelle | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-002 | Stabile Medienidentität | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-003 | Offline-Status | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-004 | Offline-Treffer erhalten | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-005 | Offline-Treffer kennzeichnen | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-006 | Medienname anzeigen | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-007 | Wiederanschluss erkennen | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-008 | Fehlendes Medium ist keine Löschung | MUSS | 0.2.0 | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| USB-009 | Portable Indizes | KANN | 1.2.x | WindowsVolumeMonitor + MediaRegistry | UNCHANGED | – |
| SMB-001 | SMB/CIFS-Freigaben | MUSS | 0.2.0 | Windows/UNC NetworkDirectory Connector | UNCHANGED | – |
| SMB-002 | Gemappte Netzlaufwerke | MUSS | 0.2.0 | Windows/UNC NetworkDirectory Connector | UNCHANGED | – |
| SMB-003 | UNC-/Netzpfade | MUSS | 0.2.0 | Windows/UNC NetworkDirectory Connector | UNCHANGED | – |
| SMB-004 | Verbindungsunterbrechung | MUSS | 0.2.0 | Windows/UNC NetworkDirectory Connector | UNCHANGED | – |
| SMB-005 | Wiederholungsstrategie | MUSS | 0.7.0 | Windows/UNC NetworkDirectory Connector | UNCHANGED | – |
| SMB-006 | Verbindungs- und Lastbegrenzung | SOLL | 0.7.0 | Windows/UNC NetworkDirectory Connector | UNCHANGED | – |
| SMB-007 | Quellberechtigungen | MUSS | 0.6.0 | Windows/UNC NetworkDirectory Connector | UNCHANGED | – |
| WEB-001 | HTTP/HTTPS | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-002 | Start-URLs | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-003 | Domain-/Host-Grenzen | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-004 | Include-/Exclude-URL-Regeln | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-005 | Crawl-Tiefe | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-006 | URL-Normalisierung | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-007 | Canonical URL | SOLL | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-008 | Redirects | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-009 | robots.txt | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-010 | Sitemap | SOLL | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-011 | Rate Limiting | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-012 | User-Agent | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-013 | Timeouts | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-014 | HTTP-Fehler | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-015 | Web-Löschung | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-016 | Crawl-Traps | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-017 | Verlinkte Dokumente | MUSS | 0.3.0 | WebCrawlerConnector | UNCHANGED | – |
| WEB-018 | Authentisierte Websites | SOLL | 1.3.x | WebCrawlerConnector | UNCHANGED | – |
| WEB-019 | JavaScript-renderte SPAs | KANN | 1.3.x | WebCrawlerConnector | UNCHANGED | – |
| EXT-001 | Textdateien | MUSS | 0.1.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-002 | HTML | MUSS | 0.1.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-003 | PDF | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-004 | DOCX | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-005 | DOC | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-006 | XLSX | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-007 | XLS | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-008 | Excel-Blätter | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-009 | PPTX | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-010 | PPT | SOLL | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-011 | OpenDocument | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-012 | RTF | SOLL | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-013 | EPUB | SOLL | 1.2.x | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-014 | E-Mail-Dateien | SOLL | 1.2.x | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-015 | PST/OST | KANN | 1.3.x | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-016 | Unbekannte Binärformate | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| EXT-017 | Parseridentität | MUSS | 0.4.0 | Tika/Toxy Content Extraction Pipeline | UNCHANGED | – |
| ARC-001 | ZIP | MUSS | 0.4.0 | Archive/Embedded Processor | UNCHANGED | – |
| ARC-002 | Archivpfad | MUSS | 0.4.0 | Archive/Embedded Processor | UNCHANGED | – |
| ARC-003 | Rekursion begrenzen | MUSS | 0.4.0 | Archive/Embedded Processor | UNCHANGED | – |
| ARC-004 | Zip-Bomb-Schutz | MUSS | 0.4.0 | Archive/Embedded Processor | UNCHANGED | – |
| ARC-005 | Weitere Archive | SOLL | 1.2.x | Archive/Embedded Processor | UNCHANGED | – |
| ARC-006 | Passwortgeschützte Archive | KANN | 1.3.x | Archive/Embedded Processor | UNCHANGED | – |
| OCR-001 | Scan-PDF erkennen | MUSS | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-002 | OCR für Scan-PDF | MUSS | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-003 | Bilder | SOLL | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-004 | Sprachen | MUSS | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-005 | Mehrsprachigkeit | MUSS | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-006 | OCR nicht unnötig ausführen | MUSS | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-007 | OCR-Metadaten | MUSS | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-008 | OCR wiederholen | SOLL | 0.7.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| OCR-009 | OCR-Ressourcenlimit | MUSS | 0.5.0 | Tesseract OCR Pipeline | UNCHANGED | – |
| ID-001 | DocumentId | MUSS | 0.1.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-002 | SourceId | MUSS | 0.1.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-003 | Canonical URI | MUSS | 0.1.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-004 | MediaId | MUSS | 0.2.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-005 | Relative Path | MUSS | 0.2.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-006 | Content Hash | MUSS | 0.4.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-007 | Zeitstempel und Größe | MUSS | 0.1.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-008 | LastSeen | MUSS | 0.1.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-009 | Availability | MUSS | 0.2.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-010 | Rename-/Move-Erkennung | SOLL | 0.4.0 | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| ID-011 | Dublettenerkennung | SOLL | 1.1.x | DocumentRegistry + ReconciliationService | UNCHANGED | – |
| META-001 | Basisfelder | MUSS | 0.1.0 | MetadataNormalizer | UNCHANGED | – |
| META-002 | Dokumenttitel | MUSS | 0.4.0 | MetadataNormalizer | UNCHANGED | – |
| META-003 | Autor | SOLL | 0.4.0 | MetadataNormalizer | UNCHANGED | – |
| META-004 | Erstellungsdatum | SOLL | 0.4.0 | MetadataNormalizer | UNCHANGED | – |
| META-005 | Sprache | SOLL | 0.5.0 | MetadataNormalizer | UNCHANGED | – |
| META-006 | EXIF | SOLL | 1.2.x | MetadataNormalizer | UNCHANGED | – |
| META-007 | GPS | KANN | 1.2.x | MetadataNormalizer | UNCHANGED | – |
| META-008 | Benutzerdefinierte Felder | SOLL | 1.2.x | MetadataNormalizer | UNCHANGED | – |
| IDX-001 | Persistenter Inhaltsindex | MUSS | 0.1.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| IDX-002 | Inkrementelle Updates | MUSS | 0.1.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| IDX-003 | Löschung | MUSS | 0.1.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| IDX-004 | Offline versus gelöscht | MUSS | 0.2.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| IDX-005 | Indexversion | MUSS | 0.4.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| IDX-006 | Reindex nach Parserupgrade | MUSS | 0.7.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| IDX-007 | Konsistenzprüfung | MUSS | 0.8.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| IDX-008 | Recovery | MUSS | 0.9.0 | ISearchIndex + Lucene.NET | UNCHANGED | – |
| SEA-001 | Stichwortsuche | MUSS | 0.1.0 | SearchApplicationService | UNCHANGED | – |
| SEA-002 | Phrase Search | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-003 | AND | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-004 | OR | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-005 | NOT | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-006 | Feldsuche | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-007 | Relevanz | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-008 | Sortierung | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-009 | Fuzzy Search | SOLL | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-010 | Prefix/Wildcard | SOLL | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-011 | Facetten | MUSS | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-012 | Sprache | SOLL | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-013 | Autor | SOLL | 0.5.0 | SearchApplicationService | UNCHANGED | – |
| SEA-014 | Synonyme | SOLL | 1.1.x | SearchApplicationService | UNCHANGED | – |
| SEA-015 | Spellcheck | SOLL | 1.1.x | SearchApplicationService | UNCHANGED | – |
| SEA-016 | Autocomplete | SOLL | 1.1.x | SearchApplicationService | UNCHANGED | – |
| SEA-017 | Gespeicherte Suchen | SOLL | 1.1.x | SearchApplicationService | UNCHANGED | – |
| SEA-018 | Suchhistorie | KANN | 1.1.x | SearchApplicationService | UNCHANGED | – |
| SEA-019 | Related/Similar | SOLL | 1.5.x | SearchApplicationService | UNCHANGED | – |
| SEA-020 | Result Pinning | SOLL | 1.4.x | SearchApplicationService | UNCHANGED | – |
| SEA-021 | Learning to Rank | KANN | 1.5.x | SearchApplicationService | UNCHANGED | – |
| UI-001 | Zentrales Suchfeld | MUSS | 0.1.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-002 | Search-as-you-type Reaktion | SOLL | 0.5.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-003 | Treffername | MUSS | 0.1.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-004 | Pfad/URL | MUSS | 0.1.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-005 | Quelle | MUSS | 0.1.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-006 | Snippet | MUSS | 0.5.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-007 | Highlighting | MUSS | 0.5.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-008 | Original öffnen | MUSS | 0.1.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-009 | Offline-Information | MUSS | 0.2.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-010 | Vorschau | MUSS | 0.5.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-011 | PDF-/Bildthumbnail | SOLL | 1.1.x | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-012 | Tastaturbedienung | MUSS | 0.9.0 | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| UI-013 | Responsive Weboberfläche | SOLL | 1.0.0 | WinForms Workspaces + MVP Presenters | AMENDED | WinForms UI statt Web UI |
| UI-014 | Desktopintegration | KANN | 1.1.x | WinForms Workspaces + MVP Presenters | UNCHANGED | – |
| AUTH-001 | Single-User-Betrieb | MUSS | 0.1.0 | WindowsIdentity/Desktop Security + future ACL abstraction | UNCHANGED | – |
| AUTH-002 | Benutzerkonten | MUSS | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-003 | Rollen | MUSS | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-004 | Gruppen | MUSS | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-005 | Source ACL | MUSS | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-006 | Security Trimming | MUSS | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-007 | Snippet-Schutz | MUSS | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-008 | Deny vor Allow | MUSS | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-009 | ACL-Fingerprint | SOLL | 0.6.0 | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-010 | LDAP/Active Directory | SOLL | 1.3.x | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-011 | OIDC/SSO | SOLL | 1.3.x | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| AUTH-012 | TOTP/MFA | KANN | 1.3.x | WindowsIdentity/Desktop Security + future ACL abstraction | REINTERPRETED | Desktop per-user; Shared ACL später |
| ADM-001 | Administrationsoberfläche | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-002 | Crawl starten | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-003 | Crawl pausieren | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-004 | Crawl fortsetzen | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-005 | Zeitpläne | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-006 | Fehlerliste | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-007 | Retry | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-008 | Failure Queue | SOLL | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-009 | Statistik | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| ADM-010 | Throttling | MUSS | 0.7.0 | WinForms Admin/Jobs/Failures/Diagnostics | UNCHANGED | – |
| API-001 | Search API | MUSS | 0.7.0 | Application Contracts + CLI/optional Loopback API | REINTERPRETED | Application Contract/CLI; HTTP optional |
| API-002 | Source API | SOLL | 0.7.0 | Application Contracts + CLI/optional Loopback API | REINTERPRETED | Application Contract/CLI; HTTP optional |
| API-003 | Job API | SOLL | 0.7.0 | Application Contracts + CLI/optional Loopback API | REINTERPRETED | Application Contract/CLI; HTTP optional |
| API-004 | Status API | MUSS | 0.7.0 | Application Contracts + CLI/optional Loopback API | REINTERPRETED | Application Contract/CLI; HTTP optional |
| API-005 | Bulk Ingest | SOLL | 1.3.x | Application Contracts + CLI/optional Loopback API | REINTERPRETED | Application Contract/CLI; HTTP optional |
| API-006 | Webhooks | SOLL | 1.4.x | Application Contracts + CLI/optional Loopback API | REINTERPRETED | Application Contract/CLI; HTTP optional |
| API-007 | CLI | MUSS | 0.7.0 | Application Contracts + CLI/optional Loopback API | REINTERPRETED | Application Contract/CLI; HTTP optional |
| BAK-001 | Konfigurationsbackup | MUSS | 0.9.0 | Backup/Restore/Rebuild Services | UNCHANGED | – |
| BAK-002 | Konfigurationsrestore | MUSS | 0.9.0 | Backup/Restore/Rebuild Services | UNCHANGED | – |
| BAK-003 | Indexbackup | SOLL | 0.9.0 | Backup/Restore/Rebuild Services | UNCHANGED | – |
| BAK-004 | Rebuild statt Backup möglich | MUSS | 0.9.0 | Backup/Restore/Rebuild Services | UNCHANGED | – |
| BAK-005 | Datenmigration | MUSS | 0.9.0 | Backup/Restore/Rebuild Services | UNCHANGED | – |
| BAK-006 | Rollback | SOLL | 0.9.0 | Backup/Restore/Rebuild Services | UNCHANGED | – |
| ORG-001 | Favoriten | SOLL | 1.1.x | Personal Search Metadata | UNCHANGED | – |
| ORG-002 | Benutzertags | SOLL | 1.2.x | Personal Search Metadata | UNCHANGED | – |
| ORG-003 | Notizen | KANN | 1.2.x | Personal Search Metadata | UNCHANGED | – |
| ORG-004 | Thesaurus | SOLL | 1.1.x | Personal Search Metadata | UNCHANGED | – |
| ORG-005 | Populäre Suchbegriffe | KANN | 1.4.x | Personal Search Metadata | UNCHANGED | – |
| ORG-006 | Suchalarme | SOLL | 1.4.x | Personal Search Metadata | UNCHANGED | – |
| ORG-007 | Benachrichtigungskanäle | KANN | 1.4.x | Personal Search Metadata | UNCHANGED | – |
| ORG-008 | Query-Tuning | SOLL | 1.4.x | Personal Search Metadata | UNCHANGED | – |
| ORG-009 | Search Analytics | SOLL | 1.4.x | Personal Search Metadata | UNCHANGED | – |
| SEM-001 | Similar Documents | SOLL | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| SEM-002 | Named Entity Recognition | SOLL | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| SEM-003 | Entity Linking | KANN | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| SEM-004 | Automatische Klassifikation | SOLL | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| SEM-005 | Embeddings | SOLL | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| SEM-006 | Vektorsuche | SOLL | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| SEM-007 | Hybridsuche | SOLL | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| SEM-008 | Semantik abschaltbar | MUSS | 1.5.x | Semantic Extension Layer | UNCHANGED | – |
| AI-001 | RAG | SOLL | 2.0.0 | Optional AI/RAG Extension | UNCHANGED | – |
| AI-002 | Quellenbezug | MUSS | 2.0.0 | Optional AI/RAG Extension | UNCHANGED | – |
| AI-003 | ACL vor Kontextbildung | MUSS | 2.0.0 | Optional AI/RAG Extension | UNCHANGED | – |
| AI-004 | Zusammenfassungen | SOLL | 2.0.0 | Optional AI/RAG Extension | UNCHANGED | – |
| AI-005 | Knowledge Graph | KANN | 2.0.0 | Optional AI/RAG Extension | UNCHANGED | – |
| AI-006 | Lokales KI-Backend | SOLL | 2.0.0 | Optional AI/RAG Extension | UNCHANGED | – |
| AI-007 | Externe KI nur opt-in | MUSS | 2.0.0 | Optional AI/RAG Extension | UNCHANGED | – |
| CON-001 | SFTP | SOLL | 1.3.x | Future Connector SDK | UNCHANGED | – |
| CON-002 | FTP/FTPS | KANN | 1.3.x | Future Connector SDK | UNCHANGED | – |
| CON-003 | WebDAV | SOLL | 1.3.x | Future Connector SDK | UNCHANGED | – |
| CON-004 | S3-kompatibler Objektspeicher | SOLL | 1.3.x | Future Connector SDK | UNCHANGED | – |
| CON-005 | Nextcloud | KANN | 1.3.x | Future Connector SDK | UNCHANGED | – |
| CON-006 | SharePoint | KANN | 2.x | Future Connector SDK | UNCHANGED | – |
| CON-007 | Confluence/Jira | KANN | 2.x | Future Connector SDK | UNCHANGED | – |
| CON-008 | Git-Repositories | KANN | 2.x | Future Connector SDK | UNCHANGED | – |
| PERF-001 | Interaktive Suche | MUSS | 0.8.0 | Performance Harness | UNCHANGED | – |
| PERF-002 | Skalierungsstufen | MUSS | 0.8.0 | Performance Harness | UNCHANGED | – |
| PERF-003 | Hintergrundarbeit | MUSS | 0.8.0 | Performance Harness | UNCHANGED | – |
| PERF-004 | Ressourcenlimits | MUSS | 0.7.0 | Performance Harness | UNCHANGED | – |
| REL-001 | Einzeldateifehler isolieren | MUSS | 0.1.0 | Durable Queue + State Machines | UNCHANGED | – |
| REL-002 | Netzwerkfehler isolieren | MUSS | 0.2.0 | Durable Queue + State Machines | UNCHANGED | – |
| REL-003 | Wiederanlauf | MUSS | 0.8.0 | Durable Queue + State Machines | UNCHANGED | – |
| REL-004 | Keine Massendeletion bei Unsicherheit | MUSS | 0.2.0 | Durable Queue + State Machines | UNCHANGED | – |
| REL-005 | Checkpoint | SOLL | 0.7.0 | Durable Queue + State Machines | UNCHANGED | – |
| SEC-001 | Least Privilege | MUSS | 0.1.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| SEC-002 | Parserisolation | MUSS | 0.4.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| SEC-003 | Größenlimits | MUSS | 0.4.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| SEC-004 | Keine Credential-Protokollierung | MUSS | 0.2.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| SEC-005 | Secrets geschützt speichern | MUSS | 0.6.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| SEC-006 | Transportverschlüsselung | MUSS | 0.6.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| SEC-007 | Audit administrativer Änderungen | MUSS | 0.7.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| SEC-008 | Webcrawler SSRF-Schutz | MUSS | 0.3.0 | Parser/Web/Local Security Controls | UNCHANGED | – |
| PRIV-001 | Keine notwendige Cloudübertragung | MUSS | 1.0.0 | Per-user Data Boundary | UNCHANGED | – |
| PRIV-002 | Externe Dienste opt-in | MUSS | 1.0.0 | Per-user Data Boundary | UNCHANGED | – |
| PRIV-003 | Suchhistorie deaktivierbar | MUSS | 1.1.x | Per-user Data Boundary | UNCHANGED | – |
| PRIV-004 | Retention für Logs | SOLL | 0.9.0 | Per-user Data Boundary | UNCHANGED | – |
| PLAT-001 | Windows | MUSS | 1.0.0 | Windows x64 Packaging | UNCHANGED | – |
| PLAT-002 | Linux | MUSS | 1.0.0 | Windows x64 Packaging | AMENDED | Linux 1.0 entfällt; späterer Service-Host |
| PLAT-003 | Containerbetrieb | SOLL | 1.0.0 | Windows x64 Packaging | AMENDED | Container nur späterer Shared/Service Mode |
| PLAT-004 | macOS | KANN | 2.x | Windows x64 Packaging | UNCHANGED | – |
| UX-001 | Einfache Erstsuche | MUSS | 0.5.0 | WinForms Accessibility/High-DPI | UNCHANGED | – |
| UX-002 | Technische Details optional | MUSS | 0.5.0 | WinForms Accessibility/High-DPI | UNCHANGED | – |
| UX-003 | Fehler verständlich | MUSS | 0.7.0 | WinForms Accessibility/High-DPI | UNCHANGED | – |
| UX-004 | Accessibility | MUSS | 0.9.0 | WinForms Accessibility/High-DPI | UNCHANGED | – |
| UX-005 | Internationalisierung | SOLL | 1.1.x | WinForms Accessibility/High-DPI | UNCHANGED | – |
| OPS-001 | Health | MUSS | 0.7.0 | Logs/Metrics/Diagnostics | UNCHANGED | – |
| OPS-002 | Strukturierte Logs | MUSS | 0.7.0 | Logs/Metrics/Diagnostics | UNCHANGED | – |
| OPS-003 | Log-Level | MUSS | 0.7.0 | Logs/Metrics/Diagnostics | UNCHANGED | – |
| OPS-004 | Metriken | SOLL | 0.8.0 | Logs/Metrics/Diagnostics | UNCHANGED | – |
| OPS-005 | Monitoringintegration | SOLL | 0.8.0 | Logs/Metrics/Diagnostics | UNCHANGED | – |
| OPS-006 | Keine personenbezogenen Volltexte in Standardlogs | MUSS | 0.7.0 | Logs/Metrics/Diagnostics | UNCHANGED | – |
| QRY-001 | Reproduzierbarer Testkorpus | MUSS | 0.4.0 | Golden Query Harness | UNCHANGED | – |
| QRY-002 | Deutsch | MUSS | 0.5.0 | Golden Query Harness | UNCHANGED | – |
| QRY-003 | Englisch | MUSS | 0.5.0 | Golden Query Harness | UNCHANGED | – |
| QRY-004 | Unicode | MUSS | 0.5.0 | Golden Query Harness | UNCHANGED | – |
| QRY-005 | Rankingtest | MUSS | 0.8.0 | Golden Query Harness | UNCHANGED | – |
| QRY-006 | Precision/Recall-Benchmark | SOLL | 0.8.0 | Golden Query Harness | UNCHANGED | – |
| PRE-001 | Textvorschau | MUSS | 0.5.0 | Safe Preview Service | UNCHANGED | – |
| PRE-002 | Keine Makroausführung | MUSS | 0.5.0 | Safe Preview Service | UNCHANGED | – |
| PRE-003 | Original öffnen | MUSS | 0.1.0 | Safe Preview Service | UNCHANGED | – |
| PRE-004 | Weblink | MUSS | 0.3.0 | Safe Preview Service | UNCHANGED | – |
| PRE-005 | Thumbnail | SOLL | 1.1.x | Safe Preview Service | UNCHANGED | – |

# 15. Release-Umsetzung

## 0.1
Local vertical slice.

## 0.2
USB/SMB.

## 0.3
Web.

## 0.4
Office/PDF/Archive.

## 0.5
OCR + vollständige Kernsuche = MVP.

## 0.6
Desktop Security + Shared-Mode-Abstraktionen.

## 0.7
Scheduler/Jobs/Failures/Diagnostics.

## 0.8
Performance/Consistency/Telemetry.

## 0.9
Installer/Backup/Migration/Accessibility/RC.

## 1.0
Stable.

# 16. Definition of Done

Eine Requirement gilt nicht als erfüllt, bevor:

- Code vorhanden,
- automatisierter oder dokumentierter manueller Test vorhanden,
- Evidence vorhanden,
- Requirement-Status aktualisiert,
- Security/Recovery-Folgen geprüft sind.

# 17. Schlussfolgerung

Pflichtenheft 0.2 ist die technische Desktop-Rebaselining-Fassung.

Die ausführliche Komponenten- und Klassensicht verbleibt im Architekturdokument 0.1. Dieses Pflichtenheft stellt sicher, dass die 258 fachlichen Anforderungen auf die neue Windows-Forms-/NET-8-Baseline abgebildet sind, ohne die alte Blazor-/Linux-Architektur weiterzutragen.
