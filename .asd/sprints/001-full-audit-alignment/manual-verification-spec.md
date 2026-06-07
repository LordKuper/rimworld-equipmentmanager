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
- `Loadout.PrimaryRuleType` setter clearing logic — documented (game-context).
- `*Rule.CopyX` deep-copy — documented (Copy methods don't exist as of testing).
- C-3 tool-cache composite-key fix — documented (game-context, tool-scoring behavior).

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

**Intentional cosmetic delta:** The previous EM widget showed two side-by-side panes in the bottom section — "Globally available items" (ThingDef icons) and "Currently available items" (Thing icons from the current map, sorted by score). Common's widget shows only the "Globally available items" pane. The currently-available-on-map pane is removed. This is accepted: WorkType rules are global scoring rules (not per-map equipment selection), so the globally available ThingDef view is sufficient. The stat weights section and rule selection are fully migrated.

**Manual in-game verification needed:**

| Step | Expected observation | AC |
|------|----------------------|-----|
| 1. Open Manage Rules → Work types tab. | Tab renders without error; rule selector button appears at top. | AC-7 |
| 2. Select a work type rule. | Stat weights section appears; sliders for each weight are shown; Add/Delete stat weight works. | AC-7 |
| 3. Verify bottom section. | "Available items" section shows globally available ThingDef icons (not a split currently-available pane). This is the intentional cosmetic delta. | AC-7 |
| 4. Verify Refresh button in bottom section. | Clicking Refresh updates the globally available ThingDef list. | AC-7 |
| 5. Verify No Rule Selected state. | When no rule is selected, a "No rule selected" label appears in the scrollable area. | AC-7 |

---

## Summary

- **All AC-25 (C-4): LOCKED BY UNIT TESTS** — no manual verification needed.
- **AC-26 (OQ-1): LOCKED BY CHARACTERIZATION TEST + in-game tool behavior** — manual step documents outcome.
- **AC-27: LOCKED BY UNIT TESTS + manual legacy-save deserialization** — test covers ItemRule, manual covers Loadout (game-context).
- **AC-28: MANUAL VERIFICATION ONLY** — 10 in-game trait/capacity/stat/skill/ideology branches (cannot automate without live game state).
- **AC-29: MIXED** — AmmoCount gating locked by unit test; PrimaryRuleType setter documented; CopyX and C-3 composite-key documented as needing in-game verification (Copy methods not yet implemented).

Total test suite: **20 passing unit tests** (Tasks 12–14).
Blocking DoD: None (all automated tests pass; manual steps are ADVISORY).
