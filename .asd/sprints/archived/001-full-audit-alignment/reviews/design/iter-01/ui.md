---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-ui]: ABORT

# Review — UI (Design Phase)

- **Phase**: design-review
- **Iteration**: 01

## Precondition Check

**ABORT CONDITION MET: Accessibility baseline missing.**

The UI reviewer requires `design/ux/accessibility.html` as a mandatory precondition for all design-review and impl-review phases (per operating contract, stop-condition rule: "accessibility.html missing → ABORT").

The `design/ux/` directory does not exist in this project. Checked:
- `design/ux/accessibility.html` — **not found**
- `design/ux/` directory — **not found**
- Path verified: absolute path `D:\Storage\Projects\RimWorld\rimworld-equipmentmanager\design\ux\`

## Context

Sprint 001-full-audit-alignment is confirmed as a lean audit-and-remediation sprint with zero UI changes. The sprint.md correctly records this scope (no ux-spec.html expected); the design artifacts are prd.html and adr.html only.

However, the accessibility baseline is not a sprint-scoped artifact. It is a persistent reference document required by the review framework for any artifact that touches UI principles, tokens, or accessibility. This review cannot proceed without it.

## Next action

**Create** `design/ux/accessibility.html` with the accessibility-baseline rules and Known Intentional Limitations for the EquipmentManager project. This baseline is a prerequisite for all future UI reviews and must be created before this review can complete.

Once the baseline exists:
1. This review will re-run with clean context (per review-policy.md).
2. The ADRs will be scanned for any embedded UI-token or accessibility decisions (as instructed: check ADR-0002 RU-7 widget mention for pixel-parity scope).
3. Verdict will be issued (expected: APPROVE or N/A, per sprint scope).

No findings from the ADR body are applicable at this iteration without the baseline in place.

