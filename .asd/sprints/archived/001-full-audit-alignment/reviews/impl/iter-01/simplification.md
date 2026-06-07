[REVIEW-impl-simplification]: APPROVE

# Review — simplification

- **Phase**: impl-review
- **Iteration**: 1

## Summary

This sprint is a net-deletion consolidation (ADR-0002) layered with the nullable flip
(ADR-0001) and targeted defect/perf fixes (ADR-0004). The over-engineering checklist was
applied to the changed code. The consolidation does what it claims: it removes duplicated
EM types in favour of `LordKuper.Common`'s public surface and introduces no new abstraction,
layer, interface, generic, factory, plugin seam, or config flag. Every checklist item was
checked and none trip. Verified deletions (`EquipmentManagerStatDefs`, `ItemCache`,
`WorkTypeRule`, `UiHelpers.GetWindowSize`, `LegacyCustomStatDefs.NormalizeStatRanges`,
`_statRanges`) leave no orphaned references or dead usings.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Checklist verification detail (no findings, recorded for audit trail)

| Checklist item | Verified against | Result |
|---|---|---|
| Interface with one implementer | no new interfaces introduced this sprint | clean |
| Generic with one concrete type param | no new generics introduced | clean |
| Factory for < 3 classes | no factory introduced | clean |
| Plugin system with no plugin | none | clean |
| Abstraction with no second use case | C-11 `GetDefaultStatWeights()` virtual is overridden by 3 real subclasses (`ToolRule:104`, `RangedWeaponRule:175`, `MeleeWeaponRule:137`), each calling `base.GetDefaultStatWeights()` and consumed at 3 call-sites — genuine polymorphic dispatch, replaces a `new`-shadow smell. Legitimate, NOT a one-implementer abstraction. | clean |
| Premature config flag | no new config flag added | clean |
| Defensive code for impossible-by-contract case | guards added (CE-absent, `pawn.skills`/`health` null) are for genuinely-reachable states per ADR-0003 / C-7, not impossible ones | clean |
| Helper wrapping one stdlib call without value | `Logger` wrapper is the intended Common consumption pattern (injects `ModId`); widget glue is thin pass-through not a re-wrapper | clean |
| Inheritance depth ≥ 3 without dispatch | `ItemRule` → {`ToolRule`,`RangedWeaponRule`,`MeleeWeaponRule`} is depth 2 with real dispatch (`GetDefaultStatWeights`, `ExposeData`); caches derive depth 1 from `Common.ThingCache` | clean |
| Framework wrapping a framework | RU-7 widget is direct delegation to `Common.UI.Widgets.WorkTypeThingRuleWidget.DoWidgetTab`, not a re-wrapper | clean |
| Mock of a mock | not in scope (code, not tests) | n/a |
| Comment that restates code | the retained comments (ItemRule:22-28/33, ToolCache:42-43, cache CE notes) explain non-obvious Scribe/CE lifecycle rationale, not the literal statement | clean |
| Dead code left "in case we need it" | deletions verified clean (no orphan refs); all 13 surviving `using JetBrains.Annotations;` back a live `[UsedImplicitly]` per ADR-0001 | clean |

## Targeted-check results (per dispatch scope)

- **C-11 (`GetDefaultStatWeights`)** — genuine simplification. Removes the static-shadow smell;
  now a `protected internal virtual` method with 3 real overrides and `base` chaining. Not
  over-engineered, no new layer. PASS.
- **C-3 (composite cache key)** — `ToolCache.GetStatValue` (`:40-55`) takes the **no-cache
  carve-out** for WorkType-dependent stats (computed on demand, never written to `StatValues`).
  No key-wrapper type or composite-key struct was introduced — the simplest correct option of
  the two ADR-0004 permitted. PASS.
- **C-8 (prune method)** — `PruneDestroyedThingCaches` (`EquipmentManagerGameComponent.cs:48-68`)
  is one internal method with one call-site (`EquipmentManagerMapComponent.cs:243`), iterating
  three concrete dictionaries. No abstraction; the small per-cache repetition is three real
  concrete collections, not a candidate for premature generalization. PASS.
- **RU-7 (widget glue)** — `ManageWeaponRulesDialog_WorkTypes.DoTab_WorkTypes` (`:30-37`) is thin
  glue forwarding state to Common's widget; EM-local reimplementation removed. Not a re-wrapper.
  PASS.
- **C-10 (assignment-helper extraction)** — correctly **NOT** done. The area-restriction
  predicate and candidate-filter boilerplate remain duplicated across the `EquipmentManagerMapComponent`
  assignment methods. This is the recorded-deferred state (OQ-3 / R-C5, needs Complication
  Approval). Per dispatch instruction, the existing duplication is NOT flagged. PASS.
- **Dead code** — none found. Deleted types leave no references; no dead usings; no commented-out
  code blocks reintroduced.

## Verdict
APPROVE

## Next action
Reviewer done. No simplification findings; consolidation reduces complexity as intended and
introduces no new abstraction requiring Complication Approval.

## Escalations (optional)
- none
