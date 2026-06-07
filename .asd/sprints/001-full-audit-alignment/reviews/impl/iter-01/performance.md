---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: CONCERNS

# Review — performance

- **Phase**: impl-review
- **Iteration**: 1

## Scope & budgets

No explicit numeric perf budgets (latency/memory/throughput thresholds) are defined in
`.asd/project/custom-coding-rules.md`. The governing perf policy is ADR-0004 plus the audit's
perf findings C-12/C-13 (and the perf-adjacent C-3, C-8). Hot paths are the assignment pipeline
(`MapComponentTick`, every 60 ticks + 6 in-game-hour debounce), the per-Thing scoring caches, and
the settings-dialog draw loop. Findings below are assessed against those policy targets and against
"no new per-frame/per-tick regression".

## Verification results (per requested item)

- **C-12 / AC-19 (incremental `assignedByOthers`) — PASS for the in-scope passes.** The per-pawn
  rebuild is gone from the primary and tool passes: `UpdatePrimaryWeapons`
  (`EquipmentManagerMapComponent.cs:490`) and `UpdateTools` (`:594`) each build the set **once**
  before the loop and pass it into `AssignPrimaryRangedWeapon`/`AssignPrimaryMeleeWeapon`/
  `AssignAllTools`/`AssignBestTool`/`AssignToolsForWorkTypes`, which mutate it incrementally via
  `assignedByOthers.Add(...)` (`:49, :93, :130, :170, :216`). No `_pawnCache.Values.SelectMany(...)`
  rebuild remains in any of those five methods. The O(pawns²×weapons) per-pawn rebuild they
  previously incurred is eliminated. (Residual: the two sidearm passes still rebuild per pawn —
  finding #1.)

- **C-13 / AC-20 (defName-keyed rule lookup) — PASS.** The per-iteration `FirstOrDefault` linear
  scan in `ToolCache.Update` is replaced by `EquipmentManager.GetWorkTypeRuleByDefName(...)`
  (`ToolCache.cs:88`), backed by a `Dictionary<string, WorkTypeThingRule>`
  (`EquipmentManagerGameComponent_WorkTypes.cs:43-56`) built once per rule-list version. The cache
  is correctly invalidated to `null` on every mutation: `AddWorkTypeRule` (`:22`),
  `DeleteWorkTypeRule` (`:29`), `ExposeData_WorkTypes` (`:35`), and `GetWorkTypeRules` default-seed
  (`:63`). Lookup is now O(1) per work type instead of O(rules).

- **C-8 (prune cost vs benefit) — PASS, no regression.** `PruneDestroyedThingCaches`
  (`EquipmentManagerGameComponent.cs:48-68`) is called exactly once per assignment pass from
  `MapComponentTick` (`:243`), which is already gated by `IsPlayerHome`, not-paused, `%60` ticks,
  and the 6-in-game-hour debounce. It is an O(n) scan over only the three **Thing-keyed** caches
  (`_rangedWeaponsCache`, `_meleeWeaponsCache`, `_toolCache`); the ThingDef-keyed caches (bounded by
  def count) are correctly excluded. Cost is trivial relative to the scoring pass it precedes and
  runs at most once per 6 in-game hours — it is not an every-frame scan and does not regress perf.
  It bounds the previously unbounded session growth (R-C6). Good cost/benefit.

- **C-3 (WorkType composite-key / cache bypass) — PASS, not an uncached hot recompute.**
  `ToolCache.GetStatValue` (`ToolCache.cs:40-55`) carves WorkType-dependent stats out of the
  `StatValues` cache and computes them on demand (`:44-46`). This is acceptable and does NOT turn a
  hot path into a per-call recompute of the expensive scoring: the costly part
  (`workTypeRule.GetThingScore(Thing)` per work type) is precomputed once per 24h cache window into
  `_workTypeScores` inside `Update` (`:84-91`, gated by `base.Update(time)` returning false when not
  due). The per-call path (`GetWorkTypesScore` → `Average(GetWorkTypeScore)`, `:69-72`) only averages
  pre-cached dictionary lookups over the small active work-type set. So the bypass adds only a cheap
  averaging per call, not a recompute. Correct tradeoff per ADR-0004.

- **No new per-frame allocations in UI/tick paths — PASS (with a pre-existing-idiom note).** RU-7's
  on-map list (`_currentlyAvailableMapThings`, `_globallyAvailableWorkTypes` in
  `ManageWeaponRulesDialog_WorkTypes.cs:13-14`) is rebuilt only in `UpdateAvailableItems_WorkTypes`
  (`:39-52`), invoked from the `SelectedWorkTypeRule` setter (`:26`) and passed as the refresh
  callback to `DoWidgetTab` (`:34`) — i.e. on select/refresh, NOT every draw frame. The
  map-`ThingsInGroup` scan and `OrderByDescending` only run on refresh. The one per-frame allocation
  in `DoTab_WorkTypes` (`GetWorkTypeRules().ToList()`, `:33`) is consistent with the pervasive
  immediate-mode IMGUI idiom already used across every other rule tab
  (`ManageWeaponRulesDialog_Tools.cs:40, 51, 62, 118, 185` etc.), runs only while the settings
  dialog is open (never on map tick), and operates on a tiny rule count — not a new hot-path
  regression.

- **ThingCache rebase (RU-3) — PASS, no regression vs old ItemCache.** `ItemCache.cs` is deleted;
  `ToolCache` now extends `LordKuper.Common.ThingCache(thing, 24f)` (`ToolCache.cs:12`). The caching
  contract is preserved: `GetToolCache`/`GetToolDefCache` (`EquipmentManagerGameComponent_ToolRules.cs:84-107`)
  are TryGetValue-cached and call `cache.Update(time)`, which short-circuits via `base.Update(time)`
  on the same 24h interval the old `ItemCache : TimedCache` used. No additional per-call work was
  introduced by the rebase.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | `EquipmentManagerMapComponent.cs:388` (`UpdateMeleeSidearms`), `:513` (`UpdateRangedSidearms`) | Both sidearm passes still rebuild the base `assignedByOthers` set **per pawn** via `new HashSet<Thing>(_pawnCache.Values.Where(pc => pc != pawn).SelectMany(pc => pc.AssignedWeapons.Keys))` — O(pawns²×weapons) allocation, the same anti-pattern C-12 removed from the primary/tool passes. AC-19's literal scope was the primary/tool passes (now fixed), but the same O(pawns²) cost survives in these two passes, partially undercutting the C-12 win on a colony with many pawns. | Compute the full assigned set once before the pawn loop and pass it in, deriving each pawn's "others" view by excluding that pawn's own `AssignedWeapons.Keys` (or maintain a single running set and Add as you go, mirroring `UpdatePrimaryWeapons`/`UpdateTools`). The passes already `Add(...)` incrementally, so only the per-pawn base rebuild needs hoisting. |

## Verdict
CONCERNS: 1

## Next action
impl-review routes the sprint back to `impl` (fix mode); the responsible dev hoists the per-pawn
`assignedByOthers` rebuild out of `UpdateMeleeSidearms`/`UpdateRangedSidearms` to complete the C-12
pattern across all assignment passes. No escalation required (mechanical optimization, no new
abstraction, no contract change). All other requested verifications PASS. Re-enter impl-review.
