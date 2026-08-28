# G0 – Architecture Feasibility Readiness

**Datum:** 28. August 2026
**Entscheidung:** **NOT READY / NO GO FÜR MILESTONE 0.1**

## Aussage

Alle fünf Architektur-Spikes besitzen inzwischen ausführbaren Code und technische Evidence.
Das reicht nach Baseline und Roadmap ausdrücklich nicht für ein G0-GO. Milestone 0.1 darf
noch nicht beginnen, weil formale Baseline-Gates und mehrere materielle Evidence-Punkte offen
sind. Dieser Bericht ist ein Readiness-Audit, keine Freigabe.

## Spike-Status

| Spike | Technisches Ergebnis | Fehlende Evidence |
|---|---|---|
| A1 WinForms/Host | CONDITIONAL GO, 9 Tests | interaktive Tray-/Second-Launch-/Forced-Kill-Smokes |
| A2 Lucene.NET | CONDITIONAL GO, 8 Tests, 1M-Lauf | Full-Disk-Fehler und realistischer Korpus |
| A3 Tika Sidecar | CONDITIONAL GO, 4 Tests + Formatprobe + 62-MiB-Runtime | Installer, SBOM/CVE, Signierung, AV/SmartScreen |
| A4 Media Identity | CONDITIONAL GO, 13 Tests + Hostprobe | physische NTFS/exFAT/FAT32-Medien und Letter-Wechsel |
| A5 Tika/Toxy | Tika bleibt Default | vollständiger realer/Legacy/verschlüsselter Korpus |

Gesamtsuite: 34 Tests, 0 Fehler, 0 übersprungen.

## G0-Kriterienaudit

| Kriterium | Status | Autoritative Evidence |
|---|---|---|
| A1–A5 abgeschlossen | **nicht erfüllt** | alle fünf nur CONDITIONAL/Rest-Evidence |
| kein Architekturblocker | **nicht bewiesen** | Packaging, Hardware und Fehlerkorpora offen |
| Stack für 0.1 fest | **teilweise erfüllt** | WinForms/.NET 8, SQLite, Lucene und Tika technisch tragfähig; ADRs Proposed |
| Pflichtenheft/Baseline synchron | **nicht erfüllt** | Pflichtenheft 0.2, CR-2026-001 und Amendment 0.1a nicht formal angenommen |
| PLAT/UI/AUTH/API-Konflikte bewertet | **nicht formal erfüllt** | Drafts vorhanden, Annahme offen |
| ADR-0001–0014 reviewed | **nicht erfüllt** | alle ADRs weiterhin `Proposed` |
| .NET-8-Lifecycle-Risiko akzeptiert/Migrationsplan | **nicht erfüllt** | formale Entscheidung fehlt |
| Repository-/Solution-Konventionen | **technisch vorhanden** | Solution, Directory.Build.props, global.json, AGENTS/RULES |
| CI-Minimum definiert | **definiert, remote unverified** | `.github/workflows/ci.yml`; Push wegen SSH-Authentifizierung nicht möglich |

## Technische Stackempfehlung aus den Spikes

- WinForms + Generic Host im per-user Desktopprozess;
- SQLite als Control Store und einzige Lifecycle-Wahrheit;
- Lucene.NET 4.8.0-beta00018 hinter `ISearchIndex` als abgeleiteter Suchindex;
- Apache Tika 3.3.2 als isolierter Referenzparser unter Supervisor;
- kein Toxy-Fast-Path in der aktuellen Produktbaseline;
- MediaId + RelativePath, automatische Bindung nur bei eindeutiger exakter Volume-GUID;
- `WM_DEVICECHANGE` nur als Hinweis, niemals als Löschwahrheit.

## Stop-/Freigabepunkt

Der verbleibende normative Blocker kann nicht autonom aufgelöst werden: Die formale Annahme
von Pflichtenheft 0.2, CR-2026-001, Amendment 0.1a, ADRs und .NET-8-Lifecycle-Risiko ist eine
Produkt-/Architekturfreigabe. Die Dokumente werden deshalb nicht stillschweigend auf
`Accepted` gesetzt.

Parallel dazu können die genannten technischen Rest-Smokes nachgeholt werden, sobald echte
Desktopinteraktion, Wechselmedien und ein Packaging-/Realkorpus-Testplatz verfügbar sind.

## Entscheidung

**G0 = NOT READY. Milestone 0.1 bleibt gesperrt.**

Nächste notwendige Aktionen:

1. normative Dokumente und ADR-0001–0014 formal reviewen/akzeptieren oder Konflikte entscheiden;
2. .NET-8-Supportende ausdrücklich akzeptieren oder Upgradeplan beschließen;
3. A1–A5-Rest-Evidence auf geeigneter Hardware/Testumgebung schließen;
4. Branch pushen und Windows-CI erstmals erfolgreich ausführen;
5. G0 erneut auditieren und erst dann GO/NO-GO final entscheiden.
