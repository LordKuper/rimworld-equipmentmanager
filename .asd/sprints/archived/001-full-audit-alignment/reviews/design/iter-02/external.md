[REVIEW-design-external]: APPROVE

# External Review Report

- **Phase**: design-review
- **Iteration**: 2
- **Severity floor (this iter)**: medium
- **External tool**: Codex CLI (codex-cli 0.130.0, model gpt-5.5) — available, invoked via `codex.cmd exec` stdin-pipe
- **Artifacts reviewed**: design/prd.html, design/adr.html (incremental — iter-1 finding set verified, scanned for new medium+ issues)

## Iter-1 fix verification

All three iteration-1 findings are resolved in the current drafts:

| Iter-1 finding | Status | Evidence |
|---|---|---|
| (external, low) prd AC-16 referenced "AC-28 test" instead of the C-4 first-sample test | RESOLVED | prd.html AC-16 now reads "AC-25 test asserts [v,v] for the first sample"; AC-25 is the C-4 first-observation assertion. |
| (documentation) prd AC-22 mis-cited audit finding RU-8 | RESOLVED | prd.html AC-22 body and traceability row now cite C-16 (Logger wrapper consumption); RU-8 now correctly appears only against AC-33 (kept-local). |
| (documentation) original-provenance docs rendered a visible provenance chip | RESOLVED | prd.html header renders only PRD / sprint / draft badges; no provenance chip (meta provenance=original). adr.html ADR-0003 still renders a "provenance: reverse-engineered" chip — correct, as that ADR is genuinely reverse-engineered. |

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| – | – | – | none | — |

No medium-or-higher findings. The 41-AC set is internally consistent (AC ids, audit-finding cross-references, and the traceability table agree); the four ADRs are all `proposed` (valid status), each with concrete decisions, negative consequences, and rejected alternatives; ADR sibling cross-references and AC drivers are consistent with the PRD. The LEAN no-UI scope (no ux-spec / design-system / mockups / c4 / DESIGN.md delta) was treated as out of scope and not flagged.

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| – | – | – | none | floor = medium on iter 2; no low findings emitted this iteration |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| – | – | none | no nitpick-category findings emitted |

## Stalemate check

Not a stalemate. Iter-1 produced findings; iter-2 produces none at the medium floor (and the three iter-1 items are verified fixed). The finding sets differ, so no two consecutive identical iterations.

## Verdict
APPROVE

## Next action
External review passes. No author action required from this reviewer. PM aggregates this APPROVE with the internal reviewers' iter-2 verdicts for the design-review Definition-of-Done check.

---
_Note on Codex run: the `codex.cmd exec` invocation returned the verdict `APPROVE` as its final message. Codex's internal sandboxed shell sub-commands (its own attempts to `rg`/`Get-Content` the files) failed with a Windows sandbox error (CreateProcessWithLogonW 1326), but this did not affect the review — the full in-scope content was supplied inline in the prompt payload, so Codex reviewed from the prompt rather than from filesystem reads. Verdict is valid._
