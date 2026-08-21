# Daten-, Backup- und Migrationsplan

## 1. Datenklassen

### Führend
- SQLite Control DB,
- Source definitions,
- Media registry,
- jobs/reconciliation state,
- processing revisions,
- user metadata such as favorites/tags later.

### Rekonstruierbar
- Lucene index,
- extraction cache,
- preview cache.

### Extern/führend außerhalb
- Originaldateien,
- Webseiten.

## 2. Datenpfad

```text
%LOCALAPPDATA%\SASD\Crawler\
```

## 3. Backup

MUSS sichern:
- SQLite konsistent,
- bootstrap config,
- secret references.

SOLL:
- Index snapshot zur Beschleunigung.

## 4. Restore

Prüfen:
- manifest,
- product version,
- DB schema,
- checksums,
- compatibility.

## 5. Migration

Jede Schemaänderung besitzt:
- migration ID,
- forward migration,
- test from previous supported version,
- pre-upgrade backup.

## 6. Indexschema

Bei inkompatibler Änderung:
- neuen Index neben aktivem bauen,
- validieren,
- active pointer wechseln.

## 7. Recovery

`doctor`/Diagnose prüft:
- SQLite integrity,
- Lucene open,
- expired leases,
- cache,
- versions,
- disk space.

## 8. Offline-Medien

Ein vollständiger Rebuild kann für Offline-Originale auf Extraction Cache zurückgreifen. Fehlt Cache und Medium, bleibt Reprocessing pending bis Medium wieder verfügbar ist.

## 9. Originaldaten

Der SASD-Crawler ist **kein Backup der Originaldateien**.
