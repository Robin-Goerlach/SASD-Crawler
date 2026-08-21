# Repository Setup

## Suggested repository name

`SASD-Crawler`

## Suggested GitHub description

> Windows Forms/.NET 8 desktop crawler for full-text search across local folders, USB/offline media, SMB shares and websites. Indexes Office/PDF content with OCR, resilient reconciliation and an extensible search architecture.

## Initial branch

`main`

## Recommended protection later

After CI exists:

- require successful build/test checks for PR merge;
- disallow force pushes to `main`;
- keep release tags immutable.

Do not enable protection rules before the repository actually has the corresponding CI checks, otherwise early bootstrap work may become unnecessarily blocked.
