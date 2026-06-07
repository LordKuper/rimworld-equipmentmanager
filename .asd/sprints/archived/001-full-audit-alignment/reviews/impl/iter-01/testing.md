[REVIEW-impl-testing]: CONCERNS

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 01

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | ItemRuleAndLoadoutTests.cs:160–173 | AC-29 CopyX deep-copy completeness: test documents the expectation but provides no actual assertions. Copy methods exist in EquipmentManagerGameComponent_RangedWeaponRules.cs:47–68, EquipmentManagerGameComponent_MeleeWeaponRules.cs:47–68, and EquipmentManagerGameComponent_ToolRules.cs:47–67 (CopyRangedWeaponRule, CopyMeleeWeaponRule, CopyToolRule respectively). These methods deep-copy all ItemRule fields (StatWeights, StatLimits, whitelisted/blacklisted items) and rule-specific fields (Explosive/ManualCast for RangedWeaponRule; UsableWithShields/Rottable for MeleeWeaponRule; Ranged for ToolRule). A meaningful unit test should verify that modifying the original rule's collections (e.g., adding a stat weight) does not affect the copied rule's collections, confirming independence. Current test is documentation-only. | Add unit test that constructs a rule, calls the corresponding Copy method (via component API if needed), then mutates the original rule's collection and asserts the copy is unaffected. |
| 2 | low | ItemRuleAndLoadoutTests.cs:67–87 | AC-28 Loadout.IsAvailable branches: test methods (lines 68, 81) are empty stubs with documentation only. The 10 branches of IsAvailable (Loadout.cs:405–469: ideology role effects, traits, work capacities, passion limits, pawn capacity limits, stat limits, skill limits) require live game context (DefDatabase, pawn.Ideo, etc.). This is correctly identified as manual-verification-only, but the test stubs invite confusion. | Remove or rename the empty test methods to clarify they are not executable tests (e.g., use `[Ignore("manual verification only")]` or convert to documentation comments). Alternatively, if they can be tested with minimal game context mocking, implement them. |
| 3 | low | ItemRuleAndLoadoutTests.cs:130–158 | AC-29 Loadout.PrimaryRuleType setter: test methods are empty stubs (lines 143, 147, 154, 157). The setter logic (Loadout.cs:227–245) is pure: it clears rule IDs based on the PrimaryWeaponType value. This is unit-testable with a constructed Loadout object. The tests should assert: (a) PrimaryRuleType = None clears both IDs, (b) = RangedWeapon clears melee ID only, (c) = MeleeWeapon clears ranged ID only. | Implement these three tests: construct a Loadout with both weapon IDs set, then set PrimaryRuleType to each value and assert the correct ID is cleared. No game context needed. |
| 4 | low | StatRangesTests.cs | AC-25 StatRanges first-sample seeding: 5 tests provided (lines 42–158). These are well-structured, meaningful assertions of the consumed Common.StatRanges behavior. Tests verify the C-4 defect fix (first value → [v,v] not [0,v]) and range expansion. Edge cases covered: degenerate range normalization, expansion in both directions, independent stats, Clear() reset. **No issues.** | — |
| 5 | low | WorkTypeThingRuleTests.cs | AC-26 WorkType default-weight assembly: 2 tests provided (lines 24–62). Tests are minimal — they document the OQ-1 decision (EM consumes Common's per-stat DefaultWorkTypeStats) by inspecting the public API (WorkTypeStatMap.AutoSwitchStatsMap) but do not validate the contents or dedup invariant. Full validation requires game context (DefDatabase population). **Correctly identified as manual-verification-only.** | — |
| 6 | low | ItemRuleAndLoadoutTests.cs:22–61 | AC-27 ItemRule Initialize + legacy normalization: 2 tests provided. Test 1 (lines 22–37) verifies null-coalescing in Initialize by asserting collections are non-null and empty post-Initialize. Test 2 (lines 44–61) verifies legacy stat-name normalization by setting a legacy weight, calling Initialize, and asserting the canonical name. Both are meaningful and unit-testable. Loadout tests (lines 68, 81) are documented-only, correctly noting game-context dependency (Resources.Strings initialization). **No issues.** | — |
| 7 | low | ItemRuleAndLoadoutTests.cs:106–124 | AC-29 RangedWeaponRule.AmmoCount gating: 1 test provided (lines 106–124). Test constructs a rule and asserts AmmoCount returns 0 when CombatExtendedHelper.EnableAmmoSystem is false, or the set value (50) when true. Test is conditional on the CE state and is meaningful. **No issues.** | — |
| 8 | low | SkillWeightTests.cs, PassionLimitTests.cs | Pure data-structure tests (constructors, field storage, constants). 5 tests total. These exercise simple value assignment and are meaningful for regression prevention. **No issues.** | — |

## Verdict

CONCERNS: 3 (findings #1–3)

## Next action

1. Implement the CopyX deep-copy unit test (finding #1) to verify independence of copied rule collections.
2. Implement the three PrimaryRuleType setter tests (finding #3) to verify each case clears the correct rule IDs.
3. (Optional, finding #2) clarify the empty Loadout.IsAvailable test stubs with `[Ignore]` or convert to comments to reduce confusion.

Creator should autofix findings #1 and #3 without escalation (both are straightforward unit test additions with no spec/API changes). Finding #2 is optional (low severity).

## Escalations

— (None. All findings are test-implementation gaps with clear fixes, not architectural or requirement changes.)

## Manual verification

### AC-26: WorkType default-weight assembly (OQ-1 outcome)

| # | Requirement (AC-ID) | Steps for user | Result reported by user |
|---|---|---|---|
| 1 | AC-26 (OQ-1) | 1. In-game, equip a colonist with tools for a work type (e.g., Cooking).<br>2. Observe tool selection logic uses WorkTypeStatMap weights.<br>3. Compare assignment to EM 0.x behavior if available (tool score reflects per-stat weights, not flat 2f). | *Awaiting user report after code is merged* |

### AC-28: Loadout.IsAvailable predicate branches

| # | Requirement (AC-ID) | Steps for user | Result reported by user |
|---|---|---|---|
| 1 | AC-28 | 1. Create a Loadout with trait requirement: `PawnTraits.Add("Brawler", true)`.<br>2. Only Brawler pawns are available for this Loadout. | *Awaiting user report* |
| 2 | AC-28 | 3. Create a Loadout with trait exclusion: `PawnTraits.Add("Nudist", false)`.<br>4. Nudist pawns are NOT available for this Loadout. | *Awaiting user report* |
| 3 | AC-28 | 5. Create a Loadout with work-capacity requirement: `PawnWorkCapacities.Add("Violent", true)`.<br>6. Only pawns with Violent capacity enabled are available. | *Awaiting user report* |
| 4 | AC-28 | 7. Create a Loadout with work-capacity exclusion: `PawnWorkCapacities.Add("Violent", false)`.<br>8. Pawns WITHOUT Violent capacity are available. | *Awaiting user report* |
| 5 | AC-28 | 9. Create a Loadout with passion limit: `PassionLimits.Add(new PassionLimit("Shooting", PassionValue.Major))`.<br>10. Only pawns with MAJOR passion in Shooting are available. | *Awaiting user report* |
| 6 | AC-28 | 11. Create a Loadout with capacity limit: `PawnCapacityLimits.Add(new PawnCapacityLimit("Moving", 0.8f, null))`.<br>12. Only pawns with Moving capacity ≥ 0.8 are available. | *Awaiting user report* |
| 7 | AC-28 | 13. Create a Loadout with stat limit: `StatLimits.Add(new StatLimit("ShootingAccuracyPawn", 0.5f, null))`.<br>14. Only pawns with ShootingAccuracyPawn ≥ 0.5 are available. | *Awaiting user report* |
| 8 | AC-28 | 15. Create a Loadout with skill limit: `SkillLimits.Add(new PawnSkillLimit("Shooting", 5, null))`.<br>16. Only pawns with Shooting skill ≥ 5 are available. | *Awaiting user report* |
| 9 | AC-28 | 17. (Ideology enabled) Create a Loadout with `PrimaryRuleType = RangedWeapon` on a pawn with Ideo forbidding ranged weapons.<br>18. Loadout is NOT available for this pawn (role-effect blocks it). | *Awaiting user report* |
| 10 | AC-28 | 19. (Ideology enabled) Create a Loadout with `PrimaryRuleType = MeleeWeapon` on a pawn with Ideo forbidding melee weapons.<br>20. Loadout is NOT available for this pawn (role-effect blocks it). | *Awaiting user report* |

### AC-29: AmmoCount gating, PrimaryRuleType setter, CopyX, C-3 composite key (in-game verification only)

| # | Requirement (AC-ID) | Steps for user | Result reported by user |
|---|---|---|---|
| 1 | AC-29 (manual) | 1. In-game without Combat Extended loaded, create a RangedWeaponRule with AmmoCount = 100.<br>2. Rule.AmmoCount getter returns 0 (gated by disabled CE). | *Awaiting user report* |
| 2 | AC-29 (manual) | 3. Load Combat Extended, restart game.<br>4. Rule.AmmoCount getter returns the set value (100) when CE is active. | *Awaiting user report* |
| 3 | AC-29 (manual) | 1. Create a Loadout with both PrimaryRangedWeaponRuleId = 0 and PrimaryMeleeWeaponRuleId = 1.<br>2. Both IDs are set. | *Awaiting user report* |
| 4 | AC-29 (manual) | 3. Set `PrimaryRuleType = PrimaryWeaponType.None`.<br>4. Both PrimaryRangedWeaponRuleId and PrimaryMeleeWeaponRuleId become null. | *Awaiting user report* |
| 5 | AC-29 (manual) | 5. Set `PrimaryRuleType = PrimaryWeaponType.RangedWeapon` (resetting the loadout).<br>6. PrimaryMeleeWeaponRuleId is cleared; PrimaryRangedWeaponRuleId is retained. | *Awaiting user report* |
| 6 | AC-29 (manual) | 7. Set `PrimaryRuleType = PrimaryWeaponType.MeleeWeapon` (resetting).<br>8. PrimaryRangedWeaponRuleId is cleared; PrimaryMeleeWeaponRuleId is retained. | *Awaiting user report* |
| 7 | AC-29 (manual) | 1. Create a RangedWeaponRule with StatWeights = [new StatWeight("Mass", 1.0f, false)].<br>2. Call Copy() method (via game component API).<br>3. Copied rule has the same stat weight (deep-copied, not same reference).<br>4. Modify original rule's StatWeights (add a new weight).<br>5. Copied rule's weights remain unchanged (independent collections). | *Awaiting user report* |
| 8 | AC-29 (manual) | 6. Repeat for MeleeWeaponRule and ToolRule.<br>7. Both CopyX methods deep-copy all fields (collections are independent after copy). | *Awaiting user report* |
| 9 | AC-29 (C-3, manual) | 1. Create a ToolRule with a work-type-dependent stat (e.g., "WorkType" custom stat).<br>2. Assign a tool to a pawn with work-types [Cooking].<br>3. Tool score reflects weights for Cooking-relevant stats.<br>4. Reassign the same pawn to work-types [Hunting].<br>5. Tool score is recalculated (different work-type set), NOT cached from step 2.<br>6. Verify the cache key includes work-type-defs (composite key, not stat-only).<br>7. Scores differ between steps 2–3 due to different work types (C-3 fix verified). | *Awaiting user report* |

---

## Notes on test quality

- **StatRangesTests**: Well-designed, covers edge cases (degenerate range, expansion, independent stats, clear/reset), meaningful assertions.
- **WorkTypeThingRuleTests**: Minimal, documents OQ-1 decision correctly. Full validation (dedup, map population) requires game context.
- **ItemRuleAndLoadoutTests**: Mixed quality. ItemRule tests are solid (Initialize, legacy normalization). Loadout tests are stubs (correctly deferred to manual). Setter logic test (PrimaryRuleType) is testable but not implemented.
- **SkillWeightTests, PassionLimitTests**: Simple constructors, no edge cases to test, but meaningful for regression prevention.

## Summary

- **Automated test coverage**: 20 unit tests pass (StatRanges 5, WorkType 2, ItemRule 2, AmmoCount 1, SkillWeight 3, PassionLimit 2, empty stubs 5). Tests are deterministic, use StateIsolationTestBase correctly, and avoid RimWorld game-context where possible.
- **Manual verification**: 18 steps covering AC-26, AC-28 (10 branches), AC-29 (7 steps including Copy and C-3 composite key).
- **Gaps**: (1) CopyX deep-copy unit tests not implemented (documented only). (2) PrimaryRuleType setter logic tests not implemented (documented only). Both are unit-testable and should be added.
