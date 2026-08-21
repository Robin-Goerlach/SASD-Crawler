# FIRST AUTONOMOUS CODEX PROMPT – Milestone 0.0.1 / A1

Arbeite autonom im aktuellen SASD-Crawler-Repository.

Lies zuerst vollständig:

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
11. `docs/codex/FIRST-TASK-0.0.1.md`

Danach führe **Milestone 0.0.1 – A1 WinForms Host Lifecycle Architecture Spike** vollständig aus.

## Arbeitsweise

Arbeite selbstständig bis zum definierten Gate A1. Unterbrich die Arbeit nicht für normale Lese-, Such-, Build-, Test-, Git- oder GitHub-Operationen.

Die projektspezifischen Regeln erlauben insbesondere autonom:

- `Get-Content`, `Get-ChildItem`, `Get-Item`, `Test-Path`, `Resolve-Path`, `Get-FileHash`, `Select-String` und `rg`;
- normale `dotnet`-Befehle, insbesondere `restore`, `build`, `test`, `publish` und `format`;
- normale Git-Arbeit einschließlich `switch`, `status`, `branch`, `fetch`, `pull`, `rev-parse`, `describe`, `add`, `commit` und `push`;
- normale GitHub-CLI-Arbeit einschließlich `gh pr create/view/checks`, `gh run view/watch` und `gh release view/create/upload`.

Wenn eine Operation laut `.codex/rules/default.rules` auf `prompt` steht, frage vor genau dieser Operation. Versuche nicht, die Regel durch andere Argumentreihenfolge, Shell-Verkettung oder alternative destruktive Befehle zu umgehen.

Fasse approval-relevante Shell-Aufrufe nicht mit unabhängigen Befehlen zu langen PowerShell-Ketten zusammen. Führe Routinebefehle einzeln oder in kurzen logisch zusammengehörigen Gruppen aus.

## Ziel des Spikes

Erzeuge eine kleine, saubere .NET-8-Windows-Forms-Solution, die ausschließlich den Prozess-/Host-Lifecycle validiert:

- Windows Forms;
- `.NET 8` / `net8.0-windows`;
- `Microsoft.Extensions.Hosting`;
- Dependency Injection;
- mindestens ein `BackgroundService`;
- SQLite;
- Cancellation und Graceful Shutdown;
- Notification-Area-/Tray-Betrieb;
- per-user Datenpfad;
- Single-Instance-Schutz;
- Aktivierung der ersten Instanz aus einem zweiten Startversuch.

Noch **nicht** implementieren:

- echten Crawler;
- Lucene.NET;
- Tika;
- Tesseract/OCR;
- USB-Crawling;
- SMB-Crawling;
- Webcrawler;
- endgültiges UI-Design.

## Architektur

Halte den Spike klein, aber sauber testbar.

WinForms darf keine Datenbank- oder Worker-Geschäftslogik im Form-Code enthalten.

Bevorzuge:

```text
Presentation / WinForms
        ↓
Application / Services
        ↓
Infrastructure / SQLite + lifecycle
```

Nutze MVP beziehungsweise Presenter/Service-Grenzen dort, wo sie für den Spike echten Testnutzen bringen. Erzeuge keine unnötige Framework-Struktur und keine leeren Zukunftsprojekte.

## Funktionale Mindestanforderungen

Die Anwendung muss:

1. normal starten;
2. einen Background Worker über Generic Host starten;
3. regelmäßig einen harmlosen Heartbeat erzeugen;
4. Heartbeat/Status persistent in SQLite schreiben;
5. Status in der WinForms-UI anzeigen, ohne Cross-Thread-Control-Zugriffe;
6. während Background-Arbeit reaktionsfähig bleiben;
7. in den Tray minimiert werden können;
8. aus dem Tray wieder geöffnet werden können;
9. sauber beendet werden können;
10. Worker dabei per Cancellation stoppen;
11. SQLite sauber schließen;
12. nur eine Instanz pro Windows-Benutzer zulassen;
13. bei zweitem Start die bestehende Instanz aktivieren und die zweite Instanz beenden.

Die Daten sollen in einem Spike-spezifischen Unterordner unter `%LOCALAPPDATA%\SASD\Crawler\` liegen. Keine Administratorrechte und keine systemweiten Änderungen verlangen.

## Tests

Implementiere automatisierte Tests mindestens für:

- Persistieren und Lesen des Heartbeat-/Statuszustands;
- Cancellation des Workers;
- Lifecycle-/Service-Logik, soweit ohne echte UI automatisierbar;
- Single-Instance-/IPC-Logik, soweit praktisch testbar;
- Wiederöffnen der SQLite-Daten nach geordnetem Shutdown.

Führe anschließend mindestens aus:

```text
dotnet restore
dotnet build
dotnet test
```

Behebe Fehler selbstständig, bis die relevanten Tests grün sind oder ein echter Architekturblocker nachgewiesen ist.

## Evidence

Erzeuge:

```text
docs/evidence/0.0.1/
  SUMMARY.md
  TESTS.md
```

Falls Screenshots in deiner Umgebung praktisch möglich sind, lege sie unter `docs/evidence/0.0.1/screenshots/` ab. Wenn nicht, dokumentiere das transparent und blockiere A1 nicht allein deswegen.

`SUMMARY.md` muss mindestens enthalten:

- Datum;
- OS;
- `dotnet --info`;
- Branch;
- Commit SHA;
- implementierte Architektur;
- Datenpfad;
- Lifecycle-Ergebnis;
- Single-Instance-Ergebnis;
- Shutdown-Ergebnis;
- bekannte Einschränkungen;
- A1-Entscheidung: `GO`, `CONDITIONAL GO` oder `NO-GO`.

`TESTS.md` dokumentiert Build-/Testbefehle und Ergebnisse.

## Projektstatus aktualisieren

Wenn A1 erfolgreich abgeschlossen ist:

- aktualisiere `ROADMAP.md`;
- aktualisiere `PROJECT-STATUS.md`;
- aktualisiere `CURRENT-CHECKLIST.md`;
- aktualisiere nur tatsächlich betroffene Einträge in `REQUIREMENTS-STATUS.md`;
- markiere keine nicht verifizierte Funktion als erledigt.

## Git und GitHub

Arbeite auf einem geeigneten Arbeitsbranch, zum Beispiel:

```text
codex/0.0.1-winforms-host-spike
```

Falls nötig, lege ihn autonom an.

Nach erfolgreichem Build/Test:

1. prüfe `git status` und `git diff`;
2. stage nur zum Task gehörende Dateien;
3. committe mit aussagekräftiger Commit-Message;
4. pushe den aktuellen Arbeitsbranch;
5. falls ein GitHub-Remote vorhanden ist und `gh` funktioniert, erstelle autonom einen Pull Request.

Der Pull Request soll enthalten:

- Ziel und Scope;
- Architekturentscheidung;
- Testergebnisse;
- Evidence;
- bekannte Einschränkungen;
- A1 Gate Decision.

Normales `git push`, `gh pr create`, `gh pr view`, `gh pr checks` und `gh run view/watch` darfst du ohne Rückfrage ausführen.

## Abschlussbericht

Gib am Ende präzise aus:

- Branch;
- HEAD SHA;
- wesentliche neue/geänderte Dateien;
- Buildstatus;
- Teststatus und Testanzahl;
- A1 Gate Decision;
- offene Risiken/Blocker;
- PR-Nummer/URL, falls erstellt;
- exakt den nächsten Roadmap-Schritt.

Arbeite bis zu diesem Zustand autonom weiter. Frage nur bei Operationen, die die Projekt-Execpolicy ausdrücklich auf `prompt` setzt, oder wenn eine echte normative Architekturentscheidung nicht aus der bestehenden Baseline abgeleitet werden kann.
