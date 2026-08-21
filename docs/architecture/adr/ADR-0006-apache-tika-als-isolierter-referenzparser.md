# ADR-0006: Apache Tika als isolierter Referenzparser

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Office/PDF-Formatbreite ist mit Eigenparsern riskant.

## Entscheidung

Tika läuft als lokaler Sidecar unter Supervisor.

## Positive Folgen

- breite Formatunterstützung
- Parserfehler vom UI-Prozess isoliert

## Negative Folgen / Trade-offs

- JRE/Packaging-Aufwand

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
