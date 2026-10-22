# Shared

This directory contains production components that are independent of both the WinForms frontend and concrete indexer implementations.

Expected components include:

- `Sasd.Crawler.Domain`;
- `Sasd.Crawler.Application`;
- `Sasd.Crawler.Contracts`;
- small abstraction projects where a stable boundary is justified.

Rules:

- no WinForms dependency;
- no SQLite/Lucene/Tika/Tesseract concrete dependency;
- no Windows UI types;
- keep security-critical lifecycle and reconciliation rules authoritative here when they are domain/application concerns;
- do not create empty projects before the current vertical slice needs them.
