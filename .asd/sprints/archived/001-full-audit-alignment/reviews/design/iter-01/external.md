[REVIEW-design-external]: CONCERNS

# External Review Report

- **Phase**: design-review
- **Iteration**: 1
- **Severity floor (this iter)**: low
- **External tool**: Codex CLI (codex-cli 0.130.0, model gpt-5.5) — available, invoked
- **Artifacts reviewed**: design/prd.html, design/adr.html (full content, iteration 1)

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | prd.html : AC-16 | Stat-range verification text says "AC-28 test asserts [v,v]", but the test that asserts first-sample [v,v] correctness (C-4) is AC-25; AC-28 covers `Loadout.IsAvailable` branches (C-21). The cross-reference is wrong, creating traceability ambiguity for the first-observation `[v,v]` characterization test. (Codex finding, source: prd.html AC-16 vs AC-25/AC-28.) | Change the AC-16 verification reference from "AC-28 test" to "AC-25 test" (AC-25 is the C-4 first-sample assertion). Optionally cross-link AC-29's C-3 composite-key assertion separately. |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| – | – | – | none | floor = low; nothing dropped below floor |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| – | – | none | no nitpick-category findings emitted |

## Verdict
CONCERNS: 1

One low-severity traceability defect. No critical, high, or medium findings. The nullable-migration strategy (ADR-0001), the consolidation/sequencing contract (ADR-0002 + AC-1), the dependency-integration pattern (ADR-0003), and the defect/performance remediation policy (ADR-0004) are internally consistent with the PRD's 41 AC: ADR statuses are valid (`proposed`), each decision is concrete, consequences include negative impacts, and alternatives are present. The intentional absence of ux-spec / design-system / mockups / c4 / DESIGN.md delta was treated as out of scope (LEAN no-UI sprint) and not flagged.

## Next action
BA to fix the AC-16 cross-reference (point first-sample `[v,v]` verification at AC-25, not AC-28). Single low-severity edit; per the iteration severity floor this is the last iteration at which a low finding qualifies (iter 2 floor rises to medium), so it should be corrected in this round or accepted/waived by the PM.
