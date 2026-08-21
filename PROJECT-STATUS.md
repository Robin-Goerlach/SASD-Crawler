# SASD-Crawler – Projektstatus

**Stichtag:** 21. August 2026  
**Gesamtstatus:** 🟡 Planung/Architektur weit fortgeschritten, Implementierung noch nicht verifiziert begonnen  
**Aktueller Meilenstein:** 0.0.x – Architektur-Spikes und Baseline-Freigabe  
**Nächster Gate:** G0 – Architecture Feasibility Gate

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
| PoC-Spikes | ⬜ NOT STARTED | A1–A5 geplant |
| Repository/Solution | ⬜ NOT VERIFIED | in diesem Dokumentationsstand keine Implementierungsevidenz |
| automatisierte Tests | ⬜ NOT STARTED | Teststrategie definiert |
| Milestone 0.1 | ⬜ NOT STARTED | wartet auf G0 |
| MVP 0.5 | ⬜ FUTURE | nicht begonnen |
| 1.0 | ⬜ FUTURE | nicht begonnen |

## 2. Aktuell offene Blocker/Entscheidungen

1. Lucene.NET muss unter .NET 8 praktisch validiert werden.
2. Tika-Sidecar muss Packaging-/Security-PoC bestehen.
3. Windows Volume Identity muss für das Offline-Medienmodell belastbar sein.
4. WinForms + Generic Host + BackgroundService + Tray + Shutdown muss im Spike validiert werden.
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
