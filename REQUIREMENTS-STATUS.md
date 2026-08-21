# Requirements Status Register

**Stand:** 21. August 2026  
**Anforderungen:** 258

## Statusdefinition

- `SPECIFIED` – fachlich im Lastenheft beschrieben.
- `NOT STARTED` – keine verifizierte Implementierung.
- `PENDING` – noch kein Verifikationsnachweis.
- `DONE` darf erst gesetzt werden, wenn Code + Test + Evidence vorhanden sind.

## Aktueller Gesamtstatus

| Dimension | Stand |
|---|---|
| Fachlich spezifiziert | 258/258 |
| Implementiert | 0/258 verifiziert |
| Verifiziert | 0/258 verifiziert |

## Register

| ID | Titel | Priorität | Zielrelease | Spezifikation | Implementierung | Verifikation | Evidence |
|---|---|---|---|---|---|---|---|
| SRC-001 | Quellen als eigenständige Objekte | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-002 | Quellentypen erweiterbar | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-003 | Quelle aktivieren/deaktivieren | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-004 | Vollständiger Neuaufbau pro Quelle | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-005 | Inkrementelle Aktualisierung | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-006 | Include-Regeln | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-007 | Exclude-Regeln | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-008 | Quellspezifische Größenlimits | SOLL | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SRC-009 | Quellspezifische Priorität | KANN | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| LOC-001 | Lokale Verzeichnisse | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| LOC-002 | Rekursive Traversierung | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| LOC-003 | Rekursion begrenzbar | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| LOC-004 | Symbolische Links/Junctions | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| LOC-005 | Dateisystemfehler | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| LOC-006 | Pfadänderungen | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-001 | Wechseldatenträger als eigene Quelle | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-002 | Stabile Medienidentität | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-003 | Offline-Status | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-004 | Offline-Treffer erhalten | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-005 | Offline-Treffer kennzeichnen | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-006 | Medienname anzeigen | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-007 | Wiederanschluss erkennen | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-008 | Fehlendes Medium ist keine Löschung | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| USB-009 | Portable Indizes | KANN | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| SMB-001 | SMB/CIFS-Freigaben | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SMB-002 | Gemappte Netzlaufwerke | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SMB-003 | UNC-/Netzpfade | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SMB-004 | Verbindungsunterbrechung | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SMB-005 | Wiederholungsstrategie | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SMB-006 | Verbindungs- und Lastbegrenzung | SOLL | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SMB-007 | Quellberechtigungen | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-001 | HTTP/HTTPS | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-002 | Start-URLs | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-003 | Domain-/Host-Grenzen | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-004 | Include-/Exclude-URL-Regeln | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-005 | Crawl-Tiefe | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-006 | URL-Normalisierung | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-007 | Canonical URL | SOLL | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-008 | Redirects | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-009 | robots.txt | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-010 | Sitemap | SOLL | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-011 | Rate Limiting | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-012 | User-Agent | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-013 | Timeouts | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-014 | HTTP-Fehler | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-015 | Web-Löschung | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-016 | Crawl-Traps | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-017 | Verlinkte Dokumente | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-018 | Authentisierte Websites | SOLL | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| WEB-019 | JavaScript-renderte SPAs | KANN | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-001 | Textdateien | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-002 | HTML | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-003 | PDF | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-004 | DOCX | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-005 | DOC | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-006 | XLSX | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-007 | XLS | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-008 | Excel-Blätter | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-009 | PPTX | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-010 | PPT | SOLL | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-011 | OpenDocument | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-012 | RTF | SOLL | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-013 | EPUB | SOLL | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-014 | E-Mail-Dateien | SOLL | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-015 | PST/OST | KANN | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-016 | Unbekannte Binärformate | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| EXT-017 | Parseridentität | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ARC-001 | ZIP | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ARC-002 | Archivpfad | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ARC-003 | Rekursion begrenzen | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ARC-004 | Zip-Bomb-Schutz | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ARC-005 | Weitere Archive | SOLL | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| ARC-006 | Passwortgeschützte Archive | KANN | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-001 | Scan-PDF erkennen | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-002 | OCR für Scan-PDF | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-003 | Bilder | SOLL | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-004 | Sprachen | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-005 | Mehrsprachigkeit | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-006 | OCR nicht unnötig ausführen | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-007 | OCR-Metadaten | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-008 | OCR wiederholen | SOLL | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OCR-009 | OCR-Ressourcenlimit | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-001 | DocumentId | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-002 | SourceId | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-003 | Canonical URI | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-004 | MediaId | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-005 | Relative Path | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-006 | Content Hash | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-007 | Zeitstempel und Größe | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-008 | LastSeen | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-009 | Availability | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-010 | Rename-/Move-Erkennung | SOLL | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ID-011 | Dublettenerkennung | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| META-001 | Basisfelder | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| META-002 | Dokumenttitel | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| META-003 | Autor | SOLL | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| META-004 | Erstellungsdatum | SOLL | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| META-005 | Sprache | SOLL | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| META-006 | EXIF | SOLL | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| META-007 | GPS | KANN | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| META-008 | Benutzerdefinierte Felder | SOLL | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-001 | Persistenter Inhaltsindex | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-002 | Inkrementelle Updates | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-003 | Löschung | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-004 | Offline versus gelöscht | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-005 | Indexversion | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-006 | Reindex nach Parserupgrade | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-007 | Konsistenzprüfung | MUSS | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| IDX-008 | Recovery | MUSS | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-001 | Stichwortsuche | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-002 | Phrase Search | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-003 | AND | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-004 | OR | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-005 | NOT | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-006 | Feldsuche | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-007 | Relevanz | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-008 | Sortierung | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-009 | Fuzzy Search | SOLL | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-010 | Prefix/Wildcard | SOLL | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-011 | Facetten | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-012 | Sprache | SOLL | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-013 | Autor | SOLL | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-014 | Synonyme | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-015 | Spellcheck | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-016 | Autocomplete | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-017 | Gespeicherte Suchen | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-018 | Suchhistorie | KANN | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-019 | Related/Similar | SOLL | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-020 | Result Pinning | SOLL | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEA-021 | Learning to Rank | KANN | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| UI-001 | Zentrales Suchfeld | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-002 | Search-as-you-type Reaktion | SOLL | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-003 | Treffername | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-004 | Pfad/URL | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-005 | Quelle | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-006 | Snippet | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-007 | Highlighting | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-008 | Original öffnen | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-009 | Offline-Information | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-010 | Vorschau | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-011 | PDF-/Bildthumbnail | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| UI-012 | Tastaturbedienung | MUSS | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-013 | Responsive Weboberfläche | SOLL | 1.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UI-014 | Desktopintegration | KANN | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-001 | Single-User-Betrieb | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-002 | Benutzerkonten | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-003 | Rollen | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-004 | Gruppen | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-005 | Source ACL | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-006 | Security Trimming | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-007 | Snippet-Schutz | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-008 | Deny vor Allow | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-009 | ACL-Fingerprint | SOLL | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-010 | LDAP/Active Directory | SOLL | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-011 | OIDC/SSO | SOLL | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| AUTH-012 | TOTP/MFA | KANN | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-001 | Administrationsoberfläche | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-002 | Crawl starten | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-003 | Crawl pausieren | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-004 | Crawl fortsetzen | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-005 | Zeitpläne | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-006 | Fehlerliste | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-007 | Retry | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-008 | Failure Queue | SOLL | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-009 | Statistik | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ADM-010 | Throttling | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| API-001 | Search API | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| API-002 | Source API | SOLL | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| API-003 | Job API | SOLL | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| API-004 | Status API | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| API-005 | Bulk Ingest | SOLL | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| API-006 | Webhooks | SOLL | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| API-007 | CLI | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| BAK-001 | Konfigurationsbackup | MUSS | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| BAK-002 | Konfigurationsrestore | MUSS | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| BAK-003 | Indexbackup | SOLL | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| BAK-004 | Rebuild statt Backup möglich | MUSS | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| BAK-005 | Datenmigration | MUSS | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| BAK-006 | Rollback | SOLL | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-001 | Favoriten | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-002 | Benutzertags | SOLL | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-003 | Notizen | KANN | 1.2.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-004 | Thesaurus | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-005 | Populäre Suchbegriffe | KANN | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-006 | Suchalarme | SOLL | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-007 | Benachrichtigungskanäle | KANN | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-008 | Query-Tuning | SOLL | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| ORG-009 | Search Analytics | SOLL | 1.4.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-001 | Similar Documents | SOLL | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-002 | Named Entity Recognition | SOLL | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-003 | Entity Linking | KANN | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-004 | Automatische Klassifikation | SOLL | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-005 | Embeddings | SOLL | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-006 | Vektorsuche | SOLL | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-007 | Hybridsuche | SOLL | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| SEM-008 | Semantik abschaltbar | MUSS | 1.5.x | SPECIFIED | NOT STARTED | PENDING | – |
| AI-001 | RAG | SOLL | 2.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AI-002 | Quellenbezug | MUSS | 2.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AI-003 | ACL vor Kontextbildung | MUSS | 2.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AI-004 | Zusammenfassungen | SOLL | 2.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AI-005 | Knowledge Graph | KANN | 2.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AI-006 | Lokales KI-Backend | SOLL | 2.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| AI-007 | Externe KI nur opt-in | MUSS | 2.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| CON-001 | SFTP | SOLL | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| CON-002 | FTP/FTPS | KANN | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| CON-003 | WebDAV | SOLL | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| CON-004 | S3-kompatibler Objektspeicher | SOLL | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| CON-005 | Nextcloud | KANN | 1.3.x | SPECIFIED | NOT STARTED | PENDING | – |
| CON-006 | SharePoint | KANN | 2.x | SPECIFIED | NOT STARTED | PENDING | – |
| CON-007 | Confluence/Jira | KANN | 2.x | SPECIFIED | NOT STARTED | PENDING | – |
| CON-008 | Git-Repositories | KANN | 2.x | SPECIFIED | NOT STARTED | PENDING | – |
| PERF-001 | Interaktive Suche | MUSS | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PERF-002 | Skalierungsstufen | MUSS | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PERF-003 | Hintergrundarbeit | MUSS | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PERF-004 | Ressourcenlimits | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| REL-001 | Einzeldateifehler isolieren | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| REL-002 | Netzwerkfehler isolieren | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| REL-003 | Wiederanlauf | MUSS | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| REL-004 | Keine Massendeletion bei Unsicherheit | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| REL-005 | Checkpoint | SOLL | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-001 | Least Privilege | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-002 | Parserisolation | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-003 | Größenlimits | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-004 | Keine Credential-Protokollierung | MUSS | 0.2.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-005 | Secrets geschützt speichern | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-006 | Transportverschlüsselung | MUSS | 0.6.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-007 | Audit administrativer Änderungen | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| SEC-008 | Webcrawler SSRF-Schutz | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRIV-001 | Keine notwendige Cloudübertragung | MUSS | 1.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRIV-002 | Externe Dienste opt-in | MUSS | 1.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRIV-003 | Suchhistorie deaktivierbar | MUSS | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| PRIV-004 | Retention für Logs | SOLL | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PLAT-001 | Windows | MUSS | 1.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PLAT-002 | Linux | MUSS | 1.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PLAT-003 | Containerbetrieb | SOLL | 1.0.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PLAT-004 | macOS | KANN | 2.x | SPECIFIED | NOT STARTED | PENDING | – |
| UX-001 | Einfache Erstsuche | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UX-002 | Technische Details optional | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UX-003 | Fehler verständlich | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UX-004 | Accessibility | MUSS | 0.9.0 | SPECIFIED | NOT STARTED | PENDING | – |
| UX-005 | Internationalisierung | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
| OPS-001 | Health | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OPS-002 | Strukturierte Logs | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OPS-003 | Log-Level | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OPS-004 | Metriken | SOLL | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OPS-005 | Monitoringintegration | SOLL | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| OPS-006 | Keine personenbezogenen Volltexte in Standardlogs | MUSS | 0.7.0 | SPECIFIED | NOT STARTED | PENDING | – |
| QRY-001 | Reproduzierbarer Testkorpus | MUSS | 0.4.0 | SPECIFIED | NOT STARTED | PENDING | – |
| QRY-002 | Deutsch | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| QRY-003 | Englisch | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| QRY-004 | Unicode | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| QRY-005 | Rankingtest | MUSS | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| QRY-006 | Precision/Recall-Benchmark | SOLL | 0.8.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRE-001 | Textvorschau | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRE-002 | Keine Makroausführung | MUSS | 0.5.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRE-003 | Original öffnen | MUSS | 0.1.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRE-004 | Weblink | MUSS | 0.3.0 | SPECIFIED | NOT STARTED | PENDING | – |
| PRE-005 | Thumbnail | SOLL | 1.1.x | SPECIFIED | NOT STARTED | PENDING | – |
