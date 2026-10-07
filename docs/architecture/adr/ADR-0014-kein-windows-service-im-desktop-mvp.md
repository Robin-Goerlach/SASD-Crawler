# ADR-0014: Kein Windows Service im Desktop-MVP

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

Ein Windows Service würde bereits im MVP Installation, Privilegien, Benutzerkontext und IPC deutlich komplizierter machen. Der primäre 1.0-Modus ist eine per-user Desktopanwendung.

## Entscheidung

Der Desktop-MVP benötigt keinen Windows Service. Scheduling und Hintergrundarbeit laufen im WinForms-/Generic-Host-Prozess. Tray und optionaler Autostart reichen für den Desktopmodus.

Ein separater Service Host bleibt eine spätere Shared-/Enterprise-Ausbaustufe.

## Positive Folgen

- einfachere Installation und Diagnose,
- keine frühen Privilegien-/Service-Account-Probleme,
- konsistenter per-user Sicherheitskontext.

## Negative Folgen / Trade-offs

- keine 24/7-Verarbeitung nach Benutzerabmeldung,
- späterer Shared Mode benötigt zusätzlichen Host.

## Verifikation

A1 bestätigt BackgroundService, Tray, Single Instance und geordneten Shutdown im Desktopprozess. Die vier manuellen A1-Desktop-Smokes bleiben als Rest-Evidence offen.
