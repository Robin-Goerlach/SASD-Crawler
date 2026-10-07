# SASD-Crawler – Projektstatus

**Stichtag:** 6. Oktober 2026  
**Gesamtstatus:** 🟡 Architekturvalidierung; A1 automatisiert verifiziert und auf `main`, manuelle Desktop-Evidence offen  
**Aktueller Meilenstein:** **0.0.2 – A2 Lucene.NET Spike**  
**Nächstes Gate:** **A2 – Lucene.NET**  
**Basis-Main vor diesem Reconciliation-Update:** `d58ed6ecf87b5a71d2003b2485362c649e6a695f`

## 1. Executive Snapshot

| Bereich | Status | Kommentar |
|---|---|---|
| Produktanalyse | ✅ DONE | auditierte Produkt-/Funktionsanalyse vorhanden |
| Lastenheft 0.1 | ✅ ACTIVE | 258 Anforderungen; Amendment 0.1a gilt ergänzend |
| CR-2026-001 / Amendment 0.1a | ✅ ACCEPTED | WinForms/.NET-8-Rebaselining wird durch die explizite Produktentscheidung und A1-Umsetzung bestätigt |
| Pflichtenheft 0.2 | ✅ ACTIVE BASELINE | WinForms/.NET 8; ersetzt widersprechende 0.1-Technikannahmen |
| Architektur 0.1 | ✅ ACTIVE / PoC-validated in parts | A1 bestätigt Host/DI/MVP/SQLite/Single Instance; A2–A5 bleiben Gates |
| A1 WinForms Host Lifecycle | 🟡 CONDITIONAL GO | Build 0/0; 9/9 Tests; Format grün; manuelle Windows-Smokes offen |
| A2 Lucene.NET | 🟢 READY | nächster Arbeitsblock; Task + autonomer Prompt vorhanden |
| CI | 🟢 BASELINE ADDED | Windows restore/format/build/test auf PR/main; erster Run muss nach Merge beobachtet werden |
| Repository-Hygiene | 🟢 RECONCILED | eingecheckte `bin/`, `obj/`, `.csproj.user` und redundante Spike-Solution werden entfernt |
| Milestone 0.1 | ⬜ NOT STARTED | wartet auf G0 |
| MVP 0.5 | ⬜ FUTURE | nicht begonnen |
| 1.0 | ⬜ FUTURE | nicht begonnen |

## 2. Verifizierter A1-Codezustand

A1 liegt auf `main` und implementiert:

- .NET-8-WinForms-Host mit Generic Host/DI,
- cancellable Heartbeat `BackgroundService`,
- SQLite/WAL-Persistenz im per-user-Pfad,
- Presenter/UI-Thread-Marshalling,
- Tray-Betrieb,
- Single-Instance Mutex + Named Pipe,
- 9 automatisierte Tests.

Evidence: `docs/evidence/0.0.1/`.

Noch manuell nachzutragen:

1. sichtbare UI-Responsiveness unter Worker-Aktivität,
2. Tray Open/Pause/Resume/Exit in einer interaktiven Sitzung,
3. Second-Launch-Fokus/Restore,
4. echter Forced-Kill/Restart.

Diese Punkte halten A1 auf `CONDITIONAL GO`, blockieren A2 aber nicht.

## 3. Aktuelle Architektur-/Technologierisiken

1. **.NET 8:** Microsoft-Support endet am **10.11.2026**; aktuelles Servicing ist 8.0.31 / SDK 8.0.425. Der gewünschte .NET-8-Entwicklungsstand wird beibehalten, aber G0 muss einen klaren Upgradepfad vor produktiver 1.0 festlegen.
2. **Lucene.NET:** 4.8.0-beta00018 unterstützt .NET 8, ist formal weiterhin Beta. A2 entscheidet Embedded-v1 vs. früheren OpenSearch-Fallback.
3. **Tika:** 4.1.0 ist aktuell; 3.3.2 bleibt gepflegte Maintenance-Linie. A3 entscheidet konkret zwischen 4.x und 3.x unter Packaging-/Security-/Migrationsgesichtspunkten.
4. **A1 manuelle Evidence:** offen, aber kein bekannter Architekturblocker.

## 4. Nächste kontrollierte Schritte

1. Reconciliation-PR mergen und CI erstmals auf Windows grün bestätigen.
2. `codex/0.0.2-lucene-benchmark` starten und A2 vollständig durchführen.
3. A1-manual-smoke bei nächster interaktiver Windows-Sitzung nachtragen.
4. Danach A3 Tika Packaging/Isolation.
5. Nach A2–A5 G0 formal entscheiden.

## 5. Fortschrittsregel

Ein Meilenstein ist nur `DONE`, wenn Exit-Kriterien, Tests/Evidence, Dokumentation und offene Risiken zusammenpassen. Dokumentiert oder kompiliert allein bedeutet nicht erledigt.
