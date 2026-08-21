# ADR-0010: Reconciliation ist Wahrheit; FileSystemWatcher nur Hinweis

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Watcher verlieren Events, besonders auf Netzfreigaben.

## Entscheidung

Nur erfolgreiche vollständige Scans dürfen Missing/Delete ableiten.

## Positive Folgen

- verhindert Massendeletion
- robust

## Negative Folgen / Trade-offs

- periodische Full Scans notwendig

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
