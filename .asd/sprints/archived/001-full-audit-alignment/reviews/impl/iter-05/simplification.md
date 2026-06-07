---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 5

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above floor (critical) | — |

## Verdict
APPROVE

## Next action
Reviewer done. No over-engineering-checklist trips and no needless complexity in commit 258df7d. No fix or escalation required from this reviewer.

## Assessment notes (informational, below floor — no action)

Targeted scope: the duplicate-sidearm/tool drop fix across `UpdateMeleeSidearms`, `UpdateRangedSidearms`, `AssignAllTools`, `AssignBestTool`, `AssignToolsForWorkTypes` in `EquipmentManagerMapComponent.cs`.

Over-engineering checklist — all clear:

- No new interface, generic, factory, plugin, or abstraction introduced. The fix REUSES the existing `WeaponAssingment.DropSidearm` API (already consumed at line 299 in `RemoveUnassignedWeapons`). Confirmed: same API, same call shape — no wrapper, no helper.
- No premature config flag — no new caller-tunable knob added.
- No defensive code for an impossible case — the `if (inferior.Spawned) SetForbidden(...)` guard is necessary because a despawned/inventory item cannot be unforbidden; it is a real-contract guard, not speculative.
- No dead code; no "in case we need it" leftovers.
- No comment-that-restates-code (these methods carry no comments).
- No inheritance, no framework-wrapping, no test-mock concerns in this diff.

Duplication-vs-extraction judgment (the borderline item flagged in the payload): the drop-inferior block recurs at ~6 inline sites (lines 60-68, 112-120, 244-252, 456-465, 489-498, 600-609, 635-644), each ~4 lines. This is borderline but does NOT cross the extraction bar here, and I deliberately do NOT demand extraction:

- Per the Simplicity Default, inlining is the correct default; extracting a shared helper would itself require Complication Approval and would ADD an abstraction (new private method taking pawn + carriedWeapons + assignedByOthers + target). That is the move this reviewer guards against, not toward.
- The sites are embedded in already-divergent assignment branches (different weapon source variable, different surrounding switch arms, one site uses a different `requestQueueing` expression at line 501-502). A single helper would not cleanly cover all arms.
- The blocks are mechanical and self-contained; the duplication cost is low and localized.

Therefore the repeated block is acceptable as-is (`keep-as-is`). Extraction is not warranted and would trip the over-engineering guard if proposed.

`AssignToolsForWorkTypes` restructure: the carried/non-carried if-else (lines 235-256) is a genuine simplification — it removes the previously-wrong branch and routes carried weapons to memory-only handling while non-carried weapons drop inferior duplicates and queue the equip job. Net complexity decreases; no added branching depth.

Cross-reviewer guard: no fix observed in this diff that adds abstraction, layer, or dependency. Should any sibling reviewer propose extracting the drop-inferior block into a shared helper as a "fix", that proposal must go through Complication Approval and should be challenged — the inline form is the simpler, approved-by-default choice.

## Escalations
- none
