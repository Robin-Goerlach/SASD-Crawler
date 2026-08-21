# ADR-0003: Per-user Desktopbetrieb als Standard

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Ein zentral privilegierter Crawler würde ACL-Komplexität unnötig früh erzwingen.

## Entscheidung

Version 1.0 läuft standardmäßig im Windows-Kontext des angemeldeten Benutzers und speichert Daten in dessen LocalAppData.

## Positive Folgen

- natürliche Security Boundary
- kein privilegierter Service nötig

## Negative Folgen / Trade-offs

- kein zentraler 24/7-Crawl ohne laufende Benutzerinstanz

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
