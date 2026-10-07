# Baseline- und Änderungssteuerung

**Stand:** 6. Oktober 2026  
**Status:** ACTIVE

## 1. Gültige Baseline

Die frühere technische Annahme `.NET 10 + Blazor + Windows/Linux + Shared-Server-first` wurde durch die explizite Produktentscheidung **Windows Forms auf .NET 8, Desktop-first** ersetzt.

Diese Rebaselining-Entscheidung ist nicht mehr nur ein Draft: Sie wurde anschließend als A1 tatsächlich umgesetzt und verifiziert. Bei diesem Reconciliation-Stand wird CR-2026-001 deshalb formal als **ACCEPTED** geführt.

## 2. Dokumentrangfolge

### Fachlich normativ

1. `docs/baseline/LASTENHEFT.md`
2. `docs/baseline/LASTENHEFT-AMENDMENT-0.1a.md`

### Technisch normativ

3. akzeptierte ADRs
4. `docs/baseline/ARCHITECTURE.md`
5. `docs/baseline/PFLICHTENHEFT.md` (Pflichtenheft 0.2)

### Steuernd / nicht selbst scope-ändernd

6. `ROADMAP.md`
7. `PROJECT-STATUS.md`
8. Planungs-, Test-, Security- und Operations-Dokumente

Eine Roadmap oder Implementierung darf keine MUSS-Anforderung stillschweigend abschaffen.

## 3. Superseded-Dokumente

Pflichtenheft 0.1 bleibt historische Referenz außerhalb der aktuellen Repository-Baseline. Widersprechende technische Aussagen aus 0.1 gelten nicht mehr.

## 4. CR-2026-001 – WinForms/.NET 8

**Status:** ACCEPTED / recorded 2026-10-06

Verbindlich für 1.0:

- Windows Forms Primär-UI,
- `net8.0-windows` als derzeit gewünschtes Entwicklungs-Target,
- Windows-first,
- per-user Desktopbetrieb,
- SQLite Control Store,
- Search Backend Abstraction,
- kein zwingender Windows Service im Desktop-MVP,
- späterer Shared/Service Mode als Erweiterung.

PoC-abhängig bleiben:

- Lucene.NET als konkretes v1 Search Backend (A2),
- Tika-Version/Packaging (A3),
- Media Identity Details (A4),
- Toxy-Rolle (A5).

## 5. .NET-8-Lifecycle

Die Produktentscheidung für .NET 8 bleibt für die aktuellen Spikes bestehen. Sie ist jedoch zeitlich begrenzt: Hersteller-Support endet am 10.11.2026.

Daher gilt:

- A2–A5 dürfen auf aktuell gepatchtem .NET 8 laufen;
- G0 MUSS den Zeitpunkt für die Migration auf eine unterstützte LTS-Version festlegen;
- eine produktive 1.0 darf nicht versehentlich auf einer ungeprüften, aus dem Support gefallenen Runtime veröffentlicht werden.

Ein Framework-Upgrade soll die Architektur nicht ändern; das Target ist zentral zu halten.

## 6. Change-Request-Verfahren

Langfristige Änderungen an Scope, Architektur, Datenformat, Security oder Releaseziel erhalten `CR-YYYY-NNN` und bei Architekturwirkung zusätzlich ein ADR.

Ein Change Request enthält mindestens Ausgangslage, Änderung, Requirements, Nutzen, Kosten, Risiken, Migration, Tests, Releaseauswirkung und Entscheidung.

## 7. Gate vor Milestone 0.1

- [x] CR-2026-001 angenommen.
- [x] Amendment 0.1a angenommen.
- [x] Pflichtenheft 0.2 aktive technische Baseline.
- [x] A1 automatisiert erfolgreich; manuelle Rest-Evidence dokumentiert.
- [ ] A2 abgeschlossen.
- [ ] A3 abgeschlossen.
- [ ] A4 abgeschlossen.
- [ ] A5 abgeschlossen.
- [ ] ADR-Entscheidungen nach PoCs aktualisiert.
- [ ] .NET-8-Migrationszeitpunkt beschlossen.
- [ ] G0 formal GO.

## 8. Änderungsprotokoll

| Datum | Änderung | Status |
|---|---|---|
| 2026-08-21 | Dokumentbaseline und Supersession-Regeln erstmalig definiert | historisch |
| 2026-10-06 | Repository/Chats/A1-Code abgeglichen; CR-2026-001 + Amendment 0.1a + Pflichtenheft 0.2 als aktive Baseline konsolidiert | active |
