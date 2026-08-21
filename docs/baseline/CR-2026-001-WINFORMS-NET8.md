# CR-2026-001 – WinForms/.NET-8-Rebaselining

**Status:** DRAFT – zur formalen Annahme vorgesehen  
**Datum:** 21. August 2026  
**Betroffene Baseline:** Lastenheft 0.1, Pflichtenheft 0.1, Architektur 0.1

## 1. Ausgangslage

Das Pflichtenheft 0.1 wurde zunächst mit einer serverorientierten technischen Zielarchitektur erstellt:

- .NET 10,
- ASP.NET Core/Blazor,
- Windows und Linux,
- Mehrbenutzer-/Shared-Server-Baseline.

Anschließend wurde die explizite Produktentscheidung getroffen, den SASD-Crawler primär als:

> **Windows Forms Application für .NET 8**

zu entwickeln.

## 2. Änderung

Die technische Baseline wird geändert auf:

- Windows Forms als Primär-UI;
- `net8.0-windows`;
- Windows-first für Version 1.0;
- per-user Desktopbetrieb als Standard;
- SQLite Control Store;
- Lucene.NET hinter `ISearchIndex`;
- Tika Sidecar;
- Tesseract OCR;
- Hintergrundworker im WinForms-/Generic-Host-Lifecycle;
- kein zwingender Windows Service im Desktop-MVP;
- Shared Mode nach 1.0 als separater Host.

## 3. Nicht geänderte fachliche Ziele

Unverändert bleiben:

- lokale Laufwerke;
- USB-/Offline-Medien;
- SMB/UNC;
- Webseiten;
- Office/PDF;
- OCR;
- klassische Volltextsuche;
- Reconciliation;
- sichere Löschlogik;
- Backup/Recovery;
- spätere semantische Funktionen.

## 4. Betroffene Lastenheftanforderungen

Besonders betroffen:

- `PLAT-002` Linux;
- `PLAT-003` Containerbetrieb;
- `UI-013` responsive Weboberfläche;
- `AUTH-002` bis `AUTH-009` soweit sie einen zentralen Mehrbenutzerbetrieb voraussetzen;
- `API-001` bis `API-007` hinsichtlich öffentlichem HTTP-Host versus internem Application Contract.

## 5. Entscheidungsvorschlag

### PLAT-002
Von MUSS 1.0 zu:
> Core- und Application-Layer sollen portierbar bleiben; ein Linux-Host ist nach 1.0 möglich, aber keine 1.0-Desktopanforderung.

### PLAT-003
Containerbetrieb auf späteren Service/Shared Mode verschieben.

### UI-013
Durch native WinForms-UI mit High-DPI-/Accessibility-Anforderung ersetzen.

### AUTH
Version 1.0 Desktop:
> Windows-Identität und per-user Index sind Standard-Sicherheitsgrenze.

Shared-Mode-ACLs bleiben Architekturvorbereitung und spätere Funktion.

### API
Internes Application Contract bleibt verbindlich. Ein öffentlicher HTTP-API-Host wird nicht zur Voraussetzung der Desktop-UI.

## 6. Auswirkungen

### Vorteile
- native Windows-UX;
- geringerer Betriebsaufwand;
- bessere USB-/UNC-Integration;
- kein zentral privilegierter Crawler notwendig;
- einfacher Desktop-Installer.

### Nachteile
- Windows-only 1.0;
- .NET-8-Supportende im November 2026;
- Shared Mode später zusätzlicher Aufwand.

## 7. Migration bestehender Dokumentation

- Lastenheft wird durch Amendment 0.1a ergänzt.
- Pflichtenheft 0.1 wird technisch durch Pflichtenheft 0.2 superseded.
- Architektur 0.1 bleibt technisch führend.
- ROADMAP wird auf die neue Baseline ausgerichtet.

## 8. Abnahmekriterien des Change Requests

- [x] Architektur 0.1 beschreibt Zielzustand.
- [x] Lastenheft-Amendment erstellt.
- [x] Pflichtenheft 0.2 erstellt.
- [x] Roadmap angepasst.
- [x] ADRs erstellt.
- [ ] menschlicher Review/Annahme.
- [ ] PoC-Gate G0 erfolgreich.

## 9. Entscheidung

**Vorgeschlagen:** ACCEPT  
**Formal angenommen:** noch offen
