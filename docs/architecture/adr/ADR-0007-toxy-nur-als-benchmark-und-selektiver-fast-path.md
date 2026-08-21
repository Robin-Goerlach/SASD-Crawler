# ADR-0007: Toxy nur als Benchmark und selektiver Fast Path

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Toxy ist .NET-nativ, aber kleiner als Tika.

## Entscheidung

Toxy ersetzt Tika nur für Formate, bei denen Benchmark/Qualität überzeugen.

## Positive Folgen

- möglicher geringerer Runtimeaufwand

## Negative Folgen / Trade-offs

- zwei Parserpfade erhöhen Testaufwand

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
