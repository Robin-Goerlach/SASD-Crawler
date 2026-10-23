# Archived architecture spikes

This directory contains the historical A1–A5 proof-of-concept projects that were used to validate major architecture choices before the canonical production code was started.

```text
archive/spikes/
  src/    historical spike implementations
  tests/  historical spike test projects
  tools/  historical benchmarks and probes
```

## Why these projects are archived

The active product architecture now lives under:

- `src/shared/`
- `src/frontend/`
- `src/indexer/`

and production tests live under the corresponding active `tests/` boundaries.

The spike projects are retained because they are useful technical evidence for decisions such as the WinForms/Generic Host lifecycle, Lucene feasibility, Tika sidecar behavior, Windows media identity and the Tika/Toxy comparison. They are **not** production modules and therefore should not appear beside active code in the top-level `src/`, `tests/` or `tools/` trees.

## Maintenance rule

Treat archived spike code as evidence:

- do not add normal product features here;
- do not make production code depend on archived projects;
- do not silently copy spike code into production;
- when a spike result is promoted, re-evaluate the behavior and implement it behind the current production contracts;
- keep accepted evidence documents under `docs/evidence/` and planning/gate documents authoritative for what a spike actually proved.

The integration solution may continue to compile these archived projects so historical evidence does not silently rot, but active frontend/indexer developer views must remain production-focused.
