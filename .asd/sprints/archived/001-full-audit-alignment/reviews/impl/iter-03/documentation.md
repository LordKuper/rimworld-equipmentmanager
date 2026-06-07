[REVIEW-impl-documentation]: APPROVE

# Review — documentation

- **Phase**: impl-review
- **Iteration**: 03

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings at or above floor (HIGH) | — |

## Verdict
APPROVE

Severity floor = HIGH (iter 3). Persistent `design/` docs reconciled against as-built code; every material doc-to-code claim verified accurate, no SSoT violations.

Verification performed (HIGH/CRITICAL drift + SSoT only):

- **stack.html / ADR-0001 — production nullable migration**: doc claims `<Nullable>enable</Nullable>` landed on the production csproj with all JetBrains nullability attributes removed. As-built confirms: `EquipmentManager.csproj:8` `<Nullable>enable</Nullable>`; grep for `[NotNull]`/`[CanBeNull]`/`[ItemNotNull]` across `Source/EquipmentManager` returns **zero** matches. The specific code anchors cited in ADR-0001 match exactly — `Loadout.cs:107` and `ItemRule.cs:29` both `public string Label = null!;`; nullable collection fields with `??=` restore pattern present (`ItemRule.cs:22-35`, `Loadout.cs:114-129`, getters do `Initialize(); return _field!;`). No `#pragma warning disable` masking found. The stack.html "confirmed (DM-1/AC-36)" status is accurate, not premature.
- **ADR-0002 — consume LordKuper.Common (reuse deletes)**: all claimed deletions verified absent from `Source/EquipmentManager`: `WorkTypeRule.cs`, `ItemCache.cs`, `EquipmentManagerStatDefs.cs`, `EquipmentManagerGameComponent_StatRanges.cs`. `GetWindowSize` now consumed from `LordKuper.Common.UI.Windows` at all four call sites (LogDialog, ImportLoadoutsDialog, ManageLoadoutsDialog, ManageWeaponRulesDialog) — local `UiHelpers.GetWindowSize` gone (RU-6 done).
- **ADR-0003 — dependency integration pattern**: `Private=False` compile-only HintPath refs confirmed in csproj for Assembly-CSharp/Unity/LordKuper.Common/SimpleSidearms; Harmony `ExcludeAssets=runtime` + `PrivateAssets=all` confirmed. Upstream misspelling `WeaponAssingment` consumed as-is (`EquipmentManagerMapComponent.cs`), not "corrected" (C-24/AC-31 honored).
- **ADR-0004 — C-3 tool-stat cache key**: doc prescribes composite `(statDef, workTypeDefs)` key OR no-cache carve-out for WorkType-dependent stats. As-built `ToolCache.GetStatValue:42-46` implements the no-cache carve-out (WorkType stats computed on demand via `GetCustomStatValue`, never written to `StatValues`), exactly the alternative the ADR sanctions. No stale-score-by-statDef-only-key defect remains.
- **concept.html**: anti-pillar "not a reimplementation of shared utilities" is now satisfied by the ADR-0002 deletes; no contradiction with as-built.
- **SSoT**: stack.html links to ADR-0001 for the nullable decision rather than restating it; ADRs cross-link rather than duplicate. No fact has two divergent homes.
- **Provenance**: stack.html / concept.html / ADR-0003 correctly flag `reverse-engineered` with `source`; ADR-0001/0002/0004 correctly `original` with no provenance badge rendered. All correct.

## Next action
Documentation reviewer requirement met for this iteration. No routing back to impl on documentation grounds.

## Escalations (optional)
- none

## Notes (non-blocking, not a finding)
- `.asd/project/custom-common-rules.md` still states "`Nullable` is NOT yet enabled on the production csproj" while the as-built csproj has it enabled. This is workflow-infrastructure / project-rules text (read-only during sprint, outside the persistent `design/` scope this reviewer governs and outside my write authority), so it is recorded here as an observation only, not a doc-drift finding against `design/`.
