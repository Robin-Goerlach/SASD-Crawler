# Indexer

This directory is the production boundary for crawling and indexing behavior.

Expected components include:

- SQLite persistence/control store;
- durable queue and scheduler;
- source connectors for local/USB/SMB/Web;
- reconciliation and document lifecycle;
- hashing and duplicate grouping;
- Lucene search implementation;
- extraction/Tika and OCR/Tesseract integration;
- Windows file/media infrastructure;
- background observability.

The indexer owns writes to the control store and search index.

Indexer code must not depend on WinForms controls or UI presentation types. It must remain testable headlessly so the same core can later be hosted by the desktop process, a dedicated `Sasd.Crawler.Indexer.exe`, or another approved host.
