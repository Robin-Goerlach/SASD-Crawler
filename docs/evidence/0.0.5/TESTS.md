# A5 – Benchmark-Verifikation

**Datum:** 28. August 2026

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A5.Benchmark/Sasd.Crawler.Spike.A5.Benchmark.csproj --configuration Release -- --jar <verified-tika-jar> --java <java.exe>
```

Der erfolgreiche Lauf verglich sieben valide Formate und ein malformed PDF. Beide Parser
mussten Marker-/Fehlerergebnisse strukturiert ausgeben; der vollständige Ergebnisstand ist in
`SUMMARY.md` dokumentiert.

```powershell
dotnet publish tools/Sasd.Crawler.Spike.A5.Benchmark/Sasd.Crawler.Spike.A5.Benchmark.csproj --configuration Release --output <temporary-output>
```

Ergebnis: 72 Dateien, 38.588.747 Byte (36,8 MiB), framework-dependent.
