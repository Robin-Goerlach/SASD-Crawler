# A3 – automatisierte und praktische Verifikation

**Datum:** 28. August 2026

## Gesamtsuite

```powershell
dotnet test Sasd.Crawler.sln --configuration Release
```

```text
Sasd.Crawler.Spike.A1.Tests: 9 passed
Sasd.Crawler.Spike.A2.Tests: 8 passed
Sasd.Crawler.Spike.A3.Tests: 4 passed
Gesamt: 21 passed, 0 failed, 0 skipped
```

## Echter Sidecar-Probe

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A3.Probe/Sasd.Crawler.Spike.A3.Probe.csproj --configuration Release -- --jar <verified-tika-jar> --java <java.exe>
```

Der Runner schlägt mit Nicht-Null-Exit fehl, sobald Formatmarker, Metadaten, Loopback,
Restart, malformed-PDF-Behandlung, Eingabegrenze, Timeout-Kill oder Cleanup nicht bestehen.
Der erfolgreiche Lauf und seine Messwerte sind in `SUMMARY.md` festgehalten.
