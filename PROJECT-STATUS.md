# SASD-Crawler – Projektstatus

**Stichtag:** 7. Oktober 2026  
**Gesamtstatus:** 🟡 Architekturvalidierung; A2 technisch erfolgreich, A3–A5 mit belastbarer Teil-Evidence, G0 noch nicht freigegeben  
**Aktueller Arbeitsblock:** **Recovery/Integration der Architektur-Spikes A2–A5**  
**Nächstes Gate:** **G0 – Architecture Feasibility Review**  
**Reconciled Main-Basis:** `428f8b0bb82c5f6736cbc78d48b4a10049da19b3`

## 1. Executive Snapshot

| Bereich | Status | Kommentar |
|---|---|---|
| Produktanalyse | ✅ DONE | auditierte Produkt-/Funktionsanalyse vorhanden |
| Lastenheft 0.1 | ✅ ACTIVE | 258 Anforderungen; Amendment 0.1a gilt ergänzend |
| CR-2026-001 / Amendment 0.1a | ✅ ACCEPTED | WinForms/.NET-8-Rebaselining bestätigt |
| Pflichtenheft 0.2 | ✅ ACTIVE BASELINE | WinForms/.NET 8; ersetzt widersprechende 0.1-Technikannahmen |
| A1 WinForms Host Lifecycle | 🟡 CONDITIONAL GO | 9 automatisierte Tests; vier interaktive Windows-Smokes offen |
| A2 Lucene.NET | 🟢 TECHNICAL GO | 1M-Dokument-, Realkorpus-, Crash-, Full-Disk- und Funktions-Evidence vorhanden; ADR-0005 formal weiter Proposed |
| A3 Tika Sidecar | 🟡 CONDITIONAL GO | Tika 3.3.2 Sidecar/62-MiB-Runtime/Formatprobe erfolgreich; 4.1-Vergleich, finales Packaging/SBOM/CVE/AV offen |
| A4 Media Identity | 🟡 CONDITIONAL GO | Windows-API-/Matching-Modell + 13 Tests; physische Wechselmedien-/Letter-Wechsel-Smokes offen |
| A5 Tika vs. Toxy | 🟡 CONDITIONAL / DECISION CLEAR | Tika bleibt Default; kein Toxy-Fast-Path. Real-/Legacy-DOC/PPT-/verschlüsselter Korpus offen |
| G0 | 🔴 NOT READY | technische PoCs weit fortgeschritten, aber Rest-Evidence, ADR-Reviews und .NET-8-Lifecycle-Entscheidung offen |
| CI | 🟢 BASELINE VERIFIED | Reconciled `main` lief unter Windows mit Restore/Format/Release-Build/Tests erfolgreich |
| Milestone 0.1 | ⬜ BLOCKED | darf erst nach G0-GO beginnen |

## 2. Verifizierte Spike-Evidence

### A1
- WinForms + Generic Host/DI
- cancellable BackgroundService
- SQLite/WAL
- MVP/UI-Thread-Marshalling
- Tray-Code
- Single Instance + Named Pipe
- 9 automatisierte Tests

Offen: sichtbare Responsiveness, Tray-Interaktion, Second-Launch-Fokus/Restore und echter Forced-Kill/Restart.

### A2
- Lucene.NET 4.8.0-beta00018
- 100k und 1M synthetische Dokumente
- realer kleiner Repository-Korpus
- Update/Delete
- deutsche/englische Analyzer
- Phrase/Boolean/Fuzzy/Prefix, Highlighting und Filter/Facetten
- koordinierter Writer + nebenläufige Leser
- harter `Environment.FailFast` + Reopen
- simulierter Full-Disk-/Write-Failure-Fall

Messwert 1M: Query p50 173,9 ms, p95 224,5 ms; Peak Working Set 114,8 MB.

### A3
Tika 3.3.2 wurde als isolierter Sidecar mit Java 21, Loopback-Bindung, Limits, Restart, Timeout, Fehlerisolation und reduzierter 62,0-MiB-Runtime praktisch erprobt. Ein partielles Komponenten-Inventar liegt vor.

Nicht bewiesen sind insbesondere Tika 4.1 im gleichen Packaging-Modell, finales SBOM/CVE-Mapping, Signierung sowie AV/SmartScreen.

### A4
Volume-GUID/Serial/Filesystem/Capacity, `MediaId + RelativePath`, konservatives Matching und `WM_DEVICECHANGE`-Infrastruktur wurden implementiert und automatisiert getestet. Physische Wechselmedien-Smokes bleiben offen.

### A5
Der reproduzierbare Vergleich umfasst DOCX/XLSX/PPTX/PDF/HTML/RTF/TXT, Legacy-XLS, 5-MiB-TXT und ein malformed PDF. Tika bleibt Referenzparser; Toxy wird derzeit nicht in Produktcode übernommen. Reale komplexe Dokumente, Legacy DOC/PPT sowie verschlüsselte/bösartige Dateien fehlen noch.

## 3. Aktuelle Blocker vor G0

1. A1: vier interaktive Desktop-Smokes.
2. A3: Tika-4.1-vs-3.3.2-Entscheidung auf gleicher Packaging-/Security-Basis sowie finales SBOM/CVE/AV/Installer-Evidence.
3. A4: echte NTFS-/exFAT-/FAT32-Wechselmedien, Attach/Detach und Laufwerksbuchstabenwechsel.
4. A5: realer/komplexer, Legacy-DOC/PPT- und verschlüsselter Fehlerkorpus.
5. PoC-abhängige ADRs (insbesondere 0005/0006/0007/0011) formal reviewen.
6. .NET-8-Supportende am 10.11.2026: Upgradepfad vor produktiver 1.0 festlegen.

## 4. Nächste kontrollierte Schritte

1. Recovery-PR gegen den reconciled `main` vollständig per CI verifizieren und mergen.
2. A1-manual-smokes nachholen.
3. A3-Rest-Evidence inklusive Tika 4.1 schließen.
4. A4-Hardware-Smokes durchführen.
5. A5-Korpus-Evidence vervollständigen.
6. PoC-ADRs und .NET-Lifecycle im G0-Review entscheiden.
7. Erst nach **G0 = GO** Milestone 0.1 starten.

## 5. Fortschrittsregel

`specified ≠ implemented ≠ verified ≠ released`. Spike-Evidence beweist Architekturhypothesen, zählt aber nicht automatisch als implementierte Produktanforderung.
