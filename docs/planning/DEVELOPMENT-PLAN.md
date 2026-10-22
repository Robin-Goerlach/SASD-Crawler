# Entwicklungsplan

**Stand:** 22. Oktober 2026

## 1. Entwicklungsstrategie

Der SASD-Crawler wird in **vertikalen, testbaren Scheiben** entwickelt.

Nicht:

```text
erst 30 Interfaces
→ dann alle Datenbanken
→ dann alle Connectoren
→ irgendwann UI
```

Sondern:

```text
kleinster realer Nutzerpfad
→ testbar
→ stabilisieren
→ nächste Quelle/Fähigkeit
```

## 2. Solution-Grundsätze

- Domain kennt keine UI/Infrastructure.
- Application enthält Use Cases.
- Infrastructure implementiert IO.
- `src/shared/` enthält Domain/Application/Contracts.
- `src/frontend/` enthält WinForms Views/Presenter/Composition Root.
- `src/indexer/` enthält Persistence, Crawl/Queue/Reconciliation, Search, Extraction/OCR und Windows-Indexer-Infrastructure.
- Fremdparser laufen isoliert.
- jeder persistente Zustand besitzt Migration.
- jeder langlaufende Prozess besitzt Cancellation.
- jeder Hintergrundjob besitzt persistenten Status.

## 3. Repository- und Build-Sicht

Produktionscode ab 0.1:

```text
src/shared/
src/frontend/
src/indexer/

tests/shared/
tests/frontend/
tests/indexer/
tests/e2e/
```

`Sasd.Crawler.sln` bleibt der vollständige Integrations-Build. Sobald echte Produktionsprojekte existieren, können Frontend und Indexer zusätzliche Solution-Filter/Entwicklereinstiege erhalten. Diese ersetzen die Gesamt-Solution nicht.

Die physische Trennung erzwingt keinen zweiten Prozess. V1 darf Indexer-Services im Desktopprozess hosten; die Contract-Grenze soll einen späteren separaten Host ermöglichen.

## 4. Workstreams

### WS-A Core Domain
Source, Media, Document, DuplicateGroup, Job, WorkItem, Reconciliation, States.

### WS-B Persistence
SQLite, migrations, repositories, queue leases.

### WS-C Search
Lucene backend, schema, versionierte DE/EN-Analyzer und Stopwortlisten, queries, snippets.

### WS-D Connectors
Local, USB, SMB, Web.

### WS-E Extraction
Tika/Toxy, cache, archive, OCR.

### WS-F WinForms
Shell, MVP, search, sources, media, jobs, errors, diagnostics.

### WS-G Security
Windows context, secret store, parser isolation, web security.

### WS-H Quality
tests, corpus, performance, recovery, evidence.

## 5. Definition of Ready

Eine Aufgabe ist READY, wenn:

- Requirement-ID beziehungsweise abgeleitete technische Story vorhanden;
- erwartetes Verhalten beschrieben;
- Architekturkomponente klar;
- Abhängigkeiten erfüllt;
- Testansatz beschrieben;
- kein ungelöster ADR-Blocker.

## 6. Definition of Done

- Code kompiliert Release.
- Unit/Integrationtest vorhanden.
- negative Fehlerfälle berücksichtigt.
- Cancellation/Logging berücksichtigt.
- Migration berücksichtigt.
- Dokumentation aktualisiert.
- Requirement-Status aktualisiert.
- CI grün.
- keine neue Critical/High Finding.

## 7. Coding-Regeln

- `Nullable=enable`.
- keine `.Result`/`.Wait()` in UI-/Serverpfaden.
- `CancellationToken` für lange IO.
- kein globaler Service Locator.
- keine statischen `Helper`-Sammelklassen.
- keine direkten Lucene-/SQLite-Aufrufe aus Forms.
- keine `async void` außer echten UI-Eventhandlern.
- keine COM-Automation von Office.

## 8. Programmiersprachen und Repository-Grenzen

- C#/.NET 8 bleibt die kanonische V1-Implementierung.
- `Sasd.Crawler.sln` bleibt der kanonische Build-Einstieg.
- zusätzliche Sprachen liegen unter `languages/<language>/`; wenn sie Frontend/Indexer/Shared spiegeln, dann unter `languages/<language>/src/<bereich>/`;
- jede zusätzliche Sprache dokumentiert Build, Tests, Runtime und Lizenz;
- sprachspezifische CI wird mit der ersten gepflegten Komponente ergänzt;
- keine sekundäre Sprache darf ohne Change Request zur zwingenden V1-Runtimeabhängigkeit werden;
- gemeinsame Domain-/Security-Invarianten werden über explizite Verträge geteilt, nicht unabhängig kopiert.

## 9. Branch-/Commit-Konzept

Bis ein bestehender Repo-Standard etwas anderes vorgibt:

- `main` bleibt releasable;
- Featurebranches kurzlebig;
- Commitnachrichten dürfen Requirement-ID enthalten.

Beispiel:

```text
feat(USB-004): keep offline media searchable
test(WEB-009): add robots policy fixtures
fix(IDX-003): avoid delete on incomplete scan
```

## 10. Arbeitsblock-Abschluss

Jeder größere Codex-/Entwicklungsauftrag endet mit:

- Branch/Commit,
- Build,
- Testzahlen,
- Requirement-IDs,
- geänderte Dokumente,
- offene Risiken,
- nächster Schritt.
