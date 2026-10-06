# ADR-0010: Reconciliation ist Wahrheit; FileSystemWatcher nur Hinweis

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

Dateisystem-Watcher können Events verlieren, insbesondere bei Bursts und Netzfreigaben. Ein fehlendes Event darf weder Inkonsistenz noch falsche Löschung erzeugen.

## Entscheidung

`FileSystemWatcher` und ähnliche Mechanismen dürfen Änderungen beschleunigt signalisieren, sind aber niemals die führende Wahrheit. Nur ein erfolgreich vollständig abgeschlossener Scan darf source-weite Missing-/Delete-Reconciliation auslösen.

Quelle offline, Root nicht erreichbar, Access denied, Abbruch oder ungeklärter globaler I/O-Fehler verhindern die globale Löschphase.

## Positive Folgen

- verhindert gefährliche Massendeletion bei NAS-/USB-Ausfall,
- konsistenter Zustand auch bei verlorenen Watcher-Events,
- nachvollziehbare Recovery-Semantik.

## Negative Folgen / Trade-offs

- periodische Full Scans bleiben notwendig,
- mehr Reconciliation-/State-Machine-Logik.

## Verifikation

Dieses Verhalten ist ein verbindliches Produktinvariant und wird spätestens im lokalen 0.1-Slice sowie USB/SMB 0.2 mit negativen E2E-Fällen verifiziert.
