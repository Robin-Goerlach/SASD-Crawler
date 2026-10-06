# ADR-0001: Windows Forms und .NET 8 als Zielplattform

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

Für die erste Produktgeneration ist eine native Windows-Desktopanwendung gewünscht. USB/Offline-Medien, UNC/SMB, Windows-Identität und eine klassische Desktop-Suche sind Kern des Produkts.

## Entscheidung

Windows Forms ist die Primär-UI und `net8.0-windows` das aktuelle Entwicklungsziel. Version 1.0 ist Windows-first.

Das Frameworkziel ist zentral zu halten, weil .NET 8 am 10.11.2026 aus dem Herstellersupport fällt. G0 muss deshalb den Upgradezeitpunkt auf eine unterstützte LTS-Version vor produktiver 1.0 festlegen.

## Positive Folgen

- native Windows-UX,
- sehr gute USB/UNC/Windows-Integration,
- Visual-Studio-2022-freundlich,
- einfacher per-user Desktopbetrieb.

## Negative Folgen / Trade-offs

- Windows-only 1.0,
- zeitnah notwendige Frameworkmigration,
- Linux/Container erst späterer Service-Host.

## Verifikation

A1 hat WinForms + .NET 8 + Generic Host automatisiert mit Build/Tests bestätigt. Manuelle Desktop-Smokes bleiben A1-Evidence, ändern aber diese Produktentscheidung nicht.
