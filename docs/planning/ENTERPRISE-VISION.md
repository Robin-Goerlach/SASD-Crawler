# Enterprise Vision – non-normative future direction

**Stand:** 6. Oktober 2026  
**Status:** FUTURE VISION / nicht Teil der 1.0-Verpflichtung

Dieses Dokument bewahrt die in Strategiegesprächen entwickelte langfristige Perspektive, ohne den Desktop-MVP aufzublähen.

## Leitbild

Aus dem Desktop-Crawler kann langfristig eine **SASD Enterprise Information Discovery**-Plattform entstehen.

Mögliche spätere Fähigkeiten:

- zentral administrierte Quellen,
- verteilte Crawler/Agents,
- Shared Search / Search Cluster,
- ACL-aware Retrieval über mehrere Identitäten,
- Scheduler und zentrale Jobsteuerung,
- Audit/Retention,
- Monitoring/Health/HA,
- stabile Search-/Ingest-APIs,
- zusätzliche Quellen wie Mail, Datenbanken, DMS, Git, SharePoint/Confluence/Jira,
- semantische/hybride Suche,
- optionale RAG-/Knowledge-Graph-Funktionen.

## Architekturbedingung

Die Desktop-1.0 wird **nicht** als verkleinerter Enterprise-Server gebaut. Stattdessen werden gezielte Abstraktionen erhalten:

- `ISourceConnector`,
- Search Backend Abstraction,
- Control/Persistence boundaries,
- Security descriptor/principal model,
- Application Services unabhängig von WinForms.

Das erlaubt später einen separaten Service Host, ohne die Desktopanwendung neu zu schreiben.

## Governance-Bezug

Im breiteren SASD-Ansatz kann PIMS/Governance Anforderungen, Releases, Risiken und Evidence koordinieren; Git/Codex/CI führen technische Arbeit aus, während SASD-Crawler seine Produktfähigkeiten implementiert.

Beispiel Traceability:

```text
Enterprise requirement
→ Crawler requirement
→ release/milestone
→ implementation
→ test
→ evidence
→ gate
```

## Nicht-Ziel vor 1.0

Keine dieser Enterprise-Ideen darf A2–G0 oder den Desktop-MVP blockieren. Insbesondere werden vor 1.0 keine verteilten Cluster, HA, allgemeine DMS-Funktionen oder AI-Abhängigkeiten erzwungen.
