# Baseline- und Änderungssteuerung

**Stand:** 22. Oktober 2026  
**Status:** ACTIVE

## 1. Gültige Baseline

Die frühere technische Annahme `.NET 10 + Blazor + Windows/Linux + Shared-Server-first` wurde durch die explizite Produktentscheidung **Windows Forms auf .NET 8, Desktop-first** ersetzt.

CR-2026-001 ist angenommen und durch A1 praktisch bestätigt. Mit CR-2026-002 wird zusätzlich festgelegt, dass die **kanonische Version 1.0 weiterhin in C# auf .NET 8** entsteht, weil sie auf verwalteten Windows-Arbeitsplätzen ohne Installation einer zusätzlichen Programmiersprachen-Runtime nutzbar sein muss. Das Repository darf trotzdem mehrere Programmiersprachen enthalten; zusätzliche Implementierungen bleiben klar isoliert und dürfen keine versteckte V1-Laufzeitabhängigkeit erzeugen.

## 2. Dokumentrangfolge

### Fachlich normativ

1. `docs/baseline/LASTENHEFT.md`
2. `docs/baseline/LASTENHEFT-AMENDMENT-0.1a.md`
3. `docs/baseline/LASTENHEFT-AMENDMENT-0.1b-SEARCH-DUPLICATES.md`

### Technisch normativ

4. akzeptierte ADRs
5. `docs/baseline/ARCHITECTURE.md`
6. `docs/baseline/PFLICHTENHEFT.md` (Pflichtenheft 0.2)

### Steuernd / nicht selbst scope-ändernd

7. `ROADMAP.md`
8. `PROJECT-STATUS.md`
9. Planungs-, Test-, Security- und Operations-Dokumente

Eine Roadmap oder Implementierung darf keine MUSS-Anforderung stillschweigend abschaffen.

## 3. Superseded-Dokumente

Pflichtenheft 0.1 bleibt historische Referenz außerhalb der aktuellen Repository-Baseline. Widersprechende technische Aussagen aus 0.1 gelten nicht mehr.

## 4. CR-2026-001 – WinForms/.NET 8

**Status:** ACCEPTED / recorded 2026-10-06

Verbindlich für 1.0:

- Windows Forms Primär-UI,
- `net8.0-windows`,
- Windows-first,
- per-user Desktopbetrieb,
- SQLite Control Store,
- Search Backend Abstraction,
- kein zwingender Windows Service im Desktop-MVP,
- späterer Shared/Service Mode als Erweiterung.

PoC-abhängig bleiben insbesondere konkrete Search-/Parser-/Media-Details.

## 5. CR-2026-002 – Search Efficiency, Duplicate Discovery and Polyglot Repository

**Status:** ACCEPTED / recorded 2026-10-22

Verbindlich:

- C#/.NET 8 bleibt die kanonische V1-Produktimplementierung.
- Der Windows-V1-Client darf keine Installation einer Go-Runtime oder einer anderen zusätzlichen Programmiersprachen-Runtime voraussetzen.
- Ein self-contained `win-x64` Publish bleibt der bevorzugte Auslieferungsweg für verwaltete Arbeitsplätze.
- Das Repository wird für mehrere Programmiersprachen vorbereitet; sekundäre Implementierungen liegen unter `languages/<language>/` und dürfen die kanonische C#-Architektur nicht duplizieren oder unterlaufen.
- `SEA-022` führt versionierte, konfigurierbare Stopwortlisten für analysierte Suchfelder ein.
- `ID-011` wird auf exakte, hashbasierte Dublettenerkennung geschärft und auf 0.4.0 vorgezogen.
- `ID-012` verlangt die fortlaufende Aktualisierung von Dublettengruppen im Zuge regulärer Hintergrund-/Reconciliation-Läufe.

## 6. CR-2026-003 – Frontend-/Indexer-Repository-Grenze

**Status:** ACCEPTED / recorded 2026-10-22

Verbindlich:

- neuer Produktionscode ab 0.1 wird unter `src/shared/`, `src/frontend/` und `src/indexer/` getrennt;
- Tests spiegeln diese Grenze unter `tests/`;
- die vorhandenen A1–A5-Spikes bleiben unverändert als PoC-/Evidence-Historie;
- V1 darf Frontend und Indexer weiterhin in einem Prozess hosten;
- die Contract-Grenze muss einen später separaten Indexer-Host ermöglichen;
- `Sasd.Crawler.sln` bleibt die autoritative Integrations-Solution.

## 7. .NET-8-Lifecycle

Microsofts Supportende für .NET 8 am 10.11.2026 bleibt ein bewusstes Risiko. Die Anforderung des verwalteten Hochschul-PCs führt jedoch zu einer expliziten Kompatibilitätsentscheidung: **Die V1-Baseline wird nicht allein wegen des Supportdatums stillschweigend auf ein anderes Target umgestellt.**

Daher gilt:

- Entwicklung und V1-Kompatibilität bleiben auf `net8.0-windows`, bis eine bewusst getestete Baselineänderung beschlossen wird;
- Publish-Artefakte sollen self-contained sein, damit keine separate .NET-Runtime-Installation erforderlich ist;
- G0 dokumentiert Patch-/Support-/Security-Risiko und einen späteren Upgradepfad, erzwingt aber nicht automatisch einen Targetwechsel;
- ein Framework-Upgrade benötigt einen eigenen Change Request und Kompatibilitätstest auf dem vorgesehenen Hochschul-PC-/Managed-PC-Szenario.

## 8. Change-Request-Verfahren

Langfristige Änderungen an Scope, Architektur, Datenformat, Security oder Releaseziel erhalten `CR-YYYY-NNN` und bei Architekturwirkung zusätzlich ein ADR.

Ein Change Request enthält mindestens Ausgangslage, Änderung, Requirements, Nutzen, Kosten, Risiken, Migration, Tests, Releaseauswirkung und Entscheidung.

## 9. Gate vor Milestone 0.1

- [x] CR-2026-001 angenommen.
- [x] Amendment 0.1a angenommen.
- [x] CR-2026-002 angenommen.
- [x] CR-2026-003 Frontend-/Indexer-Repository-Grenze angenommen.
- [x] Amendment 0.1b angenommen.
- [x] Pflichtenheft 0.2 aktive technische Baseline.
- [x] A1 automatisiert erfolgreich; manuelle Rest-Evidence dokumentiert.
- [x] A2 technisch erfolgreich.
- [ ] A3 Rest-Evidence abgeschlossen.
- [ ] A4 Hardware-Evidence abgeschlossen.
- [ ] A5 Restkorpus abgeschlossen.
- [ ] PoC-abhängige ADR-Entscheidungen aktualisiert.
- [ ] .NET-8-Support-/Upgradepfad für G0 dokumentiert.
- [ ] G0 formal GO.

## 10. Änderungsprotokoll

| Datum | Änderung | Status |
|---|---|---|
| 2026-08-21 | Dokumentbaseline und Supersession-Regeln erstmalig definiert | historisch |
| 2026-10-06 | Repository/Chats/A1-Code abgeglichen; CR-2026-001 + Amendment 0.1a + Pflichtenheft 0.2 als aktive Baseline konsolidiert | active |
| 2026-10-22 | CR-2026-002 + Amendment 0.1b: C#/.NET-8-V1 bestätigt, Polyglot-Repository vorbereitet, Stopwort- und fortlaufende Dublettenerkennung ergänzt | active |
| 2026-10-22 | CR-2026-003 + ADR-0017: Produktionscode in shared/frontend/indexer getrennt; V1 bleibt zunächst In-Process-fähig | active |
