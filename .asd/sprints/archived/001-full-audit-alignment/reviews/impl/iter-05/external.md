[REVIEW-impl-external]: APPROVE

# External Review Report

- **Phase**: impl-review
- **Iteration**: 5
- **Severity floor (this iter)**: high
- **Scope**: TARGETED adversarial verification of one new behavioral fix — commit `258df7d` (sidearm/tool inferior-duplicate drop on upgrade), file `Source/EquipmentManager/EquipmentManagerMapComponent.cs`. HIGH scrutiny (new code).
- **External engine**: Codex CLI (codex-cli 0.130.0, gpt-5.5), invoked via stdin-pipe with full diff inline. Codex ran to a verdict; its internal file-read tool calls failed under its read-only Windows sandbox (CreateProcessWithLogonW 1326), so all attack questions were answered from the inline diff. Residual flow questions Codex could not resolve were verified directly against the full method bodies by this wrapper.

## Kept findings

None at or above the severity floor.

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | — | — |

## Dropped findings (below severity floor)

| # | Severity | Location | Description | Drop reason |
|---|---|---|---|---|
| 1 | low | AssignToolsForWorkTypes (line 224-257), all AllAvailable sites | Re-selection of the just-dropped inferior is prevented by the def-uniqueness guard (`pawn.AssignedWeapons.Keys.Any(thing => thing.def == bestWeapon.def)` → continue/skip; line 232, and the `.def` filter at AllAvailable line 473), NOT by the `assignedByOthers` set the drop-block populates. The dropped inferior is added to `assignedByOthers` (line 251 etc.) but the candidate rebuilds (`things` line 226, AllAvailable filter line 472) do not re-filter `assignedByOthers` after the drop. Behavior is correct because the same-def upgrade is already in `AssignedWeapons`, but the no-double-pickup invariant rests on the def guard rather than on the set that the code visibly adds to. Robustness/clarity observation, not a defect. | below floor on iter 5 (Codex finding: "residual risk" — verified non-bug against full method bodies) |

## Dropped findings (nitpick)

| # | Location | Description | Drop reason |
|---|---|---|---|
| — | — | — | — |

## Adversarial questions — verification outcomes

All eight attack vectors were probed; each resolves to no qualifying finding:

1. **Invalid cast `(ThingWithComps)inferior`** — `inferior` comes from `carriedWeapons` = `pawn.Pawn.GetCarriedWeapons(...)`; the same elements are cast to `ThingWithComps` for `CanPickupSidearmInstance` in pre-existing code at the same sites (lines 213, 227, 437, 475), so the cast is consistent with the established element-type contract. No new cast risk. Not a finding.
2. **Mid-iteration mutation / stale `carriedWeapons`** — `carriedWeapons` and `availableWeapons` are materialized with `.ToList()` before any loop; `DropSidearm` mutates the pawn's live SimpleSidearm collection, not these snapshots. Subsequent iterations read stale (still-listing-dropped) snapshots, but the def-uniqueness guard makes the stale entry unselectable. No wrong decision. Not a finding.
3. **Same-pass re-selection of dropped item** — verified blocked by the def guard at every site (see Dropped low finding). Not a finding.
4. **Forbidden handling (`SetForbidden` only if `Spawned`)** — guard is conservative and correct: if the drop failed to spawn the item, there is nothing on the map to unforbid; if it spawned, it is unforbidden so it can be hauled. No leak path proven. Not a finding.
5. **Wrong-pair drops in AllAvailable** — each candidate drops only carried instances matching its own `toThingDefStuffDefPair()` (value equality); a later candidate of a different def/stuff cannot have had its needed carried instance dropped. Double-drop across BestOne→AllAvailable is structurally impossible: BestOne and AllAvailable are mutually exclusive `switch` arms on `rule.EquipMode` (lines 434/471), not sequential passes. Not a finding.
6. **`assignedByOthers.Add(inferior)` pollution** — `assignedByOthers` is used downstream only to remove already-handled instances from candidate pools (`RemoveAll(assignedByOthers.Contains)`, lines 211, 428). Adding the dropped inferior correctly keeps that loose instance out of other rules' pools in the same pass. No unintended exclusion of a legitimate assignment (the inferior is exactly the instance we want excluded). Not a finding.
7. **AssignToolsForWorkTypes if/else restructure** — verified behavior-preserving: carried+remembered → both branches no-op; carried+not-remembered → `InformOfAddedSidearm`; not-carried → drop-block + EquipSecondary. The nested form is logically identical to the prior `if (carried && !remembered)` for the carried cases and adds the previously-absent not-carried else-branch. No fallthrough. Not a finding.
8. **`.ThenByDescending(carriedWeapons.Contains)` tiebreaker** — applies only on exact score+memory ties; a strictly-better map instance still wins and routes to the drop-and-upgrade else-branch. On true ties the carried instance wins, eliminating churn (matches the intent and the sidearm sites). GetHashCode remains the final deterministic tiebreak. No masked upgrade. Not a finding.

## Verdict
APPROVE

## Next action
None required from the external reviewer. The targeted fix at commit `258df7d` passes adversarial external verification with no findings at or above the iteration-5 high floor. The single low-severity robustness note (drop-then-reselect is guarded by def-uniqueness, not by `assignedByOthers`) is informational and below floor; PM may record it in the decisions log if desired but it does not block.
