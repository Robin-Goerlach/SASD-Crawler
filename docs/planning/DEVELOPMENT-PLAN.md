# Entwicklungsplan

**Stand:** 21. August 2026

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
- WinForms enthält Views/Presenter/Composition Root.
- Fremdparser laufen isoliert.
- jeder persistente Zustand besitzt Migration.
- jeder langlaufende Prozess besitzt Cancellation.
- jeder Hintergrundjob besitzt persistenten Status.

## 3. Workstreams

### WS-A Core Domain
Source, Media, Document, Job, WorkItem, Reconciliation, States.

### WS-B Persistence
SQLite, migrations, repositories, queue leases.

### WS-C Search
Lucene backend, schema, analyzers, queries, snippets.

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

## 4. Definition of Ready

Eine Aufgabe ist READY, wenn:

- Requirement-ID beziehungsweise abgeleitete technische Story vorhanden;
- erwartetes Verhalten beschrieben;
- Architekturkomponente klar;
- Abhängigkeiten erfüllt;
- Testansatz beschrieben;
- kein ungelöster ADR-Blocker.

## 5. Definition of Done

- Code kompiliert Release.
- Unit/Integrationtest vorhanden.
- negative Fehlerfälle berücksichtigt.
- Cancellation/Logging berücksichtigt.
- Migration berücksichtigt.
- Dokumentation aktualisiert.
- Requirement-Status aktualisiert.
- CI grün.
- keine neue Critical/High Finding.

## 6. Coding-Regeln

- `Nullable=enable`.
- keine `.Result`/`.Wait()` in UI-/Serverpfaden.
- `CancellationToken` für lange IO.
- kein globaler Service Locator.
- keine statischen `Helper`-Sammelklassen.
- keine direkten Lucene-/SQLite-Aufrufe aus Forms.
- keine `async void` außer echten UI-Eventhandlern.
- keine COM-Automation von Office.

## 7. Branch-/Commit-Konzept

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

## 8. Arbeitsblock-Abschluss

Jeder größere Codex-/Entwicklungsauftrag endet mit:

- Branch/Commit,
- Build,
- Testzahlen,
- Requirement-IDs,
- geänderte Dokumente,
- offene Risiken,
- nächster Schritt.
