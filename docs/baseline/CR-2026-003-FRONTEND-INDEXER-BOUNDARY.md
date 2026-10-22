# CR-2026-003 – Frontend-/Indexer-Repository-Grenze

**Status:** ACCEPTED  
**Datum:** 22. Oktober 2026  
**Betroffene Baseline:** Pflichtenheft 0.2, Architektur 0.1, ADR-0016

## 1. Ausgangslage

Die kanonische C#/.NET-8-Produktimplementierung soll Frontend und Indexer unabhängig weiterentwickeln können. Bisher beschreibt die Architektur zwar saubere Schichten, die geplante Repository-Struktur legt UI-, Search-, Persistence-, Connector- und Parserprojekte aber noch flach gemeinsam unter `src/` ab.

Das erschwert langfristig:

- getrennte Entwicklung und Reviews von UI und Indexer;
- einen später optionalen eigenen Indexer-Prozess;
- alternative Frontends;
- einen möglichen Indexer in einer zusätzlichen Programmiersprache;
- getrennte Build-/Test-Sichten ohne Aufteilung in mehrere Repositories.

## 2. Entscheidung

Der Produktionscode ab Milestone 0.1 wird physisch nach Verantwortlichkeit abgelegt:

```text
src/
  shared/
  frontend/
  indexer/
```

Die Tests spiegeln diese Grenze:

```text
tests/
  shared/
  frontend/
  indexer/
  e2e/
```

Die bereits vorhandenen Architektur-Spikes A1–A5 bleiben an ihren heutigen Pfaden. Sie sind Evidence/PoC-Historie und werden nicht nur für kosmetische Ordnung verschoben.

## 3. Verantwortlichkeiten

### shared

Enthält UI- und Indexer-unabhängige Bausteine:

- Domain;
- Application Use Cases;
- Contracts/DTOs;
- Abstraktionen für Persistence, Search, Extraction, OCR und Connectors.

`shared` darf weder vom Frontend noch von konkreten Indexer-Implementierungen abhängen.

### frontend

Enthält die Benutzerseite:

- WinForms Shell;
- Views/UserControls;
- MVP Presenter und Presentation Models;
- Desktop-spezifische UI-Integration;
- Composition Root des Desktop-Hosts.

Frontend-Code darf weder SQL noch Lucene, Tika/Tesseract oder Crawler-Dateisystemlogik direkt aufrufen.

### indexer

Enthält die technische Indexierungsseite:

- SQLite Control Store;
- Work Queue/Scheduler;
- Local/USB/SMB/Web Connectoren;
- Reconciliation;
- Hashing und Duplicate Groups;
- Lucene Search Backend;
- Extraction/Tika;
- OCR/Tesseract;
- Windows Media/File-System Infrastructure;
- technische Observability der Hintergrundarbeit.

Der Indexer ist Besitzer der schreibenden Zugriffe auf Control Store und Suchindex. Das Frontend arbeitet über Application-/Contract-Grenzen.

## 4. Prozessgrenze

Die Repository-Grenze erzwingt **für V1 keinen zweiten Prozess**.

Standard für V1 bleibt:

```text
Sasd.Crawler.WinForms.exe
  ├─ Frontend
  ├─ Shared/Application
  └─ Indexer Services via Generic Host/DI
```

Die Grenze wird aber so gestaltet, dass später ohne Verschieben des Quellcodes ein eigener Host möglich ist:

```text
Sasd.Crawler.WinForms.exe
        │
        │ versionierter Contract / IPC
        ▼
Sasd.Crawler.Indexer.exe
```

Ob ein eigener Indexer-Prozess für V1 tatsächlich benötigt wird, ist eine spätere, explizite Architekturentscheidung.

## 5. Build und Solutions

`Sasd.Crawler.sln` bleibt die autoritative Integrations-Solution.

Sobald die ersten Produktionsprojekte unter 0.1 angelegt werden, sollen zusätzlich Solution-Filter beziehungsweise gleichwertige Entwickler-Einstiege für Frontend und Indexer entstehen, ohne die Gesamt-Solution zu ersetzen.

## 6. Mehrsprachenentwicklung

Sekundäre Sprachen spiegeln die Verantwortungsgrenze nur dort, wo realer Code existiert:

```text
languages/<language>/src/
  frontend/
  indexer/
  shared/
```

Ein alternativer Indexer darf dadurch später separat entwickelt werden. Er wird aber erst dann Teil des V1-Runtimepfads, wenn ein eigener Change Request die zusätzliche Runtime-/Deployment-Abhängigkeit genehmigt.

## 7. Migration

- vorhandene A1–A5-Spikeprojekte werden nicht verschoben;
- neue Produktionsprojekte ab 0.1 entstehen direkt in den neuen Bereichen;
- bei einer späteren Übernahme von Spike-Code wird Code fachlich in das Zielprojekt integriert statt das Spikeprojekt umzubenennen;
- keine History-Rewrites.

## 8. Tests und Akzeptanz

- CI baut weiterhin `Sasd.Crawler.sln`;
- Frontend-Tests dürfen keinen echten Lucene-/Tika-/Crawler-Stack benötigen, sofern ein Contract/Fake genügt;
- Indexer-Tests laufen ohne WinForms-UI;
- E2E-Tests dürfen beide Seiten integrieren;
- Architekturtests sollen später verbotene Projektabhängigkeiten absichern.

## 9. Entscheidung

**ACCEPTED.** Die Änderung verändert keine der 260 fachlichen Anforderungen; sie präzisiert die technische Repository- und Modulgrenze.
