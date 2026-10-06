# Enterprise Vision – non-normative future direction

**Stand:** 6. Oktober 2026  
**Status:** FUTURE VISION / nicht Teil der 1.0-Verpflichtung

Dieses Dokument bewahrt die in Strategiegesprächen entwickelte langfristige Perspektive, ohne den Desktop-MVP aufzublähen.

## Leitbild

Aus dem Desktop-Crawler kann langfristig eine **SASD Enterprise Information Discovery**-Plattform entstehen.

Mögliche spätere Fähigkeiten:

- zentral administrierte Quellen,
- verteilte Crawler/Agents,
- Shared Search / Search Cluster,
- ACL-aware Retrieval über mehrere Identitäten,
- Scheduler und zentrale Jobsteuerung,
- Audit/Retention,
- Monitoring/Health/HA,
- stabile Search-/Ingest-APIs,
- zusätzliche Quellen wie Mail, Datenbanken, DMS, Git, SharePoint/Confluence/Jira,
- semantische/hybride Suche,
- optionale RAG-/Knowledge-Graph-Funktionen.

## Architekturbedingung

Die Desktop-1.0 wird **nicht** als verkleinerter Enterprise-Server gebaut. Stattdessen werden gezielte Abstraktionen erhalten:

- `ISourceConnector`,
- Search Backend Abstraction,
- Control/Persistence boundaries,
- Security descriptor/principal model,
- Application Services unabhängig von WinForms.

Das erlaubt später einen separaten Service Host, ohne die Desktopanwendung neu zu schreiben.

## Mögliche gemeinsame SASD Enterprise Foundation

In früheren Strategiegesprächen wurde eine projektübergreifende SASD-Foundation als langfristige Möglichkeit diskutiert. Für SASD-Crawler wären insbesondere folgende Querschnittsfunktionen Kandidaten:

- Logging,
- Konfiguration,
- Health Checks,
- Jobs/Scheduling-Grundlagen,
- API-/CLI-Konventionen,
- Security-/Secret-Abstraktionen,
- Migrationen,
- Backup/Restore,
- Packaging/Diagnostics,
- Versionierung,
- SBOM/Lizenzberichte,
- signierte Builds.

Diese Foundation ist **kein aktuelles Crawler-Dependency**. Vor 1.0 soll keine gemeinsame Plattform erzwungen werden, solange der konkrete Nutzen nicht durch mindestens zwei SASD-Projekte nachgewiesen ist. Crawler-Code soll lediglich vermeiden, solche Querschnittsfunktionen unnötig proprietär zu verschachteln.

## Optionale externe Orchestrierung

Auch eine spätere Anbindung an einen Job-/Workflow-Orchestrator wie JS7 wurde diskutiert.

Die Grenze ist klar:

> Ein externer Scheduler/Orchestrator darf Zeitplan, Reihenfolge, Dependencies, Retry, Restart, Monitoring und Logs koordinieren – die fachliche Crawlerlogik bleibt im SASD-Crawler.

Dafür sollte ein späterer unattended CLI-/Batch-Vertrag möglichst folgende Eigenschaften besitzen:

- Exitcode `0` für Erfolg und dokumentierte Fehlercodes,
- strukturierte Logs,
- externe Konfiguration,
- keine Secrets im Code oder in Kommandozeilen,
- möglichst idempotente Operationen,
- Run-/Correlation-ID,
- Dry-Run dort, wo eine Aktion relevante Änderungen auslöst.

Eine JS7-Abhängigkeit ist **nicht** Teil von 1.0 und darf die eingebaute Desktop-Scheduler-Funktion nicht ersetzen.

## Code-Health und Architekturmetriken

Als spätere Quality-/Hardening-Funktion sollen über Releases Trends sichtbar werden, unter anderem:

- Lines of Code als Kontextmetrik, nicht als Ziel,
- zyklomatische/strukturelle Komplexität,
- Tests und Coverage dort, wo sie aussagekräftig ist,
- Duplikation,
- Analyzer-/Compilerwarnungen,
- Maintainability-Trends,
- unerwünschte Schichtenabhängigkeiten,
- Dependency-/Vulnerability-Drift.

Diese Metriken gehören primär in 0.8/Release Engineering und sollen keine lokale Optimierung auf eine einzelne Kennzahl erzwingen.

## Governance-Bezug

Im breiteren SASD-Ansatz kann PIMS/Governance Anforderungen, Releases, Risiken und Evidence koordinieren; Git/Codex/CI führen technische Arbeit aus, während SASD-Crawler seine Produktfähigkeiten implementiert.

PIMS/Governance soll dabei **nicht** selbst kompilieren, Container bauen, SBOMs erzeugen, signieren oder Produktionsmonitoring ausführen. Diese Aufgaben bleiben bei GitHub Actions/Codex/Build-/Security-/Operations-Werkzeugen; Governance sammelt Status und Evidence.

Beispiel Traceability:

```text
Enterprise requirement
→ Crawler requirement
→ release/milestone
→ implementation
→ test
→ evidence
→ gate
```

## Nicht-Ziel vor 1.0

Keine dieser Enterprise-Ideen darf A2–G0 oder den Desktop-MVP blockieren. Insbesondere werden vor 1.0 keine verteilten Cluster, HA, allgemeine DMS-Funktionen, externe Orchestrator-Abhängigkeiten, gemeinsame SASD-Foundation oder AI-Abhängigkeiten erzwungen.
