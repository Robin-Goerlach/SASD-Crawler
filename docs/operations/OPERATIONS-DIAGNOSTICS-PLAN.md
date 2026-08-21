# Operations- und Diagnoseplan

## 1. Ziel

Probleme müssen ohne Debugger nachvollziehbar sein.

## 2. Diagnose-Workspace

Anzeigen:
- App Version,
- .NET Runtime,
- Windows Version,
- DB Schema,
- Index Schema,
- Lucene Version,
- Tika Version,
- Tesseract Version,
- Datenpfad,
- Indexgröße,
- freier Speicher,
- Queue,
- aktive Jobs,
- offline Sources/Media.

## 3. Logs

Strukturiert, rotierend, keine Volltexte oder Secrets.

Correlation:
- JobId,
- WorkItemId,
- DocumentId,
- SourceId.

## 4. Fehlercodes

Stabile Codes:
- `SRC_UNAVAILABLE`
- `FILE_ACCESS_DENIED`
- `WEB_TIMEOUT`
- `WEB_SSRF_BLOCKED`
- `PARSER_TIMEOUT`
- `ARCHIVE_LIMIT`
- `OCR_TIMEOUT`
- `INDEX_WRITE_FAILED`
- `DB_WRITE_FAILED`

## 5. Support Bundle

Späterer Diagnoseexport:
- versions,
- sanitized config,
- recent logs,
- health,
- schema versions,
- no document content by default.

## 6. Health

Desktop:
- Statusbar/tray state.

Später API:
- liveness/readiness.

## 7. Telemetrie

Standardmäßig lokal/self-hosted.

Keine externe Produkttelemetrie als Voraussetzung.
