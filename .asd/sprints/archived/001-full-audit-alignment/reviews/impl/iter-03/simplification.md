[REVIEW-impl-simplification]: CONCERNS

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 03

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | critical | `Source/EquipmentManager/PassionLimit.cs:58` | Dead defensive code: `if (SkillDef == null) { }` is an empty conditional with no body. It evaluates the `SkillDef` property (which only re-enters the already-guarded `Initialize()`) and then does nothing with the result. Over-engineering checklist trips: "Defensive code for impossible-by-contract case" and "Dead code left 'in case we need it'". | Delete line 58 entirely. The empty branch has no behaviour; removing it changes nothing observable and removes a misleading no-op. No replacement guard is needed — every consumer of `SkillDef` (e.g. `Loadout.IsAvailable` via `.Where(pl => pl.SkillDef != null)`) already null-checks at the use site. |

## Verdict
CONCERNS: 1

## Next action
Route sprint back to `impl` (fix mode). The Test Engineer / responsible dev deletes `PassionLimit.cs:58`. Pure deletion, no new abstraction, layer, or dependency — autofix without escalation. Sprint re-enters impl-review.

## Escalations (optional)
- None. Finding #1 is a net deletion and introduces no complexity; it does not require Complication Approval.

## Notes (scope)
- C-10 (existing cross-rule duplication of `IsAvailable` / `SatisfiesLimits` / `GetThingScore` across `RangedWeaponRule`, `MeleeWeaponRule`, `ToolRule`) is DEFERRED per dispatch instructions and was not raised.
- No custom interfaces, factories, generics, or plugin systems exist in `Source/EquipmentManager/` (confirmed by scan). The `ItemRule` -> {Ranged,Melee,Tool}WeaponRule hierarchy is depth 2 with genuine polymorphic dispatch (`GetDefaultStatWeights`, `ExposeData`) — not a checklist trip.
- `Logger` wraps `LordKuper.Common.Logger` but adds the `ModId` argument on every call, so it is not a value-free passthrough — not a checklist trip.
- CE soft-dependency reflection delegates (`CombatExtendedHelper`, cache `*Delegate` fields, `MeleeWeaponRule.UsableWithShieldsDelegate`) are legitimate single-purpose interop hooks, not speculative abstraction — not raised.
- Empty parameterless constructors (`ItemRule()`, `Loadout()`, weapon-rule `()` ctors, weight/limit `()` ctors) are required by the RimWorld Scribe/`IExposable` load contract — not dead code.
