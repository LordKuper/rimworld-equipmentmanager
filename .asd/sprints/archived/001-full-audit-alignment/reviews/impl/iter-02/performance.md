---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: APPROVE

# Review — performance

- **Phase**: impl-review
- **Iteration**: 2

## Budget note

`.asd/project/custom-coding-rules.md` defines **no quantitative performance budgets** (no latency/memory/throughput targets). Per the reviewer stop condition, no budgets exist to enforce. The verdict is therefore reached against the anti-pattern / algorithmic-complexity / regression rubric only, applied to the assignment hot path (gated to run at most once per 6 in-game hours, on a 60-tick cadence, only when the map is the player home and unpaused).

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above MEDIUM floor | — |

## Verification of prior performance items (medium+)

- **C-12 (assignedByOthers hoisting) — COMPLETE, incl. iter-01 gap.** `EquipmentManagerMapComponent.cs` builds the cross-pawn `assignedByOthers` set exactly **once per pass**: `UpdatePrimaryWeapons` (:489), `UpdateRangedSidearms` (:510), `UpdateMeleeSidearms` (:386), `UpdateTools` (:592). The per-pawn `SelectMany` rebuild is gone — a grep for `SelectMany(pc => pc.AssignedWeapons…)` returns exactly those 4 hoisted sites and nothing inside any pawn/rule loop. The iter-01 gap (sidearm passes rebuilding per-pawn) is closed: both sidearm passes now mutate the hoisted set incrementally via `assignedByOthers.Add(...)` (:417, :440, :541, :566). The five tool/primary helper methods (`AssignAllTools` :49, `AssignBestTool` :93, `AssignPrimaryMeleeWeapon` :130, `AssignPrimaryRangedWeapon` :170, `AssignToolsForWorkTypes` :216) take the set as a parameter and `Add` to it rather than recomputing. Former O(pawns² × weapons) per-cycle allocation reduced to O(pawns × weapons) once per pass.
- **C-13 (defName-keyed work-type-rule lookup) — COMPLETE.** `ToolCache.Update` (:88) calls `EquipmentManager.GetWorkTypeRuleByDefName(...)`, backed by a `Dictionary<string, WorkTypeThingRule>` built once per rule-list version (`EquipmentManagerGameComponent_WorkTypes.cs:43-56`) and invalidated on add (:22), delete (:29), and load/`ExposeData` (:35). The former O(workTypes × rules) `FirstOrDefault` linear scan per tool cache update is eliminated.
- **C-8 (prune cost) — ACCEPTABLE.** `PruneDestroyedThingCaches` (`EquipmentManagerGameComponent.cs:48-68`) runs once per assignment pass (`MapComponentTick` :243), behind the 6-hour debounce. It is a single O(n) scan over each of the three per-`Thing` cache dictionaries, allocating only the small destroyed-key lists. Def-keyed caches are correctly left unpruned (bounded by `ThingDef` count, documented :44-46). Bounds the previously unbounded session growth; cost is negligible at the gated cadence.
- **C-3 (no uncached hot recompute) — RESOLVED.** `ToolCache.GetStatValue` (:40-55) now computes work-type-dependent stats on demand and explicitly bypasses the `StatDef`-only `StatValues` cache for `ToolStat.WorkType` (:42-46), removing the stale-key correctness/perf hazard. Non-work-type stats remain cached. The per-tool `_workTypeScores` table is precomputed once per 24h cache window in `Update` (:84-91); `GetWorkTypesScore` is then a dictionary-backed `Average` — cheap, no hot recompute.

## New per-frame allocations

None on the per-frame path. The entire pipeline is gated by `IsPlayerHome`, paused-check, `TicksGame % 60`, and a 6-in-game-hour debounce (`MapComponentTick` :234-238), so all allocations occur at most once per 6 in-game hours, not per frame. (One transient array literal at `:452`, `new[] { JobDefOf.Equip, SidearmsDefOf.EquipSecondary }`, sits inside the gated `AllAvailable` loop — a micro-allocation well below the per-frame threshold and below the MEDIUM floor; noted for awareness only.)

## Verdict
APPROVE

## Next action
Performance dimension satisfied for impl-review iteration 2. No creator action required for performance.
