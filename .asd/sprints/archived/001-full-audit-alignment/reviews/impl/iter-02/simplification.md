[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 02

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Scope verification (no new abstraction introduced by the fixes)

| Fix | Location | Verdict | Notes |
|---|---|---|---|
| C-12 sidearm hoist | `EquipmentManagerMapComponent.cs` `AssignAllTools`/`AssignBestTool`/`AssignToolsForWorkTypes`/`AssignPrimary*`/`Update*Sidearms` | keep-as-is | The already-carried path branches inline on `carriedWeapons.Contains(...)` to call `InformOfAddedSidearm`/`SetMeleeWeaponTypeAsPreferred`/`SetRangedWeaponTypeAsDefault` instead of queueing an `EquipSecondary` job. No new helper, layer, interface, or flag added — guard logic added inline at each call site. Complexity earns its weight (skips a redundant job for weapons the pawn already holds). |
| C-8 prune | `EquipmentManagerGameComponent.cs:48` `PruneDestroyedThingCaches` | keep-as-is | Three near-identical inline blocks over the three Thing-keyed caches; no extracted generic/helper introduced. Repeating three short loops is below the extraction threshold and avoids a generic-over-one-shape abstraction. Doc comment states the contract (why def-keyed caches are not pruned) — it explains intent, does not restate code. Not a comment-restates-code hit. |
| C-11 virtual method | `ItemRule.cs:137` `GetDefaultStatWeights` (virtual) + overrides `MeleeWeaponRule.cs:137`, `RangedWeaponRule.cs:175`, `ToolRule.cs:104` | keep-as-is | Genuine polymorphic dispatch: three concrete overrides, each calling `base.GetDefaultStatWeights()`, plus direct base callers in the rule-default builders. Not "abstraction with no second use case" and not a one-implementer virtual — clears the over-engineering checklist. |
| StateIsolationTestBase fail-loud guard | `StateIsolationTestBase.cs:39,62` | keep-as-is | Reflection guard throws `InvalidOperationException` when the `_equipmentManager` static field is renamed/removed, replacing a silent skip. No mock (let alone mock-of-mock), no new test abstraction. The SetUp/TearDown guard blocks are duplicated but each is ~6 lines and inline; extracting a private helper here would be churn, not simplification — the duplication is acceptable and below the extraction bar. |
| C-10 (assignment-helper extraction) | n/a | keep-as-is | Correctly DEFERRED per ADR OQ-3 / R-C5 (new layer needs Complication Approval). Pre-existing assignment-path duplication across `Update*Sidearms`/`Assign*` is NOT flagged — it is existing duplication, out of scope this iteration. |

## Dead-code check (post-deletion)

- No orphaned references to the ADR-0002–deleted types: `WorkTypeRule`, `ItemCache`, `EquipmentManagerStatDefs`, the local `StatRanges` component, or `LegacyCustomStatDefs.NormalizeStatRanges`. All `WorkType*` references resolve to Common's `WorkTypeThingRule` or the local `WorkTypeRules` collection/accessors; `GetWindowSize` now routes to `LordKuper.Common.UI.Windows.GetWindowSize` (RU-6).
- `LegacyCustomStatDefs.cs` remainder is legitimate EM-side legacy-name adaptation (C-23 PASS), not dead code; `NormalizeStatRanges` is gone as designed.
- No "in case we need it" leftover code observed in the touched files.

## Verdict
APPROVE

## Next action
None. No findings at or above the MEDIUM floor; over-engineering checklist clear. Reviewer done.

## Escalations (optional)
- None.
