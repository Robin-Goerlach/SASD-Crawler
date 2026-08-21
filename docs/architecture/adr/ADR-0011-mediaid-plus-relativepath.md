# ADR-0011: MediaId plus RelativePath

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Laufwerksbuchstaben sind für USB nicht stabil.

## Entscheidung

Wechseldatenträger bekommen interne MediaId; Dokumente speichern RelativePath.

## Positive Folgen

- Offline-Suche
- Drive-letter-unabhängig

## Negative Folgen / Trade-offs

- Media Matching muss sorgfältig sein

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
