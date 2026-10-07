# A5 – Benchmark-Verifikation

**Datum:** 28. August 2026

```powershell
dotnet run --project tools/Sasd.Crawler.Spike.A5.Benchmark/Sasd.Crawler.Spike.A5.Benchmark.csproj --configuration Release -- --jar <verified-tika-jar> --java <java.exe>
```

Der erfolgreiche Lauf verglich neun valide Eingaben (DOCX, XLSX, PPTX, PDF, HTML, RTF, TXT,
Legacy-XLS und 5-MiB-TXT) sowie ein malformed PDF. Beide Parser mussten Marker-, Fehler- und
Working-Set-Ergebnisse strukturiert ausgeben; der vollständige Ergebnisstand ist in
`SUMMARY.md` dokumentiert. Die Working-Set-Werte sind Prozess-Momentaufnahmen und keine
Peak-Messungen.

```powershell
dotnet publish tools/Sasd.Crawler.Spike.A5.Benchmark/Sasd.Crawler.Spike.A5.Benchmark.csproj --configuration Release --output <temporary-output>
```

Ergebnis: 72 Dateien, 38.588.747 Byte (36,8 MiB), framework-dependent.
