# Quality Maturity Level

**Stand:** 6. Oktober 2026  
**Current level:** **Q0 – Architecture Validation (A1 Conditional Go, A2 next)**

Dieses Modell beschreibt **Qualitätsreife**, nicht Funktionsumfang oder Marketingversion.

| Level | Bedeutung | Mindestnachweis |
|---|---|---|
| Q0 | Architecture Validation | dokumentierte Baseline, PoCs/Gates, keine behauptete Produktionsreife |
| Q1 | Vertical Slice | echter End-to-End-Pfad, automatisierte Kernintegration, Recovery-Grundtest |
| Q2 | MVP Quality | alle 0.5-MVP-Szenarien, Security-/Parserlimits, Suchqualität, Recovery |
| Q3 | Release Candidate | Installer, Migration/Restore, Accessibility, Performance, Security, SBOM |
| Q4 | Stable 1.0 | vollständige MUSS-/Change-Traceability, Acceptance Run, keine offenen Critical/High Findings |

## Aktueller Zustand

A1 beweist Host-/Lifecycle-Grundlagen, aber noch keine Crawler-/Search-Produktfunktion. Daher bleibt das Projekt auf Q0.

A2–A5 sind Bestandteil von Q0 und dürfen nicht als Endnutzerfeatures gezählt werden.

Der Übergang auf Q1 erfolgt erst mit bestandenem G0 und dem lokalen 0.1-Vertical-Slice.

## Regel

`build green` oder `tests green` allein erhöht das Quality Level nicht. Das jeweils definierte Gate und dessen Evidence müssen vollständig zusammenpassen.
