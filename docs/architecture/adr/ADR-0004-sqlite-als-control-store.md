# ADR-0004: SQLite als Control Store

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Single-user Desktop soll keine Datenbankinstallation benötigen.

## Entscheidung

SQLite speichert Quellen, Dokumentregister, Jobs, Queue und Status.

## Positive Folgen

- einfaches Deployment
- transaktional
- backupfähig

## Negative Folgen / Trade-offs

- späterer Shared Mode benötigt ggf. anderes Backend

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
