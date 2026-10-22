# Lastenheft-Amendment 0.1b – Search Efficiency und fortlaufende Dublettenerkennung

**Status:** ACCEPTED  
**Datum:** 22. Oktober 2026  
**Bezug:** Lastenheft 0.1  
**Change Request:** CR-2026-002

Dieses Amendment ergänzt zwei Anforderungen und ändert die Priorisierung/Ausprägung von `ID-011`. Alle übrigen Anforderungen aus Lastenheft 0.1 und Amendment 0.1a bleiben unverändert.

Die wirksame fachliche Baseline umfasst damit **260 Anforderungen**.

## A-101 – ID-011 Dublettenerkennung

Bisher:

- Priorität: SOLL
- Release: 1.1.x
- Inhaltsgleiche Dokumente sollen als Dubletten erkennbar sein.

Neu:

**Priorität:** MUSS  
**Release:** 0.4.0

Inhaltsgleiche Dateien müssen als exakte Dubletten erkennbar sein. Für die automatische exakte Gruppierung ist mindestens eine Kombination aus Dateigröße und kryptografischem `ContentHash` zu verwenden.

Mehrere Fundorte bleiben eigenständige Dokument-/Location-Einträge. Die Dublettenerkennung darf Originaldateien niemals automatisch löschen, verschieben, zusammenführen oder überschreiben.

## A-102 – ID-012 Fortlaufende Dublettenerkennung

**Priorität:** MUSS  
**Release:** 0.7.0

Wenn Quellen periodisch oder im Hintergrund indexiert beziehungsweise reconciled werden, müssen Dublettengruppen automatisch aktualisiert werden.

Die Funktion muss die beim normalen Crawl ohnehin erhobenen Größen-, Identitäts- und Inhaltsfingerprintdaten wiederverwenden. Ein zusätzlicher vollständiger Dateisystemdurchlauf ausschließlich zur Dublettensuche darf nicht erforderlich sein.

Neu hinzugekommene, geänderte, verschobene, offline gegangene und nach erfolgreicher Reconciliation als gelöscht bestätigte Dateien müssen die Dublettengruppen konsistent aktualisieren.

## A-103 – SEA-022 Stopwortfilterung

**Priorität:** MUSS  
**Release:** 0.5.0

Die Volltextsuche muss sprachabhängige, konfigurierbare Stopwortlisten unterstützen, damit sehr häufige inhaltsschwache Wörter analysierte Indexfelder nicht unnötig vergrößern und das Ranking nicht dominieren.

Mindestens Deutsch und Englisch erhalten konservative Defaultlisten. Benutzer beziehungsweise Administratoren müssen Wörter ergänzen oder aus der Stopwortliste herausnehmen können.

Index- und Query-Analyzer müssen dieselbe versionierte Stopwortkonfiguration verwenden. Eine Änderung der Liste muss kontrolliert reindexierbar sein.

Exakte Phrase-/Identitätsfelder dürfen Stopwörter gezielt behalten, wenn dies für korrekte Suchsemantik erforderlich ist.

Beispiele für typische Kandidaten sind Artikel und sehr häufige Funktionswörter wie `der`, `die`, `das`. Wörter wie `ja` und `nein` können fachlich relevant sein und sollen deshalb nur über die konfigurierbare Liste ausgeschlossen werden, nicht zwingend durch eine unveränderliche Defaultliste.

## Akzeptanzwirkung

- `ID-011` wird Teil des Rich-Document-/Content-Hash-Meilensteins 0.4.
- `SEA-022` wird Teil der Suchqualität in 0.5.
- `ID-012` wird mit Scheduler/Background-Reconciliation in 0.7 verifiziert.
