# ADR-0008: Tesseract als OCR-Engine

**Status:** Proposed  
**Datum:** 21. August 2026

## Kontext

Scan-PDF und Bilder müssen lokal OCR-fähig sein.

## Entscheidung

Tesseract wird über `IOcrEngine` angebunden.

## Positive Folgen

- Open Source
- lokal
- Deutsch/Englisch

## Negative Folgen / Trade-offs

- CPU-intensiv

## Verifikation

Die Entscheidung gilt erst als `Accepted`, wenn die zugehörigen PoC-/Gate-Anforderungen erfüllt oder bewusst als Architekturentscheidung akzeptiert wurden.
