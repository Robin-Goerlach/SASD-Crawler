# Pre-G0 C# Production Foundation – Tests

**CI-Lauf:** #22  
**Code-Commit:** `10a4809008b4ad4ece83dfc39dd78d443e146d55`

## CI-Schritte

- Restore: success
- `dotnet format --verify-no-changes`: success
- Release build: success
- Tests: success

## Testzahlen

Gesamt-Solution:

- Passed: **51**
- Failed: **0**
- Skipped: **0**

Neue Produktionstests:

| Projekt | Tests |
|---|---:|
| `Sasd.Crawler.Domain.Tests` | 3 |
| `Sasd.Crawler.Application.Tests` | 2 |
| `Sasd.Crawler.Persistence.Sqlite.Tests` | 3 |
| `Sasd.Crawler.Frontend.Tests` | 2 |
| `Sasd.Crawler.Architecture.Tests` | 4 |
| **Summe neu** | **14** |

## Nachgewiesene Eigenschaften

- Source IDs sind nicht leer und bleiben stabile Domainwerte.
- Source Enable/Disable ist idempotent.
- Canonical Local Directory Registration erzeugt keine doppelte Quelle.
- SQLite persistiert Source State über Repository-Instanzen hinweg.
- SQLite-Schema-Initialisierung ist idempotent.
- Canonical Locations sind im Store case-insensitive eindeutig.
- Presenter funktioniert ohne WinForms/SQLite.
- Domain kennt keine anderen SASD-Crawler-Assemblies.
- Application kennt keine Frontend-/SQLite-Implementierung.
- Presenter-Assembly kennt weder SQLite noch WinForms.
- SQLite-Indexer-Persistence kennt das Frontend nicht.

## Nicht durch Automation bewiesen

- visuelles Layout/Bedienbarkeit auf dem Ziel-Hochschul-PC;
- tatsächlicher Dateisystem-Crawl;
- Reconciliation und Löschsicherheit;
- Search-End-to-End;
- self-contained Publish/Installation.

Diese Punkte bleiben spätere Evidence und dürfen aus diesem Testlauf nicht abgeleitet werden.
