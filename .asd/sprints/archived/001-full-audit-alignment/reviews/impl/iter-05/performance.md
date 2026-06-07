---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: APPROVE

# Review — performance

- **Phase**: impl-review
- **Iteration**: 05

## Scope

Targeted review of one fix (commit 258df7d) in
`Source/EquipmentManager/EquipmentManagerMapComponent.cs` — the sidearm/tool
duplicate-assignment fix. Per iteration-5 severity floor (`review-policy.md`),
only HIGH/CRITICAL performance regressions are admissible; low/medium dropped.

## Verification

Confirmed the gating context. `MapComponentTick` (line 260-285) returns early
unless `Find.TickManager.TicksGame % 60 == 0` (line 264) **and**
`hoursPassed >= 6f` (line 267). The added work lives in the assignment passes
(`AssignAllTools` line 60-72, `AssignBestTool` line 112-124,
`AssignToolsForWorkTypes` line 244-256), all reachable only through that
throttled path, and only inside the non-carried (upgrade) `else` branch — a
rare case.

Per-site added work, all within the throttled/rare branch:
- `defPair = weapon.toThingDefStuffDefPair()` — computed **once**, hoisted out
  of the loop (lines 60, 112, 244). Not recomputed per iteration.
- `carriedWeapons.Where(w => w.toThingDefStuffDefPair() == defPair).ToList()`
  (lines 61, 113, 245) — single O(n) scan; `carriedWeapons` is one pawn's
  carried weapons (small, typically <10, sourced at lines 33-34 from
  `GetCarriedWeapons`). No nesting over large collections; no O(n²).
- `DropSidearm` + `SetForbidden` loop (lines 62-68 et al.) — bounded by the
  count of same-def-pair inferior carried weapons (effectively 0-1). Not a
  hot-path cost.
- `.ThenByDescending(carriedWeapons.Contains)` tiebreaker (lines 98, 144, 184,
  230) — a `List.Contains` (O(n), tiny n) per comparison over a 2-ordering
  sort; trivial, as noted in the brief.

No new per-tick or per-frame cost introduced. The C-12 `assignedByOthers`
hoisting (passed as a `HashSet<Thing>` parameter, O(1) `Add`/`Contains` at
lines 49, 67, etc.) and the C-13 lookups are untouched and intact. No budget
defined in `custom-coding-rules.md` is breached (that file specifies no
explicit latency/memory/throughput budgets; assessment is against
anti-pattern and complexity rubric only).

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above floor (HIGH/CRITICAL) | — |

## Verdict
APPROVE

## Next action
None. No performance regression; the fix's added work is correctly confined to
the throttled (~once per 6 in-game hours, %60-tick) assignment passes and the
rare non-carried upgrade branch, over a small per-pawn collection with the
def-pair computation hoisted. impl-review may count Performance as APPROVE for
iteration 05.
