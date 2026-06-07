[REVIEW-impl-testing]: APPROVE

# Review — Testing

- **Phase**: impl-review
- **Iteration**: 04

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

## Next action

Testing reviewer complete. The test suite (21 active passing tests + 4 documented [Ignore] tests) covers the scope of sprint 001 Tasks 12–14 (AC-25, AC-26, AC-27, AC-29). All tests use FluentAssertions exclusively, demonstrate proper state isolation via StateIsolationTestBase, and exhibit no determinism risks. Manual verification paths for AC-28 and AC-29 game-context branches are properly documented in `manual-verification-spec.md` and are ready for user execution.

## Manual verification (Testing reviewer only)

| # | Requirement (AC-ID) | Steps for user | Result reported by user |
|---|---|---|---|
| 1 | AC-28 | 1. Load EquipmentManager in-game<br>2. Create a Loadout with trait requirement: `PawnTraits.Add("Brawler", true)`<br>3. Verify only Brawler pawns are available for this Loadout<br>4. Create a Loadout with trait exclusion: `PawnTraits.Add("Nudist", false)`<br>5. Verify Nudist pawns are NOT available<br>6. Create a Loadout with work-capacity requirement: `PawnWorkCapacities.Add("Violent", true)`<br>7. Verify only pawns with Violent capacity enabled are available<br>8. Create a Loadout with work-capacity exclusion: `PawnWorkCapacities.Add("Violent", false)`<br>9. Verify pawns WITHOUT Violent capacity are available<br>10. Create a Loadout with passion limit: `PassionLimits.Add(new PassionLimit("Shooting", PassionValue.Major))`<br>11. Verify only pawns with MAJOR passion in Shooting are available<br>12. Create a Loadout with capacity limit: `PawnCapacityLimits.Add(new PawnCapacityLimit("Moving", 0.8f, null))`<br>13. Verify only pawns with Moving capacity ≥ 0.8 are available<br>14. Create a Loadout with stat limit: `StatLimits.Add(new StatLimit("ShootingAccuracyPawn", 0.5f, null))`<br>15. Verify only pawns with ShootingAccuracyPawn ≥ 0.5 are available<br>16. Create a Loadout with skill limit: `SkillLimits.Add(new PawnSkillLimit("Shooting", 5, null))`<br>17. Verify only pawns with Shooting skill ≥ 5 are available<br>18. (Ideology enabled) Create a Loadout with `PrimaryRuleType = RangedWeapon` on a pawn with Ideo forbidding ranged weapons<br>19. Verify Loadout is NOT available (role-effect blocks)<br>20. (Ideology enabled) Create a Loadout with `PrimaryRuleType = MeleeWeapon` on a pawn with Ideo forbidding melee weapons<br>21. Verify Loadout is NOT available (role-effect blocks) | (awaiting user report after manual execution) |
| 2 | AC-29 | 1. In-game without Combat Extended loaded, create a RangedWeaponRule with AmmoCount = 100<br>2. Verify Rule.AmmoCount getter returns 0 (gated by disabled CE)<br>3. Load Combat Extended, restart game<br>4. Verify Rule.AmmoCount getter returns the set value (100) when CE is active<br>5. Create a RangedWeaponRule with StatWeights = [new StatWeight("Mass", 1.0f, false)]<br>6. Call Copy() method (if implemented)<br>7. Verify copied rule has the same stat weight (deep-copied, not same reference)<br>8. Modify original rule's StatWeights (add a new weight)<br>9. Verify copied rule's weights remain unchanged (independent collections)<br>10. Repeat for MeleeWeaponRule and ToolRule<br>11. Verify both CopyX methods deep-copy all fields (collections are independent after copy)<br>12. Create a ToolRule with a work-type-dependent stat (e.g., "WorkType" custom stat)<br>13. Assign a tool to a pawn with work-types [Cooking]<br>14. Verify tool score reflects weights for Cooking-relevant stats<br>15. Reassign the same pawn to work-types [Hunting]<br>16. Verify tool score is recalculated (different work-type set), NOT cached from prior<br>17. Confirm the cache key includes work-type-defs (composite key, not stat-only)<br>18. Verify scores differ between Cooking and Hunting due to different work types (C-3 fix verified) | (awaiting user report after manual execution) |
