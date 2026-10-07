# Milestone 0.0.4 – A4 Windows Media Identity Spike

**Datum:** 28. August 2026
**OS:** Windows 10.0.26200, win-x64
**Gate-Entscheidung:** **CONDITIONAL GO**

## Implementierung

Der Spike verwendet nicht privilegierte native Windows-APIs statt WMI:

- `GetVolumeNameForVolumeMountPointW` für die stabile Volume-GUID;
- `GetVolumeInformationW` für Seriennummer, Dateisystem und Label;
- `DriveInfo` für Mountpunkt, Laufwerkstyp, Bereitschaft und Kapazität;
- eine interne `MediaId` (`Guid`) getrennt von allen Windows-Signalen;
- normalisierte relative Medienpfade ohne Laufwerksbuchstaben;
- ein eigenes `NativeWindow` auf dediziertem STA-Thread für `WM_DEVICECHANGE`;
- eine fail-safe Matching-Policy.

Das Device-Change-Fenster gehört zur Infrastructure und benötigt weder `MainForm` noch
Business-Logik. Seine Events sind nur Hinweise; sie führen nie selbst zu Reconciliation
oder Löschung.

## Matching-Invariante

Nur genau ein Treffer auf die vollständige Volume-GUID darf automatisch an eine bekannte
`MediaId` binden. Schwächere Signale werden bewusst konservativ behandelt:

| Signale | Entscheidung |
|---|---|
| eindeutige exakte Volume-GUID | automatische exakte Bindung |
| doppelt registrierte exakte GUID | mehrdeutig, keine Bindung |
| eindeutige Serial + Filesystem + Capacity | Bestätigung erforderlich |
| mehrere sekundäre Fingerprints | mehrdeutig, keine Bindung |
| kein Kandidat | neue MediaId anlegen |

Damit kann ein geklonter oder ähnlich aussehender Stick nicht allein wegen Label, Größe
oder Seriennummer automatisch mit einem vorhandenen Dokumentbestand verschmolzen werden.

## Pfad-Invariante

Dokumente speichern `MediaId + RelativePath`. `E:\archive\report.pdf` wird als
`archive/report.pdf` abgelegt und kann bei erneutem Mount unter `F:\` aufgelöst werden.
Absolute Pfade und jedes `..`-Segment werden abgewiesen.

## Automatisierte Evidence

13 A4-Tests belegen exaktes, sekundäres, unbekanntes und mehrdeutiges Matching, keine
automatische Zusammenführung duplizierter Identitäten, laufwerksbuchstabenunabhängige
Pfade, Traversal-Abwehr, Device-Message-Klassifikation, Message-Window-Lifecycle und echte
native Identitätssignale des bereiten NTFS-Systemvolumes.

## Host-Probe

| Mount | Typ | Zustand | Dateisystem | native Signale |
|---|---|---|---|---|
| `C:\` | Fixed | bereit | NTFS | GUID, Serial, Capacity erfolgreich |
| `D:\` | CD-ROM | nicht bereit | – | – |
| `E:\` | Removable | nicht bereit | – | – |

Der Device-Monitor startete und stoppte erfolgreich. Da im Lauf kein Medium in `E:\`
eingelegt war, wären Angaben zu echtem Attach/Detach, Drive-Letter-Wechsel, exFAT/FAT32
oder zwei physisch ähnlichen Medien unbewiesen.

## Offene Evidence und Grenzen

- echte NTFS-/exFAT-/FAT32-Wechselmedien fehlen;
- Attach/Detach und `WM_DEVICECHANGE`-Eventempfang wurden nicht physisch ausgelöst;
- Wiedererkennung nach Laufwerksbuchstabenwechsel und App-Neustart ist nur auf Policy-/
  Pfadebene, nicht mit Hardware bewiesen;
- optionale Hardwareidentität (Geräteinstanz/USB-Serial) ist nicht erhoben;
- persistentes MediaRegistry/Offline-Dokumentmodell gehört erst in Milestone 0.2 nach G0;
- Volume-GUIDs können durch Formatieren/Klonen verändert oder dupliziert werden; deshalb
  bleibt der Bestätigungsfallback zwingend.

## Entscheidung

**CONDITIONAL GO.** Das Windows-API-, Daten- und Sicherheitsmodell ist technisch tragfähig,
und Ambiguität schlägt sicher fehl. A5 darf beginnen. Ein uneingeschränktes A4-GO und die
Annahme von ADR-0011 benötigen die dokumentierten physischen Wechselmedien-Smokes.
