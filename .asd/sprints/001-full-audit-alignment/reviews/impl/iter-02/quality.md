[REVIEW-impl-quality]: CONCERNS

# Review — quality

- **Phase**: impl-review
- **Iteration**: 2

## Scope note

Severity floor = MEDIUM (low-severity findings dropped). Reviewed current state of
`Source/EquipmentManager/` and `Source/EquipmentManager.Tests/` for bugs, security,
best-practice, and contract drift, focused on: nullable correctness (Scribe `T?` +
guards), defect fixes C-3/C-6/C-8, C-12 perf-fix correctness (incl. sidearm passes
hoisting `assignedByOthers`), C-13 dictionary lookup, Logger routing, and Common
public-surface consumption; plus test meaningfulness / false coverage.

DEFERRED items C-10/C-14/C-15 were not assessed as findings, per instruction.

## Production-code verification (no findings)

The focus production paths are correct and clean:

- **C-3 (tool-cache key contract):** `ToolCache.GetStatValue` (`ToolCache.cs:40-55`)
  now computes WorkType-dependent stats on demand and bypasses the `StatDef`-only
  cache entirely; `_workTypeScores` is rebuilt every `Update` window (`:79-100`).
  Honors ADR-0004's "do not cache a value whose inputs are not all in the key."
- **C-6 (RangedWeaponCache band reset):** accuracy/DPSA bands are zeroed at the top of
  `Update` before recompute (`RangedWeaponCache.cs:247-248`), closing the stale-band
  carryover.
- **C-8 / Scribe nullability:** Scribe collection fields are `T?` with `??=` restore in
  `Initialize()` (`ItemRule.cs:24-39,166-179`); CE delegate fields stay nullable with
  guards (`RangedWeaponCache.cs:17,36,341`; AC-14 honored). No NRE-prone read of a
  Scribe field observed.
- **C-12 (assignedByOthers hoisting):** the set is built once per pass and mutated
  incrementally (`EquipmentManagerMapComponent.cs:386,489,510,592`); the per-pawn
  `SelectMany` rebuild is gone, including in the sidearm passes
  (`UpdateRangedSidearms`/`UpdateMeleeSidearms`) which now `.Add(...)` after each
  assignment (`:417,440,541,566`).
- **C-13 (defName dictionary lookup):** `GetWorkTypeRuleByDefName` builds a keyed dict
  once and invalidates on add/delete/expose (`EquipmentManagerGameComponent_WorkTypes.cs:14,22,29,35,43-56`).
- **Logger routing (AC-21/22):** no raw `Verse.Log.*` or `"Equipment Manager: "`
  literals remain; all diagnostics route through the EM `Logger` wrapper, which injects
  `ModId` into `Common.Logger` (`Logger.cs`). No JetBrains nullability attributes remain.
- **Common consumption:** caches re-base on `Common.ThingCache`; `StatHelper`,
  `WorkTypeThingRule`, `WorkTypeStatMap`, `StatRanges` consumed via public surface; no
  forking observed.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | high | `Source/EquipmentManager.Tests/` (no test exists; cf. `WorkTypeThingRuleTests.cs`, `ToolCache.cs:40-55`) | The C-3 tool-cache characterization test required by AC-29 and mandated as a discipline by ADR-0004 ("the differing-work-type-set assertion … is a required test") does NOT exist. No test exercises `ToolCache.GetStatValue` with two different `workTypeDefs` sets to assert they yield different scores. ADR-0004 makes a before/after characterization test contractually required for any fix that shifts scoring output (C-3 does). `ToolCache` is even pre-registered in `StateIsolationTestBase.cs:25`, confirming the slot was prepared but never filled — this is a gap, not an intentional omission. The fix's behavior is therefore uncharacterized; a future regression reintroducing WorkType caching would pass the suite silently. | Add a test that drives the WorkType-dependent stat through `ToolCache.GetStatValue` (or its computed path) with two distinct work-type sets and asserts distinct per-set scores, characterizing the C-3 fix. If a live-game `ToolCache` instance is genuinely un-constructible in unit context, characterize the underlying `GetWorkTypesScore` math directly rather than skipping the assertion. |
| 2 | medium | `ItemRuleAndLoadoutTests.cs:196-230` (`RangedWeaponRule_Copy_…`), `:237-271` (`MeleeWeaponRule_Copy_…`), `:278-304` (`ToolRule_Copy_…`) | False coverage. All three "Copy deep-copies" tests never invoke the production copy methods (`EquipmentManager.CopyRangedWeaponRule` / `CopyMeleeWeaponRule` / `CopyToolRule`, defined in the `EquipmentManagerGameComponent_*Rules.cs` files). They "manually replicate the copy logic" — constructing a second rule and hand-assigning two scalar fields — then assert the two independently-built objects are independent, which is structurally guaranteed and proves nothing about the real copy. AC-29's "*Rule.CopyX deep-copy completeness" is thus unmet: a regression in a Copy method (shared collection reference, dropped field) would not be caught. The doc-comments claim to test `RangedWeaponRule.Copy` etc., compounding the false signal. | Rewrite each test to call the actual `EquipmentManager.Copy*Rule(original)` method, then mutate the original's collections/fields and assert the returned copy is unaffected (verifying deep, not shallow, copy). If invoking the game-component copy requires game context, characterize the rule-level copy logic that the component delegates to; do not assert independence of two hand-built objects. |

## Verdict
CONCERNS: 2

## Next action
Route back to `impl` (fix mode). Test Engineer adds the missing C-3 tool-cache
characterization test (finding #1) and rewrites the three Copy tests to exercise the
real copy methods (finding #2). Both are autofixable without escalation — no concept,
contract, or scope change. Sprint re-enters impl-review afterward.
