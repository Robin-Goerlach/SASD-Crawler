# Security Plan

## 1. Schutzgüter

- Originaldokumente,
- extrahierte Volltexte,
- Index,
- OCR-Texte,
- Netzwerkcredentials,
- Suchhistorie,
- Benutzer-/Windows-Kontext,
- Konfiguration.

## 2. Trust Boundaries

### Trusted
SASD-eigener signierter Code, lokaler Control Store unter Benutzerprofil.

### Untrusted
jede Datei, jedes Archiv, jede Website, HTML, Metadaten, Dateinamen, Parseroutput.

## 3. Hauptbedrohungen

- malformed Office/PDF,
- parser RCE/CVE,
- zip bomb,
- path traversal,
- SSRF,
- crawl traps,
- XSS/HTML injection,
- secret leakage,
- local privilege exposure,
- false ACL visibility,
- malicious backup/import,
- query DoS.

## 4. Maßnahmen

### Parser
- Sidecar.
- Loopback only.
- Timeout.
- size limits.
- restart.
- temp isolation.
- no unnecessary network.

### Web
- allowed hosts.
- private-IP policy.
- redirect revalidation.
- rate limits.
- max bytes/depth/pages.

### Storage
- per-user LocalAppData.
- restrictive filesystem ACL.
- no plaintext credentials.
- Windows Credential Manager/DPAPI.

### UI
- untrusted text escaped.
- no unsafe HTML rendering.
- no Office COM preview.

### Query
- query length.
- wildcard expansion limits.
- max page size.

## 5. Security Gates

G3: Web threat tests.  
G4: Parser/archive threat tests.  
G6: Desktop data/security review.  
G-RC: full security suite + dependency scan.

## 6. Supply Chain

- locked NuGet dependencies where feasible,
- SBOM,
- vulnerability scan,
- license scan,
- checksums,
- Authenticode signing if available.
