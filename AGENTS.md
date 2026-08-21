# AGENTS.md – SASD Crawler

This file contains repository-wide instructions for Codex and other coding agents.

## 1. Mission

Develop SASD Crawler as a **native Windows Forms application for .NET 8** according to the repository baseline.

The product is an index-in-place crawler and search application for:

- local files,
- removable/offline media,
- SMB/UNC shares,
- websites,
- Office/PDF and other supported documents,
- OCR content.

Do not silently turn the product into a DMS, file-sync tool, cloud SaaS, workflow engine or AI-first product.

## 2. Source of truth

Read these before making architectural or milestone-level changes:

1. `BASELINE-AND-CHANGE-CONTROL.md`
2. `docs/baseline/LASTENHEFT.md`
3. `docs/baseline/LASTENHEFT-AMENDMENT-0.1a.md`
4. `docs/baseline/PFLICHTENHEFT.md`
5. `docs/baseline/ARCHITECTURE.md`
6. relevant ADRs under `docs/architecture/adr/`
7. `ROADMAP.md`
8. `PROJECT-STATUS.md`

If documents conflict, follow the baseline/change-control rules. Do **not** resolve normative conflicts silently.

## 3. Current technical baseline

Unless an accepted ADR/change request says otherwise:

- UI: Windows Forms.
- Framework: `net8.0-windows`.
- Architecture: modular desktop monolith, Clean Architecture boundaries, MVP presentation pattern.
- Control store: SQLite.
- Search: `ISearchIndex`, preferred v1 implementation Lucene.NET after PoC.
- Document extraction: Apache Tika sidecar as reference implementation.
- OCR: Tesseract behind `IOcrEngine`.
- Default security: per-Windows-user desktop instance.
- No mandatory Windows Service for the desktop MVP.
- Shared/server mode is later work.

## 4. Autonomy

For normal repository-local work, **act autonomously**.

Do not ask for confirmation for routine actions such as:

- reading/searching repository files;
- creating or editing source/tests/docs inside the repository;
- creating directories inside the repository;
- restoring/building/testing/formatting .NET code;
- adding appropriate NuGet dependencies when necessary and compatible with the architecture/license/security constraints;
- creating short-lived feature branches;
- staging changes;
- committing coherent changes;
- pushing the current non-protected work branch;
- creating a pull request when the task is complete and repository workflow supports PRs;
- viewing CI, issues and pull-request state.

Ask for human input only when a **material product/architecture decision is genuinely unresolved** or an action would cross the safety boundaries in `RULES.md`.

## 5. Never claim unverified work

The project distinguishes:

```text
specified
implemented
verified
released
```

Never mark a requirement, roadmap item or milestone `DONE` merely because code was written or a build succeeded.

A `DONE` state requires the applicable exit criteria and evidence.

## 6. Requirement traceability

Whenever practical, connect work to requirement IDs.

Examples:

```text
feat(USB-004): keep offline media searchable
test(WEB-009): verify robots policy
fix(IDX-003): prevent delete on incomplete scan
```

Tests should include the requirement ID in the test name, trait, comment or nearby documentation when useful.

Update `REQUIREMENTS-STATUS.md` only when there is actual implementation/test evidence.

## 7. Development style

Prefer vertical slices over large speculative frameworks.

Do not create dozens of empty interfaces/projects simply because the architecture lists future modules.

Build the smallest coherent end-to-end path for the current milestone.

### WinForms rules

Forms/UserControls may:

- gather user input,
- call presenters/application services,
- render presentation models,
- manage UI-only state.

Forms/UserControls must not:

- execute SQL directly,
- open Lucene directly,
- crawl directories,
- run Tika/Tesseract directly,
- decide reconciliation/deletion rules,
- contain credentials,
- become large business-logic code-behind files.

### C# rules

- nullable reference types enabled;
- async for I/O;
- pass `CancellationToken` through long operations;
- no `.Result` / `.Wait()` in normal UI/application paths;
- avoid static service locators;
- prefer explicit single-responsibility types;
- avoid generic `Helper`, `Utils`, `Manager` catch-all classes;
- comments should explain **why**, invariants and non-obvious failure modes rather than restating obvious code.

## 8. Safety-critical product invariants

These are architecture invariants, not optional implementation details:

### Reconciliation

A missing root, offline USB medium, inaccessible SMB share, aborted scan or global I/O failure must **not** be interpreted as mass deletion.

Only a successfully completed authoritative scan may advance source-wide missing/delete reconciliation.

### Removable media

Drive letters are not stable identity.

Use internal `MediaId` plus relative paths and carefully validated volume identity signals.

Ambiguity must fail safely; do not merge two media automatically just because they look similar.

### Search index

The search index is derived/rebuildable.

Do not make Lucene the only source of truth for sources, media, jobs or document lifecycle state.

### Parser/OCR

Treat documents and parser output as untrusted.

Respect size, archive, time and process boundaries.

### UI thread

Crawler, parser, OCR, hashing and index rebuild must never execute as blocking work on the WinForms UI thread.

## 9. Tests

For each change, run the smallest relevant tests first, then the broader applicable suite.

Before considering milestone work complete:

- unit tests;
- component/integration tests;
- negative/failure cases;
- recovery tests where persistence is touched;
- security tests where untrusted input/auth/secrets are touched;
- manual/hardware evidence where automation cannot prove USB/Windows behavior.

Do not hide skipped/failing tests.

## 10. Documentation updates

Update documentation when implementation changes:

- current status,
- roadmap status,
- requirement status,
- ADR/change request if architecture changed,
- evidence template/results,
- known limitations.

Do not rewrite baseline requirements just to match an implementation shortcut.

## 11. Git discipline

Use coherent commits.

Do not commit:

- secrets,
- access tokens,
- generated user data,
- large document corpora unless intentionally versioned,
- machine-specific IDE caches,
- temporary parser/OCR outputs,
- build outputs.

Safe Git/GitHub command policy is defined in `RULES.md`.

## 12. Stop conditions

Stop autonomous execution and report clearly if:

- a required action would delete or rewrite user data outside the repository;
- administrator elevation is required;
- repository history would need force-rewriting;
- credentials/secrets would need to be revealed or changed;
- a normative architecture conflict cannot be resolved from accepted documents;
- a test demonstrates a credible data-loss/security flaw.

Do not work around these boundaries silently.


## 13. Project execpolicy and approval behavior

`.codex/rules/default.rules` is part of this project's operating model.

Routine operations are intentionally autonomous:

- normal `git *`;
- normal `dotnet *`;
- normal `gh *`;
- read-only PowerShell repository inspection;
- `rg`;
- `dotnet restore`, `dotnet build`, `dotnet test`, `dotnet publish`;
- `git add`, `git commit`, normal `git push`;
- `gh pr create/view/checks`;
- `gh run view/watch`;
- `gh release view/create/upload`.

The following require approval and must not be worked around:

- `dotnet ef database drop`;
- `git reset`, `git clean`, `git rebase`;
- `git commit --amend`;
- `git branch -D`, `git tag -d`, `git stash drop`;
- force-push and remote-delete variants;
- `Remove-Item`, `Clear-Content`, `Stop-Process`;
- `gh repo delete/rename/transfer`;
- `gh release delete`;
- `gh secret ...`;
- `gh api ...`.

### Command composition

For approval-relevant shell actions, do not combine unrelated commands into long PowerShell chains. Use routine commands individually or in short logically related groups so project rules match reliably.

Do not interrupt autonomous work for pure reading, building, testing, normal Git operations, or normal GitHub inspection/release creation.

### Risky Git push normalization

`prefix_rule` is prefix-based. If a risky push is ever needed, write the risky token immediately after `git push`:

```text
git push --force ...
git push --force-with-lease ...
git push -f ...
git push --delete ...
```

Never move the risky flag later in the command to bypass the prompt rule.
