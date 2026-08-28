# Milestone 0.0.2 – A2 Lucene.NET Spike

**Datum:** 28. August 2026
**OS:** Windows 10.0.26200, win-x64
**Runtime:** .NET 8.0.30
**Lucene.NET:** 4.8.0-beta00018
**Gate-Entscheidung:** **CONDITIONAL GO**

## Ergebnis

Lucene.NET läuft eingebettet unter `net8.0-windows` stabil genug, um als bevorzugtes
Desktop-v1-Backend hinter `ISearchIndex` weitergeführt zu werden. Der Spike implementiert:

- persistentes Erstellen, Öffnen, Upsert, Delete und Commit;
- Phrase-, Boolean-, Fuzzy- und Prefix-Abfragen;
- deutsche und englische Analyzer;
- Highlighting, Kategorie-Facetten und Kategorie-Filter;
- koordinierten Einzel-Writer sowie nebenläufige Leser;
- explizite Cancellation- und Eingabegrenzen;
- Wiederöffnung nach normalem Shutdown und nach absichtlichem `Environment.FailFast`.

Der Index bleibt ein abgeleitetes Artefakt. Die Spike-Abstraktion enthält keine Quellen-,
Job- oder Lifecycle-Wahrheit.

## Messwerte

Alle Werte stammen vom oben genannten Host im Release-Build. Der synthetische Korpus
enthält kurze deutsch/englische Dokumente; die Query-Messung umfasst 100 Abfragen.

| Lauf | Indexrate | Größe | Query p50 | Query p95 | Peak Working Set |
|---|---:|---:|---:|---:|---:|
| 100.000 Dokumente | 6.360 docs/s | 127,3 Byte/doc | 27,0 ms | 80,3 ms | 93,8 MB |
| 1.000.000 Dokumente | 10.001 docs/s | 134,3 Byte/doc | 173,9 ms | 224,5 ms | 114,8 MB |

Ein separater 100.000-Dokument-Lauf mit 1.000 Mutationen ergab:

- Update: 0,201 ms/Dokument einschließlich Batch-Commit;
- Delete: 0,056 ms/Dokument einschließlich Batch-Commit.

Die Zahlen sind PoC-Messwerte und noch keine Produkt-SLOs. Der Runner öffnet pro Query
einen Reader und misst damit bewusst eine konservative Implementierung ohne NRT-Tuning.

## Crash-/Recovery-Probe

1. Ein persistenter Index mit 100.000 committed Dokumenten wurde erstellt.
2. Der Runner schrieb 1.000 Updates ohne Commit.
3. `Environment.FailFast` beendete den Prozess ohne Dispose/Shutdown (Exitcode 1).
4. Ein neuer Prozess öffnete denselben Index, schrieb erneut, commitete und suchte erfolgreich.

Damit sind Lock-Freigabe und Recovery des letzten vollständigen Commits nach hartem
Prozessabbruch praktisch nachgewiesen. Nicht committed Änderungen dürfen verloren gehen;
die spätere persistente Work Queue muss sie erneut zustellen.

## Offene Evidence und Grenzen

- Ein realistischer, lizenzkonform versionierbarer Dokumentkorpus wurde noch nicht gemessen.
- Voller Datenträger/Write-Failure wurde noch nicht kontrolliert simuliert.
- Facetten werden im Spike über die zurückgegebenen Top-Hits aggregiert, nicht über die
  vollständige Treffermenge. Für Produktcode ist Lucene-Faceting oder eine zweite Aggregation nötig.
- Near-real-time Reader-/Searcher-Wiederverwendung ist noch nicht optimiert.
- Der PoC beweist keine Reconciliation- oder SQLite/Lucene-Dual-Write-Semantik.

## Entscheidung

**CONDITIONAL GO.** Es wurde kein Blocker für Lucene.NET als eingebettetes Desktop-Backend
gefunden. A3 darf beginnen. Ein uneingeschränktes A2-GO und die Annahme von ADR-0005 bleiben
bis zur Full-Disk-Fehlerprobe und einem realistischen Korpus offen. OpenSearch bleibt eine
spätere Shared-/Vector-Option und wird nicht in den Desktop-MVP vorgezogen.
