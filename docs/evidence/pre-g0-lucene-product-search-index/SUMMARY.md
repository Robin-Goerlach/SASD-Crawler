# Pre-G0 Lucene Product Search Index – Summary

**Datum:** 23. Oktober 2026  
**Main-Commit:** `f46c6b858bf220b628b8179d3d7058692ba57fdb`  
**Post-Merge-CI:** #37 – success  
**Gate-Auswirkung:** A2 bleibt TECHNICAL GO; ADR-0005 bleibt formal Proposed; G0 bleibt NOT READY.

## Nachgewiesen

Der A2-Spike wurde nicht als Produktcode übernommen. Die bestätigten Konzepte wurden hinter neuen Produktionsgrenzen umgesetzt:

- `ISearchIndex` als rebuildbare Application-Abstraktion;
- Lucene.NET 4.8.0-beta00018 als isolierter Produktionsadapter;
- persistentes Upsert über stabile DocumentId;
- gezielte Delete-Operation;
- Commit/Reopen;
- Stichwortsuche und SourceId-Filter;
- DE/EN-Stemming;
- versionierte, symmetrische Stopwortlisten für Index und Query;
- konservative Defaults mit `der/die/das`, aber ohne fest verdrahtetes `ja/nein`;
- Analyzer-Version in Lucene-Commit-Metadata;
- kontrollierter Rebuild bei Analyzer-/Stopwortversionswechsel;
- Lock-Freigabe auch bei abgewiesener Indexkompatibilität.

## Grenzen

Noch nicht Teil dieses Slices waren Crawl→Content→Lucene-End-to-End, WinForms-Suche, Snippets, Open Original oder produktive Stopwort-Konfigurationspersistenz/UI.
