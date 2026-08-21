
# SASD-Crawler – Softwarearchitektur

**Dokumentstatus:** Architekturentwurf / Diskussions- und Entscheidungsbaseline  
**Stand:** 20. August 2026  
**Dokumentversion:** 0.1  
**Produkt:** SASD-Crawler  
**Zielplattform:** Windows Desktop  
**UI-Technologie:** Windows Forms  
**Zielframework:** .NET 8 (`net8.0-windows`)  
**Architekturstil:** Modularer Desktop-Monolith mit Clean-Architecture-Grenzen, MVP-Präsentationsmuster und isolierten Worker-/Sidecar-Prozessen  
**Bezug:** Lastenheft 0.1 und Pflichtenheft 0.1  
**Wichtiger Statushinweis:** Dieses Architekturdokument ersetzt die bisherigen technischen Annahmen „.NET 10 + Blazor/Linux-first“ des Pflichtenhefts dort, wo sie der neuen expliziten Vorgabe „Windows Forms auf .NET 8“ widersprechen.

---

# 1. Zweck dieses Architekturdokuments

Dieses Dokument überführt das bestehende Lasten- und Pflichtenheft in eine konkrete, wartbare Architektur für eine **native Windows-Forms-Anwendung auf .NET 8**.

Die Architektur soll nicht nur zeigen, welche Klassen oder Projekte benötigt werden. Sie soll die grundlegenden Entscheidungen klären:

- Was läuft im WinForms-Prozess?
- Was läuft bewusst außerhalb des UI-Prozesses?
- Welche Daten sind führend, welche nur rekonstruierbarer Index?
- Wie bleiben Crawling und OCR reaktionsfähig, ohne die UI zu blockieren?
- Wie werden lokale Festplatten, USB, SMB und Webseiten unter einer gemeinsamen Quellenabstraktion vereinheitlicht?
- Wie wird verhindert, dass ein kurzzeitig verschwundenes Laufwerk hunderttausende Dokumente aus dem Index löscht?
- Wie wird ein Word-, Excel-, PDF- oder Scan-PDF sicher verarbeitet?
- Wie wird Lucene.NET so gekapselt, dass später ein anderes Search Backend möglich bleibt?
- Wie kann die Anwendung zunächst als einfacher Desktop-Crawler funktionieren und trotzdem später einen gemeinsamen Dienst oder Servermodus erhalten?
- Wie bleiben Tests, Migrationen und Releases beherrschbar?

Das Dokument betrachtet die Architektur als **Produktarchitektur**, nicht als einmalige Implementierungsskizze.

---

# 2. Neue Architekturvorgabe und notwendige Korrektur des Pflichtenhefts

Das bisherige Pflichtenheft legte als technische Baseline .NET 10, ASP.NET Core/Blazor, Windows und Linux sowie einen stärker serverorientierten Betrieb fest.

Die neue Vorgabe lautet ausdrücklich:

> **Der SASD-Crawler soll als Windows-Forms-Anwendung für .NET 8 entwickelt werden.**

Diese Vorgabe ist technisch umsetzbar, verändert aber einige frühere Annahmen.

## 2.1 Was unverändert bleibt

Unverändert bleiben insbesondere:

- Index-in-place;
- lokale Dateisysteme;
- USB-/Offline-Medien;
- SMB-/UNC-Freigaben;
- Web-Crawling;
- Word/Excel/PowerPoint/PDF/OpenDocument;
- Archive;
- OCR;
- Dokumentidentität;
- Reconciliation;
- persistente Warteschlange;
- Volltextindex;
- klassische Suchsyntax;
- Filter, Facetten, Snippets und Highlighting;
- sichere Vorschau;
- Parserisolation;
- Backup/Recovery;
- spätere semantische/AI-Erweiterbarkeit.

## 2.2 Was geändert wird

### CR-ARCH-001 – .NET 8 statt .NET 10

Das Produkt zielt auf:

```xml
<TargetFramework>net8.0-windows</TargetFramework>
```

Die Architektur vermeidet unnötige Abhängigkeiten von später eingeführten .NET-10-spezifischen APIs.

### CR-ARCH-002 – WinForms statt Blazor als Primär-UI

Die Primäranwendung ist:

```text
Sasd.Crawler.WinForms.exe
```

Keine Weboberfläche ist Voraussetzung für Version 1.0.

### CR-ARCH-003 – Windows-first statt Windows/Linux UI

Der Produktkern bleibt soweit sinnvoll in plattformneutralen .NET-Bibliotheken, die **Version-1.0-Anwendung selbst ist jedoch Windows-only**.

Das Lastenheftziel „Linux als produktive Baseline“ muss deshalb formal verschoben oder gestrichen werden.

### CR-ARCH-004 – Desktop-first Security

Die Standardinstallation wird pro Windows-Benutzer betrieben.

Der Crawler läuft mit den Rechten dieses Benutzers.

Damit gilt im Standardmodus:

```text
Rechte des Crawlers = Rechte des Suchbenutzers
```

Das reduziert das Risiko, dass ein zentraler Index Inhalte sichtbar macht, die der Benutzer selbst nicht lesen dürfte.

### CR-ARCH-005 – Shared-Server-Modus wird Erweiterung

Mehrere Benutzer an einem zentralen Index benötigen später einen getrennten Worker-/Service-Host.

Die Architektur wird darauf vorbereitet, aber die WinForms-Desktopanwendung bleibt das Produktzentrum.

### CR-ARCH-006 – Desktop-Accessibility statt WCAG als alleinige Norm

Für WinForms sind insbesondere relevant:

- Windows UI Automation,
- Tastaturbedienung,
- Access Keys,
- Screenreader-Kompatibilität,
- High DPI,
- Systemschriftgrößen,
- High Contrast.

Die Web-spezifische WCAG-Anforderung wird auf eine passende Desktop-Accessibility-Baseline übertragen.

---

# 3. .NET-8-Lifecycle als bewusstes Architektur-Risiko

.NET 8 ist LTS, befindet sich im August 2026 jedoch bereits in der Maintenance-Phase.

Microsoft nennt als Supportende:

> **10. November 2026**

Das bedeutet:

- die gewünschte technische Zielversion wird respektiert;
- der Crawler wird als `net8.0-windows` geplant;
- sämtliche .NET-8-Patches müssen bis zum Supportende eingespielt werden;
- die Architektur muss einen späteren Frameworkwechsel möglichst trivial machen.

## 3.1 Konsequenz

Das Target Framework wird zentral in `Directory.Build.props` verwaltet.

Beispiel:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

Damit soll ein späteres Upgrade beispielsweise auf .NET 10 **keine Architekturänderung**, sondern eine kontrollierte Plattformmigration sein.

## 3.2 Release-Regel

Falls SASD-Crawler 1.0 nach dem offiziellen .NET-8-Supportende veröffentlicht wird, muss vor Produktionsfreigabe entschieden werden:

1. Framework auf eine unterstützte LTS-Version anheben; oder
2. das Ende des Herstellersupports ausdrücklich als akzeptiertes Produktrisiko dokumentieren.

Architektonisch wird **Option 1 vorbereitet und empfohlen**.

---

# 4. Architekturvision

Der SASD-Crawler wird als **Desktop-first modularer Monolith** entwickelt.

„Monolith“ bedeutet hier nicht:

- alles in einer Form;
- globale Singleton-Klassen;
- UI-Code greift direkt auf SQLite/Lucene zu;
- ein untestbarer `MainForm.cs` mit zehntausenden Zeilen.

Gemeint ist:

> Ein Produkt wird als ein installierbares Windows-Desktopprogramm ausgeliefert, intern aber in klar getrennte Module mit stabilen Schnittstellen zerlegt.

Die Architektur sieht fachlich so aus:

```text
┌───────────────────────────────────────────────────────────────┐
│                    Sasd.Crawler.WinForms                     │
│                                                               │
│ Search │ Sources │ Media │ Jobs │ Errors │ Settings │ Help   │
└──────────────────────────────┬────────────────────────────────┘
                               │
                               ▼
┌───────────────────────────────────────────────────────────────┐
│                      Application Layer                        │
│                                                               │
│ SearchUseCases │ CrawlUseCases │ MediaUseCases │ Admin       │
└──────────────────────────────┬────────────────────────────────┘
                               │
                               ▼
┌───────────────────────────────────────────────────────────────┐
│                         Domain/Core                           │
│                                                               │
│ Source │ Media │ Document │ Job │ State Machines │ Policies  │
└───────────────┬───────────────────────────┬───────────────────┘
                │                           │
                ▼                           ▼
┌──────────────────────────┐   ┌───────────────────────────────┐
│ Infrastructure           │   │ Background Processing         │
│ SQLite │ Lucene │ Win32  │   │ Discovery │ Queue │ Extract  │
│ HTTP │ SMB/UNC           │   │ OCR │ Reconcile │ Index      │
└───────────────┬──────────┘   └──────────────┬────────────────┘
                │                             │
                └──────────────┬──────────────┘
                               ▼
                ┌──────────────────────────────┐
                │ External Worker Processes    │
                │                              │
                │ Apache Tika │ Tesseract      │
                └──────────────────────────────┘
```

---

# 5. Architekturprinzipien

## 5.1 UI ist niemals Geschäftslogik

Eine Form darf:

- Eingaben erfassen;
- einen Presenter aufrufen;
- Ergebnisse darstellen;
- UI-Zustand verwalten.

Eine Form darf nicht:

- direkt SQL ausführen;
- direkt Lucene öffnen;
- Dateien rekursiv crawlen;
- Tika starten;
- ACLs berechnen;
- Löschlogik entscheiden.

## 5.2 Domain kennt WinForms nicht

`Sasd.Crawler.Domain` referenziert weder:

- `System.Windows.Forms`,
- SQLite,
- Lucene,
- Tika,
- Tesseract,
- ASP.NET.

## 5.3 Infrastructure wird injiziert

Anwendungsfälle greifen auf Interfaces zu:

```text
IDocumentRepository
ISourceRepository
ISearchIndex
IContentExtractor
IOcrEngine
ISourceConnector
IMediaRegistry
IClock
ISecretStore
```

## 5.4 Index ist abgeleitet

SQLite/Control Store enthält den führenden Zustand.

Lucene enthält einen performant durchsuchbaren, rekonstruierbaren Index.

## 5.5 Hintergrundarbeit ist persistent

Lange Crawls existieren nicht nur als `Task` im Speicher.

Der Job- und Work-Status liegt persistent in SQLite.

## 5.6 Windows-spezifisch nur am Rand

Windows-spezifische Funktionen liegen in Infrastructure-Modulen.

Beispiele:

- Volume GUID;
- WM_DEVICECHANGE;
- Windows File ID;
- ACL;
- Shell Open;
- DPAPI.

Der fachliche Kern bleibt davon unabhängig.

---

# 6. Architekturstil: Clean Architecture + MVP

Für WinForms wird **Model-View-Presenter (MVP)** bevorzugt.

## 6.1 Warum nicht klassisches „alles in Form1“

Dieses Muster führt schnell zu:

```text
MainForm
  ├── SQL
  ├── Dateiscanner
  ├── Search
  ├── HTTP
  ├── OCR
  ├── Settings
  └── UI Events
```

Das wäre für einen Crawler dieses Umfangs unwartbar.

## 6.2 Warum MVP

WinForms ist event- und control-orientiert. MVP passt dazu natürlicher als ein erzwungenes WPF-artiges MVVM.

Beispiel:

```text
SearchForm
    │ implements
    ▼
ISearchView
    ▲
    │ controls
SearchPresenter
    │
    ▼
ISearchApplicationService
```

Die View kennt nur Darstellungsmodelle.

## 6.3 WinForms-.NET-8-Databinding

.NET 8 besitzt verbesserte WinForms-Datenbindung und Command-Unterstützung.

Diese Möglichkeiten dürfen genutzt werden, ändern aber die Grundentscheidung nicht:

> Presentation Logic liegt in Presenter/Presentation Model und nicht in SQL-/Crawler-Code hinter Controls.

---

# 7. Prozessmodell

## 7.1 Hauptprozess

```text
Sasd.Crawler.WinForms.exe
```

enthält:

- WinForms UI;
- .NET Generic Host;
- DI Container;
- Application Services;
- SQLite;
- Lucene.NET;
- Scheduler;
- Queue Dispatcher;
- Crawler Worker.

## 7.2 Sidecar-Prozesse

Bewusst getrennt:

```text
java.exe / Tika Server
tesseract.exe
```

Warum?

- fremde Parser verarbeiten potenziell gefährliche Dateien;
- Java-Parserfehler sollen die WinForms-App nicht reißen;
- OCR kann CPU-intensiv sein;
- Timeouts und Neustarts sind leichter durchsetzbar.

## 7.3 Kein Windows Service in der ersten Desktop-Baseline

Version 1.0 benötigt nicht zwingend einen Windows Service.

Stattdessen:

- Anwendung kann im Tray weiterlaufen;
- optional Autostart beim Login;
- optional `--background`;
- Scheduling läuft, solange die Benutzerinstanz aktiv ist.

Ein späterer `Sasd.Crawler.Service.exe` soll dieselben Application-/Infrastructure-Module hosten können.

---

# 8. Startup und Generic Host

Die WinForms-Anwendung verwendet den .NET Generic Host für:

- DI;
- Logging;
- Configuration;
- BackgroundServices;
- kontrolliertes Shutdown.

Konzeptionell:

```csharp
[STAThread]
static void Main()
{
    ApplicationConfiguration.Initialize();

    using IHost host = CreateHostBuilder().Build();

    host.Start();

    var mainForm = host.Services.GetRequiredService<MainForm>();
    Application.Run(mainForm);

    host.StopAsync().GetAwaiter().GetResult();
}
```

Produktiver Code muss Shutdown/Exceptions sauberer behandeln; die Struktur ist verbindlich.

---

# 9. Projekt- und Solution-Struktur

```text
Sasd.Crawler.sln

src/
  Sasd.Crawler.Domain/
  Sasd.Crawler.Application/
  Sasd.Crawler.Contracts/

  Sasd.Crawler.Persistence.Abstractions/
  Sasd.Crawler.Persistence.Sqlite/

  Sasd.Crawler.Search.Abstractions/
  Sasd.Crawler.Search.Lucene/

  Sasd.Crawler.Extraction.Abstractions/
  Sasd.Crawler.Extraction.Tika/
  Sasd.Crawler.Extraction.Toxy/

  Sasd.Crawler.Ocr.Abstractions/
  Sasd.Crawler.Ocr.Tesseract/

  Sasd.Crawler.Connectors.Abstractions/
  Sasd.Crawler.Connectors.FileSystem/
  Sasd.Crawler.Connectors.RemovableMedia/
  Sasd.Crawler.Connectors.Web/

  Sasd.Crawler.Windows/
  Sasd.Crawler.Security/
  Sasd.Crawler.Observability/

  Sasd.Crawler.WinForms/
  Sasd.Crawler.Cli/                  # später/optional
  Sasd.Crawler.ApiHost/              # später/optional

tests/
  Sasd.Crawler.Domain.Tests/
  Sasd.Crawler.Application.Tests/
  Sasd.Crawler.Persistence.Tests/
  Sasd.Crawler.Search.Tests/
  Sasd.Crawler.Extraction.Tests/
  Sasd.Crawler.Connectors.Tests/
  Sasd.Crawler.Windows.Tests/
  Sasd.Crawler.Security.Tests/
  Sasd.Crawler.WinForms.Tests/
  Sasd.Crawler.E2E.Tests/
  Sasd.Crawler.Performance.Tests/
  Sasd.Crawler.Recovery.Tests/

testdata/
  documents/
  ocr/
  archives/
  web/
  malformed/
  usb/
  smb/
```

---

# 10. Abhängigkeitsregeln

## 10.1 Erlaubt

```text
WinForms
   ↓
Application
   ↓
Domain

Infrastructure
   ↓
Application Contracts / Domain
```

## 10.2 Verboten

```text
Domain → WinForms
Domain → SQLite
Domain → Lucene
Application → MainForm
Search.Lucene → WinForms Controls
FileSystemConnector → DataGridView
```

## 10.3 Composition Root

Nur das WinForms-Startup-Projekt kennt alle konkreten Implementierungen.

Dort wird registriert:

```text
IDocumentRepository → SqliteDocumentRepository
ISearchIndex → LuceneSearchIndex
IContentExtractor → TikaContentExtractor
IOcrEngine → TesseractOcrEngine
ISourceConnector<File> → WindowsFileSystemConnector
...
```

---

# 11. UI-Informationsarchitektur

Die Anwendung soll wie eine professionelle Windows-Anwendung und nicht wie ein technisches Adminfrontend wirken.

## 11.1 Hauptfenster

Vorgeschlagener Aufbau:

```text
┌──────────────────────────────────────────────────────────────┐
│ SASD Crawler                              [Status] [⚙] [?]  │
├──────────────────────────────────────────────────────────────┤
│ [ 🔎 Suchbegriff................................ ] [Suchen] │
├──────────────┬─────────────────────────────┬─────────────────┤
│ Navigation   │ Treffer                     │ Vorschau        │
│              │                             │                 │
│ Suche        │ Titel                       │ Metadaten       │
│ Favoriten    │ Snippet                     │ Textpreview     │
│ Quellen      │ Ort                         │                 │
│ Medien       │ Datum / Typ                 │                 │
│ Jobs         │                             │                 │
│ Fehler       │                             │                 │
├──────────────┴─────────────────────────────┴─────────────────┤
│ 123.456 Dokumente │ Index aktuell │ 1 Medium offline        │
└──────────────────────────────────────────────────────────────┘
```

## 11.2 Kein Ribbon in 1.0

Ein Ribbon wäre funktional möglich, würde bei einer Suchanwendung aber unnötige vertikale Fläche beanspruchen.

Bevorzugt:

- schlanke Command Bar;
- Navigation links;
- kontextbezogene Buttons;
- Suchfeld als dominantes UI-Element.

Ribbon kann später neu bewertet werden, wenn sehr viele Verwaltungsfunktionen entstehen.

## 11.3 Navigation

Primärbereiche:

1. **Suche**
2. **Quellen**
3. **Medien**
4. **Indexierung**
5. **Fehler**
6. **Einstellungen**
7. **Diagnose**
8. **Hilfe**

---

# 12. MainForm und Workspace-Konzept

Das Hauptfenster bleibt stabil.

Fachliche Bereiche werden als UserControls beziehungsweise Views geladen:

```text
MainForm
  ├── SearchWorkspace
  ├── SourcesWorkspace
  ├── MediaWorkspace
  ├── JobsWorkspace
  ├── FailuresWorkspace
  ├── SettingsWorkspace
  └── DiagnosticsWorkspace
```

Jeder Workspace besitzt:

- View Interface;
- Presenter;
- Presentation Model;
- Application Service.

Forms werden nicht miteinander durch direkte Referenzen verkettet.

---

# 13. UI-Threading

WinForms besitzt genau einen UI-Thread.

Crawler, Parser, OCR und Indexaufbau dürfen niemals auf diesem Thread laufen.

## 13.1 Regel

Event Handler:

```csharp
private async void SearchButton_Click(object sender, EventArgs e)
{
    await presenter.SearchAsync();
}
```

Der Presenter ruft Application Services asynchron auf.

## 13.2 Cross-Thread Updates

Hintergrunddienste ändern keine Controls direkt.

Updates laufen über:

- Event Aggregator / Notification Service;
- SynchronizationContext;
- `Control.BeginInvoke`.

## 13.3 Progress

Jobs liefern fachliche Progress Events:

```text
JobStarted
DocumentDiscovered
DocumentProcessed
JobProgressChanged
JobCompleted
JobFailed
MediaStateChanged
```

Die UI entscheidet, wie sie diese Ereignisse darstellt.

---

# 14. Interner Event Bus

Ein einfacher in-process Event Bus entkoppelt UI und Hintergrundarbeit.

Beispiele:

```text
MediaAttachedEvent
MediaDetachedEvent
SourceStatusChangedEvent
JobStatusChangedEvent
IndexChangedEvent
SearchIndexUnavailableEvent
```

Der Bus ist **kein Ersatz für die persistente Queue**.

Events sind flüchtige UI-/Integrationssignale.

WorkItems sind dauerhaft.

---

# 15. SQLite als Control Store

SQLite speichert den führenden Zustand.

## 15.1 Datenbankort

Standard pro Windows-Benutzer:

```text
%LOCALAPPDATA%\SASD\Crawler\data\crawler.db
```

## 15.2 Warum pro User

Vorteile:

- keine Administratorrechte;
- natürliche Datenschutzgrenze;
- keine Daten anderer Windows-Benutzer;
- Index entspricht effektiven Leserechten des Benutzers;
- einfache Deinstallation/Backup.

## 15.3 WAL

SQLite wird im WAL-Modus betrieben.

## 15.4 Migrationen

Schemaänderungen besitzen:

```text
DbSchemaVersion
```

Migrationen sind:

- versioniert;
- getestet;
- vorwärtsgerichtet;
- vor Ausführung gesichert.

---

# 16. Dateisystemlayout

```text
%LOCALAPPDATA%\SASD\Crawler\
  data\
    crawler.db

  index\
    active\
    build-<schema-version>\

  cache\
    extracted\
    preview\
    temp\

  logs\

  runtime\
    tika\
    ocr\

  backups\
```

## 16.1 Kein Dokumentrepository

Originaldateien werden hier nicht kopiert, außer kurzzeitig in einem kontrollierten Temp-Verzeichnis, wenn ein Parser dies zwingend benötigt.

---

# 17. Source-Domänenmodell

```text
Source
  SourceId
  Name
  Type
  Locator
  Enabled
  ProcessingProfileId
  Schedule
  IncludeRules
  ExcludeRules
  LastProbe
  LastSuccessfulScan
  Health
```

Source Types in 1.0:

```text
LocalDirectory
RemovableMedia
NetworkDirectory
WebSite
```

---

# 18. Connector-Abstraktion

Verbindlicher Vertrag:

```csharp
public interface ISourceConnector
{
    SourceKind Kind { get; }

    Task<SourceProbeResult> ProbeAsync(
        SourceDefinition source,
        CancellationToken cancellationToken);

    IAsyncEnumerable<DiscoveredItem> DiscoverAsync(
        SourceDefinition source,
        ScanContext context,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        SourceDefinition source,
        DiscoveredItem item,
        CancellationToken cancellationToken);
}
```

ACL- und Zusatzmetadaten dürfen über zusätzliche Interfaces kommen.

Connectoren schreiben niemals selbst in Lucene.

---

# 19. LocalFileSystemConnector

## 19.1 Traversierung

Streaming mit `Directory.Enumerate*`.

Nicht:

```text
GetFiles(root, "*", AllDirectories)
```

mit einer riesigen Materialisierung.

## 19.2 Pfadnormalisierung

Windows-Pfade werden intern kanonisiert.

Zu beachten:

- Laufwerksbuchstaben;
- UNC;
- `.`/`..`;
- Case Insensitivity;
- lange Pfade;
- Reparse Points.

## 19.3 Reparse Points

Default:

```text
FollowReparsePoints = false
```

Damit werden Junction-Schleifen verhindert.

## 19.4 File Watcher

`FileSystemWatcher` dient als **Beschleuniger**.

Er kann:

- Create;
- Change;
- Delete;
- Rename

melden.

Er bleibt unzuverlässig bei:

- großen Burst-Mengen;
- Netzfreigaben;
- Overflow.

Deshalb ersetzt er niemals Full Reconciliation.

---

# 20. RemovableMediaMonitor

## 20.1 Architektur

Media Detection ist Windows-spezifische Infrastructure.

Bevorzugt:

- Win32 Volume APIs;
- Gerätebenachrichtigungen via `WM_DEVICECHANGE`;
- eigene `NativeWindow`-Komponente statt Logik direkt in `MainForm.WndProc`.

## 20.2 Warum nicht MainForm

Wenn Device Detection in MainForm steckt:

- kann sie nicht getestet werden;
- funktioniert sie nur bei sichtbarer Form;
- mischt UI und OS-Infrastruktur.

Stattdessen:

```text
WindowsVolumeMonitor
    ↓
MediaRegistry
    ↓
MediaAttachedEvent
    ↓
Application Layer / UI
```

---

# 21. Media Identity

Ein Medium erhält eine interne UUID.

Zusätzliche Signale:

```text
Volume GUID
Volume Serial Number
Filesystem
Capacity
Label
Drive Type
optional Hardware Identity
```

## 21.1 Matching

```text
Exact stable volume identity
    ↓
known volume serial + filesystem
    ↓
fingerprint candidates
    ↓
ambiguous?
   /   \
 yes   no
  │     │
ask    attach
```

## 21.2 Keine falsche Zusammenführung

Bei zwei gleich aussehenden Sticks ist eine temporäre zweite MediaId besser als das Verschmelzen falscher Dokumentbestände.

---

# 22. Offline-Medien

Dokumente auf USB speichern:

```text
MediaId
RelativePath
```

nicht den Laufwerksbuchstaben als führende Identität.

Treffer offline:

```text
Titel: Vertrag 2018
Medium: Archiv USB 2018
Status: Offline
Pfad auf Medium: \Verträge\Vertrag.pdf
Zuletzt gesehen: ...
```

Das ist eine Kernfunktion und zugleich eine klare Differenzierung zu vielen bestehenden Desktop-Suchen.

---

# 23. NetworkDirectory / SMB

Da die Version 1.0 Windows-only ist, wird SMB sehr viel einfacher als im bisherigen Pflichtenheft.

## 23.1 UNC direkt

```text
\\nas\archive
\\server\department
```

werden über normale Windows-Dateisystem-APIs verarbeitet.

## 23.2 Gemappte Laufwerke

Auch:

```text
Z:\
```

sind möglich.

Intern sollte nach Möglichkeit die UNC-/stabile Quellkonfiguration erhalten bleiben, statt ausschließlich einen nutzerabhängigen Laufwerksbuchstaben zu speichern.

## 23.3 Credentials

Default:

> Verwende den aktuellen Windows-Sicherheitskontext.

Das vermeidet Passwortspeicherung im Crawler.

Eine später optionale Verbindung mit anderen Credentials wird über Windows Credential Manager realisiert, nicht über Klartext in SQLite.

---

# 24. WebCrawlerConnector

Der Webcrawler ist vollständig .NET-basiert.

Bausteine:

- `HttpClient`;
- HTML DOM Parser;
- persistente Frontier;
- URL Normalizer;
- Robots Policy;
- Sitemap Reader;
- Host Rate Limiter.

## 24.1 Frontier

SQLite:

```text
WebFrontier
  Id
  SourceId
  Url
  CanonicalUrl
  Depth
  State
  RetryCount
  NextAttemptUtc
  ETag
  LastModified
```

## 24.2 Regeln

Pro Source:

- Allowed Hosts;
- Allowed URL Prefixes;
- Include Regex;
- Exclude Regex;
- Max Depth;
- Max Pages;
- Max Bytes;
- Requests/sec;
- Max Parallel/Host.

## 24.3 robots.txt

Standardmäßig respektieren.

## 24.4 Sitemap

Unterstützen:

- Sitemap XML;
- Sitemap Index.

## 24.5 Conditional GET

ETag/Last-Modified speichern und wiederverwenden.

## 24.6 Fehler

```text
404/410 → möglicher bestätigter Verlust
Timeout/5xx/DNS → Quelle temporär gestört
429 → Retry-After
```

Netzfehler führen nicht zu Löschung.

---

# 25. SSRF- und Webcrawler-Sicherheit

Öffentliche Websources dürfen standardmäßig keine URLs aufrufen, die nach DNS-Auflösung auf:

- Loopback;
- Link-local;
- private interne Bereiche;
- bekannte Metadata Services

zeigen.

Interne Sites können in einer Source explizit erlaubt werden.

Redirects werden erneut geprüft.

---

# 26. Discovery vs. Processing

Eine besonders wichtige Trennung:

```text
Discovery
    ↓
kennt: Wo ist etwas?
    ↓
Document Registry
    ↓
Processing
    ↓
kennt: Was ist darin?
```

## 26.1 Vorteil

Ein langsames 500-MB-PDF blockiert nicht die Verzeichniserkennung.

---

# 27. Reconciliation

Jeder Full Scan erhält:

```text
ScanRunId
```

Bei Fund:

```text
LastSeenScanRunId = ScanRunId
```

Nur nach **erfolgreichem Complete Scan** darf eine Missing-Analyse beginnen.

## 27.1 Keine Massendeletion

Folgende Zustände verbieten globale Delete-Reconciliation:

- Root nicht verfügbar;
- Access denied;
- Media offline;
- Scan abgebrochen;
- unerwarteter I/O-Fehler;
- Crawlerprozess gestoppt.

---

# 28. Dokumentidentität

## 28.1 Primär

Interne:

```text
DocumentId = Guid
```

## 28.2 Location

```text
SourceId + RelativeCanonicalLocator
```

## 28.3 Windows File Identity

Wenn verfügbar:

- Volume Serial/Volume ID;
- File ID via Win32.

## 28.4 SHA-256

Bei neuem/geändertem Inhalt.

Verwendung:

- Dubletten;
- Rename;
- Integrity;
- Idempotenz.

---

# 29. Rename und Move

Algorithmus:

1. gleiche Windows File ID → starkes Signal;
2. gleicher Content Hash + Größe → starkes Signal;
3. exakt ein plausibler Missing Candidate → Zuordnung;
4. sonst neues Dokument.

Keine aggressive heuristische Verschmelzung.

---

# 30. Durable Work Queue

SQLite-basierte Queue.

```text
WorkItem
  Id
  Stage
  SourceId
  DocumentId
  Status
  Priority
  AttemptCount
  NextAttemptUtc
  LeaseOwner
  LeaseUntilUtc
  Payload
```

## 30.1 Warum kein reines Channel-System

Ein `Channel<T>` verliert seine Inhalte bei Prozessabsturz.

## 30.2 Hybrid

Verbindliche Architektur:

```text
SQLite Durable Queue
        ↓ lease
Bounded Channel in memory
        ↓
Worker
```

Damit erhalten wir:

- Crash-Recovery;
- Backpressure;
- effiziente Worker.

---

# 31. Worker-Pipeline

```text
Discover
   ↓
Reconcile
   ↓
Open Stream
   ↓
Detect MIME
   ↓
Extract
   ↓
OCR Decision
   ↓
Normalize Metadata
   ↓
Build Index Document
   ↓
Lucene Update
   ↓
Commit Processing State
```

Jede Stufe ist messbar.

---

# 32. Apache Tika als Parser-Sidecar

## 32.1 Warum Tika

Tika besitzt die breiteste und in Suchsystemen erprobte Formatabdeckung.

Die aktuelle stabile 3.3.x-Linie umfasst die relevanten Office-/PDF-Parser.

## 32.2 WinForms-App bleibt .NET

Tika ist eine **Hilfskomponente**, nicht die Anwendung.

```text
Sasd.Crawler.WinForms.exe
      │ local IPC/HTTP
      ▼
Tika Worker
```

## 32.3 JRE

Ein JRE muss entweder:

- kontrolliert mit dem Produkt gebündelt; oder
- als definierte Voraussetzung installiert

werden.

Die Lizenz-/Updatepolitik wird im Releaseprozess dokumentiert.

---

# 33. Tika Process Supervisor

Verantwortlich für:

- Start;
- Stop;
- Health;
- Port/IPC;
- Timeout;
- Neustart;
- Log Capture;
- Versionsprüfung.

Tika bindet nur lokal.

Keine ungeschützte LAN-Erreichbarkeit.

---

# 34. Toxy

Toxy bleibt als alternative .NET-Extraktionsimplementierung vorgesehen.

```text
IContentExtractor
  ├── TikaContentExtractor
  └── ToxyContentExtractor
```

## 34.1 Nutzung

Nicht dynamisch „mal so, mal so“ ohne Kontrolle.

Ein `ExtractionProfile` legt pro MIME fest, welcher Extractor maßgeblich ist.

Beispiel:

```text
text/plain → Native
text/html → Native/DOM
application/pdf → Tika
application/vnd.openxmlformats... → Tika
```

Toxy kann nach Benchmark einzelne Formate übernehmen.

---

# 35. OCR

## 35.1 Tesseract

Tesseract 5.5.x als Referenzengine.

## 35.2 Scan-PDF

Da Tesseract selbst PDF nicht als universellen Eingabestrom behandelt, wird PDF-OCR bevorzugt über die Tika/Tesseract-Integration beziehungsweise eine isolierte Renderingpipeline umgesetzt.

Das WinForms-UI kennt diese technische Einzelheit nicht.

## 35.3 OCR Modes

```text
Off
Auto
Force
Retry
```

## 35.4 Auto

OCR nur, wenn Extraktion keinen ausreichenden Text liefert.

---

# 36. Processing Profile

```text
ProcessingProfile
  Name
  MaxFileSize
  ParseTimeout
  ArchiveEnabled
  MaxArchiveDepth
  MaxExpandedBytes
  OcrMode
  OcrLanguages
  MaxOcrPages
  ParserPolicyVersion
```

Profile sind versioniert.

Eine Änderung kann Reprocessing notwendig machen.

---

# 37. Extraction Cache

Extrahierter Text kann komprimiert gespeichert werden.

Ort:

```text
%LOCALAPPDATA%\SASD\Crawler\cache\extracted\
```

Key:

```text
ContentHash + ExtractorVersion + ProfileVersion
```

Vorteile:

- Preview auch bei offline USB;
- Reindex ohne erneutes USB-Anschließen;
- schneller Indexschemawechsel.

---

# 38. Lucene.NET als eingebetteter Search Index

Lucene.NET 4.8.0-beta00018 unterstützt offiziell .NET 8.

Der formale Beta-Status wird als PoC-Risiko behandelt.

## 38.1 Interface

```csharp
public interface ISearchIndex
{
    Task UpsertAsync(SearchDocument document, CancellationToken ct);
    Task DeleteAsync(DocumentId id, CancellationToken ct);
    Task<SearchPage> SearchAsync(SearchRequest request, CancellationToken ct);
    Task RebuildAsync(IAsyncEnumerable<SearchDocument> documents, CancellationToken ct);
}
```

---

# 39. Lucene Writer Architecture

Ein koordinierter Writer.

Nicht jeder Worker erstellt einen eigenen `IndexWriter`.

```text
Processing Workers
      ↓
IndexCommand Channel
      ↓
LuceneIndexWriterService
      ↓
IndexWriter
```

Vorteile:

- Reihenfolge kontrollierbar;
- Commit Policy zentral;
- weniger Lock-/Race-Probleme.

---

# 40. Near Real Time Search

Suche soll Aktualisierungen schnell sehen.

Verwendung eines SearcherManager-/NRT-Musters.

Trennung:

- Search Refresh;
- durable Commit.

Damit müssen wir nicht nach jedem Dokument einen teuren Commit erzwingen.

---

# 41. Indexfelder

Kernschema:

```text
document_id
source_id
media_id
parent_document_id
canonical_locator
relative_path

filename
title
content_exact
content_de
content_en
author

mime_type
extension
language

size
created_utc
modified_utc

availability
content_hash

parser_name
parser_version
processing_profile_version

security_scope
acl_allow
acl_deny
```

---

# 42. Analyzer

## 42.1 content_exact

Ziel:

- stabile Phrase Search;
- neutrale Basis.

## 42.2 content_de

Deutscher Analyzer/Stemming.

## 42.3 content_en

Englischer Analyzer/Stemming.

## 42.4 Feldboosts

Initial:

```text
title          4.0
filename       3.0
content_exact  2.0
content_de     1.0
content_en     1.0
path           0.5
```

Das sind Startwerte, keine unveränderlichen fachlichen Konstanten.

Golden Query Tests kalibrieren sie.

---

# 43. Search Application Service

UI ruft nicht Lucene direkt auf.

```text
SearchPresenter
    ↓
ISearchService
    ↓
SearchQueryParser
    ↓
SecurityFilter
    ↓
ISearchIndex
```

---

# 44. Suchsyntax

## 44.1 Simple Mode

Benutzer tippt:

```text
projekt alpha
```

Keine Lucene-Syntax notwendig.

## 44.2 Advanced Mode

Unterstützt später:

```text
"Projekt Alpha"
vertrag AND müller
pdf NOT entwurf
type:pdf
source:Archiv
author:Meier
```

## 44.3 Defensive Query Limits

Begrenzen:

- Querylänge;
- Wildcards;
- Fuzzy Expansion;
- Ergebnisgröße.

---

# 45. Facetten und Filter

UI-Filter:

- Quelle;
- Medium;
- Typ;
- Datum;
- Verfügbarkeit;
- Sprache;
- Autor.

Ein offline Medium bleibt als Facette sichtbar.

---

# 46. Treffer und Snippets

`SearchResultItem`:

```text
DocumentId
Title
FileName
SourceName
MediaName
Location
Availability
MimeType
Modified
Score
Snippet
Highlights
```

Snippets werden entweder:

- aus Lucene-Termvektoren/Highlighter;
- oder aus sicherem Extraction Cache

gebildet.

---

# 47. DataGridView-Strategie

Bei großen Trefferzahlen wird nicht die gesamte Ergebnismenge in ein DataTable geladen.

Verwendung:

- Seiten/Continuation;
- optional `VirtualMode`;
- kleine ViewModels.

Das UI zeigt typischerweise 50–200 Treffer pro Page.

---

# 48. Preview Pane

Version 1.0 unterstützt vor allem sichere Textpreview.

## 48.1 Text

Read-only Control.

## 48.2 Bilder

Nur bekannte sichere Bildformate.

## 48.3 HTML

Nicht als ungefiltertes WebBrowser-Dokument rendern.

HTML wird:

- extrahiert;
- sanitisiert;
- als Text oder kontrolliertes Markup dargestellt.

## 48.4 Office

Keine COM-Automation.

Word/Excel/PowerPoint werden für Preview nicht gestartet.

---

# 49. Original öffnen

Das Original wird nur nach expliziter Benutzeraktion geöffnet.

Windows Shell:

```text
UseShellExecute = true
```

Vorher:

- existiert?
- Medium online?
- Pfad noch korrekt?
- Sicherheitskontext hat Zugriff?

---

# 50. Windows-Security-Modell der Desktop-Version

Die Standardarchitektur ist bewusst einfach:

```text
WinForms-App läuft als Benutzer Robin
          ↓
Crawler liest, was Robin lesen kann
          ↓
Index liegt in Robins LocalAppData
          ↓
nur Robin nutzt diesen Index
```

Damit ist kein zusätzliches Search-Time ACL Trimming erforderlich, um andere Windows-Benutzer voreinander zu schützen.

## 50.1 Vorteil

Das ist sicherer als:

```text
Service Account darf alles
        ↓
zentraler Index
        ↓
später komplizierte ACL-Filter
```

## 50.2 Trotzdem ACL-Metadaten

Die Architektur kann ACL-Fingerprints speichern, damit eine spätere Shared-Mode-Migration möglich bleibt.

---

# 51. Windows Credentials

Falls später ein Netzlaufwerk mit anderen Credentials geöffnet werden soll:

- Windows Credential Manager;
- keine Passwörter in SQLite;
- keine Passwörter in Logs.

---

# 52. Windows DPAPI

Lokale sensible Anwendungseinstellungen können über DPAPI geschützt werden.

Abstraktion:

```text
ISecretStore
```

Damit kann ein späterer Host andere Secret Stores verwenden.

---

# 53. Scheduler im Desktopbetrieb

Der Scheduler läuft im Prozess.

## 53.1 Wenn App läuft

Jobs werden normal geplant.

## 53.2 Wenn App beendet ist

Kein geheimer Hintergrundprozess wird behauptet.

Optionen:

1. App beim Windows-Login automatisch starten;
2. im Tray laufen;
3. `--background`-Start über Windows Task Scheduler.

## 53.3 Empfehlung

Für 1.0:

> „Mit Windows starten“ + Tray-Modus.

Kein Windows Service notwendig.

---

# 54. Notification Area / Tray

Tray Icon ist sinnvoll für:

- Indexstatus;
- laufende Jobs;
- USB-Events;
- Pause;
- Öffnen;
- Beenden.

Schließen des Hauptfensters kann konfigurierbar:

- App beenden;
- in Tray minimieren.

Standard sollte transparent dokumentiert sein und keine überraschende Hintergrundausführung erzeugen.

---

# 55. Jobverwaltung

WinForms Workspace „Indexierung“ zeigt:

- Quelle;
- Start;
- Laufzeit;
- Fortschritt;
- discovered;
- processed;
- skipped;
- errors;
- queue;
- status.

Aktionen:

- Start;
- Pause;
- Fortsetzen;
- Abbrechen;
- Retry failures.

---

# 56. Fehlerzentrum

Fehler werden nicht nur in Logdateien versteckt.

Workspace:

```text
Quelle | Dokument | Fehlercode | Zeitpunkt | Versuche | Aktion
```

Filter:

- Source;
- Parser;
- OCR;
- Network;
- Security;
- Permanent/Transient.

---

# 57. Fehlerklassifikation

```text
Transient
Permanent
Policy
Security
Parser
SourceUnavailable
Index
Database
```

Retry nur automatisch für sinnvolle transiente Fehler.

---

# 58. Last Known Good

Wenn eine aktualisierte Datei nicht mehr geparst werden kann:

- alter Indexinhalt bleibt;
- Status wird als veraltet/Processing Error markiert;
- Fehlerzentrum zeigt Problem.

Das verhindert unnötigen Informationsverlust.

---

# 59. Logging

Strukturierte Logs.

Standard:

```text
%LOCALAPPDATA%\SASD\Crawler\logs\
```

Rotation:

- tägliche Dateien;
- Größenlimit;
- Retention konfigurierbar.

Keine Dokumentvolltexte in Standardlogs.

---

# 60. Diagnostik

Workspace „Diagnose“ zeigt:

- App Version;
- .NET Runtime;
- Windows Version;
- DB Schema;
- Lucene Version;
- Index Schema;
- Tika Version;
- Tesseract Version;
- Indexpfad;
- Größe;
- freier Speicher;
- Queue;
- Parser Health.

---

# 61. Crash Handling

Globale Handler:

- `Application.ThreadException`;
- `AppDomain.CurrentDomain.UnhandledException`;
- `TaskScheduler.UnobservedTaskException`.

Sie ersetzen keine lokale Fehlerbehandlung.

Bei fatalem Fehler:

- strukturierter Crashreport;
- kein Dokumentvolltext;
- Queue-Leases laufen später aus;
- Lucene/SQLite-Recovery beim nächsten Start.

---

# 62. Single Instance

Standardmäßig nur eine Instanz pro Windows-Benutzerprofil.

Mechanismus:

- Named Mutex.

Zweite Instanz:

- sendet Aktivierungs-/Suchparameter an bestehende Instanz;
- beendet sich.

Damit werden konkurrierende SQLite-/Lucene-Writer vermieden.

---

# 63. IPC zwischen zweiter und erster Instanz

Ein kleiner Named-Pipe-Kanal kann verwendet werden.

Beispiel:

```text
sasd-crawler.exe --search "Projekt Alpha"
```

öffnet die bestehende Instanz und startet die Suche.

---

# 64. Optionaler lokaler API-Host

Die API ist nicht Kern der WinForms-UI.

Architektur erlaubt später:

```text
Sasd.Crawler.ApiHost
```

oder einen Loopback-Host im selben Prozess.

Default:

- aus;
- nur localhost;
- separate Aktivierung.

Damit bleibt die Application Layer wiederverwendbar.

---

# 65. Backup

Führend:

- SQLite;
- Konfiguration;
- Media Registry;
- Source Definitions.

Rekonstruierbar:

- Lucene;
- Extraction Cache.

Backup kann optional Lucene mitsichern, muss es aber nicht.

---

# 66. Recovery

Tool/Command im UI:

```text
Diagnose → Index prüfen
Diagnose → Index neu aufbauen
```

Rebuild nutzt SQLite + Quellen.

Bei Offline-Medien kann Cache alten Text liefern; vollständiger Rebuild bestimmter Inhalte kann deren erneutes Anschließen benötigen.

---

# 67. Index-Schema-Upgrades

Neue inkompatible Schema-Version:

```text
index\
  active -> build-v5
  build-v6\
```

Ablauf:

1. neuen Index erstellen;
2. aus Cache/Quellen befüllen;
3. validieren;
4. atomaren Active Pointer wechseln;
5. alten Index später löschen.

So bleibt die Suche während großer Migrationen möglichst verfügbar.

---

# 68. Performance Budgets

## 68.1 UI

Kein UI Event Handler darf lange I/O-Arbeit synchron ausführen.

Ziel:

- UI bleibt bei Crawl/OCR bedienbar.

## 68.2 Search

Bei 100.000 Dokumenten:

- Median < 300 ms;
- P95 < 1 s

für definierte Baseline Queries auf Referenzhardware.

## 68.3 Discovery

Streaming und bounded queues verhindern Memory Explosion.

---

# 69. Worker-Pools

Separate Limits:

```text
DiscoveryWorkers
ExtractionWorkers
OcrWorkers
WebWorkers
IndexWriter = 1 coordinated writer
```

Beispielstartwerte:

```text
Discovery  = 2
Extraction = max(2, CPU/2)
OCR        = 1
Web/Host   = 2
```

Konfiguration wird benchmarkbasiert angepasst.

---

# 70. Backpressure

Wenn Queue > High Watermark:

- Discovery drosseln;
- neue Weblinks nicht unbegrenzt laden;
- UI zeigt „Verarbeitung wartet“.

Kein unbounded `ConcurrentQueue`.

---

# 71. High DPI

WinForms .NET 8 besitzt Verbesserungen für High DPI.

Anforderungen:

- PerMonitorV2;
- keine fest verdrahteten Pixelgrößen, wenn vermeidbar;
- AutoScale;
- Test bei 100/125/150/200 %.

---

# 72. Accessibility

## 72.1 Tastatur

Alle Kernfunktionen ohne Maus.

## 72.2 Access Keys

Menüs/Buttons erhalten sinnvolle Mnemonics.

## 72.3 Screenreader

Controls nutzen:

- AccessibleName;
- AccessibleDescription;
- native WinForms Accessibility/UIA.

## 72.4 Farbunabhängigkeit

Offline/Error nicht nur rot/grün.

Zusätzlich:

- Icon;
- Text;
- Statusbezeichnung.

---

# 73. Modernes WinForms-Design

Die Anwendung soll nicht wie ein Windows-95-Tool aussehen, aber auch keine fragile Custom-Control-Plattform werden.

Empfehlung:

- Systemfonts;
- großzügige Abstände;
- klare Typografie;
- flache Command Bars;
- Windows-11-nahe Icons;
- Dark Mode erst nach stabiler Funktion;
- möglichst native Controls.

Eine Third-Party-UI-Suite ist für 1.0 nicht erforderlich.

---

# 74. Themes

1.0:

- System-/helles Theme zuverlässig.

1.1:

- optional Dark Mode.

Keine eigene Theme Engine im MVP.

---

# 75. Suche als zentrale Nutzerreise

Startup sollte direkt nutzbar sein:

```text
App öffnen
  ↓
Cursor im Suchfeld
  ↓
Begriff tippen
  ↓
Treffer
  ↓
Preview
  ↓
Original öffnen
```

Der Benutzer soll nicht zuerst „Index auswählen“ oder „Crawler starten“ müssen.

---

# 76. First Run

Assistent:

1. Willkommen;
2. Standardquellen wählen;
3. optionale Dokumentordner;
4. USB/NAS später hinzufügen;
5. OCR-Sprachen;
6. initiale Indexierung starten;
7. UI sofort nutzbar.

Initial Crawl läuft im Hintergrund.

---

# 77. Quellenverwaltung UI

Dialog/Workspace:

```text
Name
Typ
Ort
Aktiv
Dateifilter
Ausschlüsse
OCR-Profil
Zeitplan
Status
```

`Testen` führt Source Probe aus.

---

# 78. Webquelle UI

Zusätzliche Felder:

```text
Start URL
Allowed Host
Depth
Respect robots.txt
Sitemap
Rate Limit
Include
Exclude
```

Erweiterte Optionen standardmäßig eingeklappt.

---

# 79. Media UI

Zeigt:

- Name;
- Online/Offline;
- Volume Label;
- letzter Mount;
- letztes gesehen;
- Dokumentzahl;
- Indexzustand.

Benutzer kann ein Medium verständlich benennen:

```text
"Archivplatte 2015–2020"
```

---

# 80. Security Boundaries

## 80.1 Vertrauenswürdig

- SASD-eigener Code;
- SQLite Schema;
- Lucene Indexformat innerhalb eigener Datenpfade.

## 80.2 Untrusted

- jede Datei;
- jedes Archiv;
- jede Webseite;
- jedes HTML;
- Metadaten;
- Dateinamen;
- externe Parserantworten.

Alles Untrusted wird validiert/escaped.

---

# 81. Parser Sandbox

Tika erhält:

- niedrige Rechte;
- kein Zugriff auf Benutzerprofil außer Temp/Input;
- keinen offenen LAN-Port;
- Memory Limit soweit Betriebskonzept ermöglicht;
- Timeout.

Bei Hang:

- Prozess kill;
- Fehler;
- Restart.

---

# 82. Temp Files

Temp-Dateien liegen nicht unkontrolliert in `%TEMP%`.

Eigener Bereich:

```text
%LOCALAPPDATA%\SASD\Crawler\cache\temp\<job>
```

Nach Job:

- löschen;
- bei Crash Cleanup beim nächsten Start.

---

# 83. Archive Bomb Protection

Limits:

- Embedded Depth;
- Element Count;
- Expanded Bytes;
- Compression Ratio;
- Parser Time.

Bei Limit:

```text
ARCHIVE_LIMIT
```

Container bleibt im Index mit Warnstatus.

---

# 84. Web Content Security

HTML wird nie als vertrauenswürdiges Markup in WinForms/WebView gerendert.

Falls später WebView2 Preview kommt:

- isolierter Navigationsmodus;
- Script standardmäßig aus;
- keine lokalen File Privileges;
- Content Security Konzept.

Für 1.0 ist reine sichere Textpreview vorzuziehen.

---

# 85. Datenschutz

Per-User-Index reduziert unbeabsichtigte gemeinsame Sichtbarkeit.

Optional zu berücksichtigen:

- Browser-Crawl von privaten Websites nur explizit;
- Suchhistorie aus;
- Logs ohne Suchbegriffe oder konfigurierbar;
- externe KI später opt-in.

---

# 86. Installer und Publishing

## 86.1 Architektur

Zunächst:

- `win-x64`;
- self-contained .NET 8 Publish.

Vorteil:

- Zielrechner benötigt keine separate .NET-Installation.

## 86.2 Kein echtes Single-File-Ziel

Wegen:

- Tika/JRE;
- Tesseract;
- Sprachdaten;
- Parserkonfiguration;
- Plugins;
- Lucene-Dateien

ist ein sauberer Installationsordner geeigneter.

## 86.3 Installer

Bevorzugt MSI/MSIX nach PoC.

Installer muss:

- App installieren;
- Startmenü;
- Uninstall;
- Runtime Sidecars;
- Lizenztexte;
- optional Autostart.

---

# 87. Update-Architektur

Die Anwendung soll keine ungesicherten Selbstupdates aus beliebigen URLs laden.

1.0:

- neue Version explizit installieren.

Später:

- signierter Updatefeed;
- Signaturprüfung;
- Backup vor Schemaänderung.

---

# 88. Code Signing

Releasebuilds sollten Authenticode-signiert werden.

Das reduziert SmartScreen-/Vertrauensprobleme und verbessert Supply-Chain-Sicherheit.

---

# 89. Versionsinformationen

UI:

```text
SASD Crawler 0.5.0
Commit: ...
Build: ...
Database: v...
Index: v...
```

Diagnose exportiert diese Informationen.

---

# 90. Teststrategie – Architektur

## 90.1 Domain

Pure Unit Tests.

## 90.2 Persistence

Echte temporäre SQLite-DB.

## 90.3 Lucene

Echte temporäre Indexverzeichnisse.

## 90.4 Tika

Integrationstest mit realem Sidecar.

## 90.5 OCR

Golden Scans.

## 90.6 WinForms

Presenter werden ohne UI getestet.

View-Smoke-Tests nur für kritische Formzustände.

---

# 91. Warum MVP für WinForms wichtig ist

UI-Automation bei WinForms ist teurer und fragiler als Domain-/Presenter-Tests.

Deshalb:

> Möglichst viel Verhalten liegt außerhalb der Controls.

Beispiel testbar:

```text
SearchPresenter.SearchAsync()
```

ohne ein echtes Fenster zu öffnen.

---

# 92. End-to-End Tests

Separate Testumgebung:

- realer Appprozess;
- temporäres Benutzerprofil/Datenverzeichnis;
- Testdateien;
- lokaler HTTP-Testserver;
- gemappte SMB-Testfreigabe.

UI-Automation nur für Kernpfade:

- Search;
- Source add;
- Preview;
- Offline indicator.

---

# 93. USB Tests

Automatisierte Teile:

- MediaMatcher;
- Registry;
- state transitions.

Manuell/Hardware:

- echter USB-Stick;
- anderer Buchstabe;
- Entfernen;
- Reconnect;
- zwei gleich gelabelte Sticks.

---

# 94. Webcrawler Tests

Lokaler HTTP-Fixture:

- robots;
- sitemap;
- redirects;
- 404;
- 410;
- 429;
- 500;
- slow;
- huge;
- canonical;
- duplicate;
- PDF;
- crawl trap.

---

# 95. Search Quality Tests

Golden Queries.

Beispiel:

```text
Müller
Mueller
"Projekt Alpha"
vertrag AND alpha
rechnung 2025
type:pdf
```

Metriken:

- Must Hit;
- Must Not Hit;
- Rank Range;
- Query Time.

---

# 96. PoC-001 – Lucene.NET auf .NET 8

Verbindlich vor großer Implementation.

Test:

- 100.000 Dokumente;
- 1.000.000 synthetische Dokumente;
- update/delete;
- GermanAnalyzer;
- phrase;
- fuzzy;
- highlight;
- facets;
- concurrent search;
- writer crash;
- rebuild.

Ergebnis dokumentieren.

---

# 97. PoC-002 – Tika/Toxy

Tika bleibt Default.

Benchmark:

- DOC/DOCX;
- XLS/XLSX;
- PPT/PPTX;
- PDF;
- RTF;
- HTML;
- defekt;
- groß;
- verschlüsselt.

---

# 98. PoC-003 – Media Identity

Windows 11:

- NTFS;
- exFAT;
- FAT32;
- externe SSD;
- USB Stick;
- gleicher Labelname;
- Mountletter changed.

---

# 99. PoC-004 – WinForms + Generic Host + Workers

Vor Produktentwicklung wird ein kleiner Spike gebaut:

```text
WinForms
 + Generic Host
 + BackgroundService
 + SQLite
 + cancellation
 + tray
 + graceful shutdown
```

Damit wird das Lebenszyklusmodell früh validiert.

---

# 100. PoC-005 – Tika Packaging

Zu klären:

- JRE gebündelt oder prerequisite;
- Startzeit;
- RAM;
- AV/SmartScreen;
- Update;
- Lizenz/SBOM.

---

# 101. Releasearchitektur

## 0.1

- WinForms Shell;
- Local Source;
- SQLite;
- Lucene;
- Text/HTML;
- Search.

## 0.2

- USB;
- Media Registry;
- SMB/UNC;
- Offline.

## 0.3

- Webcrawler.

## 0.4

- Tika;
- Office/PDF;
- Archive.

## 0.5

- OCR;
- vollständige Search UI;
- MVP.

## 0.6

Für Desktop-Architektur neu interpretieren:

- Windows Security Metadaten;
- Source Security;
- Vorbereitung Shared Mode;
- keine künstliche Benutzerverwaltung nötig, solange per-user.

## 0.7

- Scheduler;
- Tray;
- Jobs;
- Fehlerzentrum;
- Diagnose;
- optional CLI/API adapter.

## 0.8

- Performance;
- Telemetry;
- Consistency.

## 0.9

- Installer;
- Backup;
- Migration;
- Accessibility;
- Security hardening.

## 1.0

- Stabilisierung;
- keine neue Großfunktion.

---

# 102. Shared Mode nach 1.0

Die Architektur verhindert einen späteren zentralen Betrieb nicht.

Dann kommt hinzu:

```text
Sasd.Crawler.Service.exe
        │
        ├── SQLite/PostgreSQL
        ├── Lucene/OpenSearch
        ├── Workers
        └── Local API

Sasd.Crawler.WinForms.exe
        │
        └── Client Adapter
```

Wichtig:

> Die WinForms-Oberfläche muss nicht neu geschrieben werden, wenn Presenter/Application Contracts bereits sauber getrennt sind.

---

# 103. Shared-Mode ACL

Erst im zentralen Modus wird vollständiges Search-Time Security Trimming zwingend.

Dafür bleiben Datenmodelle vorbereitet:

```text
AclFingerprint
AllowedPrincipals
DeniedPrincipals
SecurityMode
```

Im Desktopmodus darf dies zunächst passiv bleiben.

---

# 104. Spätere OpenSearch-Option

`ISearchIndex` verhindert Lock-in.

Migration:

```text
LuceneSearchIndex
        ↓
ISearchIndex
        ↑
OpenSearchSearchIndex
```

OpenSearch wird interessant für:

- zentrale Server;
- große Datenmengen;
- Vectors;
- Hybrid Search.

Nicht notwendig für Desktop 1.0.

---

# 105. Semantik ab 1.5

Die klassische Suche bleibt Kern.

Zusätzliche Pipeline:

```text
Extracted Text
    ↓
Chunker
    ↓
Embedding Provider
    ↓
Vector Store
```

WinForms kann dafür später einen Reiter „Ähnliche Dokumente“ erhalten.

---

# 106. AI/RAG ab 2.0

Nicht in den Desktopkern mischen.

Abstraktionen:

```text
IRetrievalService
IEmbeddingProvider
ILanguageModelProvider
```

UI:

- separater Research Workspace;
- Quellenreferenzen;
- nie Ersatz für klassische Trefferliste.

---

# 107. Architekturentscheidungen (ADR-Liste)

## ADR-001
**Windows Forms ist die Primär-UI.**

## ADR-002
**.NET 8 ist das gewünschte Target, mit dokumentiertem Lifecycle-Risiko.**

## ADR-003
**Version 1.0 ist Windows-first.**

## ADR-004
**Per-user Desktopbetrieb ist Standard.**

## ADR-005
**Clean Architecture + MVP.**

## ADR-006
**SQLite ist Control Store.**

## ADR-007
**Lucene.NET ist v1-Search-Backend hinter Abstraktion.**

## ADR-008
**Tika ist Referenzparser als isolierter Sidecar.**

## ADR-009
**Toxy bleibt austauschbarer PoC/Fast Path.**

## ADR-010
**Tesseract ist OCR-Engine.**

## ADR-011
**Crawler-Jobs sind persistent, nicht nur Tasks.**

## ADR-012
**FileSystemWatcher ist Hinweis, nicht Wahrheit.**

## ADR-013
**Full Reconciliation entscheidet über Löschungen.**

## ADR-014
**USB besitzt MediaId und RelativePath.**

## ADR-015
**UNC/Windows-Security wird nativ genutzt.**

## ADR-016
**Kein Windows Service im Desktop-MVP.**

## ADR-017
**Indexer läuft im selben Hauptprozess; Fremdparser isoliert.**

## ADR-018
**Ein koordinierter Lucene Writer.**

## ADR-019
**Extraction Cache ist abgeleitet und verschlüsselt nicht zwingend, aber per-user geschützt.**

## ADR-020
**Shared Mode ist Erweiterung, kein Zwang für Desktop 1.0.**

---

# 108. Bewusst verworfene Architekturvarianten

## 108.1 Alles in einer WinForms-EXE ohne Module

Verworfen wegen Wartbarkeit.

## 108.2 Fess als eingebetteter Produktkern

Verworfen für die Produktbasis, bleibt Benchmark.

## 108.3 OpenSearch zwingend ab 0.1

Verworfen wegen Installations-/Betriebsgewicht für Desktop.

## 108.4 Windows Service zwingend ab 0.1

Verworfen wegen Privilegien, Installation und Debugging.

## 108.5 Office COM Automation

Verworfen.

Gründe:

- benötigt Office;
- instabil für Hintergrundverarbeitung;
- Security;
- Lizenz/Deployment;
- Dialog-/Hang-Risiken.

## 108.6 Alles über FileSystemWatcher

Verworfen wegen verlorener Events und Netzfreigaben.

## 108.7 Pfad = Dokumentidentität

Verworfen wegen Rename/USB/Mountwechsel.

---

# 109. Architektur-Risiken

## R-ARCH-001 – .NET 8 Supportende

**Risiko:** November 2026.  
**Maßnahme:** Framework zentralisieren, Upgradepfad offenhalten.

## R-ARCH-002 – Lucene.NET Beta

**Maßnahme:** PoC und Backend-Abstraktion.

## R-ARCH-003 – Java Sidecar erhöht Paketgröße

**Maßnahme:** Benchmark Toxy; JRE minimal/bundled prüfen.

## R-ARCH-004 – OCR Performance

**Maßnahme:** separate Queue, niedrige Parallelität.

## R-ARCH-005 – WinForms UI friert ein

**Maßnahme:** Presenter, async Application Services, keine I/O im UI Thread.

## R-ARCH-006 – USB nicht eindeutig

**Maßnahme:** mehrere Fingerprints, keine aggressive Auto-Merge.

## R-ARCH-007 – SMB verschwindet

**Maßnahme:** Complete-Scan-Gate vor Delete.

## R-ARCH-008 – Parserangriff

**Maßnahme:** Sidecar, Timeout, Limits.

## R-ARCH-009 – SQLite/Lucene Dual Write

**Maßnahme:** idempotente WorkItems und Reconciliation.

## R-ARCH-010 – Produkt wird DMS

**Maßnahme:** Originale nicht verwalten/ändern; Search bleibt Produktkern.

---

# 110. Architektur-Traceability auf Anforderungsgruppen

| Lastenheft-Gruppe | Anzahl | Hauptkomponente | Schicht | Architekturbehandlung |
|---|---:|---|---|---|
| ADM-* | 10 | WinForms Admin Workspace / Scheduler | Presentation/Application | WinForms-konkretisiert |
| AI-* | 7 | AI/RAG Extension | Extension | später |
| API-* | 7 | Application Contracts / optional Loopback API | Application/Adapter | Remote-API nicht UI-Kern |
| ARC-* | 6 | Archive/Embedded Processor | Workers | unverändert |
| AUTH-* | 12 | WindowsIdentity / SecurityPolicy | Security | Desktop-first neu geschnitten |
| BAK-* | 6 | BackupService | Infrastructure | unverändert |
| CON-* | 8 | Connector SDK | Extension | später |
| EXT-* | 17 | ContentExtraction Pipeline | Workers | unverändert |
| ID-* | 11 | DocumentRegistry / Reconciliation | Core | unverändert |
| IDX-* | 8 | ISearchIndex / LuceneIndex | Infrastructure | unverändert |
| LOC-* | 6 | FileSystemConnector | Infrastructure | unverändert |
| META-* | 8 | MetadataNormalizer | Core | unverändert |
| OCR-* | 9 | Tika/Tesseract OCR Pipeline | Workers | unverändert |
| OPS-* | 6 | Structured Logging / Diagnostics | Cross-cutting | unverändert |
| ORG-* | 9 | PersonalSearchStore | Application | unverändert/später |
| PERF-* | 4 | Performance Budgets | Cross-cutting | unverändert |
| PLAT-* | 4 | Windows Packaging | Deployment | Linux-1.0-Anforderung kollidiert |
| PRE-* | 5 | PreviewPane / SafePreviewService | Presentation/Application | WinForms-konkretisiert |
| PRIV-* | 4 | Per-user Data Boundary | Security | Desktop-first konkretisiert |
| QRY-* | 6 | Search Quality Harness | Testing | unverändert |
| REL-* | 5 | State Machines / Durable Queue | Core | unverändert |
| SEA-* | 21 | SearchApplicationService | Application | unverändert |
| SEC-* | 8 | Security Boundary / Sidecars | Security | Windows-konkretisiert |
| SEM-* | 8 | SemanticSearchExtension | Extension | später |
| SMB-* | 7 | WindowsFileSystemConnector / UNC | Infrastructure | Windows-spezifisch konkretisiert |
| SRC-* | 9 | Connector Framework / SourceRepository | Core | unverändert |
| UI-* | 14 | WinForms Presentation | Presentation | ersetzt bisherige Web-UI-Annahme |
| USB-* | 9 | RemovableMediaMonitor / MediaRegistry | Infrastructure | Windows-spezifisch konkretisiert |
| UX-* | 5 | WinForms UX / UI Automation | Presentation | WCAG→Desktop Accessibility übertragen |
| WEB-* | 19 | WebCrawlerConnector | Infrastructure | unverändert |

---

# 111. Formale Abweichungen, die vor Implementierungsbaseline bestätigt werden sollten

Die neue Architektur erfüllt das fachliche Ziel, erfordert aber eine saubere Änderung an einigen bisherigen Dokumentaussagen.

## 111.1 PLAT-002 Linux

**Bisher:** Linux 1.0 MUSS.  
**Neue Architektur:** WinForms 1.0 Windows-only.

Empfehlung:

> PLAT-002 auf „Core-Komponenten sollen plattformneutral bleiben, Linux-Host ist spätere Option“ ändern.

## 111.2 PLAT-003 Container

Für Desktop WinForms keine Kernanforderung.

Kann für einen späteren Service Host bestehen bleiben.

## 111.3 UI-013 responsive Weboberfläche

Widerspricht dem gewählten Primärprodukt.

Empfehlung:

> ersetzen durch native WinForms Search UI mit High-DPI-/Accessibility-Anforderungen.

## 111.4 AUTH-002 ff.

Lokale Benutzerverwaltung ist im per-user Desktopmodus unnötig.

Empfehlung:

> lokale Windows-Identität ist primärer Security Context; eigene Multiuser-Accounts erst Shared Mode.

## 111.5 API-001

Search API bleibt als Application Contract wichtig, muss aber nicht bereits als öffentlicher HTTP-Server verstanden werden.

---

# 112. Empfohlene erste Implementierungsreihenfolge

Bevor Featureentwicklung beginnt:

## Architektur-Spike A

WinForms + Generic Host + BackgroundService + SQLite.

## Architektur-Spike B

Lucene.NET unter .NET 8.

## Architektur-Spike C

Tika Sidecar aus WinForms/.NET steuern.

## Architektur-Spike D

Volume Identity.

Danach:

```text
Milestone 0.1
  ↓
lokaler vertikaler Slice
  ↓
Milestone 0.2
  ↓
USB/SMB
```

Nicht zuerst 20 Projekte mit leeren Interfaces anlegen.

Der erste Slice muss früh **End-to-End** funktionieren:

```text
TXT-Datei
  ↓
Crawler
  ↓
SQLite
  ↓
Lucene
  ↓
WinForms Search
  ↓
Original öffnen
```

---

# 113. Definition eines guten 0.1-Slices

Benutzer kann:

1. Anwendung installieren/starten.
2. lokalen Ordner hinzufügen.
3. Indexierung sehen.
4. in einer TXT/HTML-Datei nach einem Wort suchen.
5. Treffer mit Snippet sehen.
6. Original öffnen.
7. Datei ändern.
8. Aktualisierung durchführen.
9. alten Inhalt nicht mehr finden.
10. neuen Inhalt finden.
11. Datei löschen.
12. nach bestätigtem Scan keinen aktuellen Treffer mehr sehen.

Damit ist Architektur bewiesen, bevor Tika/OCR/Web die Komplexität erhöhen.

---

# 114. Entwicklungsstandard für WinForms-Code

## Forms

- keine Business Logic;
- keine DB Queries;
- keine `Task.Run`-Wildwuchs;
- keine direkten Service-Locator-Aufrufe.

## Presenter

- testbar;
- UI-neutral;
- Cancellation;
- Zustandsübergänge.

## Application Services

- Use Cases;
- Transaktionen;
- Policies.

## Infrastructure

- IO;
- Windows;
- SQLite;
- Lucene;
- Parser.

---

# 115. Naming

Beispiele:

```text
SearchWorkspaceView
SearchPresenter
SearchPresentationModel
SearchApplicationService

SourceListView
SourceListPresenter

MediaRegistryService
RemovableMediaMonitor
WindowsVolumeIdentityProvider

FullScanCoordinator
DocumentReconciliationService
WorkItemDispatcher
DocumentProcessingWorker
LuceneIndexWriterService
```

Keine generischen Namen wie:

```text
Manager
Helper
Utils
Common
```

ohne klare Verantwortung.

---

# 116. Cancellation und Shutdown

Beim Beenden:

1. Scheduler stoppt neue Jobs.
2. Worker erhalten Cancellation.
3. laufende atomare Operationen beenden sauber.
4. Leases können auslaufen.
5. Lucene Writer flush/commit.
6. SQLite connections schließen.
7. Tika Sidecar stoppt.

Bei Timeout darf Prozess beenden; Recovery wird beim nächsten Start durchgeführt.

---

# 117. Pausieren

Pause bedeutet:

- keine neuen WorkItem-Leases;
- laufende Parseroperation kann ggf. zu Ende gehen;
- Zustand bleibt persistent.

Kein Thread.Suspend oder ähnliche unsichere Mechanismen.

---

# 118. Suchindex während Crawl

Suche bleibt verfügbar.

Crawler und Searcher teilen den Lucene-Index über koordinierte NRT-Mechanismen.

UI zeigt:

```text
Indexierung läuft – Suche bleibt verfügbar.
```

---

# 119. Datenmengen und UI

Die WinForms-App darf niemals:

- alle Dokumente in ein `DataGridView` laden;
- Millionen Logzeilen halten;
- alle Fehler in Memory materialisieren.

Alle Verwaltungslisten:

- Paging;
- Filter;
- server-/repository-seitige Queries.

---

# 120. Dateivorschau offline

Wenn USB offline:

- Extraction Cache kann Textpreview anzeigen;
- Original öffnen deaktiviert;
- Status deutlich.

Damit ist Offline Search tatsächlich nützlich und nicht nur eine Dateinamensliste.

---

# 121. Sprachmodell der Suche

Deutsch und Englisch in 1.0.

UI erlaubt keine unnötige technische Analyzer-Auswahl.

Automatische/Metadatenbasierte Spracherkennung kann intern die passenden Felder steuern.

---

# 122. Suchkomfort später

1.1:

- Autocomplete;
- Spellcheck;
- Synonyms;
- Saved Search;
- Favorites;
- optional Dark Mode.

Diese Funktionen bauen auf `SearchApplicationService` auf und erfordern keine Architekturänderung.

---

# 123. Tags und Notizen

1.2:

Separate Tabellen:

```text
UserTag
DocumentUserTag
DocumentNote
```

Originaldatei wird nicht verändert.

---

# 124. Plugin-/Connector-Erweiterbarkeit

Keine ungeprüften DLLs aus beliebigen Verzeichnissen laden.

1.x:

- Connectoren werden als signierte/vertrauenswürdige Produktmodule ausgeliefert.

Ein echtes Drittanbieter-Pluginmodell ist erst später sinnvoll.

---

# 125. API-Stabilität intern

Interfaces sind intern versionierbar.

Nicht vorschnell jedes Interface als öffentliches SDK versprechen.

Öffentliche Erweiterungspunkte werden erst festgeschrieben, wenn sie mindestens zwei Implementierungen erfolgreich tragen.

---

# 126. Dokumentation pro Architekturkomponente

Jedes größere Modul erhält:

- README;
- Verantwortlichkeit;
- Invariants;
- Dependencies;
- Failure Modes;
- Tests;
- ADR-Verweise.

---

# 127. Aktuelle technische Primärquellen

## .NET 8 Lifecycle

Microsoft nennt für .NET 8:

- LTS;
- aktuelle Maintenance-Phase;
- Supportende 10. November 2026.

Quellen:

- https://dotnet.microsoft.com/en-us/platform/support/policy
- https://learn.microsoft.com/dotnet/core/releases-and-support
- https://learn.microsoft.com/lifecycle/products/microsoft-net-and-net-core

## Windows Forms .NET 8

Microsoft dokumentiert unter anderem:

- neue Data-Binding-Engine;
- Command-Unterstützung;
- High-DPI-Verbesserungen.

Quellen:

- https://learn.microsoft.com/dotnet/desktop/winforms/
- https://learn.microsoft.com/dotnet/desktop/winforms/whats-new/net80

## Lucene.NET

4.8.0-beta00018:

- Release 22. Juni 2026;
- unterstützt offiziell .NET 8.

Quelle:

- https://lucenenet.apache.org/download/version-4.8.0-beta00018.html

## Apache Tika

- https://tika.apache.org/
- https://tika.apache.org/3.3.2/

## Tesseract

- https://github.com/tesseract-ocr/tesseract
- https://tesseract-ocr.github.io/tessdoc/

---

# 128. Gesamtempfehlung

Für das von dir gewünschte Produkt halte ich folgende Architektur für sauberer als die vorherige serverorientierte Fassung:

```text
                 SASD Crawler
             Windows Forms / .NET 8
                      │
              ┌───────┴────────┐
              │ Application    │
              │ + Domain       │
              └───────┬────────┘
                      │
         ┌────────────┼───────────────┐
         │            │               │
         ▼            ▼               ▼
      SQLite      Lucene.NET      Source Layer
      Control      Search        File/USB/SMB/Web
         │            │               │
         └────────────┼───────────────┘
                      ▼
               Durable Work Queue
                      │
             ┌────────┴─────────┐
             ▼                  ▼
         Apache Tika        Tesseract
         isolated            OCR
```

Der entscheidende Punkt ist:

> **WinForms ist die Produkthülle, aber nicht der Ort, an dem der Crawler „lebt“.**

Der Crawler lebt in testbaren Application-/Domain-/Infrastructure-Modulen. Dadurch erhalten wir gleichzeitig:

- eine native Windows-Anwendung;
- eine reaktionsfähige Oberfläche;
- eine klare C#/.NET-Codebasis;
- keine zwingende Serverinstallation;
- sehr gute USB-/Windows-/SMB-Integration;
- einen einfachen lokalen Betrieb;
- trotzdem einen späteren Weg zu Service/API/OpenSearch, ohne das Produkt neu schreiben zu müssen.

---

# 129. Architektur-Freigabepunkte

Vor dem eigentlichen 0.1-Featurebau sollten vier Entscheidungen praktisch validiert werden:

- **A1:** WinForms + Generic Host + persistente Worker funktionieren sauber.
- **A2:** Lucene.NET besteht den .NET-8-/Performance-/Recovery-PoC.
- **A3:** Tika lässt sich sicher und benutzerfreundlich paketieren.
- **A4:** Windows Volume Identity erfüllt das Offline-Medienmodell.

Wenn diese vier Punkte bestehen, ist die Architektur tragfähig genug, um das Lasten-/Pflichtenheft systematisch umzusetzen.

---

**Ende des Architekturdokuments**
