# Test layout

Production tests mirror the source boundaries:

```text
tests/
  shared/
  frontend/
  indexer/
  e2e/
```

Existing `Sasd.Crawler.Spike.*.Tests` projects remain at their historical paths as architecture-spike evidence.

Guidelines:

- shared tests cover domain/application invariants without UI or concrete infrastructure;
- frontend tests use fakes/contracts where possible and do not require a live crawler stack;
- indexer tests run without WinForms;
- e2e tests may compose frontend, shared and indexer;
- future architecture tests should enforce forbidden references between these boundaries.
