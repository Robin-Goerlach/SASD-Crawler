# Pre-G0 Native TXT/HTML Indexing – Summary

**Datum:** 24. Oktober 2026  
**Branch:** `feat/native-text-html-indexing`  
**Code-Commit verifiziert:** `6ba0fcd2f1b24d18c647da64f15a6e1e14be3a9f`  
**CI:** #39 – success  
**Gate-Auswirkung:** G0 bleibt NOT READY; Milestone 0.1 ist weiterhin formal blockiert.

## Vertikaler Pfad

```text
Local Full Scan
  → Document Registry / authoritative reconciliation
  → durable DocumentId WorkItems
  → Native TXT/HTML Extraction
  → DocumentIndexingService
  → ISearchIndex
  → Lucene
```

Discovery schreibt nicht direkt nach Lucene.

## Native Extraction

`Sasd.Crawler.Extraction.Native` unterstützt für den 0.1-Local-Scope:

- TXT;
- HTML;
- HTM;
- read-only lokale file:-URIs;
- konfigurierbares Input-Limit (Default 16 MiB);
- BOM-Erkennung + UTF-8-Default für TXT;
- HTML-DOM-Verarbeitung über AngleSharp;
- Entfernung von script/style/noscript/template aus dem Suchtext;
- HTML title/lang-Metadaten;
- keine JavaScript-Ausführung und kein Netzwerk-Resource-Loading.

Legacy-Codepage-Heuristik wird bewusst nicht geraten und bleibt spätere Arbeit.

## Durable Processing

SQLite Schema v4 ergänzt DocumentId an WorkItems und die Stufen:

- `IndexDocument`;
- `DeleteFromSearchIndex`.

Ein CrawlJob bleibt Running, solange Dokument-WorkItems offen sind. Erst nach erfolgreichem Abschluss aller Folgearbeit wird Succeeded bzw. Partial gesetzt.

Search-Löschung entsteht ausschließlich aus DocumentIds, die dieselbe autoritative Reconciliation-Transaktion neu als Missing klassifiziert hat.

## E2E

Der reale Test beweist:

1. TXT + HTML physisch anlegen;
2. vollständigen Crawl queuen;
3. echter BackgroundService verarbeitet Discovery und Dokument-WorkItems;
4. beide Inhalte sind in echtem Lucene suchbar;
5. TXT-Inhalt ändern und erneut crawlen;
6. alter Suchterm verschwindet, neuer erscheint;
7. HTML physisch löschen;
8. erst ein erfolgreicher weiterer Full Scan erzeugt Search-Delete;
9. HTML-Suchtreffer verschwindet, aktueller TXT-Treffer bleibt.

Damit sind EXT-001, EXT-002 sowie IDX-001, IDX-002 und IDX-003 für den lokalen 0.1-Pfad implementiert und automatisiert verifiziert.
