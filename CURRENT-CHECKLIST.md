# Current Checklist

**Stichtag:** 7. Oktober 2026  
**Aktueller Fokus:** **Recovery A2–A5 abschließen und G0-Readiness herstellen**

## Baseline / Governance

- [x] Produktanalyse
- [x] Lastenheft 0.1
- [x] WinForms/.NET-8-Architektur
- [x] CR-2026-001 WinForms/.NET-8-Rebaselining angenommen
- [x] Lastenheft-Amendment 0.1a angenommen
- [x] Pflichtenheft 0.2 als aktive technische Baseline
- [x] Roadmap / Teststrategie / Risk Register / ADR-Baseline
- [x] Repository-Reconciliation 2026-10-06 nach `main` gemerged
- [x] Windows-CI auf reconciled `main` grün
- [ ] PoC-abhängige ADRs nach A2–A5 formal reviewen
- [ ] .NET-8-Lifecycleentscheidung für G0
- [ ] G0 final entscheiden

## Architektur-Spikes

- [x] A1 WinForms + Generic Host – **CONDITIONAL GO**
  - [x] Build / automatisierte Tests / Format
  - [ ] interaktiver Tray-Smoke
  - [ ] sichtbare Responsiveness
  - [ ] Second-Launch-Fokus/Restore
  - [ ] echter Forced-Kill/Restart
- [x] A2 Lucene.NET – **TECHNICAL GO**
  - [x] 100k / 1M Benchmark
  - [x] Realkorpus
  - [x] Update/Delete + Suchfunktionen
  - [x] Crash/Reopen
  - [x] Full-Disk-/Write-Failure-Simulation
  - [ ] ADR-0005 formal reviewen
- [x] A3 Tika Sidecar – **CONDITIONAL GO**
  - [x] Tika 3.3.2 Sidecar + Java 21
  - [x] reduzierte Runtime und Formatprobe
  - [ ] Tika 4.1 auf gleicher Basis vergleichen
  - [ ] finales Packaging/SBOM/CVE/Signierung/AV
- [x] A4 Media Identity – **CONDITIONAL GO**
  - [x] Windows-API-/Matching-/RelativePath-Modell
  - [x] 13 automatisierte Tests
  - [ ] physische NTFS/exFAT/FAT32-Smokes
  - [ ] Attach/Detach + Drive-Letter-Wechsel
- [x] A5 Tika vs. Toxy – **CONDITIONAL; TIKA DEFAULT**
  - [x] synthetischer Formatvergleich inkl. Legacy-XLS + 5-MiB-TXT
  - [x] kein Toxy-Fast-Path freigegeben
  - [ ] reale/komplexe Dokumente
  - [ ] Legacy DOC/PPT
  - [ ] verschlüsselte/bösartige Dateien
- [ ] **G0 = GO**

## Repository-Hygiene / CI

- [x] MIT-Lizenz im README korrekt
- [x] `bin/`, `obj/`, `*.user` aus Git entfernt
- [x] `Sasd.Crawler.sln` kanonisch
- [x] Windows-CI für restore/format/Release-build/tests
- [x] erster CI-Lauf auf reconciled `main` erfolgreich
- [ ] Recovery-PR A2–A5 erfolgreich verifizieren und mergen
- [ ] alte Recovery-/Codex-Branches danach remote/lokal entfernen

## Erst nach G0

- [ ] Milestone 0.1 Local Vertical Slice starten
