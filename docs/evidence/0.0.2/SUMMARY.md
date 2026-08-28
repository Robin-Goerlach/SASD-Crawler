# Milestone 0.0.2 – A2 Lucene.NET Spike

**Datum:** 28. August 2026
**OS:** Windows 10.0.26200, win-x64
**Runtime:** .NET 8.0.30
**Lucene.NET:** 4.8.0-beta00018
**Gate-Entscheidung:** **TECHNICAL GO**

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

## Realistischer kleiner Textkorpus

Der versionierte `docs/`-Bestand wurde als realer deutsch/englischer Projektkorpus verwendet:

| Metrik | Ergebnis |
|---|---:|
| Markdown-/Textdokumente | 48 |
| Indexrate | 73,0 Dokumente/s |
| Indexgröße | 255.744 Byte / 5.328 Byte je Dokument |
| Update | 25,85 ms/Dokument (10 % des Korpus) |
| Delete | 4,11 ms/Dokument (10 % des Korpus) |
| Query p50 / p95 | 23,9 ms / 52,8 ms |
| minimale Trefferzahl je Query | 17 |
| Peak Working Set | 67,9 MB |

Der Runner bricht ab, wenn eine Benchmarkquery keine Treffer liefert. Dieser Korpus ersetzt
keinen späteren Golden-Document-Korpus, belegt für A2 aber reale, unterschiedlich lange
Inhalte, Dateinamen und deutsch/englische Architekturbegriffe statt synthetischer Wiederholung.

## Crash-/Recovery-Probe

1. Ein persistenter Index mit 100.000 committed Dokumenten wurde erstellt.
2. Der Runner schrieb 1.000 Updates ohne Commit.
3. `Environment.FailFast` beendete den Prozess ohne Dispose/Shutdown (Exitcode 1).
4. Ein neuer Prozess öffnete denselben Index, schrieb erneut, commitete und suchte erfolgreich.

Damit sind Lock-Freigabe und Recovery des letzten vollständigen Commits nach hartem
Prozessabbruch praktisch nachgewiesen. Nicht committed Änderungen dürfen verloren gehen;
die spätere persistente Work Queue muss sie erneut zustellen.

## Simulierter voller Datenträger

Ein `FilterDirectory` begrenzte den nächsten Lucene-Schreibpfad auf 32 Byte. Der große
Upsert brach reproduzierbar mit `IOException` ab. Danach:

- lehnte der Adapter weitere Writes explizit ab;
- führte Dispose ein Writer-Rollback statt eines erneuten Commits aus;
- ließ sich derselbe physische Index normal wieder öffnen;
- blieb das zuvor committed Dokument suchbar;
- war das teilweise geschriebene Dokument nicht sichtbar.

Damit ist der geforderte Full-Disk-/Write-Failure-Fall ohne Manipulation realer Volumes oder
Benutzerdaten praktisch nachgewiesen.

## Grenzen
- Facetten werden im Spike über die zurückgegebenen Top-Hits aggregiert, nicht über die
  vollständige Treffermenge. Für Produktcode ist Lucene-Faceting oder eine zweite Aggregation nötig.
- Near-real-time Reader-/Searcher-Wiederverwendung ist noch nicht optimiert.
- Der PoC beweist keine Reconciliation- oder SQLite/Lucene-Dual-Write-Semantik.

## Entscheidung

**TECHNICAL GO.** Es wurde kein Blocker für Lucene.NET als eingebettetes Desktop-Backend
gefunden. Synthetische 1M-, reale Korpus-, Crash-, Full-Disk-, Recovery- und Funktionsproben
sind grün. Die formale Annahme von ADR-0005 bleibt Teil des übergeordneten G0-Reviews.
OpenSearch bleibt eine spätere Shared-/Vector-Option und wird nicht in den Desktop-MVP vorgezogen.
