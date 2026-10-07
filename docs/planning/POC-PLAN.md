# PoC- und Architektur-Spike-Plan

**Stand:** 6. Oktober 2026  
**Aktueller Spike:** A2 – Lucene.NET

## 1. Grundsatz

Spikes liefern eine **Entscheidung mit Messdaten**, keine versteckte Produktimplementierung. Spike-Code darf verworfen werden. Produktionsübernahme verlangt weiterhin normale Qualitätsanforderungen.

## 2. A1 – WinForms/Host Lifecycle

**Status:** **CONDITIONAL GO**  
**Evidence:** `docs/evidence/0.0.1/`

Automatisiert bestätigt:

- Generic Host/DI,
- BackgroundService + Cancellation,
- SQLite/WAL,
- MVP/Presenter + UI-Marshalling,
- Single Instance + Named Pipe,
- Shutdown/Recovery-Grundlogik,
- 9/9 Tests, Build/Format grün.

Manuell offen:

- Tray in interaktiver Session,
- sichtbare Responsiveness,
- Second-Launch-Fokus,
- realer Forced-Kill/Restart.

Diese Restpunkte verhindern `GO`, aber nicht A2.

## 3. A2 – Lucene.NET

**Status:** READY / NEXT

### Hypothese
Lucene.NET 4.8.0-beta00018 ist für den eingebetteten Desktopindex stabil, ausreichend schnell und recovery-fähig.

### Muss-Nachweise

- .NET 8 Build,
- create/open/reopen,
- 100.000 und 1.000.000 synthetische Dokumente,
- `UpdateDocument`/Delete,
- NRT Search,
- German/English analyzers,
- phrase/Boolean/fuzzy/prefix/wildcard,
- highlighting,
- faceting/filter,
- concurrent readers + koordinierter Writer,
- kill/reopen recovery,
- full rebuild,
- index size/RAM/indexing throughput,
- query p50/p95.

### Entscheidungsoptionen

- **GO:** Lucene.NET bleibt v1 embedded backend.
- **CONDITIONAL GO:** Desktop ja, Shared/Vector später OpenSearch.
- **NO-GO:** OpenSearch wird früher primär.

Details: `docs/codex/FIRST-TASK-0.0.2.md`.

## 4. A3 – Tika Sidecar/Packaging

**Status:** NOT STARTED

Seit der ursprünglichen Planung hat sich die Lage geändert: **Tika 4.1.0** ist aktuell; die **3.3.2**-Linie bleibt unterstützt.

A3 muss daher zusätzlich entscheiden:

1. Tika 4.1.0 als neuer Major (Java 17, neue Distribution/Out-of-process-Architektur),
2. Tika 3.3.2 Maintenance als konservativer Pfad,
3. Packaging, Startzeit, RAM, Sandbox, Upgradepfad, Lizenz/SBOM,
4. DOCX/XLSX/PPTX/PDF, malformed input, timeout/restart,
5. loopback-only / keine unnötigen Fetcher.

Keine Version wird nur deshalb gewählt, weil sie im Pflichtenheft ursprünglich genannt wurde.

## 5. A4 – Windows Media Identity

**Status:** NOT STARTED

Test: NTFS/exFAT/FAT32, USB Flash/externe SSD, detach/attach, anderer Laufwerksbuchstabe, gleiches Label, Klon/Ambiguität, Neustart offline.

Fallback bei Ambiguität: user-assisted binding; niemals aggressives Auto-Merge.

## 6. A5 – Tika vs. Toxy

**Status:** NOT STARTED

Verglichen wird Toxy gegen die in A3 gewählte Tika-Linie, nicht gegen eine veraltete Annahme.

Bewertung: Textvollständigkeit 25, Robustheit 20, Formatbreite 15, Metadaten 10, Performance 10, RAM 5, Packaging 5, Isolation 5, Maintenance/Lizenz 5.

## 7. G0

G0 verlangt:

- A1 mindestens Conditional Go + Rest-Evidence klar,
- A2–A5 entschieden,
- ADRs entsprechend aktualisiert,
- .NET-8-Lifecycle-/Migrationsentscheidung,
- CI-Minimum grün,
- keine normative Dokumentkollision.
