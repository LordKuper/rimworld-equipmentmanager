---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-impl-performance]: APPROVE

# Review — performance

- **Phase**: impl-review
- **Iteration**: 03

## Scope

Hot-path performance and regressions only. Severity floor = HIGH (iter 03 admits high/critical
exclusively; low/medium dropped per `review-policy.md`). Hot path = the equipment-update pass in
`EquipmentManagerMapComponent.MapComponentTick` (gated to every 60th tick and further throttled to
a ≥6-hour map-time interval) plus the rule/cache lookups it drives. Re-confirmation of the four
targeted remediation fixes (C-12, C-13, C-8, C-3) verified in prior iterations.

`.asd/project/custom-coding-rules.md` defines **no numeric performance budgets** (latency / memory /
throughput) — budget-compliance findings are therefore N/A; this review enforces only hot-path
anti-patterns and regressions.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above the HIGH floor | — |

## Re-confirmation of targeted fixes (no high-severity regression)

- **C-12 — incremental `assignedByOthers` set.** Each assignment phase (`UpdatePrimaryWeapons` :489, `UpdateRangedSidearms` :510, `UpdateMeleeSidearms` :386, `UpdateTools` :592) builds one `HashSet<Thing>` from already-assigned weapons; subsequent `Contains` / `Add` are O(1). No per-candidate linear rescans. No regression.
- **C-13 — defName-keyed work-type rule lookup.** `EquipmentManagerGameComponent_WorkTypes.GetWorkTypeRuleByDefName` (:43) uses a lazily-built `Dictionary<string, WorkTypeThingRule>` with correct invalidation (nulled on `AddWorkTypeRule` :22, `DeleteWorkTypeRule` :29, `ExposeData_WorkTypes` :35). Called per work-type from `ToolCache.Update` (:88) — O(1) per lookup instead of the prior linear scan. No regression.
- **C-3 — work-type-dependent tool stat.** `ToolCache.GetStatValue` (:40) correctly bypasses the `StatDef`-only `StatValues` cache for `ToolStat.WorkType` and computes on demand (:46), while non-work-type stats remain cached (:47-53). The stale-score class is closed at the cache-contract level without forfeiting the cache hit for cacheable stats. Matches ADR-0004 decision 1. No regression.
- **C-8 — rule/cache lookups.** Tool caches are dictionary-backed by `Thing` / `ThingDef` (`GetToolCache` :84, `GetToolDefCache` :95). No linear scan on the hot path introduced. No regression.

No new hot-path anti-pattern (n+1, sync IO, unbounded allocation, quadratic-on-user-collection) was
introduced by these fixes. Allocation-heavy LINQ chains in the assignment methods are pre-existing
structure executed at most once per ~6 game-hours per colonist, not regressions from this sprint, and
fall below the iter-03 HIGH floor.

## Verdict
APPROVE

## Next action
Performance reviewer done for iter 03. No fixes required; no routing back to impl on perf grounds.
