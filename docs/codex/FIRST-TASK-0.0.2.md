# FIRST TASK 0.0.2 – Lucene.NET Search Backend Architecture Spike

**Milestone:** 0.0.2  
**Gate:** A2  
**Type:** architecture/performance/recovery spike  
**Current package baseline:** Lucene.NET 4.8.0-beta00018  
**Target:** .NET 8 / Windows

> Ready-to-paste autonomous Codex prompt: [`PROMPT-0.0.2-AUTONOMOUS.md`](PROMPT-0.0.2-AUTONOMOUS.md)

## Objective

Determine with measurements whether Lucene.NET is suitable as the embedded v1 search backend behind `ISearchIndex`.

This is **not** the production crawler and must not expand into Tika, USB, SMB or Web work.

## Required capability matrix

1. create/open/reopen an index;
2. index 100,000 documents;
3. index 1,000,000 synthetic documents for scale evidence;
4. deterministic update by stable document ID;
5. delete;
6. near-real-time search/refresh;
7. German analyzer;
8. English analyzer;
9. phrase search;
10. Boolean AND/OR/NOT;
11. fuzzy;
12. prefix and bounded wildcard behavior;
13. highlighting/snippets feasibility;
14. faceting/filter feasibility;
15. concurrent readers with one coordinated writer;
16. process-kill/reopen recovery;
17. full rebuild;
18. metrics: indexing docs/s, index size, RAM/working set when practical, query p50/p95, update/delete latency.

## Corpus

Use deterministic generated content with:

- German/English text,
- umlauts/ß,
- filename/title/content fields,
- source/type/date/availability fields,
- enough repeated and rare terms to exercise ranking/filtering.

The 1M corpus must be reproducible from a seed and not committed as raw generated files.

## Packages

Prefer exact `4.8.0-beta00018` versions for the spike and record them explicitly. Likely modules include Core, Analysis.Common, QueryParser, Highlighter and Facet only where required by the test.

Do not add OpenSearch just to make Lucene pass; OpenSearch is the documented fallback decision if A2 fails.

## Test architecture

Keep A2 isolated from A1. Suggested layout:

```text
src/Sasd.Crawler.Spike.A2.Search/
tests/Sasd.Crawler.Spike.A2.Tests/
benchmarks/Sasd.Crawler.Spike.A2.Benchmarks/
```

A smaller layout is acceptable if cleaner.

Use temp directories outside the repository for large index data. Ensure cleanup is safe and scoped.

## Recovery test

At least one test/harness must create an external worker process, perform writes, terminate it ungracefully, then reopen the index and verify:

- committed data is readable,
- the index is not corrupt,
- incomplete work is not misreported as durable,
- subsequent writes/searches still work.

## Decision criteria

### GO
Embedded Lucene is stable for tested workload, recovery succeeds, required search features are practical, and ordinary 100k queries are consistent with the product latency target (median <300 ms, p95 <1 s on documented reference hardware).

### CONDITIONAL GO
Desktop use is acceptable but specific limits are documented (for example scale/shared/vector path should use OpenSearch later).

### NO-GO
Corruption/recovery concerns, unacceptable performance/resource behavior or missing required features make embedded Lucene unsuitable.

A slower benchmark on weak hardware alone is not an automatic NO-GO; report hardware and bottleneck.

## Evidence

Create:

```text
docs/evidence/0.0.2/
  SUMMARY.md
  TESTS.md
  BENCHMARKS.md
  RECOVERY.md
```

Do not commit generated million-document indexes.

## Gate update

Only after evidence exists:

- update `ROADMAP.md`,
- update `PROJECT-STATUS.md`,
- update `CURRENT-CHECKLIST.md`,
- update ADR-0005 / `DECISION-LOG.md`,
- update relevant risks.

Do not mark product search requirements implemented: A2 is a spike.
