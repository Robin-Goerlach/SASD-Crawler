# A1 Test Evidence

**Datum:** 21. August 2026  
**SDK:** 8.0.424  
**Konfiguration:** Debug, `net8.0` / `net8.0-windows`

## Befehle und Ergebnisse

| Befehl | Ergebnis |
|---|---|
| `dotnet restore Sasd.Crawler.sln` | erfolgreich; alle vier Projekte wiederhergestellt |
| `dotnet build Sasd.Crawler.sln --no-restore` | erfolgreich; 0 Warnungen, 0 Fehler |
| `dotnet test Sasd.Crawler.sln --logger "console;verbosity=normal"` | erfolgreich; 9 bestanden, 0 fehlgeschlagen, 0 übersprungen |

## Automatisierte Abdeckung

- SQLite Heartbeat persistieren und lesen;
- SQLite nach geordnetem Shutdown exklusiv wieder öffnen;
- Zustand eines vorherigen Laufs wiederherstellen;
- Worker-Cancellation und abgeschlossenes `ExecuteTask`;
- Pause ohne neue periodische Writes;
- Presenter liest Zustand ohne echte UI und bleibt UI-neutral;
- stabile per-user IPC-Identität;
- Named-Pipe-Aktivierung der ersten Instanz;
- Mutex verhindert zweite Instanz desselben Benutzers.

## Transparenz

Ein erster Testlauf zeigte drei Fehler beim Entfernen temporärer SQLite-Verzeichnisse, weil Connection Pooling Dateihandles hielt. Die Implementierung wurde daraufhin für den Spike auf `Pooling=False` korrigiert. Der dokumentierte finale Lauf ist vollständig grün. Es wurden keine Tests ausgeblendet oder übersprungen.
