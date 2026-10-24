# Pre-G0 Native TXT/HTML Indexing – Tests

**CI:** #39  
**Code-Commit:** `6ba0fcd2f1b24d18c647da64f15a6e1e14be3a9f`

- Restore: success
- Format verification: success
- Release build: success
- Tests: **115 passed, 0 failed, 0 skipped**

## Verteilung

| Testprojekt | Tests |
|---|---:|
| Application | 15 |
| Domain | 3 |
| Frontend | 3 |
| Architecture | 8 |
| LocalFileSystem | 4 |
| Workers | 7 |
| Native Extraction | 8 |
| Lucene | 11 |
| SQLite | 16 |
| E2E | 3 |
| **Produktion/Architektur/E2E** | **78** |
| archivierte A1–A4 Spikes | 37 |
| **Gesamt** | **115** |

## Neu nachgewiesen

- TXT UTF-8 sowie BOM-basierte UTF-16-Erkennung;
- HTML sichtbarer Text, Title und Language;
- Script/Style/Noscript/Template werden nicht indexiert;
- Größenlimit greift vor Parsing;
- nicht-lokale URI wird vom Native-Extractor abgelehnt;
- durable IndexDocument/DeleteFromSearchIndex WorkItems;
- Jobabschluss wartet auf Dokument-Processing;
- Partial bleibt nach erfolgreichem Processing Partial;
- authoritative Missing-IDs werden gezielt aus Lucene gelöscht;
- vollständiger TXT/HTML→Lucene Update/Delete-E2E.
