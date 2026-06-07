[REVIEW-impl-documentation]: CONCERNS

# Review — documentation

- **Phase**: impl-review
- **Iteration**: 01

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | medium | `design/architecture/adr/adr-0001-production-nullable-migration.html:157` (Decision, IExposable bullet) | **Doc/code DRIFT.** ADR-0001's IExposable resolution prescribes, for the CS8618 majority: *non-nullable* collection fields with real initializers (`= new()`) preferred over `= null!`, and `= null!` reserved for reference fields. The as-built code diverges for **Scribe-serialized collection fields**: across 10 rule/loadout `IExposable` types (`Loadout.cs`, `ItemRule.cs`, `MeleeWeaponRule.cs`, `RangedWeaponRule.cs`, `ToolRule.cs`, the 5 `EquipmentManagerGameComponent_*` rule files; 29 occurrences) these are declared **nullable** `List<…>? _x = []` / `Dictionary<…>? = new()` with a `??=` restore in `Initialize()`, not non-nullable `= new()`. The in-code rationale (`ItemRule.cs:22-23`, `Loadout.cs:114-115`) is sound — `Scribe_Collections.Look` writes `null` back when no saved data exists, so a non-nullable `= new()` field would be violated post-load. But ADR-0001's Decision text never describes this nullable-collection-plus-`??=` pattern; it only documents `= new()` (non-nullable) for collections. The scalar-reference `= null!` guidance DOES match the code (`Loadout.cs:107`, `ItemRule.cs:29`, `PawnColumnDefOf.cs:17`), and the CE reflection-field `T?` guidance matches (`CombatExtendedHelper.cs`). The drift is isolated to the collection-field paragraph. | Reconcile ADR-0001's IExposable bullet to the as-built reality: add the Scribe-collection nuance (`Scribe_Collections.Look` nulls collections on empty load → such collection fields are declared `T?` and restored via `??= []` / `??= new()` in `Initialize()`, NOT non-nullable `= new()`). Keep the `= null!` guidance for scalar reference fields, which matches. Owned by Architect in design-promote — flag for reconciliation, not for code change. |
| 2 | medium | `design/architecture/tech-reference/csharp-net48.md:17-18` | **Internally-contradictory stale claim (DM-2 / AC-37 not fully reconciled).** The "Deprecations" (L33) and "Project conventions" (L36-39) sections correctly state `<Nullable>enable</Nullable>` is now active on production and the migration is complete. But "API surface used in project" still carries the pre-migration text: L17 — "production code carries nullability intent via attributes (the production project has **not yet** enabled the C# nullable context)"; L18 — "C# nullable reference types … enabled in the **test project only**". These two lines directly contradict the rest of the same file and the as-built csproj (`EquipmentManager.csproj:8` `<Nullable>enable</Nullable>`; 0 JetBrains nullability attributes remaining). | Update `csharp-net48.md:17-18` to reflect production nullable enabled: drop "not yet enabled" and "test project only"; note JetBrains nullability attributes (`[NotNull]`/`[CanBeNull]`) are removed and only non-nullability ones (`[UsedImplicitly]`, `[Pure]`) may remain. Architect-owned reconciliation in design-promote. |
| 3 | low | `design/architecture/tech-reference/lordkuper-common-1.6.md:40` | The `WorkTypeThingRuleWidget.DoWidgetTab` entry says it "drives the complete WorkType rule editor tab" but omits that the as-built signature is the **extended dual-pane** form taking both globally-available items and current-map things (each with its own scroll position) — see `ManageWeaponRulesDialog_WorkTypes.cs:32-36`. Not a contradiction, just an incompleteness vs the consumed surface. | Optionally note the dual-pane (global + map) argument shape in the API entry so the consumed signature is fully captured. |

## Verdict
CONCERNS: 3

## Cross-checks that PASSED (no finding)

- **stack.html** — reconciled correctly: nullable enabled as-built (`Nullable enable` on csproj confirmed, 224 JetBrains nullability attributes removed → grep returns 0), no `#pragma warning disable` masking. Statement at L119 matches code. SSoT clean (links to ADR-0001, no duplication of decision rationale).
- **commands.yaml** (`.asd/project/commands.yaml:13-19`) — R-C8 resolved: the working `jb cleanupcode/inspectcode … --toolset-path="…\10.0.300\MSBuild.dll"` invocation is present; no "jb is unrunnable" claim remains in any doc. stack.html L164 jb description is consistent.
- **lordkuper-common-1.6.md** — API-surface additions verified accurate against code: `ThingCache` (caches extend it), `StatHelper.GetStatsByCategory(StatCategory.X)`, `StatRanges.NormalizeStatValue`, `Windows.GetWindowSize`, `WorkTypeThingRule`, `WorkTypeStatMap`, `WorkTypeThingRuleWidget.DoWidgetTab` all grep-confirmed. The doc correctly flags `SkillStatMap.Map` as internal/not directly accessible from EM (matches: no direct EM reference).
- **concept.html** — "Not a reimplementation of shared utilities" anti-pillar now holds more strongly post-RU-1…RU-6 (Common types consumed, local reimplementations deleted). No drift.
- **AC ↔ ADR ↔ code traceability** — AC-9/10/11/12 (nullable) trace to ADR-0001 and are satisfied in code; AC-14 (CE fields stay `T?`) matches `CombatExtendedHelper.cs`; AC-30 (Common public surface incl. WorkTypeThingRule/StatRanges/ThingCache/StatHelper/Windows) matches code consumption.
- **Provenance/responsibility frontmatter** — stack.html (`reverse-engineered` + source), concept.html (`reverse-engineered` + source), ADR-0001 (`original`), tech-reference md files (responsibility block present) all correct; provenance badges consistent with field values.

## Next action
Findings #1 and #2 are persistent-doc reconciliation items owned by the Architect, to be applied in **design-promote** (not by a code change and not by this reviewer — Documentation reviewer never writes to `design/`). They are doc-vs-code actuality drifts surfaced for the creator. PM routes #1/#2 to the Architect for ADR-0001 + csharp-net48.md reconciliation; #3 optional. No code rework required.

## Escalations (optional)
- none — no concept/contract/abstraction change; all findings are documentation-reconciliation, autofixable by the domain creator without user approval.
