# Teststrategie

**Stand:** 21. August 2026

## 1. Qualitätsziel

Der SASD-Crawler verarbeitet fremde, potenziell beschädigte Dokumente und entscheidet über Sichtbarkeit beziehungsweise Entfernung aus einem Suchindex.

Daher sind Tests kein nachgelagerter Schritt.

Besonders kritisch:

- keine falsche Massendeletion,
- keine Indexkorruption,
- keine ACL-/Security-Leaks,
- keine UI-Blockade durch Crawler/OCR,
- reproduzierbare Suchqualität.

## 2. Testpyramide

### Unit
Domain, Policies, URL normalization, Media matching, State Machines, Query Parsing.

### Component
SQLite, Lucene, Tika, OCR, individual connectors.

### Integration
Discovery → Registry → Queue → Extraction → Index.

### E2E
WinForms-nahe Nutzerpfade und reale Quellen.

### Performance
100k/1M, indexing, query, OCR.

### Recovery
kill, restart, corrupted cache, expired leases, rebuild.

### Security
malformed docs, archive bomb, SSRF, XSS text, secret leakage.

## 3. Golden Document Corpus

Versionierter Testbestand:

- TXT,
- HTML,
- DOC/DOCX,
- XLS/XLSX,
- PPT/PPTX,
- ODT/ODS/ODP,
- PDF born-digital,
- PDF scan,
- mixed PDF,
- JPG/PNG/TIFF,
- ZIP,
- malformed,
- encrypted,
- very large.

## 4. Excel-Spezialfälle

- mehrere Sheets,
- hidden sheets,
- Formeln,
- Kommentare,
- Datumswerte,
- große Tabellen,
- Umlaute.

## 5. OCR-Korpus

- 300dpi sauber,
- 150dpi,
- schief,
- schlechte Kopie,
- Deutsch,
- Englisch,
- gemischt,
- Tabellen.

## 6. Web-Fixture

Lokaler HTTP-Testserver mit:

- robots,
- sitemap,
- canonical,
- redirects,
- redirect loop,
- 404,
- 410,
- 429,
- 500,
- timeout,
- huge response,
- private-IP redirect,
- crawl trap,
- PDF/Office link.

## 7. USB

Automatisiert:
- Matching-/State-Machine-Tests.

Hardware:
- detach,
- drive letter change,
- same label,
- reconnect,
- offline search.

## 8. SMB

- share online,
- root denied,
- single file denied,
- disconnect,
- reconnect,
- rename,
- delete,
- many files.

## 9. Golden Query Set

Jede Query definiert:

- Must-Hit,
- Must-Not-Hit,
- optional Rank Range,
- max latency.

Beispiele:
- `Müller`
- `"Projekt Alpha"`
- `vertrag AND alpha`
- `rechnung 2025`
- `type:pdf`

## 10. Regression

Jeder behobene Defect erhält nach Möglichkeit einen Test.

Insbesondere:
- Massendeletion,
- duplicate after rename,
- stale cache,
- UI freeze,
- parser hang.

## 11. UI-Teststrategie

Möglichst viel Verhalten im Presenter testen.

UI Automation nur für:
- Start,
- Search,
- Add Source,
- Preview,
- Offline Indicator,
- Job status.

## 12. Testevidence

Je Gate:
- Test Summary,
- failed/skipped tests,
- environment,
- commit SHA,
- artifact version.
