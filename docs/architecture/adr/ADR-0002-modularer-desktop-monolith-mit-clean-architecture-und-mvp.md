# ADR-0002: Modularer Desktop-Monolith mit Clean Architecture und MVP

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

WinForms eignet sich sehr gut für das Produkt, darf aber nicht dazu führen, dass Datenbank-, Crawler-, Search- und Parserlogik im Form-Code zusammenläuft.

## Entscheidung

Der SASD-Crawler wird als modularer Desktop-Monolith mit klaren Domain-/Application-/Infrastructure-/Presentation-Grenzen entwickelt. Die WinForms-Präsentation folgt bevorzugt Model-View-Presenter.

## Positive Folgen

- testbare Application-/Domain-Logik,
- UI bleibt austauschbar,
- späterer Service Host kann dieselben Kernmodule verwenden,
- WinForms-Code bleibt klein und verständlich.

## Negative Folgen / Trade-offs

- etwas mehr Struktur als bei einer einfachen WinForms-App,
- Disziplin bei Abhängigkeitsrichtungen erforderlich.

## Verifikation

A1 bestätigt die Grundidee praktisch: `Program.cs` ist Composition Root, Services werden über DI registriert, der Presenter marshalt UI-Zustand, und die Form enthält keine SQLite-/Crawlerlogik.
