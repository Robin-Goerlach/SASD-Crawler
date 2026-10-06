# ADR-0015: Shared Mode als separater späterer Host

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

Langfristig kann ein zentraler Mehrbenutzer-/Enterprisebetrieb sinnvoll werden. Dies darf den Desktop-MVP aber nicht mit Server-, HA- und ACL-Komplexität überladen.

## Entscheidung

Ein späterer Shared Mode wird als separater Host aufgebaut, der dieselben Application-/Domain-Verträge nutzt. Die WinForms-Anwendung bleibt Produktoberfläche; Persistenz-, Search- und Security-Backends dürfen für den Shared Mode ausgetauscht/erweitert werden.

## Positive Folgen

- Desktop-MVP bleibt klein und sicher,
- Zukunftspfad zu zentraler Suche/Agents/OpenSearch bleibt offen,
- kein UI-Neuschreiben erforderlich, wenn Application Contracts sauber bleiben.

## Negative Folgen / Trade-offs

- Shared Mode ist ein eigenständiger späterer Architektur-/Betriebsaufwand,
- Search-Time ACLs und zentrale Identity werden erst dann vollständig benötigt.

## Verifikation

Die Architekturgrenzen werden bereits in A1 gepflegt. Shared-Mode-Funktionalität selbst ist bewusst nach 1.0 beziehungsweise in späteren Enterprise-Meilensteinen zu verifizieren.
