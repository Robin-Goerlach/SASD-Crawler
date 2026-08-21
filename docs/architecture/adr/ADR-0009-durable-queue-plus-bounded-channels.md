# ADR-0009: Durable Queue plus bounded Channels

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

In-memory Queues verlieren Arbeit bei Crash, reine DB-Pollingqueues sind ineffizient.

## Entscheidung

SQLite ist dauerhaft; leased Items gehen in bounded Channels zu Workern.

## Positive Folgen

- Recovery
- Backpressure
- Performance

## Negative Folgen / Trade-offs

- mehr Zustandslogik

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
