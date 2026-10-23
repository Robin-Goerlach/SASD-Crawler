# Production source layout

The production code starting with milestone 0.1 is organized by runtime responsibility rather than by technology alone.

```text
src/
  shared/     UI- and indexer-independent domain/application/contracts
  frontend/   WinForms presentation and desktop composition
  indexer/    crawling, persistence, search, extraction, OCR and background work
```

Historical `Sasd.Crawler.Spike.*` projects are stored under `archive/spikes/`. They are architecture evidence and are intentionally outside the active production source tree.

See ADR-0017 and CR-2026-003 for the authoritative dependency rules.
