# ADR-0002: Modularer Desktop-Monolith mit Clean Architecture und MVP

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Eine große WinForms-Anwendung darf nicht in Forms/Code-behind kollabieren.

## Entscheidung

Domain, Application, Infrastructure und Presentation werden getrennt; WinForms nutzt MVP.

## Positive Folgen

- testbar
- UI austauschbar
- späterer Service Host möglich

## Negative Folgen / Trade-offs

- mehr Projekte und anfängliche Struktur

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
