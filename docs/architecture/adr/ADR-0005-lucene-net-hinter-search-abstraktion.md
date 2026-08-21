# ADR-0005: Lucene.NET hinter Search-Abstraktion

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Desktop-Volltextsuche soll ohne separaten Search Server funktionieren.

## Entscheidung

Lucene.NET wird als v1-Referenzbackend verwendet, aber nur über `ISearchIndex`.

## Positive Folgen

- embedded
- leistungsfähig
- kein Server

## Negative Folgen / Trade-offs

- formaler Beta-Status
- PoC erforderlich

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
