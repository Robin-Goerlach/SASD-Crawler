# Contributing to SASD Crawler

## Before coding

Read:

1. `AGENTS.md`
2. `RULES.md`
3. `BASELINE-AND-CHANGE-CONTROL.md`
4. `ROADMAP.md`
5. the relevant baseline/ADR documents.

## Work item structure

Every meaningful task should identify:

- target milestone;
- affected requirement IDs;
- expected behavior;
- relevant architecture component;
- test plan;
- evidence to produce.

## Development workflow

```text
understand requirement
→ verify architecture
→ create/switch feature branch
→ implement smallest coherent slice
→ add/adjust tests
→ run build/tests
→ review diff
→ update status/traceability/evidence
→ commit
→ push branch
→ PR when appropriate
```

## Pull requests

A useful PR description should include:

- summary;
- requirement IDs;
- architecture impact;
- tests;
- manual evidence;
- known limitations;
- migration/security impact.

## Documentation

Do not allow docs to drift behind code.

Update `PROJECT-STATUS.md`, `ROADMAP.md` and `REQUIREMENTS-STATUS.md` when their state really changes.

## Architecture changes

Do not bury long-term architectural changes in implementation commits.

Create/update an ADR and, if scope or requirements change, a Change Request.
