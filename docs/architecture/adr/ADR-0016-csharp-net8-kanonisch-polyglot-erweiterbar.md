# ADR-0016: C#/.NET 8 kanonisch, Repository polyglot-erweiterbar

**Status:** Accepted  
**Datum:** 22. Oktober 2026

## Kontext

Die produktive Desktopversion muss auf verwalteten Windows-PCs nutzbar sein, auf denen keine zusätzliche Go-Runtime installiert werden darf. Gleichzeitig soll das Repository spätere Komponenten, Werkzeuge oder alternative Implementierungen in weiteren Programmiersprachen aufnehmen können.

## Entscheidung

Die kanonische Version 1.0 wird in **C# auf .NET 8** als Windows-Forms-Anwendung entwickelt.

Die bestehende Struktur `src/`, `tests/` und `tools/` bleibt die kanonische C#/.NET-8-Codebasis und wird nicht nur für eine kosmetische Polyglot-Struktur verschoben.

Zusätzliche Programmiersprachen werden unter folgendem Namensraum isoliert:

```text
languages/
  <language>/
    README.md
    src/
    tests/
    tools/
```

Nicht jede Sprache benötigt alle drei Unterordner.

Für jede zusätzliche Sprache müssen Build, Test, Runtimeanforderungen, Lizenz und Zweck dokumentiert sein. Wenn ein sprachübergreifender Prozess- oder Datenvertrag entsteht, wird er an einer neutralen Contract-Grenze dokumentiert, statt Domainlogik mehrfach unabhängig zu erfinden.

Eine zusätzliche Sprache darf keine zwingende V1-Laufzeitabhängigkeit erzeugen, solange dies nicht durch einen neuen Change Request ausdrücklich beschlossen wird. Insbesondere setzt die V1 keine installierte Go-Runtime voraus.

## Folgen

### Positiv

- Hochschul-/Managed-PC-Kompatibilität der V1 bleibt erhalten.
- C#/.NET-8-Baseline bleibt eindeutig.
- spätere Experimente in C++, Java, Go oder anderen Sprachen sind strukturell möglich, ohne die Haupt-Solution zu vermischen;
- sprachspezifische CI kann bei Bedarf additiv ergänzt werden.

### Negativ / Trade-offs

- mehrere Toolchains erhöhen langfristig Wartungs- und CI-Aufwand;
- parallele Implementierungen können divergieren;
- gemeinsame Verträge müssen bewusst versioniert werden.

## Verifikation

- `Sasd.Crawler.sln` bleibt kanonischer C#/.NET-8-Build.
- der Windows-V1-Publish benötigt keine installierte Go-Runtime;
- zusätzliche Sprachbereiche dokumentieren ihren Build vollständig;
- keine sekundäre Sprache darf stillschweigend zur Voraussetzung für den Kernpfad werden.
