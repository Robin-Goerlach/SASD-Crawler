# ADR-0013: Index und Extraction Cache sind rekonstruierbar

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Suchindex darf nicht einzige Source of Truth sein.

## Entscheidung

SQLite/Originalquellen führen; Index/Cache können neu aufgebaut werden.

## Positive Folgen

- Recovery
- Migration

## Negative Folgen / Trade-offs

- Rebuild kann lange dauern

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
