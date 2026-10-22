# Multi-language development

This directory is the extension boundary for SASD-Crawler components written in programming languages other than the canonical C#/.NET 8 implementation.

## Canonical product

The Version 1.0 desktop product remains:

- C#;
- .NET 8 / `net8.0-windows`;
- Windows Forms;
- `Sasd.Crawler.sln` as the canonical build entry point.

The existing top-level `src/`, `tests/` and `tools/` directories therefore remain the C#/.NET codebase.

## Layout for additional languages

When a second language is actually introduced, use:

```text
languages/
  <language>/
    README.md
    src/
      frontend/   # only when that language implements a frontend
      indexer/    # only when that language implements indexer components
      shared/     # only for an explicitly versioned cross-language boundary
    tests/
    tools/
```

Only create directories that contain real code or documentation; do not add empty scaffolding.

Each language README must state:

1. purpose and architectural boundary;
2. supported toolchain/compiler version;
3. build command;
4. test command;
5. runtime/deployment requirement;
6. external dependencies and license constraints;
7. how the component communicates with the canonical C# application.

## Runtime rule

A secondary language must not become a hidden runtime prerequisite for the Windows V1 client.

In particular, V1 must not require an installed Go runtime. Any future mandatory runtime or sidecar changes require an explicit architecture/change-control decision.

## Architecture rule

Prefer stable process/data contracts over duplicate business logic. Domain rules, reconciliation invariants, source identity and deletion safety remain authoritative in the canonical architecture unless an explicit cross-language contract is approved.

ADR-0017 also separates the canonical C# code into frontend/shared/indexer. A later alternative-language indexer should therefore live under `languages/<language>/src/indexer/` and integrate through an approved contract rather than reaching into WinForms internals.

## CI rule

A language-specific CI job is added when the first maintained component in that language is introduced. Until then the Windows .NET 8 CI remains the release gate for the canonical product.
