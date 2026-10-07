# Milestone 0.0.5 – A5 Tika-vs-Toxy Parser Benchmark

**Datum:** 28. August 2026
**Apache Tika:** 3.3.2
**Toxy:** 2.6.0
**Entscheidung:** **Tika bleibt Default; kein Toxy-Fast-Path freigegeben**
**Gate-Status:** **CONDITIONAL – Korpus unvollständig**

## Reproduzierbarer Runner

Der A5-Runner erzeugt denselben synthetischen OOXML-/PDF-Korpus wie A3 und ergänzt HTML,
RTF, TXT, Legacy-XLS, eine 5-MiB-Textdatei sowie ein malformed PDF. Er führt beide Parser
im selben Lauf aus und erfasst:

- Parserfolg;
- Vorhandensein eines eindeutigen Vollständigkeitsmarkers;
- Textlänge;
- Metadatenfeldzahl beziehungsweise Metadatenfehler;
- verstrichene Zeit;
- den Working Set des jeweiligen Parserprozesses nach der Extraktion;
- Fehlerklasse und -text;
- Prozess-/Cancellation-Eigenschaften als Architekturmerkmal.

## Ergebnisse des finalen Laufs

| Format | Tika Text/Marker | Tika Metadaten | Tika ms | Toxy Text/Marker | Toxy Metadaten | Toxy ms |
|---|---|---:|---:|---|---:|---:|
| DOCX | ja/ja | 5 | 1.490 | ja/ja | 0 | 108 |
| XLSX | ja/ja | 6 | 78 | ja/ja | 0 | 53 |
| PPTX | ja/ja | 6 | 62 | ja/ja | 0 | 95 |
| PDF | ja/ja | 29 | 478 | ja/ja | nicht unterstützt | 242 |
| HTML | ja/ja | 9 | 134 | ja/ja | nicht unterstützt | 2 |
| RTF | ja/ja | 5 | 49 | ja/ja | nicht unterstützt | 16 |
| TXT | ja/ja | 8 | 33 | ja/ja | nicht unterstützt | < 1 |
| XLS | ja/ja | 5 | 185 | ja/ja | 0 | 47 |
| 5-MiB-TXT | ja/ja | 8 | 257 | ja/ja | nicht unterstützt | 22 |
| malformed PDF | kontrolliert abgewiesen | – | 16 | kontrolliert abgewiesen | – | 2 |

Die DOCX-Zeit enthält bei beiden Implementierungen Cold-Initialisierung und ist nicht als
Steady-State-Durchsatz zu interpretieren. Die Minimaldateien beweisen nur den eindeutigen
Marker, nicht Layout-, Tabellen-, Header-/Footer- oder Metadatenvollständigkeit realer Dateien.

Der Tika-Sidecar lag nach den erfolgreichen Extraktionen bei rund 108 MB Working Set. Der
Benchmark-Host mit dem eingebetteten Toxy stieg nach der 5-MiB-Datei auf rund 170 MB. Das sind
Momentaufnahmen nach dem jeweiligen Lauf, keine Peak- oder Delta-Messungen. Die Werte sind zudem
nicht direkt gleichzusetzen: Der Toxy-Wert enthält Runner und Fixture-Verwaltung, während der
Tika-Wert ausschließlich den isolierten Java-Sidecar beschreibt.

## Architektur- und Robustheitsvergleich

| Kriterium | Tika | Toxy |
|---|---|---|
| Isolation | eigener Java-Prozess | im .NET-Hauptprozess |
| Cancellation | Timeout beendet Prozessbaum | synchrones `Parse()`, kein `CancellationToken` |
| Ressourcenlimit | konfiguriertes `-Xmx512m` | nur Gesamtprozessgrenzen |
| malformed PDF | HTTP 422, Host bleibt stabil | Exception im Hostprozess |
| Dateihandles | Sidecar-Prozessgrenze | erster Fehlerlauf hielt PDF bis zur Finalisierung offen |
| Metadaten im Korpus | 5–29 Felder | 0 oder Factory nicht unterstützt |
| Paketform | 73,0-MiB Server-JAR plus JRE | framework-dependent Runner 36,8 MiB/72 Dateien |
| Lizenz | Apache-2.0 plus NOTICE/Drittlizenzen | Apache-2.0 plus zahlreiche Drittabhängigkeiten |

Der Toxy-Publish-Footprint umfasst den Benchmark, dessen .NET-Abhängigkeiten und 72 Dateien;
er ist deshalb kein reiner `Toxy.dll`-Wert. `Toxy.dll` selbst ist rund 86 KiB, zieht aber
unter anderem NPOI, OpenXML, PdfPig, ImageSharp, BouncyCastle und weitere Pakete transitiv ein.

## Bewertungsmatrix

Die im PoC-Plan gewichteten Kriterien können mit dem kleinen Korpus nicht seriös vollständig
numerisch bewertet werden. Fest steht jedoch: Der mögliche Geschwindigkeitsvorteil bei HTML,
RTF und TXT kompensiert aktuell weder die fehlende Metadatenparität noch die fehlende harte
Parserisolation. Für Office/PDF ist ebenfalls keine durchgehend bessere Leistung nachgewiesen.

Nach der vorgegebenen Entscheidungsregel qualifiziert sich deshalb **kein MIME-Typ** für einen
Toxy-Primär- oder Fast-Path. Die Produktarchitektur bleibt einfacher, wenn Tika alleiniger
Referenzparser bleibt und TXT/HTML im 0.1-Slice durch kleine, sichere native Extraktoren statt
durch das gesamte Toxy-Abhängigkeitsset verarbeitet werden.

## Offene Evidence

- Legacy DOC/PPT fehlen; ein synthetisches Legacy-XLS ist abgedeckt;
- komplexe reale DOCX/XLSX/PPTX/PDF-Dateien fehlen;
- verschlüsselte und gezielt bösartige Dateien fehlen; die Großdatei-Evidence beschränkt sich
  auf eine synthetische 5-MiB-Textdatei;
- Tabellen-/Blatt-/Folienvollständigkeit und Metadatenqualität sind nicht fachlich bewertet;
- wiederholte Warm-Läufe, Peak-RAM und Parallelitäts-/Crash-Korpus fehlen;
- vollständige Lizenz-/CVE-Auswertung aller Toxy-Transitivabhängigkeiten fehlt.

## Entscheidung

**Tika bleibt Default; Toxy wird nicht in Produktcode übernommen.** Diese Entscheidung ist
bereits sicher, weil die Beweislast laut ADR-0007 bei Toxy liegt und kein Format ohne relevanten
Trade-off überzeugt. A5 bleibt dennoch `READY FOR REVIEW / CONDITIONAL`, weil die vollständige
Roadmap-Korpus-Evidence nicht vorliegt. Ein späterer Re-Benchmark kann die Entscheidung ändern,
ohne die `IContentExtractor`-Grenze zu verändern.

Offizielle Quellen:

- https://www.nuget.org/packages/Toxy/2.6.0
- https://github.com/nissl-lab/toxy
