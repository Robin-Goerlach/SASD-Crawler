# ADR-0017: Frontend und Indexer als getrennte Repository-Bereiche

**Status:** Accepted  
**Datum:** 22. Oktober 2026

## Kontext

SASD-Crawler ist für Version 1.0 eine C#/.NET-8-WinForms-Anwendung. Die UI soll jedoch nicht der Ort sein, an dem Crawler-, Persistence-, Search- oder Parserlogik lebt. Gleichzeitig soll das Repository später alternative Hosts oder eine anderssprachige Indexer-Implementierung zulassen, ohne die Anwendung in mehrere Repositories aufzuteilen.

## Entscheidung

Der Produktionscode wird ab Milestone 0.1 in drei physische Bereiche gegliedert:

```text
src/shared/
src/frontend/
src/indexer/
```

Die Teststruktur spiegelt diese Bereiche.

### Abhängigkeitsrichtung

```text
frontend ─────► shared ◄───── indexer
    │                        │
    └── Desktop Composition Root may wire concrete indexer implementations
```

`shared` kennt weder Frontend noch Indexer.

Presentation-Code im Frontend darf konkrete Indexer-Technologien nicht kennen. Nur der Desktop-Composition-Root darf für den In-Process-V1-Host konkrete Implementierungen registrieren.

### Prozessmodell

Die physische Repository-Trennung bedeutet nicht automatisch Prozess-Trennung. V1 darf Frontend und Indexer im selben Prozess hosten. Die Contract-Grenze muss jedoch einen späteren Out-of-Process-Indexer ermöglichen.

### Datenbesitz

Der Indexer ist für schreibende Zugriffe auf SQLite Control Store, Lucene-Index, Extraction Cache und Crawl-/Reconciliation-Zustand verantwortlich. Das Frontend konsumiert Application Services/Contracts.

## Folgen

### Positiv

- Frontend und Indexer können unabhängig entwickelt und reviewed werden;
- UI-Tests werden leichter;
- Headless-/Service-/CLI-Hosts bleiben möglich;
- ein später anderssprachiger Indexer bekommt eine natürliche Grenze;
- kein zweites Repository und keine voreilige IPC-Komplexität für V1.

### Negativ / Trade-offs

- die Solution erhält zusätzliche Ordner-/Projektgrenzen;
- der Desktop-Composition-Root muss Abhängigkeiten diszipliniert verdrahten;
- eine spätere Prozess-Trennung erfordert einen versionierten IPC-Adapter.

## Nicht entschieden

Dieser ADR entscheidet **nicht**, dass Version 1.0 zwei EXE-Prozesse benötigt.

## Verifikation

- neue Produktionsprojekte entstehen unter `src/shared`, `src/frontend` oder `src/indexer`;
- Tests spiegeln die Bereiche;
- keine WinForms-View referenziert SQLite/Lucene/Tika/Crawler-Implementierungen direkt;
- Indexer-Komponenten bauen/testen ohne WinForms-UI;
- `Sasd.Crawler.sln` bleibt der Integrations-Build.
