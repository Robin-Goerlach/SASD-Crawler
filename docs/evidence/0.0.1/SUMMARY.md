# Milestone 0.0.1 – A1 WinForms Host Lifecycle

**Datum:** 21. August 2026  
**OS:** Windows 10.0.26200, win-x64  
**Branch:** `codex/0.0.1-winforms-host-spike`  
**Commit SHA:** wird nach dem Evidence-Commit mit `git rev-parse HEAD` bestimmt  
**Gate-Entscheidung:** **CONDITIONAL GO**

## Laufzeit und SDK

- Gepinntes SDK: .NET SDK 8.0.424
- Testhost: .NET 8.0.30, x64
- Windows Desktop Runtime: 8.0.30
- Vollständige Erfassung erfolgte mit `dotnet --info` am Evidence-Datum.

## Implementierte Architektur

```text
WinForms MainForm / IMainView
        ↓ MainPresenter + SynchronizationContext
Core services / Heartbeat BackgroundService
        ↓ IHeartbeatStore
Infrastructure / SQLite (WAL, short-lived connections)

Process boundary: per-user named mutex + named-pipe activation
Host boundary: Generic Host start → WinForms loop → cancellation → awaited stop
```

Der Form-Code enthält keine SQL- oder Worker-Logik. Der Worker publiziert nur UI-neutrale Zustände; der Presenter marshalled sie auf den WinForms-Kontext.

## Datenpfad

`%LOCALAPPDATA%\SASD\Crawler\spikes\a1\lifecycle.db`

Der Pfad ist pro Benutzer, benötigt keine Administratorrechte und nimmt keine systemweiten Änderungen vor.

## Ergebnisse

- **Lifecycle:** Generic Host, DI, zwei Hosted Services (Heartbeat und IPC Listener), WinForms Message Loop und geordneter Stop sind implementiert. Build und automatisierte Service-/Presenter-Tests sind grün.
- **Single Instance:** Mutex-Namensraum ist pro Windows-Benutzer abgeleitet. Ein zweiter Prozess startet Host/SQLite nicht, signalisiert über eine Named Pipe und beendet sich. Mutex-Ausschluss und IPC-Aktivierung sind automatisiert getestet.
- **Shutdown:** Host-Cancellation stoppt den Worker. SQLite verwendet begrenzte Connections ohne Pooling; exklusives Wiederöffnen und Lesen durch einen simulierten Folgelauf sind verifiziert.
- **Recovery:** Ein persistierter Zustand eines vorherigen Laufs bleibt lesbar; WAL-Modus ist aktiviert.
- **Tray:** NotifyIcon, Open, Pause/Resume und Exit sind implementiert. Schließen des Fensters minimiert transparent in den Tray; explizites Exit beendet den Host.

## Bekannte Einschränkungen

- Die Ausführungsumgebung bot keine steuerbare interaktive Windows-Desktop-Sitzung für reproduzierbare Screenshots. Deshalb wurden keine Screenshots erzeugt.
- Tray-Interaktion, sichtbare Responsiveness, Second-Launch-Fokusverhalten und echter Forced-Kill/Restart wurden nicht manuell beobachtet. Ihre zugrunde liegende Service-, IPC- und Recovery-Logik ist automatisiert getestet.
- Lokales Console-Logging ist absichtlich minimal; persistente strukturierte File-Logs sind nicht Produktziel dieses Spikes.
- SQLite-Pooling ist im Spike deaktiviert, um den geordneten File-Handle-Shutdown direkt nachweisen zu können. Diese PoC-Entscheidung ist vor Produktionsübernahme neu zu bewerten.

## A1-Entscheidung

**CONDITIONAL GO.** Die Architekturhypothese ist durch Build und 9 automatisierte Tests bestätigt; kein Architekturblocker wurde gefunden. Für ein uneingeschränktes formales GO bleiben ausschließlich die manuellen Windows-Desktop-Smokes (Tray, sichtbare Responsiveness, zweiter Start, Forced Kill/Restart) nachzuholen. Das Fehlen von Screenshots allein blockiert die weitere Spike-Arbeit nicht.
