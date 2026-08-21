# Baseline- und Änderungssteuerung

**Stand:** 21. August 2026  
**Status:** aktiv

## 1. Problemstellung

Im bisherigen Projektverlauf wurde die technische Zielarchitektur wesentlich geändert:

- vorherige Pflichtenheftannahme: .NET 10, ASP.NET Core/Blazor, Windows + Linux, serverorientierter;
- aktuelle explizite Architekturvorgabe: **Windows Forms auf .NET 8**, Desktop-first.

Beide Dokumente existieren. Ohne formale Regel wäre bei einer späteren Implementierung unklar, welche Aussage gilt.

## 2. Aktuelle Dokumentbaseline

### Fachlich führend

`docs/baseline/02-LASTENHEFT.md`

Es definiert das Produktziel und die fachlichen Anforderungen.

### Technisch führend

`docs/baseline/04-ARCHITECTURE-WINFORMS-NET8.md`

Es ersetzt widersprechende technische Annahmen des älteren Pflichtenhefts bezüglich:

- .NET 10 → .NET 8,
- Blazor → Windows Forms,
- Windows/Linux-1.0 → Windows-first,
- Shared-Server-first → per-user Desktop-first,
- Web-UI-Accessibility → native Windows-Accessibility.

### Teilweise gültig

`docs/baseline/03-PFLICHTENHEFT.md`

Alle technischen Inhalte, die **nicht** der aktuellen Architektur widersprechen, bleiben als Konkretisierung verwendbar.

Eine revidierte Fassung liegt jetzt als `docs/baseline/07-PFLICHTENHEFT-0.2-WINFORMS-NET8.md` vor. Sie ist noch als **DRAFT** markiert und muss gemeinsam mit CR-2026-001 und Amendment 0.1a formal reviewed/angenommen werden.

## 3. Rangfolge bei Widersprüchen

1. explizit freigegebene Change Requests;
2. Lastenheft für fachliche Ziele;
3. freigegebene ADRs für einzelne Architekturentscheidungen;
4. aktuelle Architektur;
5. aktuelles Pflichtenheft, soweit nicht superseded;
6. Roadmap/Planungsdokumente;
7. ältere Analyseunterlagen.

Ein Roadmap-Eintrag darf keine MUSS-Anforderung stillschweigend abschaffen.

## 4. Change-Request-Verfahren

Jede Änderung mit Auswirkung auf Scope, Architektur, Datenformat, Security oder Releaseziel erhält eine ID:

```text
CR-YYYY-NNN
```

Ein Change Request enthält mindestens:

- Ausgangslage,
- vorgeschlagene Änderung,
- betroffene Anforderungen,
- betroffene ADRs,
- Nutzen,
- Kosten,
- Risiken,
- Migration,
- Testfolgen,
- Releaseauswirkung,
- Entscheidung,
- Datum.

## 5. Architekturänderungen

Architekturänderungen mit langfristiger Wirkung erhalten zusätzlich ein ADR.

Beispiel:

```text
CR-2026-001: Desktop-first WinForms statt Blazor
ADR-0001: Windows Forms und .NET 8 als Zielplattform
```

## 6. Status der aktuellen Architekturänderung

| Punkt | Status |
|---|---|
| WinForms als Primär-UI | entschieden |
| .NET 8 als gewünschtes Target | entschieden, Lifecycle-Risiko dokumentiert |
| Windows-first 1.0 | Architekturentscheidung, Lastenheftabgleich erforderlich |
| Linux-1.0-MUSS | **Konflikt – formal zu ändern** |
| responsive Web-UI als 1.0-SOLL | **Konflikt – formal zu ändern** |
| lokale Mehrbenutzerkonten in Desktop 1.0 | **neu zu bewerten** |
| HTTP Search API als 1.0-MUSS | intern als Application Contract erhalten; öffentliche Hostpflicht neu zu bewerten |
| Containerbetrieb 1.0 | für Desktop nicht primär; späterer Service-Host |

## 7. Baseline-Gate vor Implementierung 0.1

Milestone 0.1 darf erst starten, wenn:

- [ ] Architektur-Spikes A1–A4 erfolgreich oder mit dokumentiertem Fallback abgeschlossen sind.
- [x] Pflichtenheft 0.2 als Draft liegt vor.
- [ ] Pflichtenheft 0.2 / CR-2026-001 / Amendment 0.1a formal angenommen.
- [ ] betroffene Lastenheftanforderungen PLAT/UI/AUTH/API bewertet sind.
- [ ] ADRs 0001–0014 mindestens reviewed sind.
- [ ] Roadmap und Requirement-Status auf die freigegebene Baseline zeigen.

## 8. Änderungsprotokoll

| Datum | Änderung | Status |
|---|---|---|
| 2026-08-21 | Dokumentbaseline und Supersession-Regeln erstmalig definiert | aktiv |
