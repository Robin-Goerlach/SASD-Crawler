# ADR-0015: Shared Mode als separater späterer Host

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Später kann zentrale Suche benötigt werden.

## Entscheidung

Application/Infrastructure werden so getrennt, dass ein Service Host später dieselben Module nutzt.

## Positive Folgen

- Zukunftspfad ohne UI-Neuschreibung

## Negative Folgen / Trade-offs

- Shared ACL/DB/Search erfordern zusätzliche Architektur

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
