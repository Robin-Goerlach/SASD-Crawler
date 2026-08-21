# ADR-0012: SMB über Windows/UNC im Benutzerkontext

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Windows kann SMB stabiler als ein eigener Protokollstack.

## Entscheidung

UNC/gemappte Pfade werden mit aktuellen Windows-Rechten verarbeitet.

## Positive Folgen

- keine Passwortkopie
- native Security

## Negative Folgen / Trade-offs

- Spezialcredentials später separat

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
