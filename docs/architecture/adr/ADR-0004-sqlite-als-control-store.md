# ADR-0004: SQLite als Control Store

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

Der Single-user-Desktop soll keine separate Datenbankinstallation benötigen. Quellen, Dokumentregister, Jobs, Reconciliation- und Queue-Zustände brauchen trotzdem einen transaktionalen führenden Store.

## Entscheidung

SQLite ist der Control-/Metadata-Store der Desktop-1.0-Baseline. Search Index und Extraction Cache bleiben davon getrennte, rekonstruierbare Artefakte.

## Positive Folgen

- einfaches Deployment,
- transaktional und backupfähig,
- sehr gut für Single-Node/per-user,
- funktioniert ohne Datenbankserver.

## Negative Folgen / Trade-offs

- späterer Shared/Scale-out-Mode kann ein anderes Persistenzbackend benötigen,
- SQLite und Search Index besitzen keine gemeinsame ACID-Transaktion; Idempotenz/Reconciliation sind erforderlich.

## Verifikation

A1 bestätigt SQLite/WAL, Persistenz, Wiederöffnen und Shutdown im geplanten Hostmodell. Produktionsschema, Queue und Reconciliation werden in späteren Slices separat verifiziert.
