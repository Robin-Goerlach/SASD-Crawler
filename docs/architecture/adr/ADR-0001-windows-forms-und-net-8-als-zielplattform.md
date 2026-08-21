# ADR-0001: Windows Forms und .NET 8 als Zielplattform

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Der Benutzer wünscht eine native Windows-Forms-Anwendung für .NET 8.

## Entscheidung

WinForms ist die Primär-UI und `net8.0-windows` das Entwicklungsziel. Das Lifecycle-Risiko von .NET 8 wird separat behandelt.

## Positive Folgen

- Native Windows-UX
- sehr gute USB/UNC/Windows-Integration
- Visual-Studio-2022-freundlich

## Negative Folgen / Trade-offs

- Windows-only 1.0
- .NET-8-Supportende muss vor 1.0 neu bewertet werden

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
