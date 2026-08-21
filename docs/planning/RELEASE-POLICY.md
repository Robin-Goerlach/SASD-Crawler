# Release- und Versionierungspolitik

## 1. SemVer

```text
MAJOR.MINOR.PATCH
```

Vor 1.0:
- 0.x darf intern noch breaking changes enthalten,
- Migrationen müssen trotzdem kontrolliert sein.

## 2. Versionen

- 0.0.x Architekturspikes.
- 0.1–0.9 Produktmeilensteine.
- 1.0 erste stabile Baseline.
- 1.x additive Produktentwicklung.
- 2.0 AI/RAG-Generation nur bei tatsächlichem Bedarf.

## 3. Releaseartefakte

Mindestens:
- Win-x64 Paket/Installer,
- SHA-256,
- Release Notes,
- SBOM,
- Lizenzübersicht,
- Test Summary,
- Known Issues,
- Support Matrix.

## 4. Release Candidate

`0.9.x` gilt als RC-Linie.

In der RC-Phase keine neuen Großfeatures.

## 5. Reproduzierbarkeit

Jeder Releasebericht enthält:
- Git commit SHA,
- Tag,
- SDK-Version,
- Runtime,
- Third-party versions,
- Testzahlen,
- Checksums.

## 6. Hotfix

1.0.x/1.x.x Hotfix:
- minimaler Scope,
- Regressionstest,
- keine versteckte Featurearbeit.

## 7. Branching

Bis ein Repositorystandard etwas anderes vorschreibt:
- `main` releasable,
- kurze Featurebranches,
- Tags für Releases.

## 8. Release-Naming

Beispiel:
```text
SASD-Crawler-0.5.0-win-x64
```
