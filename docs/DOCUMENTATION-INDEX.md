# Dokumentationsindex und Zweck der Dokumente

**Stand:** 21. August 2026

Dieses Dokument beschreibt, welche Projektunterlagen existieren, welchen Zweck sie haben und wann sie aktualisiert werden müssen.

| Dokument | Zweck | Änderungsfrequenz | Autorität |
|---|---|---|---|
| `ROADMAP.md` | zeitliche/inhaltliche Projektsteuerung | fortlaufend | Planung |
| `PROJECT-STATUS.md` | aktueller Snapshot | häufig | Status |
| `BASELINE-AND-CHANGE-CONTROL.md` | gültige Dokumentbaseline und Änderungsregeln | bei Baselineänderungen | **hoch** |
| `REQUIREMENTS-STATUS.md` | Status aller 258 Anforderungen | je Umsetzung | Nachverfolgung |
| `docs/baseline/01-PRODUCT-ANALYSIS.md` | Markt-/Funktionsanalyse | bei strategischer Neubewertung | Referenz |
| `docs/baseline/02-LASTENHEFT.md` | fachliche Anforderungen | kontrolliert | **fachlich normativ** |
| `docs/baseline/03-PFLICHTENHEFT.md` | bisherige technische Konkretisierung | kontrolliert | derzeit teilweise superseded |
| `docs/baseline/04-ARCHITECTURE-WINFORMS-NET8.md` | aktuelle Zielarchitektur | kontrolliert | **technisch führend** |
| `docs/baseline/05-CR-2026-001-WINFORMS-NET8.md` | formaler Architekturwechsel | einmalig/review | Baselineänderung |
| `docs/baseline/06-LASTENHEFT-AMENDMENT-0.1a.md` | fachliche Anpassung kollidierender Plattform-/UI-Anforderungen | einmalig/review | fachlicher Draft |
| `docs/baseline/07-PFLICHTENHEFT-0.2-WINFORMS-NET8.md` | rebaselinierte technische Spezifikation | kontrolliert | **neue technische Draft-Baseline** |
| `docs/planning/POC-PLAN.md` | Architektur-Spikes und Entscheidungsgates | bis PoC-Abschluss | Planung/Nachweis |
| `docs/planning/DEVELOPMENT-PLAN.md` | Entwicklungsablauf, Workstreams, Definition of Ready/Done | fortlaufend | Entwicklungsprozess |
| `docs/testing/TEST-STRATEGY.md` | Testarten, Testdaten, Automatisierung | bei Testarchitekturänderung | Qualität |
| `docs/testing/QUALITY-GATES.md` | Freigabekriterien pro Meilenstein | je Release | **Release normativ** |
| `docs/planning/RISK-REGISTER.md` | Risiken und Maßnahmen | regelmäßig | Risikosteuerung |
| `docs/planning/RELEASE-POLICY.md` | Versionierung, Branching, Packaging, Releasebelege | selten | Releaseprozess |
| `docs/planning/TRACEABILITY-STRATEGY.md` | Verbindung Requirement → Code → Test → Evidence | selten | Governance |
| `docs/security/SECURITY-PLAN.md` | Threat Model und Sicherheitsmaßnahmen | fortlaufend | Security |
| `docs/operations/DATA-BACKUP-MIGRATION-PLAN.md` | Daten, Schema, Backup, Restore, Migration | bei Persistenzänderung | Betrieb |
| `docs/operations/OPERATIONS-DIAGNOSTICS-PLAN.md` | Logs, Health, Diagnose, Support | fortlaufend | Betrieb |
| `docs/architecture/adr/*.md` | einzelne Architekturentscheidungen | je Entscheidung | **Architekturhistorie** |

## Noch bewusst nicht erzeugt

Folgende Unterlagen sollen erst entstehen, wenn der entsprechende Entwicklungsstand existiert:

- `CHANGELOG.md` – mit dem ersten echten Code-/Release-Change;
- Benutzerhandbuch – ab belastbarer UI;
- Administratorhandbuch – ab Quellen-/Jobadministration;
- Installationshandbuch – sobald Packaging stabil ist;
- API-Referenz – wenn öffentliche API-Verträge stabilisiert werden;
- SBOM und Lizenzreport – durch die Releasepipeline;
- Performancebericht – nach den ersten Benchmarks;
- Security-Testbericht – nach dem ersten Security Gate.

Dadurch vermeiden wir Dokumente, die nur Platzhalter enthalten und schnell veralten.
