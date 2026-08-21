# Lastenheft – SASD-Crawler

**Dokumentstatus:** Entwurf zur fachlichen Baseline  
**Stand:** 20. August 2026  
**Dokumentversion:** 0.1  
**Produkt:** SASD-Crawler  
**Dokumenttyp:** Lastenheft / fachliche Produktanforderungen  
**Grundlage:** „SASD-Crawler – Konsolidierte Produkt- und Funktionsanalyse – AUDITIERT v2“  
**Zielrelease der ersten stabilen Produktgeneration:** 1.0

---

## 1. Zweck des Lastenhefts

Dieses Lastenheft beschreibt die fachlichen Anforderungen an den SASD-Crawler. Es legt fest, **was** das Produkt leisten soll, nicht mit welchen konkreten Bibliotheken, Frameworks oder Suchengines die Anforderungen umgesetzt werden.

Die zuvor untersuchten Produkte – insbesondere Fess, Recoll, Datafari, Open Semantic Search, DocFetcher, sist2, Paperless-ngx, Docspell, Mayan EDMS, ManifoldCF, OpenSearch, Solr, Lucene.NET, Apache Tika, Tesseract, Toxy, Everything und weitere – dienen als Funktions- und Qualitätsbenchmarks.

Das Lastenheft verfolgt drei Ziele:

1. die für einen brauchbaren SASD-Crawler **notwendigen Kernfunktionen verbindlich festzulegen**;
2. Funktionen, die aus Vergleichsprodukten bekannt sind, aber den Kern zunächst unnötig vergrößern würden, **bewusst späteren Releases zuzuordnen**;
3. Funktionen, die nicht zum Produktziel passen, **explizit auszuschließen**, damit der Crawler nicht schleichend zu einem DMS, Fileserver, Collaboration-System oder KI-Chatprodukt wird.

---

## 2. Produktvision

Der SASD-Crawler soll Informationen wiederauffindbar machen, **ohne die vorhandene Ablageordnung ersetzen zu müssen**.

Ein Anwender soll eine Suche ausführen können und Treffer aus mehreren, technisch unterschiedlichen Quellen gemeinsam erhalten:

- lokale Festplatten,
- lokale Verzeichnisbäume,
- USB-Sticks,
- externe Festplatten,
- eingebundene Netzlaufwerke,
- SMB/CIFS-Freigaben,
- Webseiten,
- dort verlinkte unterstützte Dokumente.

Dabei sollen Word-, Excel-, PowerPoint-, PDF-, HTML-, Text- und weitere relevante Dokumente **inhaltlich** durchsucht werden und nicht nur über ihren Dateinamen.

Der zentrale fachliche Nutzen lautet:

> **„Finde mir zuverlässig das Dokument oder die Webseite, in der diese Information steht – unabhängig davon, auf welchem konfigurierten Datenträger, Netzlaufwerk oder Webangebot sie liegt.“**

---

## 3. Produktgrundsätze

### 3.1 Index-in-place

Der SASD-Crawler ist primär ein **Index-in-place-System**.

Dateien verbleiben grundsätzlich am vorhandenen Ort. Das Produkt soll nicht verlangen, Dokumente zunächst in ein eigenes Repository zu importieren.

### 3.2 Original bleibt führend

Das Originaldokument beziehungsweise die Originalwebseite bleibt die führende Quelle.

Der Suchindex darf extrahierten Text, Metadaten, Vorschauinformationen und technische Zustände speichern, aber das Produkt ersetzt nicht automatisch den ursprünglichen Datenspeicher.

### 3.3 Offline-Medien sind kein Löschereignis

Das vorübergehende Fehlen eines USB-Datenträgers oder Netzlaufwerks darf nicht automatisch als Löschung aller dort bekannten Dokumente interpretiert werden.

### 3.4 Klassische Suche vor KI

Deterministische Volltextsuche, Filter, Metadaten, Phrase Search und nachvollziehbare Treffer müssen unabhängig von semantischer oder generativer KI funktionieren.

### 3.5 Sicherheit vor Komfort

Ein Treffer darf einem Benutzer keine Informationen über ein Dokument verraten, wenn dieser Benutzer für das Dokument keine Leseberechtigung besitzt.

### 3.6 Erweiterbarkeit ohne Kernüberladung

Quellen, Parser, OCR, Suchbackend und optionale Analysefunktionen sollen konzeptionell austauschbar beziehungsweise erweiterbar bleiben.

### 3.7 Open-Source- und Offenheitsprinzip

Für Kernkomponenten sollen bevorzugt aktive Open-Source-Komponenten mit klarer Lizenz und reproduzierbarer Beschaffung eingesetzt werden.

Ein proprietärer Dienst darf für eine spätere Zusatzfunktion optional integrierbar sein, darf aber die grundlegende Volltextsuche nicht voraussetzen.

---

## 4. Abgrenzung

### 4.1 Das Produkt ist

- ein Crawler,
- ein Dokument- und Webindexer,
- eine Volltextsuchlösung,
- eine Quellenverwaltung,
- eine Suchoberfläche,
- eine Plattform für spätere zusätzliche Such- und Analysefunktionen.

### 4.2 Das Produkt ist in Version 1.x ausdrücklich kein

- vollwertiges Dokumentenmanagementsystem,
- Ersatz für SharePoint oder Nextcloud,
- Fileserver,
- Synchronisationsdienst,
- Backup-System,
- Records-Management-System,
- Groupware-System,
- Office-Editor,
- Workflow-/BPM-System,
- öffentlicher Filesharing-Dienst,
- allgemeiner KI-Assistent.

---

## 5. Zielgruppen

### 5.1 Einzelanwender

Anwender mit großen, über Jahre gewachsenen Dokumentbeständen auf:

- lokalen Laufwerken,
- externen Festplatten,
- USB-Sticks,
- NAS-/Netzlaufwerken,
- Webseitenarchiven.

### 5.2 Kleine Arbeitsgruppen

Kleine Teams, die gemeinsame Netzfreigaben durchsuchen und dabei bestehende Zugriffsrechte respektiert sehen müssen.

### 5.3 Administratoren

Personen, die:

- Quellen konfigurieren,
- Crawl-Jobs überwachen,
- Fehler analysieren,
- Indizes warten,
- Zugriffsregeln kontrollieren,
- Backups durchführen.

### 5.4 Entwickler und Integratoren

Personen, die über APIs:

- Suchabfragen ausführen,
- Quellen verwalten,
- Statusinformationen abrufen,
- Indexierungsjobs anstoßen,
- Zusatzanwendungen anbinden.

---

# 6. Release-Strategie

Die Entwicklung wird bewusst in aufeinander aufbauende Versionen geteilt.

| Version | Schwerpunkt | Produktstatus |
|---|---|---|
| **0.1.0** | lokaler vertikaler Kern | technischer Produktkern |
| **0.2.0** | USB-/Offline-Medien und SMB/NAS | Quellenbasis |
| **0.3.0** | produktiver Webcrawler | Quellenbasis vollständig |
| **0.4.0** | robuste Dokumentextraktion, Archive, Metadaten | Inhaltsbasis |
| **0.5.0** | OCR und vollständige Kern-Suchoberfläche | **MVP** |
| **0.6.0** | Benutzer, Rollen, ACL/Security Trimming | Mehrbenutzerfähigkeit |
| **0.7.0** | API, Administration, Scheduling, Betrieb | verwaltbares System |
| **0.8.0** | Performance, Skalierung, Zuverlässigkeit | Produktionshärtung |
| **0.9.0** | RC, Migration, Packaging, Recovery, Accessibility | Release Candidate |
| **1.0.0** | stabile Baseline | erste produktive Hauptversion |
| **1.1.x** | Suchkomfort und persönliche Organisation | Komfortrelease |
| **1.2.x** | reichere Metadaten und Dokumentanalyse | Content-Release |
| **1.3.x** | Enterprise-Authentisierung und weitere Connectoren | Integrationsrelease |
| **1.4.x** | Suchanalyse, Query-Tuning, Benachrichtigungen | Betriebs-/Qualitätsrelease |
| **1.5.x** | Ähnlichkeit, NER und optionale Vektorsuche | semantische Vorstufe |
| **2.0.0** | RAG, Knowledge Graph und KI-gestützte Recherche | optionale KI-Generation |

Die Versionszuordnung ist fachliche Planung. Funktionen dürfen früher geliefert werden, wenn dies ohne erhebliche Zusatzkomplexität möglich ist. Eine für ein Release als MUSS definierte Funktion darf jedoch nicht kommentarlos in ein späteres Release verschoben werden.

---

# 7. Prioritätsklassen

| Priorität | Bedeutung |
|---|---|
| **MUSS** | für das angegebene Zielrelease verbindlich |
| **SOLL** | hoher Nutzen; darf nur mit dokumentierter Begründung verschoben werden |
| **KANN** | sinnvoll, aber nicht releaseblockierend |
| **NICHT** | bewusst außerhalb des geplanten Produktumfangs |

---

# 8. Funktionale Anforderungen

## 8.1 Quellenverwaltung

### SRC-001 – Quellen als eigenständige Objekte
**Priorität:** MUSS  
**Release:** 0.1.0

Jede indexierte Quelle muss als eigenständiges konfigurierbares Objekt verwaltet werden.

Eine Quelle muss mindestens besitzen:

- eindeutige Source-ID,
- Anzeigename,
- Quellentyp,
- Startpunkt,
- Aktiv/Inaktiv-Status,
- Crawl-Strategie,
- Zeitplan,
- Include-/Exclude-Regeln,
- Fehlerstatus,
- letzten erfolgreichen Lauf,
- nächsten geplanten Lauf.

### SRC-002 – Quellentypen erweiterbar
**Priorität:** MUSS  
**Release:** 0.1.0

Die fachliche Quellenarchitektur muss zusätzliche Quellentypen ermöglichen, ohne das bestehende Datenmodell neu entwerfen zu müssen.

### SRC-003 – Quelle aktivieren/deaktivieren
**Priorität:** MUSS  
**Release:** 0.1.0

Eine Quelle muss deaktiviert werden können, ohne ihre bereits bekannten Dokumente unmittelbar aus dem Index zu löschen.

### SRC-004 – Vollständiger Neuaufbau pro Quelle
**Priorität:** MUSS  
**Release:** 0.1.0

Für jede Quelle muss eine vollständige Reindexierung ausgelöst werden können.

### SRC-005 – Inkrementelle Aktualisierung
**Priorität:** MUSS  
**Release:** 0.1.0

Nach einer initialen Indexierung sollen nur neue, geänderte oder entfernte Dokumente erneut verarbeitet werden.

### SRC-006 – Include-Regeln
**Priorität:** MUSS  
**Release:** 0.1.0

Administratoren müssen festlegen können, welche Pfade, URLs und Dateitypen berücksichtigt werden.

### SRC-007 – Exclude-Regeln
**Priorität:** MUSS  
**Release:** 0.1.0

Administratoren müssen temporäre, technische, vertrauliche oder irrelevante Bereiche vom Crawling ausschließen können.

### SRC-008 – Quellspezifische Größenlimits
**Priorität:** SOLL  
**Release:** 0.4.0

Für eine Quelle sollen maximale Dokumentgrößen und vergleichbare Verarbeitungsgrenzen einstellbar sein.

### SRC-009 – Quellspezifische Priorität
**Priorität:** KANN  
**Release:** 1.4.x

Crawler-Jobs dürfen später eine Priorität erhalten.

---

## 8.2 Lokale Dateisysteme

### LOC-001 – Lokale Verzeichnisse
**Priorität:** MUSS  
**Release:** 0.1.0

Beliebige für den Prozess lesbare lokale Verzeichnisse müssen indexiert werden können.

### LOC-002 – Rekursive Traversierung
**Priorität:** MUSS  
**Release:** 0.1.0

Unterverzeichnisse müssen rekursiv verarbeitet werden können.

### LOC-003 – Rekursion begrenzbar
**Priorität:** MUSS  
**Release:** 0.1.0

Maximale Verzeichnistiefe muss konfigurierbar sein.

### LOC-004 – Symbolische Links/Junctions
**Priorität:** MUSS  
**Release:** 0.4.0

Das System muss Schleifen durch symbolische Links, Junctions oder vergleichbare Dateisystemverweise erkennen beziehungsweise verhindern.

### LOC-005 – Dateisystemfehler
**Priorität:** MUSS  
**Release:** 0.1.0

Eine unlesbare Datei darf nicht den gesamten Crawl abbrechen.

### LOC-006 – Pfadänderungen
**Priorität:** MUSS  
**Release:** 0.4.0

Umbenannte oder verschobene Dokumente sollen nach Möglichkeit als dasselbe logische Dokument erkannt werden.

---

## 8.3 USB- und Wechseldatenträger

### USB-001 – Wechseldatenträger als eigene Quelle
**Priorität:** MUSS  
**Release:** 0.2.0

USB-Sticks und externe Laufwerke müssen als Quellen verwaltet werden können.

### USB-002 – Stabile Medienidentität
**Priorität:** MUSS  
**Release:** 0.2.0

Ein Datenträger darf nicht ausschließlich über seinen Laufwerksbuchstaben oder Mountpunkt identifiziert werden.

Das Produkt muss eine stabilere Medienidentität verwenden beziehungsweise verwalten können.

### USB-003 – Offline-Status
**Priorität:** MUSS  
**Release:** 0.2.0

Ein aktuell nicht angeschlossener Datenträger muss als **offline** erkannt werden.

### USB-004 – Offline-Treffer erhalten
**Priorität:** MUSS  
**Release:** 0.2.0

Treffer eines offline befindlichen Datenträgers müssen im Index erhalten bleiben.

### USB-005 – Offline-Treffer kennzeichnen
**Priorität:** MUSS  
**Release:** 0.2.0

Die Trefferanzeige muss erkennen lassen, dass das Originalmedium aktuell nicht verfügbar ist.

### USB-006 – Medienname anzeigen
**Priorität:** MUSS  
**Release:** 0.2.0

Bei Offline-Treffern muss der Benutzer erkennen können, auf welchem Datenträger sich das Original befindet.

### USB-007 – Wiederanschluss erkennen
**Priorität:** MUSS  
**Release:** 0.2.0

Nach erneutem Anschließen soll ein Medium trotz geändertem Laufwerksbuchstaben beziehungsweise Mountpunkt wiedererkannt werden.

### USB-008 – Fehlendes Medium ist keine Löschung
**Priorität:** MUSS  
**Release:** 0.2.0

Ein nicht verfügbares Medium darf nicht automatisch eine Massendeletion im Index auslösen.

### USB-009 – Portable Indizes
**Priorität:** KANN  
**Release:** 1.2.x

Optional darf später ein Index gemeinsam mit einem Datenträger transportierbar sein.

---

## 8.4 SMB/CIFS und Netzlaufwerke

### SMB-001 – SMB/CIFS-Freigaben
**Priorität:** MUSS  
**Release:** 0.2.0

Windows-/Samba-Freigaben müssen indexierbar sein.

### SMB-002 – Gemappte Netzlaufwerke
**Priorität:** MUSS  
**Release:** 0.2.0

Bereits vom Betriebssystem eingebundene Netzlaufwerke müssen als Quelle verwendbar sein.

### SMB-003 – UNC-/Netzpfade
**Priorität:** MUSS  
**Release:** 0.2.0

Direkte Netzwerkpfade sollen ohne zwingendes manuelles Mapping nutzbar sein, sofern Plattform und Berechtigungen dies unterstützen.

### SMB-004 – Verbindungsunterbrechung
**Priorität:** MUSS  
**Release:** 0.2.0

Eine vorübergehend nicht erreichbare Freigabe darf nicht als Löschung des gesamten Quellenbestands gewertet werden.

### SMB-005 – Wiederholungsstrategie
**Priorität:** MUSS  
**Release:** 0.7.0

Temporäre Netzwerkfehler müssen kontrolliert wiederholt werden.

### SMB-006 – Verbindungs- und Lastbegrenzung
**Priorität:** SOLL  
**Release:** 0.7.0

Gleichzeitige Zugriffe auf langsame oder empfindliche Netzquellen sollen begrenzbar sein.

### SMB-007 – Quellberechtigungen
**Priorität:** MUSS  
**Release:** 0.6.0

Für Multiuser-Betrieb müssen relevante Leseberechtigungen einer Netzwerkquelle soweit technisch verfügbar übernommen werden können.

---

## 8.5 Webcrawler

### WEB-001 – HTTP/HTTPS
**Priorität:** MUSS  
**Release:** 0.3.0

Webseiten über HTTP und HTTPS müssen crawlfähig sein.

### WEB-002 – Start-URLs
**Priorität:** MUSS  
**Release:** 0.3.0

Pro Webquelle müssen eine oder mehrere Start-URLs konfigurierbar sein.

### WEB-003 – Domain-/Host-Grenzen
**Priorität:** MUSS  
**Release:** 0.3.0

Der Crawler muss auf definierte Hosts beziehungsweise Domains beschränkt werden können.

### WEB-004 – Include-/Exclude-URL-Regeln
**Priorität:** MUSS  
**Release:** 0.3.0

URLs müssen anhand von Regeln ein- oder ausgeschlossen werden können.

### WEB-005 – Crawl-Tiefe
**Priorität:** MUSS  
**Release:** 0.3.0

Die maximale Linktiefe muss begrenzbar sein.

### WEB-006 – URL-Normalisierung
**Priorität:** MUSS  
**Release:** 0.3.0

Semantisch identische URL-Varianten sollen nicht unnötig als verschiedene Dokumente indexiert werden.

### WEB-007 – Canonical URL
**Priorität:** SOLL  
**Release:** 0.3.0

Canonical-Hinweise sollen berücksichtigt werden.

### WEB-008 – Redirects
**Priorität:** MUSS  
**Release:** 0.3.0

HTTP-Redirects müssen korrekt verarbeitet und begrenzt werden.

### WEB-009 – robots.txt
**Priorität:** MUSS  
**Release:** 0.3.0

Für öffentliche Webseiten muss die Beachtung von `robots.txt` standardmäßig aktiviert sein.

### WEB-010 – Sitemap
**Priorität:** SOLL  
**Release:** 0.3.0

XML-Sitemaps sollen als Discovery-Quelle genutzt werden können.

### WEB-011 – Rate Limiting
**Priorität:** MUSS  
**Release:** 0.3.0

Zugriffsgeschwindigkeit und Parallelität müssen pro Webquelle begrenzt werden können.

### WEB-012 – User-Agent
**Priorität:** MUSS  
**Release:** 0.3.0

Der Crawler muss einen konfigurierbaren, identifizierbaren User-Agent verwenden.

### WEB-013 – Timeouts
**Priorität:** MUSS  
**Release:** 0.3.0

Verbindungs- und Download-Timeouts müssen vorhanden und konfigurierbar sein.

### WEB-014 – HTTP-Fehler
**Priorität:** MUSS  
**Release:** 0.3.0

404, 410, 401, 403, 429 und Serverfehler müssen unterscheidbar behandelt werden.

### WEB-015 – Web-Löschung
**Priorität:** MUSS  
**Release:** 0.3.0

Dauerhaft entfernte Webseiten sollen nach definierter Bestätigung beziehungsweise Policy aus dem aktuellen Index entfernt werden können.

### WEB-016 – Crawl-Traps
**Priorität:** MUSS  
**Release:** 0.3.0

Das Produkt muss Mechanismen gegen URL-Explosion, Session-ID-Schleifen, Kalenderfallen und vergleichbare Crawl-Traps besitzen.

### WEB-017 – Verlinkte Dokumente
**Priorität:** MUSS  
**Release:** 0.3.0

Von Webseiten verlinkte unterstützte PDF- und Office-Dokumente müssen nach Regelwerk indexierbar sein.

### WEB-018 – Authentisierte Websites
**Priorität:** SOLL  
**Release:** 1.3.x

Authentisierung gegen geschützte Websites soll später unterstützt werden.

### WEB-019 – JavaScript-renderte SPAs
**Priorität:** KANN  
**Release:** 1.3.x

Browserbasiertes Rendering für JavaScript-lastige Websites darf als optionaler Connector ergänzt werden.

---

## 8.6 Dokument- und Dateiformate

### EXT-001 – Textdateien
**Priorität:** MUSS  
**Release:** 0.1.0

Reine Textdateien müssen indexiert werden.

### EXT-002 – HTML
**Priorität:** MUSS  
**Release:** 0.1.0

HTML muss in bereinigten sichtbaren Text und relevante Metadaten zerlegt werden können.

### EXT-003 – PDF
**Priorität:** MUSS  
**Release:** 0.4.0

Textbasierte PDF-Dokumente müssen vollständig inhaltsindexiert werden können.

### EXT-004 – DOCX
**Priorität:** MUSS  
**Release:** 0.4.0

Microsoft Word DOCX muss indexiert werden.

### EXT-005 – DOC
**Priorität:** MUSS  
**Release:** 0.4.0

Alte Word-Binärformate müssen unterstützt werden, soweit ein stabiler Parser vorhanden ist.

### EXT-006 – XLSX
**Priorität:** MUSS  
**Release:** 0.4.0

Excel-XLSX muss indexiert werden.

### EXT-007 – XLS
**Priorität:** MUSS  
**Release:** 0.4.0

Alte Excel-XLS-Dateien müssen unterstützt werden.

### EXT-008 – Excel-Blätter
**Priorität:** MUSS  
**Release:** 0.4.0

Text aus mehreren Tabellenblättern muss auffindbar sein.

### EXT-009 – PPTX
**Priorität:** MUSS  
**Release:** 0.4.0

PowerPoint PPTX muss indexiert werden.

### EXT-010 – PPT
**Priorität:** SOLL  
**Release:** 0.4.0

Alte PowerPoint-Formate sollen unterstützt werden.

### EXT-011 – OpenDocument
**Priorität:** MUSS  
**Release:** 0.4.0

ODT, ODS und ODP müssen als offene Office-Formate unterstützt werden.

### EXT-012 – RTF
**Priorität:** SOLL  
**Release:** 0.4.0

RTF-Dokumente sollen indexiert werden.

### EXT-013 – EPUB
**Priorität:** SOLL  
**Release:** 1.2.x

EPUB soll später als Dokumentquelle unterstützt werden.

### EXT-014 – E-Mail-Dateien
**Priorität:** SOLL  
**Release:** 1.2.x

EML und verbreitete E-Mail-Container sollen später indexierbar sein.

### EXT-015 – PST/OST
**Priorität:** KANN  
**Release:** 1.3.x

Outlook-Archive dürfen später als spezialisierter Connector ergänzt werden.

### EXT-016 – Unbekannte Binärformate
**Priorität:** MUSS  
**Release:** 0.4.0

Unbekannte oder nicht unterstützte Formate dürfen den Crawl nicht stoppen.

### EXT-017 – Parseridentität
**Priorität:** MUSS  
**Release:** 0.4.0

Für ein indexiertes Dokument soll nachvollziehbar sein, mit welchem Parser beziehungsweise welcher Parsergeneration der Inhalt erzeugt wurde.

---

## 8.7 Archive und eingebettete Dokumente

### ARC-001 – ZIP
**Priorität:** MUSS  
**Release:** 0.4.0

ZIP-Archive müssen optional rekursiv durchsucht werden können.

### ARC-002 – Archivpfad
**Priorität:** MUSS  
**Release:** 0.4.0

Treffer innerhalb eines Archives müssen ihre Lage innerhalb des Containers anzeigen.

### ARC-003 – Rekursion begrenzen
**Priorität:** MUSS  
**Release:** 0.4.0

Archivrekursion muss durch maximale Tiefe, maximale Elementanzahl und maximale entpackte Größe begrenzbar sein.

### ARC-004 – Zip-Bomb-Schutz
**Priorität:** MUSS  
**Release:** 0.4.0

Archive Bombs beziehungsweise extreme Kompressionsverhältnisse müssen abgefangen werden.

### ARC-005 – Weitere Archive
**Priorität:** SOLL  
**Release:** 1.2.x

7z, TAR und vergleichbare verbreitete Formate sollen ergänzt werden.

### ARC-006 – Passwortgeschützte Archive
**Priorität:** KANN  
**Release:** 1.3.x

Ein optionaler sicherer Passwortprovider darf später unterstützt werden. Das Produkt darf keine Passwörter erraten oder Schutzmechanismen umgehen.

---

## 8.8 OCR

### OCR-001 – Scan-PDF erkennen
**Priorität:** MUSS  
**Release:** 0.5.0

Das System muss erkennen können, ob ein PDF keinen brauchbaren extrahierbaren Text besitzt.

### OCR-002 – OCR für Scan-PDF
**Priorität:** MUSS  
**Release:** 0.5.0

Reine Scan-PDFs müssen per OCR inhaltsdurchsuchbar gemacht werden können.

### OCR-003 – Bilder
**Priorität:** SOLL  
**Release:** 0.5.0

JPG, PNG und TIFF sollen für OCR indexiert werden können.

### OCR-004 – Sprachen
**Priorität:** MUSS  
**Release:** 0.5.0

Mindestens Deutsch und Englisch müssen konfigurierbar sein.

### OCR-005 – Mehrsprachigkeit
**Priorität:** MUSS  
**Release:** 0.5.0

Mehrere OCR-Sprachen sollen gemeinsam konfiguriert werden können.

### OCR-006 – OCR nicht unnötig ausführen
**Priorität:** MUSS  
**Release:** 0.5.0

Vorhandener brauchbarer Text soll nicht standardmäßig erneut per OCR verarbeitet werden.

### OCR-007 – OCR-Metadaten
**Priorität:** MUSS  
**Release:** 0.5.0

Folgende Informationen sollen technisch nachvollziehbar sein:

- OCR angewendet ja/nein,
- Sprache,
- Engine,
- Engine-Version,
- Verarbeitungszeitpunkt,
- gegebenenfalls Confidence.

### OCR-008 – OCR wiederholen
**Priorität:** SOLL  
**Release:** 0.7.0

Nach Upgrade einer OCR-Engine soll eine gezielte Neuverarbeitung möglich sein.

### OCR-009 – OCR-Ressourcenlimit
**Priorität:** MUSS  
**Release:** 0.5.0

OCR muss begrenzbare CPU-/Zeit-/Seitenressourcen verwenden.

---

## 8.9 Dokumentidentität und Änderungsmodell

### ID-001 – DocumentId
**Priorität:** MUSS  
**Release:** 0.1.0

Jedes logische Dokument muss eine interne eindeutige Identität erhalten.

### ID-002 – SourceId
**Priorität:** MUSS  
**Release:** 0.1.0

Dokumente müssen ihrer Quelle eindeutig zugeordnet sein.

### ID-003 – Canonical URI
**Priorität:** MUSS  
**Release:** 0.1.0

Für jedes Dokument muss ein kanonischer Locator beziehungsweise URI gespeichert werden.

### ID-004 – MediaId
**Priorität:** MUSS  
**Release:** 0.2.0

Dokumente auf Wechseldatenträgern müssen mit einer stabilen Media-ID verknüpft sein.

### ID-005 – Relative Path
**Priorität:** MUSS  
**Release:** 0.2.0

Bei portablen beziehungsweise wechselnden Mountpunkten muss zusätzlich ein relativer Pfad gespeichert werden.

### ID-006 – Content Hash
**Priorität:** MUSS  
**Release:** 0.4.0

Für geeignete Dateien soll ein kryptografischer Inhaltsfingerprint gespeichert werden.

### ID-007 – Zeitstempel und Größe
**Priorität:** MUSS  
**Release:** 0.1.0

Größe, Änderungszeit und soweit sinnvoll Erstellungszeit müssen gespeichert werden.

### ID-008 – LastSeen
**Priorität:** MUSS  
**Release:** 0.1.0

Der letzte bestätigte Sichtkontakt eines Dokuments muss gespeichert werden.

### ID-009 – Availability
**Priorität:** MUSS  
**Release:** 0.2.0

Dokumente müssen Zustände wie verfügbar, offline, gelöscht, fehlerhaft oder unbekannt unterscheiden können.

### ID-010 – Rename-/Move-Erkennung
**Priorität:** SOLL  
**Release:** 0.4.0

Das Produkt soll Pfadänderungen möglichst ohne Erzeugen künstlicher Dubletten erkennen.

### ID-011 – Dublettenerkennung
**Priorität:** SOLL  
**Release:** 1.1.x

Inhaltsgleiche Dokumente sollen als Dubletten erkennbar sein.

---

## 8.10 Metadaten

### META-001 – Basisfelder
**Priorität:** MUSS  
**Release:** 0.1.0

Mindestens folgende Felder müssen vorhanden sein:

- Dateiname beziehungsweise Titel,
- Pfad/URL,
- Quelle,
- MIME-Type,
- Dateityp,
- Größe,
- Änderungszeit,
- Indexierungszeit.

### META-002 – Dokumenttitel
**Priorität:** MUSS  
**Release:** 0.4.0

Ein im Dokument vorhandener Titel soll extrahiert werden.

### META-003 – Autor
**Priorität:** SOLL  
**Release:** 0.4.0

Dokumentautoren sollen soweit verfügbar übernommen werden.

### META-004 – Erstellungsdatum
**Priorität:** SOLL  
**Release:** 0.4.0

Dokumentinterne Erstellungsdaten sollen übernommen werden.

### META-005 – Sprache
**Priorität:** SOLL  
**Release:** 0.5.0

Dokumentsprache soll soweit zuverlässig bestimmbar gespeichert werden.

### META-006 – EXIF
**Priorität:** SOLL  
**Release:** 1.2.x

Bildmetadaten sollen später indexierbar sein.

### META-007 – GPS
**Priorität:** KANN  
**Release:** 1.2.x

GPS-Metadaten dürfen später als filterbare Felder aufgenommen werden.

### META-008 – Benutzerdefinierte Felder
**Priorität:** SOLL  
**Release:** 1.2.x

Administratoren sollen später zusätzliche Metadatenfelder definieren können.

---

## 8.11 Suchindex

### IDX-001 – Persistenter Inhaltsindex
**Priorität:** MUSS  
**Release:** 0.1.0

Dokumentinhalte müssen persistent indexiert werden. Die Suche darf nicht bei jeder Anfrage sämtliche Originaldateien erneut durchsuchen.

### IDX-002 – Inkrementelle Updates
**Priorität:** MUSS  
**Release:** 0.1.0

Ein Dokument muss aktualisierbar sein, ohne den gesamten Index neu aufzubauen.

### IDX-003 – Löschung
**Priorität:** MUSS  
**Release:** 0.1.0

Bestätigt gelöschte Dokumente müssen gezielt aus dem aktuellen Index entfernt werden können.

### IDX-004 – Offline versus gelöscht
**Priorität:** MUSS  
**Release:** 0.2.0

Der Index muss zwischen offline und gelöscht unterscheiden.

### IDX-005 – Indexversion
**Priorität:** MUSS  
**Release:** 0.4.0

Indexschema beziehungsweise Indexgeneration muss versioniert sein.

### IDX-006 – Reindex nach Parserupgrade
**Priorität:** MUSS  
**Release:** 0.7.0

Dokumente müssen anhand Parser-/Schema-Version gezielt neu indexierbar sein.

### IDX-007 – Konsistenzprüfung
**Priorität:** MUSS  
**Release:** 0.8.0

Index und Quellenstatus müssen auf Inkonsistenzen geprüft werden können.

### IDX-008 – Recovery
**Priorität:** MUSS  
**Release:** 0.9.0

Ein beschädigter Index darf aus gespeicherter Konfiguration und Quellen reproduzierbar neu aufgebaut werden können.

---

## 8.12 Suche

### SEA-001 – Stichwortsuche
**Priorität:** MUSS  
**Release:** 0.1.0

Einzelne Suchbegriffe müssen im Inhalt und in wesentlichen Metadaten gesucht werden können.

### SEA-002 – Phrase Search
**Priorität:** MUSS  
**Release:** 0.5.0

Exakte Wortfolgen müssen durchsuchbar sein.

### SEA-003 – AND
**Priorität:** MUSS  
**Release:** 0.5.0

Boolesches AND muss unterstützt werden.

### SEA-004 – OR
**Priorität:** MUSS  
**Release:** 0.5.0

Boolesches OR muss unterstützt werden.

### SEA-005 – NOT
**Priorität:** MUSS  
**Release:** 0.5.0

Boolesches NOT muss unterstützt werden.

### SEA-006 – Feldsuche
**Priorität:** MUSS  
**Release:** 0.5.0

Mindestens Felder für:

- Dateiname,
- Inhalt,
- Pfad/URL,
- Quelle,
- Dateityp,
- Datum

müssen gezielt such- beziehungsweise filterbar sein.

### SEA-007 – Relevanz
**Priorität:** MUSS  
**Release:** 0.5.0

Treffer müssen nach nachvollziehbarer textbasierter Relevanz sortiert werden können.

### SEA-008 – Sortierung
**Priorität:** MUSS  
**Release:** 0.5.0

Mindestens sortierbar nach:

- Relevanz,
- Name,
- Änderungsdatum.

### SEA-009 – Fuzzy Search
**Priorität:** SOLL  
**Release:** 0.5.0

Unscharfe Suche soll typische Tippfehler auffangen.

### SEA-010 – Prefix/Wildcard
**Priorität:** SOLL  
**Release:** 0.5.0

Prefix- beziehungsweise Wildcard-Suche soll verfügbar sein.

### SEA-011 – Facetten
**Priorität:** MUSS  
**Release:** 0.5.0

Mindestens folgende Facetten/Filter sollen verfügbar sein:

- Quelle,
- Dateityp,
- Datum,
- Verfügbarkeitsstatus.

### SEA-012 – Sprache
**Priorität:** SOLL  
**Release:** 0.5.0

Nach Dokumentensprache soll filterbar sein, soweit erkannt.

### SEA-013 – Autor
**Priorität:** SOLL  
**Release:** 0.5.0

Nach Autor soll filterbar beziehungsweise suchbar sein.

### SEA-014 – Synonyme
**Priorität:** SOLL  
**Release:** 1.1.x

Administrierbare Synonyme sollen später verfügbar sein.

### SEA-015 – Spellcheck
**Priorität:** SOLL  
**Release:** 1.1.x

Bei wahrscheinlich falsch geschriebenen Suchbegriffen sollen Vorschläge angeboten werden.

### SEA-016 – Autocomplete
**Priorität:** SOLL  
**Release:** 1.1.x

Suchvorschläge während der Eingabe sollen später verfügbar sein.

### SEA-017 – Gespeicherte Suchen
**Priorität:** SOLL  
**Release:** 1.1.x

Benutzer sollen Suchen speichern können.

### SEA-018 – Suchhistorie
**Priorität:** KANN  
**Release:** 1.1.x

Eine persönliche Suchhistorie darf optional verfügbar sein und muss deaktivierbar sein.

### SEA-019 – Related/Similar
**Priorität:** SOLL  
**Release:** 1.5.x

Ähnliche Dokumente sollen später auf Anfrage gefunden werden können.

### SEA-020 – Result Pinning
**Priorität:** SOLL  
**Release:** 1.4.x

Administratoren sollen für definierte Suchbegriffe bestimmte Ergebnisse anheben können.

### SEA-021 – Learning to Rank
**Priorität:** KANN  
**Release:** 1.5.x

Datengetriebenes Ranking darf später optional ergänzt werden.

---

## 8.13 Trefferanzeige und Benutzeroberfläche

### UI-001 – Zentrales Suchfeld
**Priorität:** MUSS  
**Release:** 0.1.0

Die Suche muss ohne vorherige technische Auswahl eines Indexes oder Backends erreichbar sein.

### UI-002 – Search-as-you-type Reaktion
**Priorität:** SOLL  
**Release:** 0.5.0

Die Oberfläche soll subjektiv schnell reagieren; eine Anfrage darf gegebenenfalls erst nach kurzem Debounce ausgelöst werden.

### UI-003 – Treffername
**Priorität:** MUSS  
**Release:** 0.1.0

Titel beziehungsweise Dateiname muss angezeigt werden.

### UI-004 – Pfad/URL
**Priorität:** MUSS  
**Release:** 0.1.0

Der Fundort muss angezeigt werden.

### UI-005 – Quelle
**Priorität:** MUSS  
**Release:** 0.1.0

Die zugehörige Quelle muss erkennbar sein.

### UI-006 – Snippet
**Priorität:** MUSS  
**Release:** 0.5.0

Ein inhaltlicher Suchausschnitt rund um den Treffer muss angezeigt werden.

### UI-007 – Highlighting
**Priorität:** MUSS  
**Release:** 0.5.0

Suchbegriffe sollen im Trefferkontext hervorgehoben werden.

### UI-008 – Original öffnen
**Priorität:** MUSS  
**Release:** 0.1.0

Ein Benutzer muss das Originaldokument beziehungsweise die URL öffnen können, sofern verfügbar und berechtigt.

### UI-009 – Offline-Information
**Priorität:** MUSS  
**Release:** 0.2.0

Bei Offline-Medien muss die UI klar anzeigen:

- Mediumname,
- Offline-Status,
- bekannten relativen Pfad,
- letzten Sichtzeitpunkt.

### UI-010 – Vorschau
**Priorität:** MUSS  
**Release:** 0.5.0

Für unterstützte Formate muss mindestens eine sichere Textvorschau möglich sein.

### UI-011 – PDF-/Bildthumbnail
**Priorität:** SOLL  
**Release:** 1.1.x

Visuelle Thumbnails sollen später verfügbar sein.

### UI-012 – Tastaturbedienung
**Priorität:** MUSS  
**Release:** 0.9.0

Die Kernsuche muss vollständig per Tastatur bedienbar sein.

### UI-013 – Responsive Weboberfläche
**Priorität:** SOLL  
**Release:** 1.0.0

Eine webbasierte Oberfläche soll auf üblichen Desktop- und Tabletgrößen nutzbar sein.

### UI-014 – Desktopintegration
**Priorität:** KANN  
**Release:** 1.1.x

Ein optionaler Desktop-Client beziehungsweise Launcher darf später ergänzt werden.

---

## 8.14 Benutzer, Rollen und Berechtigungen

### AUTH-001 – Single-User-Betrieb
**Priorität:** MUSS  
**Release:** 0.1.0

Das System muss sicher als Einzelanwendersystem betrieben werden können.

### AUTH-002 – Benutzerkonten
**Priorität:** MUSS  
**Release:** 0.6.0

Für Mehrbenutzerbetrieb müssen Benutzerkonten vorhanden sein.

### AUTH-003 – Rollen
**Priorität:** MUSS  
**Release:** 0.6.0

Mindestens Benutzer- und Administratorrolle müssen unterschieden werden.

### AUTH-004 – Gruppen
**Priorität:** MUSS  
**Release:** 0.6.0

Benutzer müssen Gruppen zugeordnet werden können.

### AUTH-005 – Source ACL
**Priorität:** MUSS  
**Release:** 0.6.0

Soweit eine Quelle Zugriffslisten bereitstellt, müssen diese dem Dokument zugeordnet werden können.

### AUTH-006 – Security Trimming
**Priorität:** MUSS  
**Release:** 0.6.0

Ein Suchergebnis darf nur angezeigt werden, wenn der Benutzer dafür nach dem gespeicherten Berechtigungsmodell leseberechtigt ist.

### AUTH-007 – Snippet-Schutz
**Priorität:** MUSS  
**Release:** 0.6.0

Auch Titel, Snippet, Facetten und Metadaten dürfen keine nicht autorisierten Dokumente verraten.

### AUTH-008 – Deny vor Allow
**Priorität:** MUSS  
**Release:** 0.6.0

Das Berechtigungsmodell muss explizite Verbote korrekt berücksichtigen.

### AUTH-009 – ACL-Fingerprint
**Priorität:** SOLL  
**Release:** 0.6.0

ACL-Zustände sollen versionierbar beziehungsweise über einen Fingerprint auf Änderungen prüfbar sein.

### AUTH-010 – LDAP/Active Directory
**Priorität:** SOLL  
**Release:** 1.3.x

LDAP beziehungsweise Active Directory sollen später als Identitätsquelle unterstützt werden.

### AUTH-011 – OIDC/SSO
**Priorität:** SOLL  
**Release:** 1.3.x

OpenID Connect soll später als moderne SSO-Option unterstützt werden.

### AUTH-012 – TOTP/MFA
**Priorität:** KANN  
**Release:** 1.3.x

Zusätzliche Mehrfaktor-Authentisierung darf für lokale Konten ergänzt werden.

---

## 8.15 Administration und Scheduling

### ADM-001 – Administrationsoberfläche
**Priorität:** MUSS  
**Release:** 0.7.0

Administratoren müssen zentrale Produktfunktionen ohne direkte Datenbankmanipulation verwalten können.

### ADM-002 – Crawl starten
**Priorität:** MUSS  
**Release:** 0.7.0

Ein Crawl muss manuell gestartet werden können.

### ADM-003 – Crawl pausieren
**Priorität:** MUSS  
**Release:** 0.7.0

Längere Jobs sollen pausierbar sein.

### ADM-004 – Crawl fortsetzen
**Priorität:** MUSS  
**Release:** 0.7.0

Pausierte beziehungsweise unterbrochene Jobs sollen soweit möglich fortgesetzt werden.

### ADM-005 – Zeitpläne
**Priorität:** MUSS  
**Release:** 0.7.0

Quellen müssen regelmäßig nach Zeitplan aktualisiert werden können.

### ADM-006 – Fehlerliste
**Priorität:** MUSS  
**Release:** 0.7.0

Fehlgeschlagene Dokumente und URLs müssen mit Fehlergrund einsehbar sein.

### ADM-007 – Retry
**Priorität:** MUSS  
**Release:** 0.7.0

Fehlerhafte Objekte müssen erneut verarbeitet werden können.

### ADM-008 – Failure Queue
**Priorität:** SOLL  
**Release:** 0.7.0

Wiederholt fehlerhafte Dokumente sollen getrennt behandelt werden können.

### ADM-009 – Statistik
**Priorität:** MUSS  
**Release:** 0.7.0

Mindestens anzuzeigen:

- Anzahl Dokumente,
- Anzahl Fehler,
- Crawl-Dauer,
- neuer/geänderter/gelöschter Bestand,
- Indexgröße,
- letzter erfolgreicher Lauf.

### ADM-010 – Throttling
**Priorität:** MUSS  
**Release:** 0.7.0

Ressourcen- und Zugriffslimits sollen quellspezifisch administrierbar sein.

---

## 8.16 API und Automatisierung

### API-001 – Search API
**Priorität:** MUSS  
**Release:** 0.7.0

Suchabfragen müssen programmatisch ausführbar sein.

### API-002 – Source API
**Priorität:** SOLL  
**Release:** 0.7.0

Quellen sollen programmatisch gelesen und verwaltet werden können.

### API-003 – Job API
**Priorität:** SOLL  
**Release:** 0.7.0

Indexierungsjobs sollen programmatisch start- und statusabfragbar sein.

### API-004 – Status API
**Priorität:** MUSS  
**Release:** 0.7.0

Health- und Betriebsstatus müssen maschinenlesbar verfügbar sein.

### API-005 – Bulk Ingest
**Priorität:** SOLL  
**Release:** 1.3.x

Externe Anwendungen sollen Dokumente beziehungsweise Dokumentmetadaten direkt zuführen können.

### API-006 – Webhooks
**Priorität:** SOLL  
**Release:** 1.4.x

Ereignisse wie Crawl abgeschlossen oder Fehlergrenze überschritten sollen später Webhooks auslösen können.

### API-007 – CLI
**Priorität:** MUSS  
**Release:** 0.7.0

Wesentliche administrative Funktionen sollen zusätzlich über eine Kommandozeile automatisierbar sein.

---

## 8.17 Backup, Restore und Migration

### BAK-001 – Konfigurationsbackup
**Priorität:** MUSS  
**Release:** 0.9.0

Quellen, Zeitpläne, Suchkonfiguration und Berechtigungseinstellungen müssen exportierbar sein.

### BAK-002 – Konfigurationsrestore
**Priorität:** MUSS  
**Release:** 0.9.0

Ein Backup muss auf einer kompatiblen Installation wiederherstellbar sein.

### BAK-003 – Indexbackup
**Priorität:** SOLL  
**Release:** 0.9.0

Ein schneller Indexbackup-/restore-Weg soll bereitgestellt werden, sofern das gewählte Backend dies sinnvoll unterstützt.

### BAK-004 – Rebuild statt Backup möglich
**Priorität:** MUSS  
**Release:** 0.9.0

Der Index muss grundsätzlich aus Quellen und Konfiguration neu aufbaubar bleiben.

### BAK-005 – Datenmigration
**Priorität:** MUSS  
**Release:** 0.9.0

Schema- und Konfigurationsmigrationen zwischen unterstützten Produktversionen müssen definiert sein.

### BAK-006 – Rollback
**Priorität:** SOLL  
**Release:** 0.9.0

Für fehlgeschlagene Upgrades soll ein dokumentierter Rückweg existieren.

---


## 8.18 Persönliche Organisation und Suchkomfort nach 1.0

### ORG-001 – Favoriten
**Priorität:** SOLL  
**Release:** 1.1.x

Benutzer sollen Suchtreffer als Favoriten markieren können, ohne die Originaldatei verändern zu müssen.

### ORG-002 – Benutzertags
**Priorität:** SOLL  
**Release:** 1.2.x

Benutzer sollen Dokumenten zusätzliche persönliche beziehungsweise freigegebene Tags zuordnen können, die außerhalb der Originaldatei gespeichert werden.

### ORG-003 – Notizen
**Priorität:** KANN  
**Release:** 1.2.x

Benutzer dürfen kurze Notizen zu indexierten Dokumenten hinterlegen können, ohne das Original zu verändern.

### ORG-004 – Thesaurus
**Priorität:** SOLL  
**Release:** 1.1.x

Administratoren sollen fachliche Begriffe, Ober-/Unterbegriffe und alternative Bezeichnungen in einem Suchthesaurus pflegen können.

### ORG-005 – Populäre Suchbegriffe
**Priorität:** KANN  
**Release:** 1.4.x

Im Mehrbenutzerbetrieb dürfen häufig verwendete Suchbegriffe als administrativ auswertbare Statistik verfügbar sein, sofern Datenschutzkonfiguration dies erlaubt.

### ORG-006 – Suchalarme
**Priorität:** SOLL  
**Release:** 1.4.x

Benutzer sollen eine gespeicherte Suche so konfigurieren können, dass sie bei neuen passenden Dokumenten optional benachrichtigt werden.

### ORG-007 – Benachrichtigungskanäle
**Priorität:** KANN  
**Release:** 1.4.x

Suchalarme dürfen über UI, E-Mail oder Webhook ausgeliefert werden. Externe Kanäle müssen optional sein.

### ORG-008 – Query-Tuning
**Priorität:** SOLL  
**Release:** 1.4.x

Administratoren sollen Suchfelder, Boosts und ausgewählte Rankingparameter kontrolliert konfigurieren können.

### ORG-009 – Search Analytics
**Priorität:** SOLL  
**Release:** 1.4.x

Das Produkt soll datenschutzkonform auswerten können:

- häufige Suchanfragen,
- Suchanfragen ohne Treffer,
- Klicks auf Ergebnisse,
- Antwortzeiten,
- häufig genutzte Filter.

Rohdaten und Aufbewahrungsdauer müssen konfigurierbar sein.

---

## 8.19 Semantische und KI-gestützte Funktionen

### SEM-001 – Similar Documents
**Priorität:** SOLL  
**Release:** 1.5.x

Ausgehend von einem Dokument sollen inhaltlich ähnliche Dokumente auffindbar sein.

### SEM-002 – Named Entity Recognition
**Priorität:** SOLL  
**Release:** 1.5.x

Das Produkt soll optional Entitäten wie Personen, Organisationen, Orte und weitere konfigurierbare Kategorien aus Dokumenten extrahieren können.

### SEM-003 – Entity Linking
**Priorität:** KANN  
**Release:** 1.5.x

Erkannte Entitäten dürfen optional mit normalisierten Entitäten beziehungsweise Wissensquellen verknüpft werden.

### SEM-004 – Automatische Klassifikation
**Priorität:** SOLL  
**Release:** 1.5.x

Dokumente sollen optional anhand definierter Kategorien automatisch klassifiziert werden können.

### SEM-005 – Embeddings
**Priorität:** SOLL  
**Release:** 1.5.x

Für ausgewählte Dokumentfelder dürfen Embeddings erzeugt und versioniert gespeichert werden.

### SEM-006 – Vektorsuche
**Priorität:** SOLL  
**Release:** 1.5.x

Eine optionale Ähnlichkeitssuche über Vektoren soll bereitgestellt werden können.

### SEM-007 – Hybridsuche
**Priorität:** SOLL  
**Release:** 1.5.x

Keyword- und Vektorsuche sollen kombinierbar sein, ohne die klassische Suche zu ersetzen.

### SEM-008 – Semantik abschaltbar
**Priorität:** MUSS  
**Release:** 1.5.x

Alle semantischen Funktionen müssen deaktivierbar sein. Die klassische Volltextsuche muss unabhängig davon vollständig funktionieren.

### AI-001 – RAG
**Priorität:** SOLL  
**Release:** 2.0.0

Das Produkt soll optional Fragen gegen definierte, berechtigte Dokumentbestände beantworten können.

### AI-002 – Quellenbezug
**Priorität:** MUSS  
**Release:** 2.0.0

KI-generierte Antworten müssen auf die verwendeten indexierten Quellen zurückverweisen können.

### AI-003 – ACL vor Kontextbildung
**Priorität:** MUSS  
**Release:** 2.0.0

Nicht berechtigte Dokumente dürfen weder in Retrieval noch in den LLM-Kontext gelangen.

### AI-004 – Zusammenfassungen
**Priorität:** SOLL  
**Release:** 2.0.0

Einzelne Dokumente oder berechtigte Treffermengen dürfen optional zusammengefasst werden.

### AI-005 – Knowledge Graph
**Priorität:** KANN  
**Release:** 2.0.0

Entitäten und Beziehungen dürfen in einer Wissensgraphstruktur explorierbar werden.

### AI-006 – Lokales KI-Backend
**Priorität:** SOLL  
**Release:** 2.0.0

Die Architektur soll ein lokal beziehungsweise selbst gehostet betreibbares KI-Backend ermöglichen.

### AI-007 – Externe KI nur opt-in
**Priorität:** MUSS  
**Release:** 2.0.0

Die Übertragung von Dokumentinhalt an externe KI-Dienste darf ausschließlich nach expliziter administrativer Aktivierung erfolgen.

---

## 8.20 Zusätzliche Connectoren nach 1.0

### CON-001 – SFTP
**Priorität:** SOLL  
**Release:** 1.3.x

SFTP soll als zusätzlicher Dateiquellentyp unterstützt werden.

### CON-002 – FTP/FTPS
**Priorität:** KANN  
**Release:** 1.3.x

FTP beziehungsweise FTPS darf bei nachgewiesenem Bedarf als zusätzlicher Connector verfügbar sein.

### CON-003 – WebDAV
**Priorität:** SOLL  
**Release:** 1.3.x

WebDAV soll als zusätzlicher Dokumentquellentyp unterstützt werden.

### CON-004 – S3-kompatibler Objektspeicher
**Priorität:** SOLL  
**Release:** 1.3.x

S3-kompatible Objektspeicher sollen als zusätzlicher Quellentyp unterstützt werden.

### CON-005 – Nextcloud
**Priorität:** KANN  
**Release:** 1.3.x

Eine direkte Nextcloud-Integration darf ergänzt werden, wenn dies gegenüber WebDAV oder eingebundenem Storage einen ausreichenden Mehrwert bietet.

### CON-006 – SharePoint
**Priorität:** KANN  
**Release:** 2.x

Ein SharePoint-Connector ist eine optionale spätere Enterprise-Erweiterung und kein Bestandteil von Version 1.x.

### CON-007 – Confluence/Jira
**Priorität:** KANN  
**Release:** 2.x

Connectoren für Wissens- und Ticketsysteme dürfen später ergänzt werden.

### CON-008 – Git-Repositories
**Priorität:** KANN  
**Release:** 2.x

Quellcode- und Dokumentationssuche in Git-Repositories darf als eigener Quellentyp später ergänzt werden.


# 9. Nichtfunktionale Anforderungen

## 9.1 Performance

### PERF-001 – Interaktive Suche
**Priorität:** MUSS  
**Release:** 0.8.0

Bei einem repräsentativen lokalen Index sollen normale Suchanfragen im Regelfall subjektiv unmittelbar beantwortet werden.

Zielwert für die Testumgebung:

- Median unter 300 ms,
- 95. Perzentil unter 1 s,

ohne externe OCR- oder Crawl-Arbeit im Anfragepfad.

### PERF-002 – Skalierungsstufen
**Priorität:** MUSS  
**Release:** 0.8.0

Das Produkt muss mindestens mit folgenden Testgrößen geprüft werden:

- 1.000 Dokumente,
- 10.000 Dokumente,
- 100.000 Dokumente,
- 1.000.000 Dokumente.

Die 1.000.000er-Stufe ist ein Skalierungsziel, nicht zwingend Mindesthardware für jeden Installationsmodus.

### PERF-003 – Hintergrundarbeit
**Priorität:** MUSS  
**Release:** 0.8.0

Indexierung darf die Suchfunktion nicht unnötig blockieren.

### PERF-004 – Ressourcenlimits
**Priorität:** MUSS  
**Release:** 0.7.0

Crawler, Parser und OCR müssen begrenzbare Ressourcen verwenden.

---

## 9.2 Zuverlässigkeit

### REL-001 – Einzeldateifehler isolieren
**Priorität:** MUSS  
**Release:** 0.1.0

Eine beschädigte Datei darf keinen vollständigen Jobabbruch verursachen.

### REL-002 – Netzwerkfehler isolieren
**Priorität:** MUSS  
**Release:** 0.2.0

Eine nicht verfügbare Netzwerkquelle darf andere Quellen nicht blockieren.

### REL-003 – Wiederanlauf
**Priorität:** MUSS  
**Release:** 0.8.0

Nach Prozess- oder Systemneustart muss ein konsistenter Zustand wiederherstellbar sein.

### REL-004 – Keine Massendeletion bei Unsicherheit
**Priorität:** MUSS  
**Release:** 0.2.0

Wenn eine Quelle nicht erreichbar ist, dürfen Dokumente nicht allein deswegen als gelöscht markiert werden.

### REL-005 – Checkpoint
**Priorität:** SOLL  
**Release:** 0.7.0

Lange Crawl-Jobs sollen Fortschrittspunkte besitzen.

---

## 9.3 Sicherheit

### SEC-001 – Least Privilege
**Priorität:** MUSS  
**Release:** 0.1.0

Dienste sollen mit den geringstmöglichen benötigten Rechten betrieben werden.

### SEC-002 – Parserisolation
**Priorität:** MUSS  
**Release:** 0.4.0

Die Verarbeitung fremder Dokumente muss so isoliert beziehungsweise begrenzt sein, dass ein Parserfehler nicht ohne Weiteres den Gesamtdienst kompromittiert.

### SEC-003 – Größenlimits
**Priorität:** MUSS  
**Release:** 0.4.0

Extreme Dateien, Archive und eingebettete Objekte müssen begrenzt werden.

### SEC-004 – Keine Credential-Protokollierung
**Priorität:** MUSS  
**Release:** 0.2.0

Passwörter, Tokens und andere Geheimnisse dürfen nicht im Klartext in Logs erscheinen.

### SEC-005 – Secrets geschützt speichern
**Priorität:** MUSS  
**Release:** 0.6.0

Notwendige Zugangsdaten müssen verschlüsselt beziehungsweise über sichere Betriebssystem-/Secret-Mechanismen gespeichert werden.

### SEC-006 – Transportverschlüsselung
**Priorität:** MUSS  
**Release:** 0.6.0

Web- und API-Zugriffe im Mehrbenutzerbetrieb müssen TLS unterstützen.

### SEC-007 – Audit administrativer Änderungen
**Priorität:** MUSS  
**Release:** 0.7.0

Änderungen an Quellen, Rollen, Berechtigungen und kritischen Einstellungen müssen nachvollziehbar protokolliert werden.

### SEC-008 – Webcrawler SSRF-Schutz
**Priorität:** MUSS  
**Release:** 0.3.0

Webquellen müssen so begrenzbar sein, dass ein falsch konfigurierter Crawler nicht unbeabsichtigt beliebige interne Dienste abfragt.

---

## 9.4 Datenschutz

### PRIV-001 – Keine notwendige Cloudübertragung
**Priorität:** MUSS  
**Release:** 1.0.0

Die Kernfunktionen müssen vollständig lokal beziehungsweise selbst gehostet nutzbar sein.

### PRIV-002 – Externe Dienste opt-in
**Priorität:** MUSS  
**Release:** 1.0.0

Spätere Cloud-KI-/Analysefunktionen dürfen nur nach ausdrücklicher Konfiguration Daten an externe Dienste übertragen.

### PRIV-003 – Suchhistorie deaktivierbar
**Priorität:** MUSS  
**Release:** 1.1.x

Persönliche Suchhistorie muss deaktivier- und löschbar sein.

### PRIV-004 – Retention für Logs
**Priorität:** SOLL  
**Release:** 0.9.0

Log- und Auditaufbewahrung soll konfigurierbar sein.

---

## 9.5 Plattformen und Betrieb

### PLAT-001 – Windows
**Priorität:** MUSS  
**Release:** 1.0.0

Die produktive Baseline muss auf einer unterstützten aktuellen Windows-x64-Version betrieben werden können.

### PLAT-002 – Linux
**Priorität:** MUSS  
**Release:** 1.0.0

Die produktive Baseline muss auf einer unterstützten Linux-x64-Serverdistribution betrieben werden können.

### PLAT-003 – Containerbetrieb
**Priorität:** SOLL  
**Release:** 1.0.0

Serverkomponenten sollen containerisiert betreibbar sein, sofern dies die Dateisystem- und USB-Zugriffe nicht unnötig erschwert.

### PLAT-004 – macOS
**Priorität:** KANN  
**Release:** 2.x

macOS darf später unterstützt werden, ist aber keine Voraussetzung für die erste Hauptversion.

---

## 9.6 Bedienbarkeit und Accessibility

### UX-001 – Einfache Erstsuche
**Priorität:** MUSS  
**Release:** 0.5.0

Ein normaler Anwender muss ohne Kenntnis von Query DSL, Indexnamen oder Backenddetails suchen können.

### UX-002 – Technische Details optional
**Priorität:** MUSS  
**Release:** 0.5.0

Erweiterte Suchsyntax darf verfügbar sein, aber die Standardsuche nicht dominieren.

### UX-003 – Fehler verständlich
**Priorität:** MUSS  
**Release:** 0.7.0

Quell- und Indexfehler müssen verständliche Meldungen und technische Details für Administratoren liefern.

### UX-004 – Accessibility
**Priorität:** MUSS  
**Release:** 0.9.0

Die Weboberfläche soll sich an WCAG 2.2 AA orientieren.

### UX-005 – Internationalisierung
**Priorität:** SOLL  
**Release:** 1.1.x

UI-Texte sollen lokalisierbar sein; Deutsch und Englisch sind die ersten Zielsprachen.

---

# 10. Betriebs- und Beobachtbarkeitsanforderungen

### OPS-001 – Health
**Priorität:** MUSS  
**Release:** 0.7.0

Ein klarer Health-Status muss anzeigen, ob:

- Suchindex,
- Metadatenspeicher,
- Parser,
- OCR,
- Scheduler

betriebsbereit sind.

### OPS-002 – Strukturierte Logs
**Priorität:** MUSS  
**Release:** 0.7.0

Logs müssen maschinenlesbar strukturierbar sein.

### OPS-003 – Log-Level
**Priorität:** MUSS  
**Release:** 0.7.0

Mindestens Error, Warning, Information und Debug müssen unterscheidbar sein.

### OPS-004 – Metriken
**Priorität:** SOLL  
**Release:** 0.8.0

Metriken sollen exportierbar sein, darunter:

- Crawlrate,
- Fehlerrate,
- Warteschlangenlänge,
- Query-Latenzen,
- Indexgröße,
- OCR-Durchsatz.

### OPS-005 – Monitoringintegration
**Priorität:** SOLL  
**Release:** 0.8.0

Ein standardisierter Metrikexport, beispielsweise Prometheus-kompatibel, soll bereitgestellt werden.

### OPS-006 – Keine personenbezogenen Volltexte in Standardlogs
**Priorität:** MUSS  
**Release:** 0.7.0

Dokumentvolltext und Suchergebnisinhalte dürfen nicht unnötig in Standardlogs geschrieben werden.

---

# 11. Suchqualität

### QRY-001 – Reproduzierbarer Testkorpus
**Priorität:** MUSS  
**Release:** 0.4.0

Die Entwicklung muss einen versionierten Suchtestkorpus besitzen.

### QRY-002 – Deutsch
**Priorität:** MUSS  
**Release:** 0.5.0

Deutsche Umlaute und `ß` müssen korrekt verarbeitet werden.

### QRY-003 – Englisch
**Priorität:** MUSS  
**Release:** 0.5.0

Englische Dokumente und Suchbegriffe müssen korrekt verarbeitet werden.

### QRY-004 – Unicode
**Priorität:** MUSS  
**Release:** 0.5.0

Index und Suche müssen Unicode korrekt unterstützen.

### QRY-005 – Rankingtest
**Priorität:** MUSS  
**Release:** 0.8.0

Für definierte Abfragen müssen erwartete relevante Dokumente in festgelegten Zielbereichen der Trefferliste liegen.

### QRY-006 – Precision/Recall-Benchmark
**Priorität:** SOLL  
**Release:** 0.8.0

Suchqualität soll mit reproduzierbaren Precision-/Recall-Testfällen bewertet werden.

---

# 12. Vorschau und sicheres Öffnen

### PRE-001 – Textvorschau
**Priorität:** MUSS  
**Release:** 0.5.0

Extrahierter Text muss sicher in der Anwendung angezeigt werden können.

### PRE-002 – Keine Makroausführung
**Priorität:** MUSS  
**Release:** 0.5.0

Vorschau darf keine Office-Makros oder eingebetteten aktiven Inhalte ausführen.

### PRE-003 – Original öffnen
**Priorität:** MUSS  
**Release:** 0.1.0

Das Öffnen des Originals erfolgt über eine explizite Benutzeraktion.

### PRE-004 – Weblink
**Priorität:** MUSS  
**Release:** 0.3.0

Webtreffer müssen die Original-URL öffnen können.

### PRE-005 – Thumbnail
**Priorität:** SOLL  
**Release:** 1.1.x

Sichere Thumbnails für geeignete Dokumente sollen erzeugt werden können.

---

# 13. Funktionen nach Version 1.0

## 13.1 Version 1.1 – Suchkomfort und persönliche Organisation

Diese Funktionen haben hohen Benutzerwert, sind aber nicht für das Kernproblem notwendig:

- Synonymwörterbücher,
- Thesaurus,
- Spellcheck,
- Autocomplete,
- gespeicherte Suchen,
- Favoriten,
- optionale Suchhistorie,
- Thumbnails,
- Dublettendarstellung,
- UI-Lokalisierung,
- optionaler Desktop-Launcher.

**Begründung:**  
Sie verbessern den täglichen Komfort, lösen aber kein grundlegendes Indexierungsproblem. Ein fehlerfreier Crawler ist wichtiger als Favoriten oder Autocomplete.

---

## 13.2 Version 1.2 – reichere Inhalte und Metadaten

Geplant:

- EXIF,
- GPS,
- Benutzertags und optionale Notizen,
- EPUB,
- E-Mail-Dateien,
- weitere Archivformate,
- benutzerdefinierte Metadaten,
- portable Indexoption,
- erweiterte Dokumentvorschau.

**Begründung:**  
Diese Fähigkeiten verbreitern den Content-Typenmix, ohne das Kernprodukt konzeptionell zu verändern.

---

## 13.3 Version 1.3 – Enterprise-Integration

Geplant:

- LDAP,
- Active Directory,
- OpenID Connect,
- optional TOTP,
- SFTP,
- FTP/FTPS soweit benötigt,
- WebDAV,
- S3-kompatible Objektspeicher,
- weitere Repository-Connectoren,
- authentisierte Webseiten,
- optional Browser-Rendering,
- Bulk-Ingest-API,
- PST/OST-Spezialconnector.

**Begründung:**  
Diese Funktionen sind für Organisationen wertvoll, erzeugen aber erheblichen Test- und Sicherheitsaufwand und sollen die erste stabile Baseline nicht verzögern.

---

## 13.4 Version 1.4 – Suchbetrieb und Qualitätstuning

Geplant:

- Result Pinning/Key Match,
- Query Tuning,
- Suchanalyse,
- populäre Suchbegriffe,
- Alerts,
- Webhooks,
- priorisierte Crawl-Jobs,
- erweiterte Metriken und Reports.

**Begründung:**  
Diese Funktionen werden erst sinnvoll, wenn reale Nutzungsmuster vorliegen.

---

## 13.5 Version 1.5 – semantische Vorstufe

Geplant:

- Similar Documents,
- Named Entity Recognition,
- Entity Linking,
- optionale Embeddings,
- optionale Vektorsuche,
- hybride Keyword-/Semantiksuche,
- automatische Klassifikation,
- optional Learning to Rank.

**Wichtige Leitplanke:**  
Semantische Treffer dürfen die klassische Volltextsuche nicht ersetzen. Der Benutzer muss weiterhin deterministisch klassisch suchen können.

---

## 13.6 Version 2.0 – KI-gestützte Recherche

Mögliche Funktionen:

- RAG über den eigenen Index,
- Fragen an definierte Dokumentbestände,
- Quellenzitate auf Dokumentebene,
- Zusammenfassungen,
- Knowledge Graph,
- semantische Exploration,
- Agent-/Tool-Schnittstellen,
- lokale oder externe LLM-Backends.

**Voraussetzungen:**

- vollständige ACL-/Security-Trimming-Kette,
- klare Datenherkunft,
- sichere Prompt-/Context-Erzeugung,
- keine Weitergabe gesperrter Inhalte,
- klassische Suchfunktion bleibt erhalten.

---

# 14. Bewusst nicht geplante beziehungsweise nur als separates Produkt zu betrachtende Funktionen

## 14.1 Vollwertiges DMS
**Status:** NICHT im SASD-Crawler-Kern

Nicht geplant:

- Check-in/Check-out,
- Dokumentversionierung als eigener Speicher,
- Records Management,
- Aufbewahrungsfristen als DMS,
- Dokumentfreigabeprozesse,
- Dokumenteigentümerschaft.

## 14.2 Office-Editing
**Status:** NICHT

Das Produkt soll Dokumente finden und öffnen, nicht Office-Dokumente selbst bearbeiten.

## 14.3 Öffentliche Dokumentfreigabelinks
**Status:** NICHT

Ein öffentlicher Sharing-Dienst erhöht den Angriffs- und Datenschutzumfang erheblich und gehört nicht zum Crawlerziel.

## 14.4 BPM-/Workflow-Engine
**Status:** NICHT in 1.x

Crawler-interne Jobs sind notwendig; fachliche Dokumentworkflows sind ein anderes Produktproblem.

## 14.5 Barcode-Dokumenttrennung
**Status:** NICHT im Kern

Diese Paperless-ngx-Funktion gehört zu einem Scan-/DMS-Importworkflow und ist für Index-in-place nicht erforderlich.

## 14.6 Digitale Signaturen
**Status:** NICHT im Kern

Signaturprüfung kann später als Metadatenanalyse interessant sein; das Signieren selbst ist kein Crawlerziel.

## 14.7 Redaction/Schwärzung
**Status:** NICHT

Das Verändern beziehungsweise Publizieren redigierter Dokumente gehört nicht zum Suchprodukt.

## 14.8 Datei-Synchronisation
**Status:** NICHT

Der Crawler darf keine Dateisynchronisationsplattform werden.

## 14.9 Backup der Originaldaten
**Status:** NICHT

Der SASD-Crawler sichert Konfiguration und gegebenenfalls Index. Die Originaldokumente bleiben Aufgabe des bestehenden Backupsystems.

---

# 15. Produktdatenmodell – fachliche Mindestobjekte

## 15.1 Source

Eine Quelle besitzt mindestens:

- SourceId,
- Name,
- Type,
- Locator,
- Status,
- Schedule,
- IncludeRules,
- ExcludeRules,
- SecurityConfiguration,
- LastRun,
- LastSuccessfulRun.

## 15.2 Media

Für entfernbare Medien:

- MediaId,
- Label,
- erkannte technische IDs,
- letzter Mountpunkt/Laufwerksbuchstabe,
- LastSeen,
- Availability.

## 15.3 Document

Mindestens:

- DocumentId,
- SourceId,
- optional MediaId,
- CanonicalUri,
- RelativePath,
- DisplayName,
- MimeType,
- Size,
- CreatedTime,
- ModifiedTime,
- ContentHash,
- LastSeen,
- Availability,
- ParserName,
- ParserVersion,
- IndexVersion,
- Language,
- OCR-Information,
- Sicherheitsinformationen.

## 15.4 CrawlJob

Mindestens:

- JobId,
- SourceId,
- Start,
- Ende,
- Status,
- Fortschritt,
- Anzahl entdeckt,
- Anzahl neu,
- Anzahl geändert,
- Anzahl gelöscht,
- Anzahl Fehler.

## 15.5 CrawlFailure

Mindestens:

- FailureId,
- JobId,
- Document/URI,
- Kategorie,
- Meldung,
- RetryCount,
- letzter Versuch,
- Status.

## 15.6 User / Group / Role

Für Mehrbenutzerbetrieb:

- UserId,
- Gruppen,
- Rollen,
- externe Identitätsreferenz,
- Aktivstatus.

---

# 16. Abnahmekriterien der Version 1.0

Die Version 1.0 darf erst als stabile erste Hauptversion bezeichnet werden, wenn mindestens folgende End-to-End-Szenarien reproduzierbar funktionieren.

## AC-001 – Lokales Word-Dokument

1. DOCX liegt in konfiguriertem lokalen Ordner.
2. Crawler indexiert es.
3. Begriff kommt nur im Dokumentinhalt vor.
4. Suche liefert das Dokument.
5. Treffer zeigt Snippet.
6. Original lässt sich öffnen.

**Ergebnis:** MUSS erfolgreich sein.

## AC-002 – Excel

Ein Suchbegriff in einem nicht ersten Tabellenblatt einer XLSX-Datei muss gefunden werden.

## AC-003 – PDF

Text in einem normalen PDF muss gefunden werden.

## AC-004 – Scan-PDF

Text, der ausschließlich als Bild in einem Scan-PDF vorhanden ist, muss nach OCR gefunden werden.

## AC-005 – USB online

Dokument auf USB muss indexiert und geöffnet werden können.

## AC-006 – USB offline

Nach Entfernen des USB-Mediums:

- Treffer bleibt vorhanden,
- Status lautet offline,
- Medium ist identifizierbar,
- Massendeletion findet nicht statt.

## AC-007 – USB Laufwerksbuchstabenwechsel

Nach erneutem Anschließen mit anderem Laufwerksbuchstaben soll das Medium wieder erkannt werden.

## AC-008 – SMB verfügbar

Dokument auf SMB-Freigabe muss gefunden werden.

## AC-009 – SMB Ausfall

Bei ausgeschalteter SMB-Quelle darf der Bestand nicht als gelöscht behandelt werden.

## AC-010 – Website

Eine HTML-Seite innerhalb definierter Domain- und Tiefenregeln muss gefunden werden.

## AC-011 – verlinktes PDF

Ein von der Website verlinktes PDF muss entsprechend der Crawl-Policy indexierbar sein.

## AC-012 – Web-Rate-Limit

Der Crawler muss die konfigurierte Zugriffsbeschränkung einhalten.

## AC-013 – Datei geändert

Nach Dokumentänderung muss neuer Inhalt gefunden und veralteter Inhalt nicht mehr als aktuell präsentiert werden.

## AC-014 – Datei gelöscht

Nach bestätigter Löschung muss das Dokument aus aktuellen Ergebnissen verschwinden.

## AC-015 – Datei verschoben

Ein verschobenes Dokument darf nach erfolgreicher Reconciliation nicht dauerhaft doppelt im aktuellen Index erscheinen.

## AC-016 – beschädigtes Dokument

Eine beschädigte Datei erzeugt einen dokumentierten Fehler; andere Dateien werden weiter verarbeitet.

## AC-017 – ZIP

Ein unterstütztes Dokument in einem ZIP muss gefunden werden, wenn Archivindexierung aktiv ist.

## AC-018 – Zip Bomb

Ein präpariertes extrem expandierendes Archiv muss durch Limits gestoppt werden.

## AC-019 – Multiuser ACL

Benutzer A darf einen Treffer sehen, Benutzer B nicht. Benutzer B darf weder Titel noch Snippet noch Facetteninformation dieses Dokuments erhalten.

## AC-020 – Restart

Nach einem Dienstneustart muss die Suche mit konsistentem Bestand wieder verfügbar sein.

## AC-021 – Backup/Restore

Konfiguration muss gesichert und auf einer kompatiblen Installation wiederhergestellt werden können.

## AC-022 – Rebuild

Der Index muss aus den konfigurierten Quellen reproduzierbar neu aufgebaut werden können.

## AC-023 – Performance

Definierte Referenzabfragen müssen die vereinbarten Latenzziele auf der Referenzhardware erfüllen.

## AC-024 – Accessibility

Die Kernsuche muss ohne Maus bedienbar sein.

---

# 17. Testkorpus

Für Entwicklung und Abnahme soll ein kontrollierter Korpus mindestens enthalten:

- TXT,
- HTML,
- DOC,
- DOCX,
- XLS,
- XLSX,
- PPT,
- PPTX,
- ODT,
- ODS,
- PDF mit Text,
- Scan-PDF,
- gemischtes PDF,
- JPG/PNG/TIFF mit Text,
- ZIP mit Dokumenten,
- beschädigte Dokumente,
- sehr große Dateien,
- deutsche und englische Inhalte.

Für Excel sind zusätzlich zu prüfen:

- mehrere Blätter,
- Formeln,
- Kommentare,
- ausgeblendete Blätter,
- Datums- und Zahlenfelder.

Für PDF sind mindestens zu prüfen:

- born-digital,
- reiner Scan,
- gemischter Inhalt,
- fehlerhaft,
- verschlüsselt.

---

# 18. Bewertete Funktionen aus den Vergleichsprodukten

## 18.1 Sofort übernehmen

| Funktionsidee | Referenz | Entscheidung |
|---|---|---|
| Offline-Wechseldatenträger | Recoll | **Kernfunktion 0.2** |
| File + Web in einer Suche | Fess | **Kernfunktion bis 0.5** |
| Source-Konfiguration | Fess/Datafari | **Kernfunktion 0.1** |
| ACL/Security Trimming | Datafari/ManifoldCF | **Kernfunktion 0.6** |
| OCR nur bei Bedarf | Paperless-ngx | **Kernfunktion 0.5** |
| extrem schnelle Suchinteraktion | Everything/Spotlight | **Qualitätsziel** |
| Facetten/Filter | Fess/Solr/OpenSearch | **Kernfunktion 0.5** |
| sichere Parserlimits | Tika/sist2 | **Kernfunktion 0.4** |
| inkrementelle Jobs | Fess/ManifoldCF | **Kernfunktion** |

## 18.2 Nach 1.0 übernehmen

| Funktionsidee | Referenz | Ziel |
|---|---|---|
| Favoriten | Fess | 1.1 |
| Saved Search | Datafari/Fess | 1.1 |
| Spellcheck | Datafari/Fess/Recoll | 1.1 |
| Synonyme | Fess/Recoll/Solr | 1.1 |
| Thumbnails | sist2/Fess | 1.1 |
| EXIF/GPS | sist2 | 1.2 |
| weitere Archive/E-Mail | DocFetcher/Tika | 1.2 |
| LDAP/OIDC | Fess/Mayan/Docspell | 1.3 |
| zusätzliche Connectoren | ManifoldCF | 1.3 |
| Search Analytics | Datafari | 1.4 |
| Result Pinning | Fess | 1.4 |
| Alerts/Webhooks | Datafari/Paperless | 1.4 |
| NER | Open Semantic Search/sist2 | 1.5 |
| Similar Documents | Paperless/OpenSearch | 1.5 |
| Vektorsuche | OpenSearch/Solr | 1.5 |

## 18.3 Nicht in den Crawler übernehmen

| Funktion | Referenzprodukt | Entscheidung |
|---|---|---|
| Records Management | Mayan/OpenKM | nicht Crawler |
| Dokument-Checkout | DMS | nicht Crawler |
| Office Online Editing | DMS | nicht Crawler |
| öffentlicher Fileshare | Paperless/Docspell | nicht Crawler |
| Barcode-Splitting | Paperless | nicht Crawler |
| BPM-Dokumentworkflow | Mayan/OpenKM | separates Thema |
| Redaction | Mayan | nicht Crawler |
| digitale Signaturerstellung | DMS | nicht Crawler |
| Datei-Synchronisation | Nextcloud | nicht Crawler |
| Backup der Originaldateien | DMS/Backup | nicht Crawler |

---

# 19. Technische Entscheidungen, die das Lastenheft bewusst offen lässt

Folgende Punkte sollen in Pflichtenheft, Architekturentscheidung oder PoC entschieden werden:

- eigener Crawler versus Fess-basierter Start,
- Lucene.NET versus OpenSearch versus Solr,
- Apache Tika Server versus Toxy versus Kombination,
- genaue OCR-Engine-Integration,
- konkrete Datenbank für Metadaten,
- Web-UI-Framework,
- Desktop-Client ja/nein,
- Deployment als Windows Service, Linux Service, Container oder Kombination,
- genaue Strategie zur SMB-ACL-Abbildung,
- konkrete Dateisystem-Watcher versus periodische Scans,
- Index-Backupstrategie.

Das Lastenheft verlangt die Fähigkeiten; es schreibt nicht unnötig früh die konkrete Implementierung fest.

---

# 20. Risiken und fachliche Gegenmaßnahmen

## R-001 – Feature Creep

**Risiko:** Der Crawler entwickelt sich zum DMS.  
**Gegenmaßnahme:** Kapitel 14 als verbindliche Abgrenzung.

## R-002 – USB-Verlustlogik

**Risiko:** Offline-Medium wird als Löschung interpretiert.  
**Gegenmaßnahme:** MediaId, Availability und AC-006/007.

## R-003 – ACL-Informationsleck

**Risiko:** Titel/Snippet verrät gesperrte Dokumente.  
**Gegenmaßnahme:** Security Trimming vor Ergebnisbildung.

## R-004 – Parser-Angriffe

**Risiko:** manipulierte Dokumente greifen Parser an.  
**Gegenmaßnahme:** Isolation, Limits, Updates, Quarantäne.

## R-005 – Web Crawl Trap

**Risiko:** unendliche URL-Räume.  
**Gegenmaßnahme:** Domainbegrenzung, Depth, Rate Limit, Normalisierung, URL-Limits.

## R-006 – KI überdeckt Suchqualität

**Risiko:** frühe RAG-Funktionen kaschieren unzuverlässige klassische Suche.  
**Gegenmaßnahme:** KI erst ab 1.5/2.0.

## R-007 – Backend Lock-in

**Risiko:** Produkt wird untrennbar an eine Suchengine gekoppelt.  
**Gegenmaßnahme:** fachliche Trennung von Source, Extraction, Index und Search Service.

---

# 21. Definition „MVP“

Der MVP ist mit **Version 0.5.0** erreicht.

Er muss mindestens können:

1. lokale Verzeichnisse indexieren;
2. USB-Medien stabil identifizieren und offline weiter anzeigen;
3. SMB-/Netzquellen indexieren;
4. Websites begrenzt und respektvoll crawlen;
5. Word, Excel, PowerPoint, PDF, OpenDocument, HTML und Text verarbeiten;
6. ZIP-Inhalte sicher indexieren;
7. Scan-PDF per OCR durchsuchbar machen;
8. Änderungen und bestätigte Löschungen inkrementell verarbeiten;
9. Stichwort, Phrase und Boolesche Suche anbieten;
10. nach Quelle, Dateityp und Datum filtern;
11. Snippets und Highlighting anzeigen;
12. Originale öffnen beziehungsweise Offline-Medien benennen;
13. mit defekten Einzeldateien weiterarbeiten;
14. grundlegende Sicherheits- und Parserlimits einhalten.

Der MVP darf zunächst Single-User sein.

---

# 22. Definition „Version 1.0“

Version 1.0 ist **nicht lediglich ein umbenannter MVP**.

Zusätzlich zum MVP müssen bis 1.0 vorhanden und gehärtet sein:

- Mehrbenutzerbetrieb,
- Rollen und Gruppen,
- ACL/Security Trimming,
- Administration,
- Scheduler,
- Retry/Failure Queue,
- Search API,
- CLI,
- Health/Status,
- Audit administrativer Änderungen,
- Backup/Restore der Konfiguration,
- Upgrade/Migration,
- Recovery/Rebuild,
- reproduzierbare Performance- und Securitytests,
- Windows- und Linux-Betrieb,
- Accessibility-Baseline,
- belastbare Dokumentation.

---

# 23. Spätere Produktoptionen ohne feste Zusage

Diese Themen sollen architektonisch nicht verhindert werden, besitzen aber derzeit keine verbindliche Implementierungszusage:

- Audio-/Video-Transkription,
- Bilderkennung jenseits OCR,
- Cloud-Drive-Connectoren für einzelne SaaS-Anbieter,
- SharePoint-Connector,
- Confluence/Jira-Connector,
- Git-Repository-Inhaltsindex,
- Quellcode-spezifische Suche,
- WARC/WACZ-Archive,
- Browser-Erweiterung/Web Clipper,
- mobile Apps,
- macOS-native Integration,
- verteilte Cluster über mehrere Rechenzentren,
- mandantenfähiger SaaS-Betrieb.

Solche Funktionen werden nur aufgenommen, wenn ein konkreter Anwendungsfall den zusätzlichen Betriebs- und Sicherheitsaufwand rechtfertigt.

---

# 24. Entscheidungskriterien für Technologieauswahl

Ein möglicher Baustein muss gegen folgende Kriterien bewertet werden:

1. Funktionsabdeckung,
2. Extraktionsqualität,
3. Suchqualität,
4. Aktualität und Wartung,
5. Lizenz,
6. Sicherheit,
7. Plattformunterstützung,
8. Erweiterbarkeit,
9. API-Qualität,
10. Ressourcenverbrauch,
11. Performance,
12. Betriebskomplexität,
13. Upgradefähigkeit,
14. Lock-in-Risiko,
15. Testbarkeit.

Für Parser gilt zusätzlich der direkte Vergleich:

- Apache Tika,
- Toxy,
- gegebenenfalls weitere spezialisierte Parser.

Für Suchbackend:

- Lucene.NET,
- OpenSearch,
- Apache Solr,
- gegebenenfalls SQLite FTS5 für kleine Editionen.

---

# 25. Abschlussbewertung

Die Kernentscheidung dieses Lastenhefts lautet:

> **Der SASD-Crawler soll zuerst eine außergewöhnlich zuverlässige klassische Suchplattform werden.**

Der wertvollste Unterschied gegenüber einfachen Desktop-Suchprogrammen liegt nicht in KI, sondern in der Kombination aus:

- lokalen Quellen,
- Netzwerkquellen,
- Web,
- Office/PDF,
- OCR,
- stabilen Offline-Wechseldatenträgern,
- nachvollziehbarer Dokumentidentität,
- inkrementeller Aktualisierung,
- sicherem Mehrbenutzerbetrieb,
- schneller und verständlicher Suche.

Die aus den analysierten Vergleichsprodukten bekannten Komfort-, DMS-, Semantik- und KI-Funktionen werden nicht verworfen. Sie werden bewusst in spätere Releases eingeordnet, wenn die jeweils vorausgesetzte Kernfunktion bereits belastbar ist.

Dadurch bleibt das Produktziel beherrschbar und verhindert, dass die erste stabile Version gleichzeitig Suchmaschine, DMS, Collaboration-Plattform, Records-Management-System und KI-Assistent werden muss.

---

**Ende des Lastenhefts**
