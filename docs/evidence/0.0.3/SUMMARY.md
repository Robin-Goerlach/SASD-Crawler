# Milestone 0.0.3 – A3 Tika Sidecar/Packaging Spike

**Datum:** 28. August 2026
**OS:** Windows 10.0.26200, win-x64
**Runtime:** .NET 8.0.30
**Java:** Amazon Corretto OpenJDK 21.0.11 LTS
**Apache Tika:** 3.3.2
**Gate-Entscheidung:** **CONDITIONAL GO**

## Lieferkette und Laufzeit

Für die Ausführung wurde `tika-server-standard-3.3.2.jar` direkt vom offiziellen
Apache-Download bezogen und nur unter `.tmp/` abgelegt. Es ist nicht Teil des Commits.

| Artefakt | Wert |
|---|---:|
| Server-JAR | 76.497.574 Byte (73,0 MiB) |
| SHA-512 | `fb1f2fe57ac458b09d44d41d816f582e1d2fc93488acff6275caf414d8d5ef94e42166edc0b488dc2fb6ef3aa21fab62b107c43b9060385ff6d675e393c2c9e9` |
| Installiertes vollständiges Corretto-JDK | 343.740.447 Byte (327,8 MiB) |
| Nachgewiesene reduzierte Laufzeit | 65.018.144 Byte (62,0 MiB), 214 Dateien, 26 Module |
| JAR + reduzierte Laufzeit | 141.515.718 Byte (135,0 MiB) vor Installer-Kompression |

Der lokale SHA-512-Wert stimmt exakt mit der Apache-Prüfsummendatei überein. Tika 3.3.2
und seine Distribution stehen unter Apache License 2.0; NOTICE-/Drittlizenztexte müssen
bei einer späteren Bündelung mitgeliefert werden. Für Corretto/JRE ist vor Packaging eine
separate Lizenz- und Komponentenliste erforderlich. `jdeps` konnte die Modulliste des
Fat-JARs wegen nicht auflösbarer Modulmetadaten (`com.fasterxml.jackson.annotation`) nicht
direkt ableiten. Deshalb wurde eine konservative 26-Modul-Liste erstellt und praktisch mit
dem vollständigen A3-Probe validiert. Der `--build-runtime`-Modus des A3-Probes reproduziert
diesen Aufbau ohne Änderung der lokalen PowerShell-Ausführungsrichtlinie.

Offizielle Quellen:

- https://tika.apache.org/download.html
- https://downloads.apache.org/tika/3.3.2/

## Implementierte Isolation

`TikaSidecar`:

- startet Java ohne Shell und ohne sichtbares Fenster;
- übergibt Argumente ohne String-Shell-Interpolation;
- bindet explizit an `127.0.0.1` auf einem dynamischen Port;
- verwendet ausschließlich feste `/version`, `/tika` und `/meta`-Endpunkte;
- sendet lokale Streams, niemals Fetch-URLs;
- setzt `-Xmx512m` und ein pro Prozess erzeugtes Temp-Verzeichnis;
- begrenzt Eingabedateien vor Prozessstart;
- begrenzt Start und vollständige Extraction durch Cancellation/Timeout;
- beendet bei Timeout/Transportfehler den gesamten Sidecar-Prozessbaum;
- startet nach Fehler oder auf Anforderung mit neuer PID;
- erfasst maximal 200 Logzeilen im Speicher;
- löscht ausschließlich das selbst erzeugte Temp-Unterverzeichnis nach Stop;
- beendet den Sidecar beim asynchronen Dispose.

## Echter Format- und Lifecycle-Probe

Der Probe-Runner erzeugte kleine synthetische Dateien mit einem eindeutigen Marker. Alle
Extraktionen enthielten den Marker und Metadaten:

| Format | Extraction | Metadatenfelder |
|---|---:|---:|
| DOCX | 2.273 ms | 5 |
| XLSX | 107 ms | 6 |
| PPTX | 88 ms | 6 |
| PDF | 607 ms | 29 |

Weitere Ergebnisse:

- Startzeit: 4.600 ms;
- Listener ausschließlich auf `127.0.0.1` (aktive TCP-Listener geprüft);
- expliziter Restart erzeugte eine neue PID und Extraction blieb funktionsfähig;
- malformed PDF ergab einen kontrollierten Parserfehler, Hauptprozess blieb aktiv;
- 1-Byte-Limit wies die Eingabe ab, ohne Java zu starten;
- praktisch sofortiger Timeout stoppte den Java-Prozess;
- stdout/stderr-Logcapture war aktiv;
- Dispose beendete den Java-Prozess und Temp-Cleanup gelang.

Der identische Probe bestand zusätzlich mit der reduzierten 62,0-MiB-Runtime: alle vier
Formate, Metadaten, Loopback, Restart, malformed PDF, Größenlimit, Timeout und Cleanup waren
grün. Damit ist eine gebündelte Runtime technisch nachgewiesen, aber noch kein Installer.

Die Werte sind Cold-/PoC-Werte auf synthetischen Minimaldateien und keine Produkt-SLOs.

## Offene Evidence und Grenzen

- SmartScreen-/Antivirus-Verhalten kann ohne signiertes Installationspaket nicht sinnvoll
  bewertet werden.
- Produktentscheidung „gebündelte Runtime versus Voraussetzung“ ist noch formal offen;
  eine funktionierende reduzierte Runtime ist technisch nachgewiesen.
- SBOM, NOTICE-Sammlung, CVE-Scan, Signierung und komprimierte Installergröße fehlen.
- OS-Sandbox/Job-Object, Low-Integrity-Token und Netzwerk-Firewall-Isolation sind noch
  nicht implementiert; Loopback-Bindung allein verhindert ausgehende Parserverbindungen
  nicht auf Betriebssystemebene.
- Echte große, verschlüsselte, komplexe und bösartig präparierte Dokumentkorpora gehören
  in A5/G4 und sind mit den synthetischen Dateien nicht bewiesen.
- Die dynamische Portreservierung besitzt zwischen Freigabe und Java-Bindung ein kleines
  lokales Race; Startup schlägt dabei sicher fehl, benötigt aber einen späteren Retry.

## Entscheidung

**CONDITIONAL GO.** Tika 3.3.2 kann unter Aufsicht aus einem .NET-8-Windows-Prozess lokal
gestartet, begrenzt, neu gestartet und beendet werden. Parserfehler und Timeout reißen den
Hauptprozess nicht mit. A4 darf beginnen. Ein uneingeschränktes A3-GO und die Annahme von
ADR-0006 bleiben bis zum Packaging-/SBOM-/AV-Prototyp offen.
