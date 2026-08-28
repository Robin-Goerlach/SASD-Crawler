# SASD-Crawler – Projektstatus

**Stichtag:** 28. August 2026
**Gesamtstatus:** 🟡 A1–A5 technisch bearbeitet, jeweils Rest-Evidence/formaler Review offen
**Aktueller Meilenstein:** G0 – Architecture Feasibility Readiness
**Nächster Gate:** G0 – derzeit noch nicht GO-fähig

## 1. Executive Snapshot

| Bereich | Status | Kommentar |
|---|---|---|
| Produktanalyse | ✅ DONE | auditierte Produkt- und Funktionsanalyse vorhanden |
| Lastenheft | ✅ DONE / Review-Baseline | 258 Anforderungen definiert |
| Pflichtenheft 0.1 | ⚪ SUPERSEDED DRAFT | bleibt historische technische Fassung |
| Pflichtenheft 0.2 WinForms/.NET 8 | 🟡 DRAFT CREATED | rebaseliniert, formaler Review ausstehend |
| CR-2026-001 + Amendment 0.1a | 🟡 DRAFT CREATED | formale Annahme ausstehend |
| WinForms/.NET-8-Architektur | ✅ DRAFT COMPLETE | 129 Architekturkapitel; noch durch PoCs zu validieren |
| ADR-Baseline | 🟡 CREATED | erste ADRs in diesem Dokumentationspaket |
| Roadmap | ✅ CREATED | ausführliche steuernde Roadmap vorhanden |
| PoC-Spikes | 🟡 READY FOR REVIEW | A1–A5 mit CONDITIONAL/Rest-Evidence |
| Repository/Solution | ✅ VERIFIED FOR A1 | .NET-8-WinForms-Spike-Solution baut ohne Warnungen |
| automatisierte Tests | ✅ A1–A4 VERIFIED | 34/34 Spike-Tests bestanden |
| Milestone 0.1 | ⬜ NOT STARTED | wartet auf G0 |
| MVP 0.5 | ⬜ FUTURE | nicht begonnen |
| 1.0 | ⬜ FUTURE | nicht begonnen |

## 2. Aktuell offene Blocker/Entscheidungen

Der vollständige Readiness-Audit steht unter `docs/evidence/0.0/G0-decision.md` und hält
Milestone 0.1 ausdrücklich gesperrt.

1. A2-Rest-Evidence: Full-Disk-Fehlerprobe und realistischer Korpus.
2. A3-Rest-Evidence: Installer, SBOM/CVE, Signierung und AV/SmartScreen; 62-MiB-Runtime ist verifiziert.
3. A4-Rest-Evidence: physische NTFS/exFAT/FAT32-Medien, Letter-Wechsel und Clone-Ambiguität.
4. A1: manueller Tray-/Second-Launch-/Forced-Kill-Smoke mit Screenshots ist noch nachzuholen; automatisierte Lifecycle-Evidence ist grün.
5. Pflichtenheft 0.2, CR-2026-001 und Amendment 0.1a müssen formal reviewed/angenommen werden.

## 3. Nächste drei kontrollierte Schritte

### Schritt 1 – Baseline formalisieren
- ADRs reviewen.
- Change-Request für WinForms/.NET-8-Supersession erzeugen.
- Pflichtenheft 0.2 vorbereiten.

### Schritt 2 – PoC-Milestone 0.0.x
- A1 WinForms Host Lifecycle.
- A2 Lucene.NET.
- A3 Tika Packaging/Isolation.
- A4 Volume Identity.
- A5 Tika vs. Toxy als Parservergleich.

### Schritt 3 – Gate G0
Erst nach dokumentiertem Go beginnt der vertikale 0.1-Slice.

## 4. Fortschrittsregel

Fortschritt wird nicht nach geschriebenen Zeilen oder Dokumentmenge bewertet.

Ein Meilenstein zählt nur als `DONE`, wenn:

- Exit-Kriterien erfüllt,
- Tests grün,
- Evidence gespeichert,
- Dokumentation aktualisiert,
- offene Risiken akzeptiert oder geschlossen sind.
