[REVIEW-impl-external]: CONCERNS

# External Review Report

- **Phase**: impl-review
- **Iteration**: 1
- **Severity floor (this iter)**: low
- **External tool**: Codex CLI (codex-cli 0.130.0, model gpt-5.5) — AVAILABLE
- **Payload**: focused impl diff `git diff 1d931b5...HEAD`, split into two Codex calls. Call A — caches (ToolCache C-3, RangedWeaponCache C-6, MeleeWeaponCache), RU-7 widget glue (UiHelpers), nullable Scribe data/rule files (ItemRule, Loadout, PawnLoadout, PassionLimit, SkillWeight, PawnCapacityWeight). Call B — EquipmentManagerMapComponent (C-12) and test files.
- **Call A verdict**: APPROVE (no findings on caches, widget glue, or nullable Scribe migration).
- **Call B verdict**: CONCERNS (3 findings, all in test files).

## Kept findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | Source/EquipmentManager.Tests/StateIsolationTestBase.cs:39 (RestoreState / SnapshotState `if (field != null)`) | State-isolation snapshot/restore silently skips any `CachingTypes` entry whose private static `_equipmentManager` field is absent (renamed, removed, or mistyped). A misconfigured caching type would then leak process-global state across test cases, making the suite order-dependent without any failure signal. Confirmed against current source — both SetUp and TearDown use a null-tolerant guard. | Fail-fast when an expected caching type lacks `_equipmentManager` (e.g. throw/assert if `field == null` for a type listed in `CachingTypes`), or explicitly document and narrow the list to types where a missing field is intentional. |
| 2 | medium | Source/EquipmentManager.Tests/ItemRuleAndLoadoutTests.cs:67,80,131,143,153 | Five `[Test]` methods labeled as AC-27/AC-29 coverage (`Loadout_Initialize_CoalescesNullCollections`, `Loadout_Initialize_NormalizesLegacyStatNames`, `Loadout_PrimaryRuleType_SetToNone_ClearsWeaponRules`, `_SetToRanged_ClearsMeleeOnly`, `_SetToMelee_ClearsRangedOnly`) have empty bodies (comments only, no assertions, no exercised behavior). They pass unconditionally and would not catch regressions in Loadout initialization, null-coalescing, legacy-name normalization, or PrimaryRuleType clearing. Confirmed against current source. | Replace with executable assertions using a properly initialized fixture/context, or remove the `[Test]` attribute and relocate the intent to manual-verification-spec so the suite does not report false coverage. |
| 3 | medium | Source/EquipmentManager.Tests/WorkTypeThingRuleTests.cs:46 (`WorkTypeThingRule_ToolRuleIntegration_ConsumesWorkTypeStatMap`) | Test claims to cover the ToolRule → WorkTypeStatMap consumption path but contains no assertion and never instantiates or exercises `ToolRule`; it cannot catch the integration defect it documents. (Sibling test `_OQ1Decision_ConsumesCommonPerStatWeights` at least asserts `AutoSwitchStatsMap` non-null.) Confirmed against current source. | Exercise the `ToolRule` path directly with an assertion, or convert this to non-test documentation outside NUnit so it is not counted as automated coverage. |

## Dropped findings (below severity floor)

None. Iteration 1 floor is low; all findings are at or above low.

## Dropped findings (nitpick)

None. No findings fell into anti-nitpick categories (wording/style/speculative/reformatting).

## Verdict
CONCERNS: 3

## Next action
PM/Test Engineer to address the three test-quality findings: (1) make StateIsolationTestBase fail-fast on a missing caching-type field; (2) give the five empty Loadout `[Test]` methods real assertions or demote them out of the automated suite; (3) make the ToolRule integration test actually exercise `ToolRule` or relocate it to manual-verification-spec. Production code under review (caches incl. C-3 composite key and C-6 reset, C-12 MapComponent, RU-7 widget glue, and the nullable Scribe migration) drew no external findings (Call A APPROVE; Call B production scope clean). DEFERRED items C-10/C-14/C-15 were excluded from review by instruction.
