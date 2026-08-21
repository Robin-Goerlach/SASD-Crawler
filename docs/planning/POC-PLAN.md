# PoC- und Architektur-Spike-Plan

**Stand:** 21. August 2026  
**Ziel:** die risikoreichsten Architekturannahmen vor Milestone 0.1 praktisch bestätigen

## 1. Grundsatz

Die Spikes sind keine Produktfeatures. Ihr Ergebnis ist eine **Entscheidung mit Messdaten**.

Spike-Code darf verworfen werden.

Produktionscode darf nur übernommen werden, wenn er zusätzlich die normalen Qualitätsanforderungen erfüllt.

## 2. A1 – WinForms/Host Lifecycle

### Hypothese
WinForms, `Microsoft.Extensions.Hosting`, BackgroundServices, SQLite, Tray und Single-Instance-Lifecycle lassen sich stabil kombinieren.

### Mess-/Testfälle
- Startzeit.
- MainForm responsiveness.
- Worker start/stop.
- Cancellation.
- Tray.
- Single instance.
- second-instance activation via named pipe.
- SQLite write during background work.
- clean shutdown.
- forced kill/restart.

### Go
Keine DB-Korruption; kein hängender Prozess; UI bleibt responsive.

### Evidence
Screenshots, logs, test report, exact .NET SDK/runtime.

## 3. A2 – Lucene.NET

### Hypothese
Lucene.NET ist für den Desktopindex ausreichend stabil und performant.

### Daten
Synthetisch 1M plus realistischer kleiner Dokumentkorpus.

### Benchmarks
- indexing docs/s,
- index bytes/doc,
- RAM,
- query p50/p95,
- update latency,
- delete latency,
- reopen/recovery.

### Suchfunktionen
Phrase, Boolean, fuzzy, prefix, GermanAnalyzer, EnglishAnalyzer, highlighting, facets.

### Negative Tests
Process kill während Write; ungültiger Indexpfad; voller Datenträger simulieren.

## 4. A3 – Tika Sidecar

### Hypothese
Tika kann kontrolliert als lokaler Sidecar betrieben und paketiert werden.

### Tests
- process spawn,
- loopback only,
- health,
- DOCX/XLSX/PPTX/PDF,
- malformed PDF,
- timeout,
- restart,
- max size,
- parser error,
- temporary files,
- clean shutdown.

### Security
Keine externe Bindung, keine beliebigen Fetcher, restricted temp.

## 5. A4 – Windows Media Identity

### Hypothese
Medien können mit ausreichend hoher Sicherheit wiedererkannt werden.

### Geräte
NTFS, exFAT, FAT32, USB flash, external SSD.

### Fälle
detach/attach, drive letter change, same label, cloned content, app restart offline.

### Fallback
Bei Ambiguität user-assisted binding, niemals aggressives Auto-Merge.

## 6. A5 – Tika vs. Toxy

### Ziel
Nicht „wer ist schneller?“, sondern pro Format feststellen, ob Toxy bei geringerem Betriebsaufwand gleichwertig ist.

### Bewertungsmatrix
| Kriterium | Gewicht |
|---|---:|
| Textvollständigkeit | 25 |
| Parserrobustheit | 20 |
| Formatbreite | 15 |
| Metadaten | 10 |
| Performance | 10 |
| Memory | 5 |
| Packaging | 5 |
| Security Isolation | 5 |
| Maintenance/Lizenz | 5 |

### Entscheidungsregel
Toxy darf nur für einen MIME-Typ primär werden, wenn es dort keinen relevanten Funktionsverlust gibt.

## 7. G0-Report

Nach A1–A5 entsteht `G0-decision.md`:

- Ergebnis jedes Spikes,
- Messdaten,
- gewählter Stack,
- Fallbacks,
- offene Risiken,
- ADR-Updates,
- Go/No-Go.
