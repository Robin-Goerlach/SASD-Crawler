# ADR-0003: Per-user Desktopbetrieb als Standard

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

Ein zentral privilegierter Crawler würde ACL-Komplexität und Betriebsaufwand unnötig früh erzwingen. Für den Desktopmodus kann die Windows-Benutzeridentität selbst als natürliche Sicherheitsgrenze dienen.

## Entscheidung

Version 1.0 läuft standardmäßig im Sicherheitskontext des angemeldeten Windows-Benutzers. Daten, Index und Cache liegen unter dem Benutzerprofil. Der Crawler soll im Standardmodus nur lesen können, was dieser Benutzer lesen kann.

Ein späterer Shared Mode ist eine eigene Host-/Security-Ausbaustufe.

## Positive Folgen

- natürliche Security Boundary,
- kein privilegierter Windows Service nötig,
- keine zusätzliche Benutzerverwaltung im Desktop-MVP,
- einfacher Installer/Betrieb.

## Negative Folgen / Trade-offs

- Scheduling läuft nur, solange die Benutzerinstanz aktiv ist,
- zentraler 24/7-Mehrbenutzerbetrieb kommt später.

## Verifikation

A1 nutzt bereits einen per-user Datenpfad und per-user Single-Instance-Koordination. Die eigentliche Source-/Search-Security wird in späteren Milestones weiter verifiziert.
