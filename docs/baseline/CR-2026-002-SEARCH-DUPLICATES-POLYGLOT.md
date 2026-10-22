# CR-2026-002 – Search Efficiency, Duplicate Discovery and Polyglot Repository

**Status:** ACCEPTED  
**Datum:** 22. Oktober 2026  
**Betroffene Baseline:** Lastenheft 0.1 + Amendment 0.1a, Pflichtenheft 0.2, Architektur 0.1

## 1. Ausgangslage

Der Crawler liest für Indexierung und Reconciliation große Teile konfigurierter Dateisysteme. Daraus ergeben sich zwei zusätzliche Produktchancen:

1. sehr häufige, inhaltlich schwache Wörter sollen den analysierten Volltextindex nicht unnötig vergrößern;
2. bereits gelesene Inhaltsfingerprints können ohne separaten Vollscan zur fortlaufenden Erkennung exakter Dubletten verwendet werden.

Zusätzlich muss die kanonische Anwendung auf einem verwalteten Hochschul-PC nutzbar sein, auf dem keine zusätzliche Go-Runtime installiert werden darf. Gleichzeitig soll das Repository zukünftige Entwicklung in weiteren Programmiersprachen ermöglichen.

## 2. Entscheidung

### 2.1 Kanonische Produktimplementierung

Version 1.0 bleibt eine **C#-/Windows-Forms-Anwendung auf .NET 8 (`net8.0-windows`)**.

Der bevorzugte Publish ist `win-x64` self-contained, damit auf dem Zielrechner keine separate .NET-Runtime installiert werden muss.

Eine zusätzliche Programmiersprache darf als Experiment, Tool, Sidecar oder spätere alternative Implementierung in das Repository aufgenommen werden. Sie darf jedoch nicht stillschweigend zur zwingenden V1-Laufzeitabhängigkeit werden. Insbesondere setzt die V1 keine installierte Go-Runtime voraus.

### 2.2 Stopwortfilterung

Neue Anforderung `SEA-022`:

- sprachabhängige, versionierte Stopwortlisten;
- identische Stopwortkonfiguration in Index- und Query-Analyzer;
- konservative Standardlisten für Deutsch und Englisch;
- konfigurierbare Ergänzungen und Ausnahmen;
- Änderungen an der Liste erzeugen eine neue Processing-/Analyzer-Profilversion und einen kontrollierten Reindex;
- Felder für exakte Phrase-/Identitätssemantik dürfen Stopwörter gezielt behalten.

### 2.3 Dublettenerkennung

`ID-011` wird geschärft und von 1.1.x auf 0.4.0 vorgezogen:

- exakte Dubletten mindestens über Dateigröße + kryptografischen `ContentHash`;
- mehrere Fundorte bleiben eigenständige Dokument-/Location-Datensätze;
- Dublettenerkennung löscht, verschiebt oder überschreibt niemals Originaldateien.

Neue Anforderung `ID-012`:

- reguläre Reconciliation-/Hintergrundläufe aktualisieren Dublettengruppen automatisch;
- bereits im normalen Crawl erzeugte Hashes werden wiederverwendet;
- kein zusätzlicher vollständiger Dateisystemdurchlauf allein zur Dublettensuche;
- Änderungen, Moves, Offline-Zustände und bestätigte Löschungen aktualisieren die Gruppen konsistent.

## 3. Nutzen

- kleinerer und signalreicherer analysierter Volltextindex;
- konsistente Suchanalyse zwischen Index- und Query-Seite;
- Dublettenerkennung praktisch ohne separaten Dateisystem-Crawl;
- klarer Nutzerwert aus ohnehin notwendiger Indexpflege;
- C#/.NET-8-Kompatibilität für verwaltete Windows-PCs;
- zukünftige Mehrsprachenentwicklung ohne Architekturvermischung.

## 4. Risiken und Gegenmaßnahmen

### Stopwörter

Zu aggressive Listen können relevante Inhalte verstecken. Wörter wie `ja` oder `nein` können je nach Dokumenttyp semantisch wichtig sein und gehören deshalb nicht zwingend in die konservative Defaultliste. Sie können vom Benutzer explizit ergänzt werden.

Gegenmaßnahmen:

- versionierte, konfigurierbare Listen;
- Golden-Query-Tests;
- getrenntes Exact-/Phrase-Feld, wenn die Suchsemantik Stopwörter benötigt.

### Dubletten

Hashing erzeugt I/O- und CPU-Kosten und darf nicht zu falschen Löschentscheidungen führen.

Gegenmaßnahmen:

- Hashing beim ohnehin erforderlichen Content-Read mitführen;
- Größe + kryptografischer Hash für exakte Gruppen;
- keine automatische Dateilöschung;
- Reconciliation bleibt Autorität für Existenz/Löschung.

### Polyglot Repository

Mehrere Sprachen können Build-/CI-/Dokumentationsaufwand erhöhen.

Gegenmaßnahmen:

- C#/.NET 8 bleibt kanonisch;
- zusätzliche Sprachen klar unter `languages/<language>/`;
- jede Sprache dokumentiert Build, Test, Runtime und Lizenz;
- neue Runtimeabhängigkeiten brauchen explizite Entscheidung.

## 5. Tests

Mindestens:

- Stopwort-Unit-Tests DE/EN;
- Index- und Query-Analyzer verwenden dieselbe Profilversion;
- Reindex nach Stopwortlistenänderung;
- exakte Dubletten: gleiche Größe + gleicher Hash;
- gleiche Größe + anderer Hash ist keine Dublette;
- Move/Rename erzeugt keine künstliche zweite Dublette;
- Offline-Medium löscht keine Gruppe;
- Änderung löst alte Dublettenzuordnung;
- Hintergrund-Reconciliation aktualisiert Gruppen idempotent;
- CI bleibt für die kanonische C#/.NET-8-Solution grün.

## 6. Releaseauswirkung

- `ID-011`: 0.4.0 statt 1.1.x.
- `SEA-022`: 0.5.0.
- `ID-012`: 0.7.0, gemeinsam mit periodischen/background Jobs.
- V1 bleibt C#/.NET 8.

## 7. Entscheidung

**ACCEPTED.** Die wirksame fachliche Baseline umfasst nach Amendment 0.1b **260 Anforderungen**.
