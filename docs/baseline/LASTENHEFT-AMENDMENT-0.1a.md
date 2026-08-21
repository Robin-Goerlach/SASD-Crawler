# Lastenheft-Amendment 0.1a – Desktop-/WinForms-Baseline

**Status:** DRAFT  
**Bezug:** Lastenheft 0.1  
**Change Request:** CR-2026-001  
**Datum:** 21. August 2026

Dieses Amendment ändert ausschließlich die Punkte, die der später gewählten Windows-Forms-Desktoparchitektur widersprechen. Alle übrigen 258 Anforderungen bleiben unverändert, sofern hier nicht ausdrücklich anders bestimmt.

## A-001 – PLAT-001 Windows

`PLAT-001` bleibt **MUSS 1.0**.

Präzisierung:

> Version 1.0 wird als native Windows-x64-Desktopanwendung ausgeliefert.

## A-002 – PLAT-002 Linux

Bisher:
> produktive Baseline muss Linux-x64 unterstützen.

Neu:
> Ein Linux-Host ist **nicht Bestandteil der Version 1.0**. Domain-/Application-Komponenten sollen soweit wirtschaftlich sinnvoll keine unnötigen Windows-Abhängigkeiten erhalten, damit ein späterer Linux-Service-Host möglich bleibt.

**Neue Priorität:** KANN / nach 1.0

## A-003 – PLAT-003 Container

Bisher:
> Serverkomponenten sollen bis 1.0 containerisierbar sein.

Neu:
> Containerbetrieb gehört nicht zur primären Desktop-1.0-Baseline. Er wird beim späteren Shared-/Service-Host neu bewertet.

**Neue Priorität:** KANN / nach 1.0

## A-004 – UI-013 Weboberfläche

Bisher:
> responsive Weboberfläche SOLL 1.0.

Neu:
> Version 1.0 besitzt eine native Windows-Forms-Oberfläche. Sie MUSS High-DPI-fähig, vollständig tastaturbedienbar und mit Windows-Accessibility-Technologien kompatibel sein.

Die Weboberfläche ist DEFERRED.

## A-005 – AUTH-002 bis AUTH-009

Für den **per-user Desktopmodus** gilt:

> Die Anwendung läuft im Sicherheitskontext des angemeldeten Windows-Benutzers und speichert Index/Cache in dessen Benutzerprofil.

Daher ist eine zweite interne Benutzer-/Gruppenverwaltung nicht Voraussetzung des Desktop-1.0.

Die fachliche Forderung „keine unberechtigten Treffer“ wird dadurch für den Desktopmodus über die Windows-Zugriffsgrenze erfüllt.

Für einen späteren Shared Mode bleiben:
- ACL Descriptor,
- Principal Tokens,
- Search-Time Security Trimming

weiterhin verbindliche Architekturziele.

## A-006 – API-001 Search API

Die Forderung nach programmatischer Suchbarkeit bleibt bestehen.

Für Version 1.0 reicht:
- stabiler Application-Service-Vertrag;
- optional CLI/Loopback-Adapter.

Ein remote erreichbarer HTTP-Server ist keine notwendige Primärkomponente der Desktopanwendung.

## A-007 – API-002 bis API-007

Die fachliche Automatisierbarkeit bleibt erhalten.

Zeitpunkt und Transport werden neu bewertet:
- intern Application Services;
- CLI bevorzugt;
- HTTP-API optional;
- Webhooks erst später.

## A-008 – UX-004 Accessibility

Die Zielsetzung bleibt MUSS.

Für WinForms lautet die Konkretisierung:
- Windows UI Automation;
- Keyboard;
- Access Keys;
- High Contrast;
- Screenreader-Smoke;
- 100–200 % DPI.

## A-009 – Datenschutz

Die per-user Desktoparchitektur verstärkt `PRIV-001`:
- keine Cloud nötig;
- Index/Cache lokal;
- externe KI später opt-in.

## A-010 – Release 0.6

0.6 wird neu interpretiert als:
> Desktop-Security-Härtung plus Shared-Mode-Vorbereitung

und nicht als vollständiger zentraler Multiuser-Server.

## Gültigkeit

Nach formaler Annahme von CR-2026-001 gilt dieses Amendment gemeinsam mit Lastenheft 0.1 als fachliche Baseline.
