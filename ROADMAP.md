# SASD-Crawler – Roadmap

**Stand:** 21. August 2026  
**Roadmap-Version:** 0.1  
**Ziel:** kontrollierbare Entwicklung vom Architektur-Spike bis zur stabilen Version 1.0 und danach  
**Aktueller Status:** Vorentwicklung / Architekturvalidierung – A1 technisch abgeschlossen  
**Aktuelles Gate:** A2 – Lucene.NET Spike

---

# 1. Zweck der Roadmap

Diese Datei ist das zentrale Steuerungsdokument für den SASD-Crawler.

Sie soll zu jedem Zeitpunkt beantworten:

1. Wo stehen wir?
2. Was ist als Nächstes zu tun?
3. Was blockiert die nächste Stufe?
4. Welche Anforderungen gehören in welchen Meilenstein?
5. Welche Nachweise müssen existieren, bevor ein Meilenstein als abgeschlossen gilt?
6. Welche Architekturentscheidungen sind noch hypothetisch und welche praktisch bestätigt?
7. Welche Arbeiten dürfen bewusst **noch nicht** begonnen werden?
8. Welche späteren Funktionen sind geplant, ohne den MVP zu überladen?

Die Roadmap unterscheidet strikt zwischen:

- **Spezifikation:** etwas ist beschrieben;
- **Implementierung:** Code existiert;
- **Verifikation:** Tests/Nachweise belegen die Funktion;
- **Release:** alle Gates sind erfüllt.

---

# 2. Statusmodell

| Status | Bedeutung |
|---|---|
| `NOT STARTED` | keine verifizierte Umsetzung |
| `READY` | Entry-Kriterien erfüllt, Arbeit darf beginnen |
| `IN PROGRESS` | Umsetzung aktiv |
| `BLOCKED` | externer/technischer Blocker |
| `READY FOR REVIEW` | technische Arbeit beendet, formale Prüfung offen |
| `DONE` | Exit-Kriterien + Evidence erfüllt |
| `DEFERRED` | bewusst später |
| `REJECTED` | bewusst nicht Bestandteil |

## 2.1 Fortschrittsregel

Ein Punkt ist **nicht DONE**, nur weil:

- eine Markdown-Datei existiert,
- Code kompiliert,
- ein manueller Happy Path einmal funktioniert,
- Codex meldet „fertig“.

DONE verlangt die in dieser Roadmap genannten Nachweise.

---

# 3. Aktueller Stand

## 3.1 Bereits erarbeitet

| Artefakt | Status |
|---|---|
| Produkt-/Funktionsanalyse | DONE |
| auditierte Produktanalyse | DONE |
| Lastenheft 0.1 | DONE als fachliche Baseline |
| Pflichtenheft 0.1 | DONE als ältere technische Spezifikation, aber revisionsbedürftig |
| WinForms/.NET-8-Architektur 0.1 | DONE als Architekturentwurf |
| Baseline-/Change-Control | DONE |
| Roadmap | DONE |
| PoC-Plan | DONE als Plan |
| Teststrategie | DONE als Plan |
| Risk Register | DONE als initiale Baseline |
| ADR-Grundsatzentscheidungen | CREATED, Review ausstehend |

## 3.2 Noch nicht als umgesetzt zu betrachten

- Repository-Solution;
- WinForms-Shell;
- SQLite-Schema;
- Lucene-Index;
- Dateisystemcrawler;
- USB-Detection;
- SMB;
- Webcrawler;
- Tika-Integration;
- OCR;
- Search UI;
- Installer;
- automatisierte E2E-Tests.

## 3.3 Aktuelle kritische Lücke

Das Pflichtenheft enthält noch .NET-10/Blazor-/Linux-/Shared-Server-Annahmen. Die neue Architektur definiert WinForms/.NET 8/Windows-first.

**Vor Milestone 0.1 MUSS diese Divergenz formal geschlossen werden.**

---

# 4. Gesamtmeilensteine

| Milestone | Ziel | Status | Gate danach |
|---|---|---|---|
| **0.0.0** | Dokument- und Baseline-Vorbereitung | IN PROGRESS | – |
| **0.0.1** | WinForms Host Lifecycle Spike | READY FOR REVIEW | A1: CONDITIONAL GO |
| **0.0.2** | Lucene.NET Spike | NOT STARTED | A2 |
| **0.0.3** | Tika Sidecar/Packaging Spike | NOT STARTED | A3 |
| **0.0.4** | Windows Media Identity Spike | NOT STARTED | A4 |
| **0.0.5** | Tika-vs-Toxy Parser Benchmark | NOT STARTED | **G0** |
| **0.1.0** | lokaler vertikaler Slice | NOT STARTED | G1 |
| **0.2.0** | USB/Offline + SMB | NOT STARTED | G2 |
| **0.3.0** | Webcrawler | NOT STARTED | G3 |
| **0.4.0** | Office/PDF/Archive/Metadaten | NOT STARTED | G4 |
| **0.5.0** | OCR + vollständige Kernsuche = MVP | NOT STARTED | **G-MVP** |
| **0.6.0** | Desktop-Security-Härtung + Shared-Mode-Vorbereitung | NOT STARTED | G6 |
| **0.7.0** | Scheduler, Jobs, Fehlerzentrum, Diagnose | NOT STARTED | G7 |
| **0.8.0** | Performance, Konsistenz, Telemetrie | NOT STARTED | G8 |
| **0.9.0** | Packaging, Migration, Recovery, Accessibility, RC | NOT STARTED | **G-RC** |
| **1.0.0** | Stable | NOT STARTED | **G-1.0** |
| **1.1.x** | Suchkomfort | FUTURE | |
| **1.2.x** | reichere Metadaten/Inhalte | FUTURE | |
| **1.3.x** | Enterprise-Connectoren/Identity | FUTURE | |
| **1.4.x** | Search Operations/Analytics | FUTURE | |
| **1.5.x** | Semantik/Vektor | FUTURE | |
| **2.0.0** | optionale RAG/AI-Recherche | FUTURE | |

---

# 5. Gate G0 – Architecture Feasibility Gate

G0 ist der wichtigste Vorentwicklungs-Gate.

Ohne G0 dürfen wir keinen großen Produktionscode schreiben.

## 5.1 Voraussetzungen

- [x] A1 WinForms/Generic Host technisch erfolgreich; manuelle UI-/Crash-Evidence vor finalem Review nachholen.
- [ ] A2 Lucene.NET erfolgreich oder Fallback auf OpenSearch beschlossen.
- [ ] A3 Tika Packaging/Isolation erfolgreich.
- [ ] A4 Volume Identity ausreichend belastbar.
- [ ] A5 Parservergleich dokumentiert.
- [ ] Pflichtenheft 0.2 oder äquivalente Änderungsmatrix erstellt.
- [ ] ADR-0001 bis ADR-0014 reviewed.
- [ ] .NET-8-Lifecycle-Risiko explizit akzeptiert oder Migrationsplan beschlossen.
- [ ] Repository-/Solution-Konventionen definiert.
- [ ] CI-Minimum definiert.

## 5.2 Go-Kriterien

**GO**, wenn:

- kein Architekturblocker übrig ist;
- jeder kritische PoC einen nachvollziehbaren Evidence-Bericht hat;
- der technische Stack für den 0.1-Slice feststeht;
- Fallbackentscheidungen dokumentiert sind.

**NO-GO**, wenn zum Beispiel:

- Lucene.NET instabil ist und kein akzeptabler Backendfallback beschlossen wurde;
- Tika nicht sicher paketierbar ist;
- Volume Identity das zentrale Offline-Medienversprechen nicht erfüllen kann;
- WinForms/Host-Lifecycle zu inkonsistent ist.

## 5.3 Evidence

Unter einem zukünftigen Repositorypfad:

```text
docs/evidence/0.0/
  A1-winforms-host.md
  A2-lucene.md
  A3-tika.md
  A4-media-identity.md
  A5-parser-benchmark.md
  G0-decision.md
```

---

# 6. Milestone 0.0.0 – Dokument- und Baseline-Vorbereitung

**Zweck:** Vor dem Coding dafür sorgen, dass Anforderungen und Architektur nicht gegeneinander arbeiten.

## 6.1 Deliverables

- [x] Produktanalyse.
- [x] Lastenheft.
- [x] Pflichtenheft 0.1.
- [x] WinForms/.NET-8-Architektur.
- [x] Roadmap.
- [x] Teststrategie als Plan.
- [x] Risk Register als Plan.
- [x] Baseline-/Change-Control.
- [x] initiale ADRs.
- [x] Pflichtenheft 0.2 WinForms/.NET 8 als Draft erstellt.
- [x] Change Request CR-2026-001 als Draft erstellt.
- [x] Lastenheft-Amendment 0.1a als Draft erstellt.
- [ ] CR-2026-001 + Amendment 0.1a + Pflichtenheft 0.2 formal reviewed/angenommen.
- [ ] finaler Dokumentreview.

## 6.2 Exit-Kriterium

Der Meilenstein wird erst DONE, wenn es **keine ungeklärte normative Kollision** zwischen Lastenheft, Pflichtenheft und Architektur gibt.

---

# 7. Milestone 0.0.1 – A1 WinForms Host Lifecycle Spike

**Ziel:** beweisen, dass WinForms + Generic Host + BackgroundService + SQLite + Tray + Shutdown sauber zusammenarbeiten.

## 7.1 Umfang

Minimalprojekt:

```text
WinForms MainForm
  + Generic Host
  + DI
  + BackgroundService
  + SQLite
  + Tray Icon
  + Cancellation
  + graceful shutdown
```

## 7.2 Muss-Szenarien

- [ ] App startet ohne UI-Blockade.
- [ ] BackgroundService startet.
- [ ] Service schreibt periodisch Status in SQLite.
- [ ] UI liest Status über Application Service.
- [ ] UI bleibt responsive.
- [ ] Minimize-to-Tray funktioniert.
- [ ] App kann aus Tray geöffnet werden.
- [ ] Shutdown cancelt Worker.
- [ ] SQLite bleibt konsistent.
- [ ] erzwungener Crash hinterlässt beim Neustart reparierbaren Zustand.
- [ ] Single-Instance-Mutex funktioniert.
- [ ] zweite Instanz kann bestehende Instanz aktivieren.

## 7.3 Nicht im Spike

- kein Crawler,
- kein Lucene,
- kein Tika,
- kein „schönes“ finales UI.

## 7.4 Gate A1

GO, wenn Lifecycle und Shutdown reproduzierbar stabil sind.

---

# 8. Milestone 0.0.2 – A2 Lucene.NET Spike

**Ziel:** Embedded Search auf .NET 8 technisch absichern.

## 8.1 Testumfang

- [ ] `net8.0-windows` build.
- [ ] Index create/open.
- [ ] 100.000 synthetische Dokumente.
- [ ] 1.000.000 synthetische Dokumente.
- [ ] UpdateDocument.
- [ ] Delete.
- [ ] phrase search.
- [ ] Boolean.
- [ ] fuzzy.
- [ ] wildcard/prefix.
- [ ] German analyzer.
- [ ] English analyzer.
- [ ] highlight.
- [ ] faceting/filter.
- [ ] concurrent readers.
- [ ] koordinierter einzelner Writer.
- [ ] process kill während Updates.
- [ ] reopen/recovery.
- [ ] full rebuild.
- [ ] Speicherbedarf.
- [ ] Indexgröße.
- [ ] p50/p95 Query-Latenz.

## 8.2 Entscheidung

### GO Lucene
Lucene bleibt v1-Backend.

### CONDITIONAL
Lucene nur für Desktop, OpenSearch für Shared/Vector später.

### NO-GO
OpenSearch wird früher primäres Backend.

## 8.3 Wichtig

Die Entscheidung wird nicht nach „funktioniert bei 100 Dateien“ getroffen.

---

# 9. Milestone 0.0.3 – A3 Tika Sidecar/Packaging

**Ziel:** sicheren Dokumentparserbetrieb aus der WinForms-Anwendung beweisen.

## 9.1 Spike-Funktionen

- [ ] Tika-Prozess starten.
- [ ] Health prüfen.
- [ ] lokalen IPC/HTTP-Endpunkt verwenden.
- [ ] DOCX extrahieren.
- [ ] XLSX extrahieren.
- [ ] PPTX extrahieren.
- [ ] PDF extrahieren.
- [ ] Metadaten erhalten.
- [ ] Timeout erzwingen.
- [ ] hängenden Parser beenden.
- [ ] Parser neu starten.
- [ ] keine LAN-Bindung.
- [ ] Logcapture.
- [ ] JRE-Version erfassen.
- [ ] Packaging-Größe messen.
- [ ] Lizenz-/SBOM-Folgen dokumentieren.
- [ ] Antiviren-/SmartScreen-Verhalten beobachten.

## 9.2 Gate A3

Tika darf den WinForms-Hauptprozess bei Parserfehlern nicht mitreißen.

---

# 10. Milestone 0.0.4 – A4 Windows Media Identity

**Ziel:** das zentrale Offline-Medienversprechen praktisch validieren.

## 10.1 Geräte

Mindestens:

- NTFS USB-Stick/SSD,
- exFAT,
- FAT32,
- zwei Medien mit gleichem Label.

## 10.2 Testfälle

- [ ] Volume GUID/Serial lesen.
- [ ] interne MediaId erzeugen.
- [ ] Medium entfernen.
- [ ] Offline-Event.
- [ ] App neu starten während Medium fehlt.
- [ ] Treffer/Media-Eintrag bleibt.
- [ ] Medium erneut an anderem Buchstaben.
- [ ] Wiedererkennung.
- [ ] zwei ähnlich aussehende Medien werden nicht falsch zusammengeführt.
- [ ] mehrdeutige Situation erzeugt User-/Admin-Hinweis.
- [ ] `WM_DEVICECHANGE`-Monitor funktioniert unabhängig vom MainForm-Code.

## 10.3 Gate A4

Kein automatisches falsches Merge.

---

# 11. Milestone 0.0.5 – A5 Tika vs. Toxy

**Ziel:** entscheiden, ob Toxy für ausgewählte Formate einen .NET-nativen Fast Path rechtfertigt.

## 11.1 Korpus

- DOC,
- DOCX,
- XLS,
- XLSX,
- PPT,
- PPTX,
- RTF,
- PDF,
- HTML,
- defekte Dateien,
- große Dateien,
- verschlüsselte Dateien.

## 11.2 Metriken

- Parse-Erfolg,
- Textvollständigkeit,
- Metadatenqualität,
- Laufzeit,
- RAM,
- Crashverhalten,
- Threading,
- Lizenz,
- Paketgröße.

## 11.3 Entscheidung

Tika bleibt Default, solange Toxy nicht für einen konkreten MIME-Typ überzeugend besser ist.

---

# 12. Milestone 0.1.0 – Local Vertical Slice

**Ziel:** erster echter End-to-End-Produktpfad.

## 12.1 Benutzerwert

Ein Benutzer kann:

1. die Anwendung starten;
2. einen lokalen Ordner hinzufügen;
3. TXT/HTML indexieren;
4. nach einem Wort suchen;
5. Treffer mit Snippet sehen;
6. Original öffnen;
7. Datei ändern;
8. Aktualisierung erkennen;
9. alten Text nicht mehr finden;
10. Datei löschen;
11. nach erfolgreicher Reconciliation den Treffer entfernen.

## 12.2 Technische Deliverables

- [ ] Solution-Struktur.
- [ ] WinForms Shell.
- [ ] MVP Presenters.
- [ ] SQLite Schema v1.
- [ ] `Source`.
- [ ] `Document`.
- [ ] `CrawlJob`.
- [ ] `WorkItem`.
- [ ] LocalFileSystemConnector.
- [ ] Full Scan.
- [ ] ScanRunId.
- [ ] Reconciliation.
- [ ] SHA-256 bei Änderung.
- [ ] Lucene Index.
- [ ] SearchService.
- [ ] einfache Query.
- [ ] Snippet.
- [ ] Open Original.
- [ ] structured logging.
- [ ] Unit-/Integrationtests.
- [ ] Smoke E2E.

## 12.3 Bewusst nicht enthalten

- USB,
- SMB,
- Web,
- Tika Office/PDF,
- OCR,
- Multiuser,
- Favorites.

## 12.4 Gate G1

- [ ] Happy Path automatisiert.
- [ ] Delete nur nach Complete Scan.
- [ ] fehlerhafte Einzeldatei stoppt Scan nicht.
- [ ] UI bleibt während Crawl responsive.
- [ ] Crash-Recovery testbar.
- [ ] keine kritischen offenen Defects.

---

# 13. Milestone 0.2.0 – USB/Offline + SMB

**Ziel:** der Crawler wird zu einer echten Multi-Storage-Suche.

## 13.1 USB

- [ ] Media Registry.
- [ ] Volume Monitor.
- [ ] MediaId.
- [ ] RelativePath.
- [ ] Online/Offline.
- [ ] Offline-Treffer.
- [ ] Offline-Preview aus Cache.
- [ ] Wiederanschluss.
- [ ] anderer Laufwerksbuchstabe.
- [ ] Media UI.

## 13.2 SMB

- [ ] UNC Source.
- [ ] gemapptes Laufwerk.
- [ ] aktueller Windows-Sicherheitskontext.
- [ ] Source Probe.
- [ ] Netzwerkunterbrechung.
- [ ] Reconnect.
- [ ] keine Massendeletion.
- [ ] Zugriff verweigert als strukturierter Fehler.

## 13.3 Gate G2

Kritisches Szenario:

```text
100.000 bekannte Dateien
NAS offline
Full Scan gestartet
```

Erwartung:

> **0 Dokumente werden aufgrund der Nichterreichbarkeit gelöscht.**

---

# 14. Milestone 0.3.0 – Webcrawler

**Ziel:** Webseiten als gleichwertige Quelle.

## 14.1 Funktionen

- [ ] HTTP/HTTPS.
- [ ] Start-URL.
- [ ] Allowed Hosts.
- [ ] Include/Exclude.
- [ ] Max Depth.
- [ ] Max Pages.
- [ ] URL-Normalisierung.
- [ ] Canonical.
- [ ] Redirect.
- [ ] robots.txt.
- [ ] sitemap.xml.
- [ ] Rate Limiting.
- [ ] 429 Retry-After.
- [ ] Timeout.
- [ ] Max Response Size.
- [ ] ETag.
- [ ] Last-Modified.
- [ ] 304.
- [ ] 404/410 Policy.
- [ ] SSRF Guard.
- [ ] Crawl-Trap-Schutz.
- [ ] HTML Content Extraction.
- [ ] Links auf unterstützte Dokumente in Processing Queue.

## 14.2 Gate G3

Lokaler Testserver deckt alle negativen HTTP-Fälle reproduzierbar ab.

---

# 15. Milestone 0.4.0 – Rich Documents

**Ziel:** Office/PDF/Archive als belastbare Inhaltsquellen.

## 15.1 Formate

MUSS:

- TXT,
- HTML,
- DOC,
- DOCX,
- XLS,
- XLSX,
- PPTX,
- PDF,
- ODT,
- ODS,
- ODP,
- ZIP.

SOLL:

- PPT,
- RTF.

## 15.2 Technische Funktionen

- [ ] MIME Detection.
- [ ] Tika Supervisor.
- [ ] Content Extraction.
- [ ] Metadata Normalization.
- [ ] Extraction Cache.
- [ ] Parser Version.
- [ ] Processing Profile Version.
- [ ] Embedded Documents.
- [ ] Parent/Container Relations.
- [ ] Archive Limits.
- [ ] Zip-Bomb-Schutz.
- [ ] Rename/Move.
- [ ] Last Known Good.
- [ ] Parser Failure Center.

## 15.3 Gate G4

Golden Document Corpus muss reproduzierbar bestehen.

---

# 16. Milestone 0.5.0 – MVP

**Ziel:** erster fachlich vollständiger Crawler-MVP.

## 16.1 OCR

- [ ] Scan-PDF-Erkennung.
- [ ] Tesseract.
- [ ] `deu`.
- [ ] `eng`.
- [ ] `deu+eng`.
- [ ] Auto/Force/Off/Retry.
- [ ] OCR Limits.
- [ ] OCR-Metadaten.
- [ ] Bild-OCR SOLL.

## 16.2 Suche

- [ ] Phrase.
- [ ] AND/OR/NOT.
- [ ] Feldsuche.
- [ ] Fuzzy.
- [ ] Prefix/Wildcard.
- [ ] Facetten.
- [ ] Sortierung.
- [ ] German/English Analyzer.
- [ ] Highlighting.
- [ ] Snippets.
- [ ] sichere Textpreview.

## 16.3 UI

- [ ] Search Workspace.
- [ ] Quellenfilter.
- [ ] Typfilter.
- [ ] Datumsfilter.
- [ ] Verfügbarkeitsfilter.
- [ ] Offline-Status.
- [ ] Preview Pane.
- [ ] Fehlerverständlichkeiten.

## 16.4 G-MVP

MVP ist nur bestanden, wenn die im Lastenheft definierten Szenarien für:

- local,
- USB,
- SMB,
- Web,
- Word,
- Excel,
- PDF,
- Scan-PDF,
- Archive

End-to-End funktionieren.

---

# 17. Milestone 0.6.0 – Security-Härtung und Shared-Mode-Vorbereitung

Die frühere serverorientierte 0.6-Planung wird für den Desktop neu interpretiert.

## 17.1 Desktop 1.0

- [ ] per-user Data Directory.
- [ ] Windows Security Context.
- [ ] Credential Manager für optionale Spezialfälle.
- [ ] DPAPI/ISecretStore.
- [ ] ACL Fingerprint als Metadatum.
- [ ] Preview respektiert OS-Zugriff.
- [ ] Cache-/Indexverzeichnis nur für Benutzer.

## 17.2 Shared Mode vorbereiten

- [ ] `SecurityDescriptor`-Datenmodell.
- [ ] namespaced Principal IDs.
- [ ] `ISecurityFilter`-Abstraktion.
- [ ] Search Backend kann ACL-Felder aufnehmen.

## 17.3 Nicht erzwingen

Keine künstliche lokale Benutzerverwaltung im per-user Desktop-MVP, nur weil das alte Pflichtenheft sie vorsah.

---

# 18. Milestone 0.7.0 – Operations

## 18.1 Scheduler

- [ ] periodische Scans.
- [ ] Autostart-Option.
- [ ] Tray Mode.
- [ ] Pause/Resume.
- [ ] Cancel.

## 18.2 Queue

- [ ] durable queue.
- [ ] leases.
- [ ] retry.
- [ ] exponential backoff.
- [ ] dead letter.

## 18.3 UI

- [ ] Jobs Workspace.
- [ ] Failures Workspace.
- [ ] Diagnostics.
- [ ] Sources Health.
- [ ] Media Health.

## 18.4 Adapter

- [ ] CLI-Grundlage SOLL.
- [ ] Application Contracts stabil.
- [ ] optional Loopback API nur wenn begründet.

---

# 19. Milestone 0.8.0 – Hardening

## 19.1 Performance

- [ ] 1k.
- [ ] 10k.
- [ ] 100k.
- [ ] 1M.
- [ ] Search p50.
- [ ] Search p95.
- [ ] initial indexing rate.
- [ ] update rate.
- [ ] OCR pages/min.
- [ ] RAM.
- [ ] CPU.
- [ ] Indexgröße.

## 19.2 Consistency

- [ ] DB vs. Index checker.
- [ ] orphan work items.
- [ ] expired leases.
- [ ] cache integrity.
- [ ] source state.

## 19.3 Observability

- [ ] structured logs.
- [ ] metrics.
- [ ] traces.
- [ ] diagnostics export.
- [ ] health summary.

---

# 20. Milestone 0.9.0 – Release Candidate

## 20.1 Packaging

- [ ] win-x64 self-contained.
- [ ] Installer.
- [ ] Start Menu.
- [ ] Uninstall.
- [ ] optional Autostart.
- [ ] Tika/JRE handling.
- [ ] Tesseract/languages.
- [ ] license files.
- [ ] SBOM.
- [ ] checksums.
- [ ] code signing, sofern Zertifikat verfügbar.

## 20.2 Migration

- [ ] DB migrations.
- [ ] Index schema upgrade.
- [ ] rebuild.
- [ ] backup.
- [ ] restore.
- [ ] rollback plan.

## 20.3 Accessibility

- [ ] Keyboard only.
- [ ] High DPI 100–200%.
- [ ] High Contrast.
- [ ] Screenreader smoke test.
- [ ] Accessible names.

## 20.4 Documentation

- [ ] Installationshandbuch.
- [ ] Benutzerhandbuch.
- [ ] Administrationshinweise.
- [ ] Troubleshooting.
- [ ] Known Limitations.

---

# 21. Gate G-RC

RC nur, wenn:

- alle 169 MUSS-Anforderungen der freigegebenen Baseline entweder erfüllt oder durch formellen Change ersetzt sind;
- keine Critical/High Security Findings offen sind;
- Migration/Restore getestet;
- vollständiger Acceptance Run bestanden;
- Lizenz-/SBOM-Review bestanden;
- keine ungeklärte Datenverlustklasse existiert.

---

# 22. Milestone 1.0.0 – Stable

1.0 enthält **keine neue Großfunktion**.

Erlaubt:

- RC-Fixes,
- Doku,
- Packaging-Fixes,
- Performancefixes ohne Semantikbruch,
- finaler Security-/Acceptance-Run.

Nicht erlaubt:

- „kurz noch“ neue Connectoren,
- AI,
- Dark Mode als Releaseblocker,
- DMS-Features,
- neues Search Backend ohne zwingenden Grund.

## G-1.0

- [ ] alle Must Gates grün.
- [ ] release tag.
- [ ] signed/checksummed artifacts.
- [ ] SBOM.
- [ ] install test auf sauberem Windows.
- [ ] backup/restore.
- [ ] migration.
- [ ] release notes.
- [ ] known issues.
- [ ] support matrix.
- [ ] evidence archive.

---

# 23. Version 1.1 – Suchkomfort

Geplant:

- Favoriten,
- Saved Searches,
- Synonyme,
- Thesaurus,
- Spellcheck,
- Autocomplete,
- Thumbnails,
- optionale Suchhistorie,
- Dark Mode KANN.

Diese Funktionen dürfen 1.0 nicht verzögern.

---

# 24. Version 1.2 – Rich Metadata

Geplant:

- Tags,
- Notizen,
- EXIF,
- GPS,
- EPUB,
- EML,
- weitere Archive,
- Custom Metadata,
- optional portable Indexfunktionen.

---

# 25. Version 1.3 – Enterprise Integrations

Geplant:

- LDAP/AD,
- OIDC,
- SFTP,
- WebDAV,
- S3,
- authentisierte Websites,
- optional Browser Rendering,
- PST/OST bei konkretem Bedarf.

---

# 26. Version 1.4 – Search Operations

Geplant:

- Search Analytics,
- Zero Result Queries,
- Result Pinning,
- Query Tuning,
- Alerts,
- Webhooks,
- Reports.

---

# 27. Version 1.5 – Semantik

Geplant:

- Similar Documents,
- NER,
- Entity Linking,
- automatische Klassifikation,
- Embeddings,
- Vektorsuche,
- Hybridsuche,
- optional Learning to Rank.

Leitplanke:

> Keyword-Suche bleibt vollständig unabhängig.

---

# 28. Version 2.0 – RAG/AI

Geplant als optionale Erweiterung:

- Fragen gegen den Index,
- Zusammenfassungen,
- Quellenbezug,
- Knowledge Graph,
- lokale Modelle,
- optionale externe Provider.

Nicht erlaubt:

- Dokumente außerhalb des Security Context in Retrieval/Prompt;
- externe Datenübertragung ohne explizites Opt-in.

---

# 29. Abhängigkeitsgraph

```text
Baseline
  ↓
A1 Host ─────────────┐
A2 Lucene ───────────┤
A3 Tika ─────────────┤
A4 Media ────────────┤
A5 Parser Benchmark ─┘
        ↓
       G0
        ↓
      0.1 Local
        ↓
      0.2 USB/SMB
        ↓
      0.3 Web
        ↓
      0.4 Rich Docs
        ↓
      0.5 MVP
        ↓
      0.6 Security
        ↓
      0.7 Ops
        ↓
      0.8 Hardening
        ↓
      0.9 RC
        ↓
      1.0 Stable
```

Tika A3 muss zwar erst für 0.4 vollständig produktiv sein, soll aber vor 0.1 als Architekturabhängigkeit validiert werden, um spätere Packaging-Überraschungen zu vermeiden.

---

# 30. Kritischer Pfad

Der wahrscheinlich kritische Pfad bis MVP ist:

```text
Lucene feasibility
→ Document Registry
→ Reconciliation
→ USB identity
→ Web frontier
→ Tika
→ OCR
→ Search quality
```

Nicht kritisch für MVP:

- Favoriten,
- Dark Mode,
- Search Analytics,
- AI,
- S3,
- OIDC,
- Thumbnails.

---

# 31. No-Go-Liste vor MVP

Folgende Arbeiten dürfen den MVP nicht dominieren:

- WPF-Neuentwurf;
- Ribbon;
- Cloud-SaaS;
- Plugin Marketplace;
- RAG;
- Vector DB;
- BPM;
- Office Editing;
- Public Sharing;
- Backup der Originaldokumente.

---

# 32. Statuskontrolle pro Sprint/Arbeitsblock

Auch ohne formale Scrum-Sprints soll jeder Arbeitsblock enden mit:

1. Was wurde umgesetzt?
2. Welche Requirement-IDs?
3. Welche Tests?
4. Welche Evidence?
5. Welche Risiken neu?
6. Welche ADRs geändert?
7. Welcher Roadmapstatus?
8. Was ist der nächste kleinste vertikale Schritt?

Ein Abschlussbericht sollte nicht nur „Build grün“ sagen.

---

# 33. Evidence-Standard

Für jeden Milestone:

```text
docs/evidence/<version>/
  SUMMARY.md
  requirements.md
  tests.md
  performance.md       # falls relevant
  security.md          # falls relevant
  migration.md         # falls relevant
  screenshots/         # UI/Install evidence
  checksums/
```

`SUMMARY.md` enthält:

- Git Branch,
- Commit SHA,
- Datum,
- Build,
- Testzahlen,
- offene Known Issues,
- Gate Decision.

---

# 34. Defect Policy

## Critical
Datenverlust, Security-Leak, Indexkorruption, falsche Massendeletion.

→ Release blockiert.

## High
Kernfunktion unzuverlässig, häufiger Crash, falsche Suchsichtbarkeit.

→ Release grundsätzlich blockiert.

## Medium
Workaround vorhanden.

→ kann mit dokumentierter Entscheidung offen bleiben.

## Low
kosmetisch/gering.

→ kein automatischer Blocker.

---

# 35. Risikoabhängige Roadmap-Entscheidungen

## Lucene.NET
Wenn A2 scheitert, wird nicht die Roadmap verworfen. Nur das Backend ändert sich.

## Tika
Wenn Packaging unvertretbar ist:
1. Toxy + spezialisierte Parser evaluieren;
2. extern installierter Tika Runtime prüfen;
3. erst danach Architektur ändern.

## USB Identity
Wenn Windows keine ausreichend stabile automatische Identität liefert:
- user-assisted media registration als verbindlicher Fallback.

Das Produktversprechen „offline auffindbar“ bleibt bestehen.

---

# 36. Kalenderplanung

Diese Roadmap enthält bewusst **keine erfundenen Fertigstellungsdaten**.

Warum:

- wir haben noch keine gemessene Entwicklungsgeschwindigkeit;
- PoCs können Architekturpfade ändern;
- Funktionsumfang ist groß.

Nach Abschluss von G0 und Milestone 0.1 soll erstmals eine belastbare Velocity-/Aufwandsprognose erstellt werden.

Bis dahin gilt:

> Reihenfolge und Gates sind verbindlicher als Kalenderdaten.

---

# 37. Roadmap-Review-Rhythmus

Roadmap prüfen:

- nach jedem Milestone;
- nach jedem Architektur-No-Go;
- nach einem Change Request mit Releaseauswirkung;
- nach neuem Critical/High Risk;
- vor jedem Release Candidate.

Nicht bei jedem kleinen Commit umschreiben.

---

# 38. Definition „Wo stehen wir?“

Zum aktuellen Stichtag ist die Antwort:

> Die fachliche und technische Planung ist weit fortgeschritten. Die neue WinForms/.NET-8-Zielarchitektur ist ausführlich beschrieben, aber noch nicht durch die entscheidenden Architektur-Spikes praktisch bestätigt. Die Implementation darf deshalb noch nicht als begonnen oder Architektur als final freigegeben gelten. Der nächste operative Schritt ist Milestone 0.0.x mit den A1–A5-Spikes und anschließend Gate G0.

---

# 39. Unmittelbare nächste Aufgaben

- [ ] ADRs reviewen und akzeptieren.
- [x] Change Request für die WinForms/.NET-8-Baseline erstellt.
- [x] Pflichtenheft 0.2 erzeugt.
- [x] A1 Spike-Repository/Solution angelegt.
- [x] A1 Evidence unter `docs/evidence/0.0.1/` dokumentiert.
- [ ] A2 Lucene Benchmark aufsetzen.
- [ ] A3 Tika Sidecar testen.
- [ ] A4 reale USB-Medien testen.
- [ ] A5 Tika/Toxy Benchmark.
- [ ] G0-Entscheidung dokumentieren.
- [ ] erst danach Milestone 0.1 starten.

---

# 40. Pflegehinweis

Nach jedem Gate wird oben im Dokument aktualisiert:

```text
Aktueller Status:
Aktueller Meilenstein:
Nächster Gate:
Letzte Änderung:
```

Abgeschlossene Checklisten bleiben zur Historie erhalten. Sie werden nicht gelöscht, sondern mit Evidence verlinkt.

---

**Ende der Roadmap**
