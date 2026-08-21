# FIRST TASK 0.0.1 – WinForms Host Lifecycle Architecture Spike


> Ready-to-paste autonomous Codex prompt: [`PROMPT-0.0.1-AUTONOMOUS.md`](PROMPT-0.0.1-AUTONOMOUS.md)
**Milestone:** 0.0.1  
**Gate:** A1  
**Type:** disposable architecture spike / evidence task  
**Production features:** none  
**Target:** Visual Studio 2022, .NET 8, Windows Forms

## 1. Objective

Prove that the planned desktop process lifecycle is technically sound before crawler/search code is introduced.

Build a minimal solution that combines:

- Windows Forms;
- `Microsoft.Extensions.Hosting`;
- dependency injection;
- one `BackgroundService`;
- SQLite;
- cancellation;
- graceful shutdown;
- notification-area/tray operation;
- per-user application data directory;
- single-instance enforcement;
- activation of the first instance from a second launch.

Do **not** add Lucene, Tika, OCR, USB or crawler functionality in this task.

## 2. Required solution shape

Keep it small.

Suggested:

```text
src/
  Sasd.Crawler.Spike.A1.Core/
  Sasd.Crawler.Spike.A1.Infrastructure/
  Sasd.Crawler.Spike.A1.WinForms/

tests/
  Sasd.Crawler.Spike.A1.Tests/
```

If fewer projects can prove the architecture cleanly, prefer fewer projects.

## 3. Functional behavior

### Main window

Show:

- application name;
- worker status;
- timestamp of most recent background heartbeat;
- button to write/read a small SQLite record;
- button to minimize to tray;
- clean Exit command.

### Background worker

Every few seconds:

- update an in-memory heartbeat;
- persist a harmless heartbeat/status row in SQLite;
- publish a presentation-safe notification.

Do not update controls directly from the worker thread.

### Tray

- icon present while app is running;
- Open;
- Pause/Resume worker if practical;
- Exit.

### Single instance

Use a named mutex or equivalent per-user mechanism.

When a second instance starts:

- do not start another SQLite writer/worker;
- signal the first instance over a named pipe or similarly safe local IPC;
- activate/restore the existing window;
- then exit.

### Shutdown

On Exit:

1. stop scheduling new work;
2. cancel worker;
3. wait for reasonable graceful completion;
4. close SQLite;
5. dispose host/services;
6. exit without orphan process.

## 4. Storage

Use:

```text
%LOCALAPPDATA%\SASD\Crawler\spikes\a1\
```

or an equivalent temporary per-user project path.

Do not write to Program Files or require elevation.

## 5. Tests

At minimum automate:

- application service can persist/retrieve heartbeat;
- cancellation stops worker;
- shutdown leaves SQLite openable;
- stale/previous run state is recoverable;
- single-instance coordination logic is unit/component tested where practical.

Manual evidence:

- screenshot of running UI;
- tray screenshot;
- second-instance activation;
- forced kill followed by successful restart.

## 6. Logging

Structured local logging.

No document content or secrets.

Record:

- process start/stop;
- host start/stop;
- worker lifecycle;
- SQLite path;
- second-instance activation;
- shutdown duration.

## 7. Acceptance / Gate A1

A1 is GO only if:

- UI remains responsive while worker writes to SQLite;
- normal shutdown completes cleanly;
- forced process termination does not make SQLite unusable;
- second instance does not start competing worker/storage access;
- tray lifecycle is predictable;
- no administrator privileges are required;
- tests pass.

## 8. Evidence

Create:

```text
docs/evidence/0.0.1/
  SUMMARY.md
  TESTS.md
  screenshots/
```

`SUMMARY.md` must include:

- OS version;
- .NET SDK/runtime;
- branch;
- commit SHA;
- build command/result;
- test command/result;
- manual test results;
- issues found;
- A1 decision: GO / CONDITIONAL GO / NO-GO.

## 9. Repository governance

Follow root:

- `AGENTS.md`
- `RULES.md`
- `ROADMAP.md`

Codex may autonomously create/edit/build/test/commit/push the work branch according to those rules.

## 10. Non-goals

Do not:

- implement the real crawler;
- add Lucene;
- add Tika;
- add OCR;
- add network sources;
- implement final UI styling;
- install system-wide services;
- modify registry/firewall;
- require administrator rights.

This spike exists to validate process/lifecycle architecture only.
