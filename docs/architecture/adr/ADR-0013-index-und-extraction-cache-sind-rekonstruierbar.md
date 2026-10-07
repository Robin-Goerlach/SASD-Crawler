# ADR-0013: Index und Extraction Cache sind rekonstruierbar

**Status:** Accepted  
**Datum:** 21. August 2026  
**Accepted:** 6. Oktober 2026

## Kontext

Ein Suchindex darf nicht die einzige Quelle für fachlichen Lebenszyklus-, Quellen- oder Medienzustand sein. Indexschema und Parser ändern sich im Produktverlauf.

## Entscheidung

SQLite/Control Store und die Originalquellen führen den dauerhaften fachlichen Zustand. Lucene/OpenSearch-Index und Extraction-/Preview-Caches sind abgeleitete Artefakte und müssen grundsätzlich neu aufgebaut werden können.

## Positive Folgen

- sichere Indexmigration und Recovery,
- Search Backend bleibt austauschbar,
- Parser-/Analyzerschema kann über Rebuild aktualisiert werden.

## Negative Folgen / Trade-offs

- Rebuild kann zeit- und ressourcenintensiv sein,
- Offline-Medien benötigen Extraction Cache oder späteren Wiederanschluss für vollständiges Reprocessing.

## Verifikation

A2 prüft Index-Reopen/Recovery/Rebuild. Spätere Milestones prüfen Rebuild aus dem echten Document Registry und aus Offline-Caches.
