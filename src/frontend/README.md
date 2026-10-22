# Frontend

This directory is the production boundary for user-facing desktop code.

Expected first project:

```text
src/frontend/Sasd.Crawler.WinForms/
```

Responsibilities:

- WinForms shell and workspaces;
- Views/UserControls;
- MVP presenters/presentation models;
- desktop notifications/tray/window behavior;
- desktop composition root.

Presentation code must not directly:

- execute SQL;
- open Lucene;
- crawl directories;
- invoke Tika/Tesseract;
- decide reconciliation/deletion;
- mutate indexer-owned persistence.

For Version 1.0 the desktop composition root may register concrete in-process indexer services through DI. That wiring exception does not permit business logic in Forms.
