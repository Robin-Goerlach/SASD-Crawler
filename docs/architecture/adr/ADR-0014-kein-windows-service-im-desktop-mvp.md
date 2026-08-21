# ADR-0014: Kein Windows Service im Desktop-MVP

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Service erhöht Privileg-/Installationskomplexität.

## Entscheidung

Tray/Autostart/Background Mode genügen für 1.0 Desktop.

## Positive Folgen

- einfacher Betrieb
- per-user Security

## Negative Folgen / Trade-offs

- Crawler läuft nicht bei ausgeloggtem Benutzer

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
