# A2 – automatisierte Verifikation

**Datum:** 28. August 2026

## Befehl

```powershell
dotnet test Sasd.Crawler.sln --configuration Release
```

## Ergebnis

```text
Sasd.Crawler.Spike.A1.Tests: 9 passed, 0 failed, 0 skipped
Sasd.Crawler.Spike.A2.Tests: 8 passed, 0 failed, 0 skipped
Gesamt: 17 passed, 0 failed, 0 skipped
```

Die A2-Tests belegen Query-Varianten, Sprach-Stemming, Update/Delete ohne Alt-Treffer,
Highlight/Facetten/Filter, persistente Wiederöffnung, koordinierte parallele Updates,
Lesen während Updates und explizites Versagen bei ungültigem Indexpfad.

## Reproduzierbarer Benchmark

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A2.Benchmark/Sasd.Crawler.Spike.A2.Benchmark.csproj --configuration Release -- --index <path> --documents 1000000 --queries 100
```

Der Crash-Modus wird absichtlich mit einem Nicht-Null-Exit beendet:

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A2.Benchmark/Sasd.Crawler.Spike.A2.Benchmark.csproj --configuration Release -- --index <existing-path> --documents 1000 --queries 1 --crash-after 1000
```
