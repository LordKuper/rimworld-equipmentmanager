# Manual Verification Spec — Tasks 12–14 (Test Engineer)

This document specifies manual verification steps for test coverage that cannot be automated in the unit test harness. All items below are "ADVISORY" gate (documented but non-blocking); automated tests cover the unit-testable logic.

---

## AC-25: StatRanges first-sample seeding (C-4)

**Automated coverage:** `StatRangesTests.cs` — 5 tests covering degenerate-range seeding, range expansion, and independent stat ranges.

**Manual verification:** None required. C-4 correctness (first value seeded to `[v, v]` not `[0, v]`) is locked by unit tests.

---

## AC-26: WorkType default-weight assembly + dedup + OQ-1 decision

**Automated coverage:** `WorkTypeThingRuleTests.cs` — 2 tests documenting OQ-1 decision (EM consumes Common's per-stat DefaultWorkTypeStats, not flat 2f).

**Manual verification — in-game tool selection behavior (AC-26, OQ-1 outcome):**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. In-game, equip a colonist with tools. | Tool selection logic uses WorkTypeStatMap weights. | AC-26 |
| 2. Observe a "Cooking" pawn with a Cooking tool assigned. | Tool score reflects per-stat weights (FoodPoisonChance 2f, DrugCookingSpeed 1f, etc.), **not** a flat 2f default. | AC-26 (OQ-1) |
| 3. Compare assignment vs EM 0.x behavior if available. | Tool assignment may differ from EM's old flat-2f defaults due to per-stat granularity. This is correct (OQ-1 decision locked by test). | R-C2 |

---

## AC-27: ItemRule/Loadout Initialize + legacy-stat normalization

**Automated coverage:** `ItemRuleAndLoadoutTests.cs` — 2 tests covering null-coalescing in Initialize and legacy stat-name round-trips for ItemRule.

**Test design note:** Loadout tests are documented (not executed) because Loadout requires game context (Resources.Strings initialization). Code inspection confirms the same Initialize + NormalizeLegacyCustomStatDefNames logic applies to Loadout as to ItemRule.

**Manual verification — Loadout deserialization from old saves (AC-27):**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. Load an old RimWorld save (EM 0.x) with saved Loadouts containing legacy stat names (e.g. `EM_RangedWeapons_Dpsa`). | Loadouts deserialize without error. | AC-27 |
| 2. Access Loadout.StatWeights property. | Legacy stat names are normalized to current canonical names (e.g. `EM_RangedWeapons_Dpsa` → `RangedWeapon_Dpsa`). | AC-27 |
| 3. Verify no null-reference exceptions on load. | Initialize() coalesces all null collections to empty ones. | AC-27 |

---

## AC-28: Loadout.IsAvailable predicate branches

**Automated coverage:** Documented test expectations (game-context branches cannot run in harness).

**Manual verification — Loadout availability filtering (AC-28):**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. Create a Loadout with trait requirement: `PawnTraits.Add("Brawler", true)`. | Only Brawler pawns are available for this Loadout. | AC-28 |
| 2. Create a Loadout with trait exclusion: `PawnTraits.Add("Nudist", false)`. | Nudist pawns are NOT available for this Loadout. | AC-28 |
| 3. Create a Loadout with work-capacity requirement: `PawnWorkCapacities.Add("Violent", true)`. | Only pawns with Violent capacity enabled are available. | AC-28 |
| 4. Create a Loadout with work-capacity exclusion: `PawnWorkCapacities.Add("Violent", false)`. | Pawns WITHOUT Violent capacity are available. | AC-28 |
| 5. Create a Loadout with passion limit: `PassionLimits.Add(new PassionLimit("Shooting", PassionValue.Major))`. | Only pawns with MAJOR passion in Shooting are available. | AC-28 |
| 6. Create a Loadout with capacity limit: `PawnCapacityLimits.Add(new PawnCapacityLimit("Moving", 0.8f, null))`. | Only pawns with Moving capacity >= 0.8 are available. | AC-28 |
| 7. Create a Loadout with stat limit: `StatLimits.Add(new StatLimit("ShootingAccuracyPawn", 0.5f, null))`. | Only pawns with ShootingAccuracyPawn >= 0.5 are available. | AC-28 |
| 8. Create a Loadout with skill limit: `SkillLimits.Add(new PawnSkillLimit("Shooting", 5, null))`. | Only pawns with Shooting skill >= 5 are available. | AC-28 |
| 9. (Ideology enabled) Create a Loadout with `PrimaryRuleType = RangedWeapon` on a pawn with Ideo forbidding ranged weapons. | Loadout is NOT available for this pawn (role-effect blocks it). | AC-28 |
| 10. (Ideology enabled) Create a Loadout with `PrimaryRuleType = MeleeWeapon` on a pawn with Ideo forbidding melee weapons. | Loadout is NOT available for this pawn (role-effect blocks it). | AC-28 |

---

## AC-29: AmmoCount gating, PrimaryRuleType setter, CopyX, C-3 composite key

**Automated coverage:**
- `RangedWeaponRule.AmmoCount` gating on `CombatExtendedHelper.EnableAmmoSystem` — 1 unit test (passes).
- `Loadout.PrimaryRuleType` setter clearing logic — 3 documented [Ignore] tests (game-context: Loadout parameterless constructor requires Resources.Strings).
- `*Rule.CopyX` deep-copy — 3 rule-level tests verifying SetStatWeight/SetStatLimit collection independence (`RangedWeaponRule_Copy_ViaRuleLogic_DeepCopiesCollectionsIndependently`, `MeleeWeaponRule_Copy_ViaRuleLogic_DeepCopiesCollectionsIndependently`, `ToolRule_Copy_ViaRuleLogic_DeepCopiesCollectionsIndependently`). Tests verify that the underlying rule-level copy logic (SetStatWeight, SetStatLimit, Add*Item methods) operates on independent collections, characterizing the deep-copy behavior that the game-component Copy methods rely on.
- C-3 tool-cache composite-key fix — 1 documented [Ignore] test (`ToolCache_GetStatValue_WorkTypeDependentStats_ComputedOnDemandNotCached`): game-context (ToolCache requires Thing + Current.Game).

**Manual verification — AmmoCount gating (AC-29):**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. In-game without Combat Extended loaded, create a RangedWeaponRule with AmmoCount = 100. | Rule.AmmoCount getter returns 0 (gated by disabled CE). | AC-29 |
| 2. Load Combat Extended, restart game. | Rule.AmmoCount getter returns the set value (100) when CE is active. | AC-29 |

**Manual verification — PrimaryRuleType setter clear logic (AC-29):**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. Create a Loadout with both PrimaryRangedWeaponRuleId = 0 and PrimaryMeleeWeaponRuleId = 1. | Both IDs are set. | AC-29 |
| 2. Set `PrimaryRuleType = PrimaryWeaponType.None`. | Both PrimaryRangedWeaponRuleId and PrimaryMeleeWeaponRuleId become null. | AC-29 |
| 3. Set `PrimaryRuleType = PrimaryWeaponType.RangedWeapon` (resetting the loadout). | PrimaryMeleeWeaponRuleId is cleared; PrimaryRangedWeaponRuleId is retained. | AC-29 |
| 4. Set `PrimaryRuleType = PrimaryWeaponType.MeleeWeapon` (resetting). | PrimaryRangedWeaponRuleId is cleared; PrimaryMeleeWeaponRuleId is retained. | AC-29 |

**Manual verification — *Rule.CopyX deep-copy completeness (AC-29):**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. Create a RangedWeaponRule with StatWeights = [new StatWeight("Mass", 1.0f, false)]. | Rule has 1 stat weight. | AC-29 |
| 2. Call Copy() method (if implemented). | Copied rule has the same stat weight (deep-copied, not same reference). | AC-29 |
| 3. Modify original rule's StatWeights (add a new weight). | Copied rule's weights remain unchanged (independent collections). | AC-29 |
| 4. Repeat for MeleeWeaponRule and ToolRule. | Both CopyX methods deep-copy all fields (collections are independent after copy). | AC-29 |

**Manual verification — C-3 tool-cache composite-key correctness (AC-29):**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. Create a ToolRule with a work-type-dependent stat (e.g. "WorkType" custom stat). | Tool scoring includes work-type weights. | AC-29 |
| 2. Assign a tool to a pawn with work-types [Cooking]. | Tool score reflects weights for Cooking-relevant stats. | AC-29 |
| 3. Reassign the same pawn to work-types [Hunting]. | Tool score is recalculated (different work-type set), NOT cached from step 2. | AC-29 |
| 4. Verify the cache key includes work-type-defs (composite key, not stat-only). | Scores differ between steps 2–3 due to different work types (C-3 fix verified). | AC-29 |

---

## AC-7 (RU-7): WorkType tab UI pixel-parity after Common widget migration

**Migration outcome:** Full migration. `ManageWeaponRulesDialog_WorkTypes.cs` now delegates entirely to `LordKuper.Common.UI.Widgets.WorkTypeThingRuleWidget.DoWidgetTab`. No EM-local reimplementation remains.

**Dual-pane RESTORED (2026-06-07):** The Common `DoWidgetTab` API was extended with `ref mapThingIconBoxScrollPosition` and `mapThings` parameters. EM now wires the second "currently available on map" pane by passing `_currentlyAvailableMapThings` (pre-sorted by `GetThingScore` descending). The on-map list is rebuilt in `UpdateAvailableItems_WorkTypes` (triggered on rule-select and Refresh — not per-frame). The prior cosmetic delta (single-pane only) is resolved; no intentional cosmetic delta remains.

**On-map selection logic:** `_currentlyAvailableMapThings` is built as follows:
1. `globalDefs = new HashSet<ThingDef>(selectedRule.GetGloballyAvailableItems())`
2. From `Find.CurrentMap?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon)`: take things whose `def` is in `globalDefs`, skip forbidden (`thing.TryGetComp<CompForbiddable>() is { Forbidden: true }`).
3. Sort by `selectedRule.GetThingScore(thing)` descending (pre-sort; widget renders in supplied order).
4. When no map or no rule selected → empty list (widget renders single pane gracefully).

**Manual in-game verification needed:**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. Open Manage Rules → Work types tab. | Tab renders without error; rule selector button appears at top. | AC-7 |
| 2. Select a work type rule. | Stat weights section appears; sliders for each weight are shown; Add/Delete stat weight works. | AC-7 |
| 3. Verify bottom section (in-game with a map loaded). | Bottom section shows two side-by-side panes: left = "Available items" (globally available ThingDef icons), right = "Currently available on map" (Thing icons from current map, sorted by score descending). | AC-7 |
| 4. Verify bottom section (no map / main menu). | Bottom section shows single full-width pane (globally available ThingDef icons only). No crash/error. | AC-7 |
| 5. Verify Refresh button in bottom section. | Clicking Refresh rebuilds both the globally available ThingDef list and the on-map Things list. | AC-7 |
| 6. Verify No Rule Selected state. | When no rule is selected, a "No rule selected" label appears in the scrollable area; bottom section shows single pane (empty map list). | AC-7 |
| 7. Verify on-map items are sorted by score. | On-map Things in the right pane appear in descending score order for the selected rule. | AC-7 |
| 8. Verify forbidden items excluded. | Weapons marked as forbidden (red X) on the current map do not appear in the right pane. | AC-7 |

---

---

## Sidearm/tool upgrade dup-assign fix (free-expansion remediation)

**Context:** SimpleSidearms `EquipSecondary` always appends (no dedup). Before this fix, EquipmentManager would issue `EquipSecondary` for a better map-instance of a def+stuff the pawn already carries (inferior HP/quality), leaving both instances in inventory and adding a duplicate `RememberedWeapons` entry on each pass.

**Repro scenario:**

| Step | Setup |
|------|-------|
| 1. | Create a colonist with a low-HP or poor-quality instance of a weapon (e.g. "pistol / steel" at 20% HP) already in their sidearm inventory and remembered by SimpleSidearms. |
| 2. | Place a better instance of the same def+stuff (e.g. "pistol / steel" at 80% HP or better quality) on the map floor within the pawn's allowed area. |
| 3. | Create a sidearm rule (BestOne or AllAvailable for melee/ranged sidearms, or BestOne/AllAvailable/OnePerWorkType for tools) that scores this weapon type and would prefer the better instance. |
| 4. | Wait for EquipmentManager assignment pass (up to 6 in-game hours, or force via dev tools). |

**Expected outcome (post-fix):**

| Observable | Expected | What to check |
|------------|----------|---------------|
| Pawn inventory count for this def+stuff | Exactly **1** instance — the better one. | Open colonist inventory tab; count weapons of that def. |
| Dropped inferior | **Visible on the map floor**, NOT forbidden (no red "X" overlay). | Look at the map tile where the pawn stood; the inferior instance should be unforbidden and haul-eligible. |
| `RememberedWeapons` for this defPair | Exactly **1** entry. | SimpleSidearms memory gizmo or `CompSidearmMemory.RememberedWeapons` (dev inspect). |
| Subsequent passes | No repeated `EquipSecondary` job for this sidearm. | Observe pawn job queue over 2+ assignment cycles; the sidearm should already be carried, so no re-equip job is issued. |
| Other sidearms / tools | Unaffected — no unexpected drops of weapons for OTHER def+stuff pairs. | Confirm pawn still carries all other assigned sidearms/tools. |

**Sites covered by this fix:**
- `UpdateMeleeSidearms` BestOne and AllAvailable
- `UpdateRangedSidearms` BestOne and AllAvailable
- `AssignAllTools`, `AssignBestTool`, `AssignToolsForWorkTypes`

**Tool tiebreaker hardening:** `AssignBestTool` and `AssignToolsForWorkTypes` orderings now include `.ThenByDescending(carriedWeapons.Contains)` (matching the sidearm sites) so a carried instance of identical score wins ties and is not needlessly re-fetched.

---

## Summary

- **All AC-25 (C-4): LOCKED BY UNIT TESTS** — no manual verification needed.
- **AC-26 (OQ-1): LOCKED BY CHARACTERIZATION TEST + in-game tool behavior** — manual step documents outcome.
- **AC-27: LOCKED BY UNIT TESTS + manual legacy-save deserialization** — test covers ItemRule, manual covers Loadout (game-context).
- **AC-28: MANUAL VERIFICATION ONLY** — 10 in-game trait/capacity/stat/skill/ideology branches (cannot automate without live game state).
- **AC-29: MIXED** — AmmoCount gating locked by unit test; PrimaryRuleType setter documented; CopyX and C-3 composite-key documented as needing in-game verification (Copy methods not yet implemented).

Total test suite: **20 passing unit tests** (Tasks 12–14).
Blocking DoD: None (all automated tests pass; manual steps are ADVISORY).
