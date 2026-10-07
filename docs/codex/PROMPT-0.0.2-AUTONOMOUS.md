# AUTONOMOUS CODEX PROMPT – Milestone 0.0.2 / A2 Lucene.NET

Arbeite autonom im aktuellen SASD-Crawler-Repository und führe **Milestone 0.0.2 – A2 Lucene.NET** bis zur dokumentierten Gate-Entscheidung aus.

## Zuerst lesen

1. `AGENTS.md`
2. `RULES.md`
3. `.codex/rules/default.rules`
4. `BASELINE-AND-CHANGE-CONTROL.md`
5. `ROADMAP.md`
6. `PROJECT-STATUS.md`
7. `docs/baseline/PFLICHTENHEFT.md`
8. `docs/baseline/ARCHITECTURE.md`
9. `docs/planning/POC-PLAN.md`
10. `docs/testing/QUALITY-GATES.md`
11. `docs/codex/FIRST-TASK-0.0.2.md`
12. `docs/evidence/0.0.1/SUMMARY.md`

## Autonomie

Unterbrich die Arbeit nicht für normale Lese-, `rg`-, `dotnet`-, Git- oder GitHub-Operationen, die die Projektregeln erlauben. Nutze einen eigenen Branch, bevorzugt:

```text
codex/0.0.2-lucene-benchmark
```

Frage nur bei Befehlen, die `.codex/rules/default.rules` auf `prompt` setzt, oder bei einer echten nicht dokumentierten Strategieentscheidung.

Approval-relevante Shell-Kommandos nicht in lange unabhängige PowerShell-Ketten verstecken.

## Aufgabe

Implementiere einen **isolierten Lucene.NET-Architekturspike**, keinen Produktionscrawler.

Verwende Lucene.NET **4.8.0-beta00018** und .NET 8. Der Spike muss mindestens nachweisen:

- create/open/reopen,
- 100k und 1M deterministische synthetische Dokumente,
- stable-ID update,
- delete,
- NRT refresh/search,
- GermanAnalyzer und EnglishAnalyzer,
- phrase,
- Boolean AND/OR/NOT,
- fuzzy,
- prefix und kontrollierte wildcard,
- highlighting,
- faceting/filter,
- parallele Leser mit genau einem koordinierten Writer,
- harter Prozessabbruch + Reopen/Recovery,
- full rebuild,
- Performance-/Größenmessungen.

Die generierten großen Indizes und Corpora gehören **nicht** ins Git-Repository.

## Datenmodell des Spikes

Nutze mindestens:

```text
document_id
source_id
filename
title
content_exact
content_de
content_en
mime_type
language
modified_utc
availability
```

Die konkrete Spike-API soll bereits zeigen, dass die restliche Anwendung später nicht direkt von Lucene-Typen abhängen muss. Erzeuge aber keine unnötige vollständige Produktionsarchitektur.

## Performance

Dokumentiere für 100k und 1M:

- Hardware/OS/.NET-Version,
- Indexierungsdauer und docs/s,
- Indexgröße,
- Prozess-RAM/Working Set soweit sinnvoll messbar,
- Query p50/p95 für ein festes Queryset,
- Update-Latenz,
- Delete-Latenz,
- Reopen-Zeit.

Das Lastenheft-Ziel für gewöhnliche Suche auf der Referenzumgebung bleibt:

```text
Median < 300 ms
P95 < 1 s
```

Wenn Hardware deutlich abweicht, nicht Messwerte schönreden: Hardware und Ursache dokumentieren und Gate entsprechend beurteilen.

## Recovery

Baue einen reproduzierbaren Recovery-Test mit externem Prozess:

1. Index öffnen/schreiben,
2. definierte Daten committen,
3. weitere Arbeit starten,
4. Prozess hart beenden,
5. Index im Eltern-/Folgeprozess öffnen,
6. Integrität und committed data prüfen,
7. weitere Schreib-/Suchoperation durchführen.

Kein `Stop-Process` ohne Approval; wenn die Execpolicy dafür prompt verlangt, frage an dieser Stelle. Alternativ darf ein Testprozess sich kontrolliert selbst hart terminieren, wenn dadurch die Policy nicht umgangen wird und der Testzweck sauber dokumentiert ist.

## Tests und Befehle

Führe autonom mindestens aus:

```text
dotnet restore
dotnet format --verify-no-changes
dotnet build --configuration Release
dotnet test --configuration Release
```

Behebe Fehler selbstständig.

Lang laufende 1M-Benchmarks dürfen separat dokumentiert werden, müssen aber für eine finale A2-Gate-Entscheidung tatsächlich gelaufen sein.

## Evidence

Erzeuge:

```text
docs/evidence/0.0.2/SUMMARY.md
docs/evidence/0.0.2/TESTS.md
docs/evidence/0.0.2/BENCHMARKS.md
docs/evidence/0.0.2/RECOVERY.md
```

`SUMMARY.md` enthält am Ende exakt eine Entscheidung:

```text
A2 Gate Decision: GO
```

oder

```text
A2 Gate Decision: CONDITIONAL GO
```

oder

```text
A2 Gate Decision: NO-GO
```

mit Begründung.

## Statuspflege

Nach erfolgreicher Verifikation aktualisieren:

- `ROADMAP.md`,
- `PROJECT-STATUS.md`,
- `CURRENT-CHECKLIST.md`,
- `DECISION-LOG.md`,
- ADR-0005,
- `docs/planning/RISK-REGISTER.md`.

`REQUIREMENTS-STATUS.md` nicht als Produktimplementierung hochzählen: A2 bleibt ein Architekturspike.

## Git/GitHub

Nach Build/Test/Evidence:

1. `git status` und `git diff` prüfen,
2. nur Task-Dateien stagen,
3. kohärent committen,
4. normalen Branch pushen,
5. Pull Request nach `main` erstellen,
6. CI mit `gh pr checks` / `gh run view` / `gh run watch` beobachten und normale Fehler autonom beheben.

Kein Force-Push, Reset, Clean, Rebase oder andere prompt-pflichtige Operation ohne Freigabe.

## Abschlussbericht

Melde am Ende:

- Branch,
- HEAD SHA,
- genaue Lucene-Paketversionen,
- Build/Tests,
- 100k/1M Benchmark-Kerndaten,
- Recovery-Ergebnis,
- A2 Gate Decision,
- offene Risiken,
- PR-Nummer/Link,
- exakt nächsten Roadmap-Schritt.
