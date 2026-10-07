# G0 – Architecture Feasibility Readiness

**Stand:** 7. Oktober 2026  
**Entscheidung:** **NOT READY / NO GO FÜR MILESTONE 0.1**

## Aussage

Die Architekturspikes A1–A5 besitzen inzwischen ausführbaren Code und technische Evidence. Das ist ein wesentlicher Fortschritt, reicht nach Roadmap und Baseline aber weiterhin nicht für ein G0-GO. Milestone 0.1 darf erst beginnen, wenn die verbleibende Rest-Evidence und die formalen PoC-abhängigen Architekturentscheidungen geschlossen sind.

Die Repository-Reconciliation vom 6./7. Oktober 2026 hat inzwischen CR-2026-001, Lastenheft-Amendment 0.1a und Pflichtenheft 0.2 als aktive Baseline konsolidiert. Außerdem ist die Windows-CI auf dem reconciled `main` mit Restore, Formatprüfung, Release-Build und Tests erfolgreich gelaufen. Diese früheren Readiness-Lücken sind damit geschlossen.

## Spike-Status

| Spike | Technisches Ergebnis | Noch offen |
|---|---|---|
| A1 WinForms/Host | **CONDITIONAL GO**, 9 automatisierte Tests | interaktive Responsiveness-, Tray-, Second-Launch- und Forced-Kill-Smokes |
| A2 Lucene.NET | **TECHNICAL GO**, 9 A2-Tests, 1M + Realkorpus + Crash/Recovery + Full-Disk | formale ADR-0005-/G0-Annahme; Beta-Risiko bleibt bewusst |
| A3 Tika Sidecar | **CONDITIONAL GO**, Tika 3.3.2 + Java 21 + 62-MiB-Runtime + Formatprobe + Teilinventar | Tika 4.1 auf gleicher Basis, finales Packaging/SBOM/CVE/Signierung/AV |
| A4 Media Identity | **CONDITIONAL GO**, 13 Tests + Windows-Hostprobe | physische NTFS/exFAT/FAT32-Medien, Attach/Detach und Drive-Letter-Wechsel |
| A5 Tika/Toxy | **CONDITIONAL; Tika bleibt Default** | realer/komplexer Korpus, Legacy DOC/PPT, verschlüsselte/bösartige Dateien |

Historische Spike-Gesamtsuite der wiederhergestellten Evidence: **37 Tests, 0 Fehler, 0 übersprungen**. Der aktuelle Recovery-PR muss zusätzlich gegen den reconciled `main` per CI grün sein, bevor diese Arbeit gemerged wird.

## G0-Kriterienaudit

| Kriterium | Status | Begründung |
|---|---|---|
| normative WinForms/.NET-8-Baseline konsistent | **erfüllt** | CR-2026-001, Amendment 0.1a und Pflichtenheft 0.2 aktiv |
| Repository-/Solution-/CI-Baseline | **erfüllt** | kanonische `Sasd.Crawler.sln`; Windows-CI auf reconciled `main` grün |
| A2 Embedded Search technisch tragfähig | **erfüllt technisch** | Lucene.NET 4.8.0-beta00018 erfüllt die gemessenen A2-Kriterien |
| A1/A3/A4/A5 vollständig abgeschlossen | **nicht erfüllt** | jeweils dokumentierte Rest-Evidence |
| Tika-Linie für Produktion entschieden | **nicht erfüllt** | 3.3.2 bewiesen; 4.1-Vergleich fehlt |
| PoC-abhängige ADRs formal reviewed | **nicht erfüllt** | insbesondere ADR-0005/0006/0007/0011 bleiben Proposed |
| .NET-8-Lifecycle vor produktiver 1.0 geklärt | **nicht erfüllt** | Supportende 10.11.2026; Upgradepfad muss terminiert werden |
| kein kritischer Architekturblocker übrig | **noch nicht abschließend bewiesen** | Packaging-, Hardware- und Fehlerkorpus-Evidence offen |

## Aktuelle Stackempfehlung aus den PoCs

- WinForms + Generic Host im per-user Desktopprozess;
- SQLite als Control Store/Lifecycle-Wahrheit;
- Lucene.NET 4.8.0-beta00018 hinter `ISearchIndex` als abgeleiteter Suchindex;
- Apache Tika als isolierter Referenzparser unter Supervisor, konkrete Produktionslinie 3.3.x vs. 4.x noch zu entscheiden;
- kein Toxy-Fast-Path in der aktuellen Baseline;
- `MediaId + RelativePath`, automatische Bindung nur bei eindeutiger exakter Volume-GUID;
- `WM_DEVICECHANGE` nur als Hint, niemals als Löschwahrheit.

## Nächste notwendige Aktionen

1. Recovery-PR A2–A5 gegen den reconciled `main` grün verifizieren und mergen.
2. A1 interaktive Desktop-Smokes nachholen.
3. A3 Tika-4.1-Vergleich und Packaging-/SBOM-/CVE-/AV-Evidence schließen.
4. A4 physische Wechselmedien-Smokes durchführen.
5. A5 realen/Legacy-/verschlüsselten Fehlerkorpus ergänzen.
6. ADR-0005/0006/0007/0011 und .NET-8-Lifecycle im G0-Review formal entscheiden.
7. G0 erneut auditieren.

## Entscheidung

**G0 = NOT READY. Milestone 0.1 bleibt gesperrt.**
