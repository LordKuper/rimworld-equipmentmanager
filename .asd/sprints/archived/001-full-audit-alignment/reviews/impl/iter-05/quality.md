[REVIEW-impl-quality]: APPROVE

# Review — quality

- **Phase**: impl-review
- **Iteration**: 05
- **Scope**: TARGETED adversarial verification of one behavioral fix (commit 258df7d) — sidearm/tool duplicate-assignment fix in `Source/EquipmentManager/EquipmentManagerMapComponent.cs`. Reviewed as new code at HIGH scrutiny.

## Change under review

At the 7 non-carried `EquipSecondary` sites (UpdateMeleeSidearms BestOne/AllAvailable, UpdateRangedSidearms BestOne/AllAvailable, AssignAllTools, AssignBestTool, AssignToolsForWorkTypes), before queueing `EquipSecondary` the code now computes `defPair = weapon.toThingDefStuffDefPair()`, collects same-pair carried instances into a local `.ToList()`, and for each: `WeaponAssingment.DropSidearm(pawn, (ThingWithComps)inferior, intentionalDrop:true, unmemorise:true)`, then `if (inferior.Spawned) inferior.SetForbidden(false, warnOnFail:false)`, then `assignedByOthers.Add(inferior)`. AssignToolsForWorkTypes if/else restructured so carried+remembered only informs memory (no EquipSecondary). Two tool orderings gained `.ThenByDescending(carriedWeapons.Contains)` tiebreaker.

## Adversarial verification results

**Cast safety `(ThingWithComps)inferior`** — SAFE. `inferior` originates from `carriedWeapons` (`GetCarriedWeapons`); the pre-existing code already casts the same collection's members to `ThingWithComps` (lines 46, 95, 213, 227, 437, 579 …) and `CanPickupSidearmInstance` requires `ThingWithComps`. Consistent with established invariant that all sidearm-eligible things are `ThingWithComps`.

**Collection-modified-during-iteration** — SAFE. `carriedWeapons` and `availableWeapons` are materialized via `.ToList()`. The drop loop iterates a separate filtered `.ToList()` (`inferiorCarried`). `DropSidearm` mutates `pawn.equipment`/`pawn.inventory`, neither of which is the enumerated collection. The AllAvailable outer `foreach` enumerates a deferred `Where` over the unmodified `availableWeapons` list; the predicate's `AssignedWeapons.Keys.All(...)` completes within each predicate call, so no "collection modified" exception across MoveNext.

**Re-selection of the dropped ground instance in the same pass** — SAFE on two independent guards. (a) Def-level dedup: after equipping the better instance it is added to `AssignedWeapons`; the inferior shares `def`, so AllAvailable's `AssignedWeapons.Keys.All(thing => thing.def != weapon.def)` and BestOne's `Any(thing => thing.def == bestWeapon.def)` skip it. (b) `assignedByOthers.Add(inferior)` prevents a LATER rule/pawn in the same tick from re-equipping the dropped ground instance (each method removes `assignedByOthers` members from its `availableWeapons` at entry). Adding inferior to `assignedByOthers` is correct and necessary, consistent with the C-12 in-place hoisting; no regression to that logic.

**defPair scoping in AllAvailable** — CORRECT. `inferiorCarried` is filtered to `w.toThingDefStuffDefPair() == defPair` for the CURRENT weapon only. `ThingDefStuffDefPair` is a value struct with a defined `==` operator (value equality). Other def-pairs another loop iteration intends to keep are untouched.

**Memory consistency (`unmemorise:true`)** — CORRECT. `DropSidearm(unmemorise:true, intentionalDrop:true)` → `InformOfDroppedSidearm` → `ForgetSidearmMemory(defPair)` removes ONE occurrence of the pair from `rememberedWeapons`. The subsequently queued `EquipSecondary` re-adds the pair via SimpleSidearms' own `InformOfAddedSidearm` on pickup, so the remembered set converges to the correct single entry for the better instance. No stale/duplicate memory left for that pair.

**AssignToolsForWorkTypes if/else restructure** — CORRECT. carried+remembered now falls through `if (carriedWeapons.Contains(bestWeapon))` and only informs memory (no spurious EquipSecondary for an already-carried weapon, fixing the prior over-issue); non-carried branch drops same-pair duplicates then queues EquipSecondary. Now structurally identical to AssignBestTool / AssignAllTools.

**Forbidden-clear** — CORRECT API + guard. `SetForbidden(false, warnOnFail:false)` is the standard `ForbidUtility` extension (used identically inside SimpleSidearms `Intercepts_UI.cs:141`). Guarded by `if (inferior.Spawned)` — after a successful drop the inferior is spawned on the ground; the guard correctly avoids calling map-touching `SetForbidden` on the equipment-drop edge case where the thing is not (yet) spawned.

**Upgrade preservation** — PRESERVED. Better instance still selected and equipped; only the same-pair inferior carried instance is removed, eliminating the duplicate while keeping the upgrade.

**Tiebreaker addition `.ThenByDescending(carriedWeapons.Contains)`** — Behaviorally benign; biases toward already-carried instances on score/memory ties, reducing needless swaps. No correctness impact.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above iteration floor (critical); no high/medium real-defect risks identified | — |

### Non-blocking observation (below floor, not a finding — recorded only)
- All 7 sites duplicate the same drop-inferior-duplicates block (defPair → filter same-pair carried → drop/forget/unforbid/track). A private helper (e.g. `DropSamePairCarried(pawn, weapon, carriedWeapons, assignedByOthers)`) would remove ~6 copies and centralize the invariant. This is a simplification/maintainability point for the Simplification reviewer, not a quality defect; raising a new abstraction would require Complication Approval, so it is intentionally NOT raised as a finding here.

## Verdict
APPROVE

## Next action
Reviewer done. No fixes required from the quality dimension. The fix is correct: it preserves intended upgrades, removes the inferior same-pair duplicate without NRE, keeps SimpleSidearm memory consistent, prevents same-pass re-selection via def-dedup + assignedByOthers, scopes drops to the current def-pair, and the AssignToolsForWorkTypes restructure is correct. No regression to the assignedByOthers hoisting.

## Escalations
- None.
