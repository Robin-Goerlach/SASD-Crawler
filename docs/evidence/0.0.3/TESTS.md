# A3 – automatisierte und praktische Verifikation

**Datum:** 28. August 2026

## Gesamtsuite

```powershell
dotnet test Sasd.Crawler.sln --configuration Release
```

```text
Sasd.Crawler.Spike.A1.Tests: 9 passed
Sasd.Crawler.Spike.A2.Tests: 8 passed
Sasd.Crawler.Spike.A3.Tests: 6 passed
Sasd.Crawler.Spike.A4.Tests: 13 passed
Gesamt: 36 passed, 0 failed, 0 skipped
```

## Echter Sidecar-Probe

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A3.Probe/Sasd.Crawler.Spike.A3.Probe.csproj --configuration Release -- --jar <verified-tika-jar> --java <java.exe>
```

Der Runner schlägt mit Nicht-Null-Exit fehl, sobald Formatmarker, Metadaten, Loopback,
Restart, malformed-PDF-Behandlung, Eingabegrenze, Timeout-Kill oder Cleanup nicht bestehen.
Der erfolgreiche Lauf und seine Messwerte sind in `SUMMARY.md` festgehalten.

## Reduzierte Runtime

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A3.Probe/Sasd.Crawler.Spike.A3.Probe.csproj --configuration Release -- --build-runtime --java-home <jdk> --output <temporary-runtime>
dotnet run --project tools/Sasd.Crawler.Spike.A3.Probe/Sasd.Crawler.Spike.A3.Probe.csproj --configuration Release -- --jar <verified-tika-jar> --java <temporary-runtime>/bin/java.exe
```

Ergebnis: 26 Module, 62,0 MiB; vollständiger A3-Probe erfolgreich.

## Partielles CycloneDX-Inventar

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A3.Probe/Sasd.Crawler.Spike.A3.Probe.csproj --configuration Release -- --inventory --jar <verified-tika-jar> --output <temporary-bom.json>
```

Ergebnis: 122 eindeutige Komponenten, 5 LICENSE-/NOTICE-Einträge, verifizierter JAR-Hash.
Zwei automatisierte Tests prüfen Deduplizierung, Hash/JSON und Nicht-Überschreiben.
