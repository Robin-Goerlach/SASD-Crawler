# Production source layout

The production code starting with milestone 0.1 is organized by runtime responsibility rather than by technology alone.

```text
src/
  shared/     UI- and indexer-independent domain/application/contracts
  frontend/   WinForms presentation and desktop composition
  indexer/    crawling, persistence, search, extraction, OCR and background work
```

The existing `Sasd.Crawler.Spike.*` projects remain at their historical paths. They are architecture evidence and are not the production layout.

See ADR-0017 and CR-2026-003 for the authoritative dependency rules.
