# A2 – automatisierte Verifikation

**Datum:** 28. August 2026

## Befehl

```powershell
dotnet test Sasd.Crawler.sln --configuration Release
```

## Ergebnis

```text
Sasd.Crawler.Spike.A1.Tests: 9 passed, 0 failed, 0 skipped
Sasd.Crawler.Spike.A2.Tests: 9 passed, 0 failed, 0 skipped
Sasd.Crawler.Spike.A3.Tests: 6 passed, 0 failed, 0 skipped
Sasd.Crawler.Spike.A4.Tests: 13 passed, 0 failed, 0 skipped
Gesamt: 37 passed, 0 failed, 0 skipped
```

Die A2-Tests belegen Query-Varianten, Sprach-Stemming, Update/Delete ohne Alt-Treffer,
Highlight/Facetten/Filter, persistente Wiederöffnung, koordinierte parallele Updates,
Lesen während Updates und explizites Versagen bei ungültigem Indexpfad.
Zusätzlich simuliert ein Byte-budgetierter Lucene-Directory-Wrapper einen vollen Datenträger
und verifiziert Writer-Sperre, Rollback sowie Wiederöffnung des letzten vollständigen Commits.

## Reproduzierbarer Benchmark

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A2.Benchmark/Sasd.Crawler.Spike.A2.Benchmark.csproj --configuration Release -- --index <path> --documents 1000000 --queries 100
```

Der Crash-Modus wird absichtlich mit einem Nicht-Null-Exit beendet:

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A2.Benchmark/Sasd.Crawler.Spike.A2.Benchmark.csproj --configuration Release -- --index <existing-path> --documents 1000 --queries 1 --crash-after 1000
```

Der reale Repository-Korpus ist ebenfalls reproduzierbar:

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A2.Benchmark/Sasd.Crawler.Spike.A2.Benchmark.csproj --configuration Release -- --index <temporary-index> --corpus docs --queries 100
```

Ergebnis am Evidence-Datum: 48 Dokumente, mindestens 17 Treffer/Query, p95 52,8 ms.
