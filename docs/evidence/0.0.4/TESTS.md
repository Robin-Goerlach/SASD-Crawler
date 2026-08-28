# A4 – automatisierte und Host-Verifikation

**Datum:** 28. August 2026

```powershell
dotnet test tests/Sasd.Crawler.Spike.A4.Tests/Sasd.Crawler.Spike.A4.Tests.csproj --configuration Release
```

```text
13 passed, 0 failed, 0 skipped
```

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A4.Probe/Sasd.Crawler.Spike.A4.Probe.csproj --configuration Release
```

Der Probe-Lauf erfasst native Signale aller bereiten Laufwerke und prüft den unabhängigen
Start/Stop des Device-Monitors. Nicht bereite Laufwerke werden als solche protokolliert und
nicht als gelöscht oder fehlende Dokumente interpretiert.
