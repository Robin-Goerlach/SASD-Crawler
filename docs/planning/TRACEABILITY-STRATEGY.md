# Traceability-Strategie

## Ziel

Jede normative Anforderung soll von der Spezifikation bis zum Nachweis verfolgt werden können.

Kette:

```text
Requirement
→ Architekturkomponente
→ Issue/Task
→ Commit
→ Test
→ Evidence
→ Release Gate
```

## Requirement IDs

Die 258 Lastenheft-IDs bleiben stabil.

Beispiel:
```text
USB-004
```

## Task-Bezug

Tasktitel:
```text
[USB-004] Offline-Treffer im Index erhalten
```

## Tests

Beispiel:
```text
USB_004_OfflineMedia_DoesNotRemoveIndexedDocuments
```

## Evidence

```text
docs/evidence/0.2/USB-004.md
```

## Requirement-Status

`REQUIREMENTS-STATUS.md` hat drei getrennte Dimensionen:

- Specification,
- Implementation,
- Verification.

„Specified“ darf niemals als „Done“ gezählt werden.

## Änderungsregel

Wenn eine Requirement-ID fachlich geändert wird:
- Lastenheft ändern,
- Change Request,
- alle Traceability-Zuordnungen prüfen.

IDs werden möglichst nicht wiederverwendet.
