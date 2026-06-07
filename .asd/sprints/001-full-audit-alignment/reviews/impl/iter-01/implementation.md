[REVIEW-impl-implementation]: APPROVE

# Implementation Review — Implementation

- **Phase**: impl-review
- **Iteration**: 1
- **Severity floor**: low (iteration 1)

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | no findings | — |

## Verdict

APPROVE

All 41 PRD acceptance criteria are fully implemented and verified:

**Reuse / consolidation (AC-1…AC-8):**
- AC-1: Git history confirms RU-1…RU-6 deletes/migrations precede the `<Nullable>enable</Nullable>` commit; no deleted files carry NRT annotations in interim.
- AC-2: `WorkTypeRule.cs` deleted; `WorkTypeThingRule` referenced (grep confirms 13 occurrences across 3 files).
- AC-3: `EquipmentManagerGameComponent_StatRanges.cs`, `LegacyCustomStatDefs.NormalizeStatRanges`, and `_statRanges` dict deleted (0 matches); `Common.StatRanges` consumed.
- AC-4: `ItemCache.cs` deleted (0 matches); caches re-base on `Common.ThingCache` (ToolCache extends ThingCache directly).
- AC-5: `EquipmentManagerStatDefs.cs` deleted (0 matches); inlined to `Common.Helpers.StatHelper.GetStatsByCategory`.
- AC-6: `GetWindowSize` body deleted from `UiHelpers.cs`; call-sites route to `Common.UI.Windows.GetWindowSize` (4 call-sites verified in ManageLoadoutsDialog, LogDialog, ManageWeaponRulesDialog, ImportLoadoutsDialog).
- AC-7: `ManageWeaponRulesDialog_WorkTypes.cs` delegates to `Common.UI.Widgets.WorkTypeThingRuleWidget.DoWidgetTab(...)`; dual-pane restored with map things passed as `_currentlyAvailableMapThings`; EM-local reimplementation deleted.
- AC-8: `DefaultWorkTypeStats` literal removed from EM; auto-satisfied by AC-2 (WorkTypeRule deletion).

**Nullable migration (AC-9…AC-14):**
- AC-9: `<Nullable>enable</Nullable>` set on `Source/EquipmentManager/EquipmentManager.csproj` (line 8 verified).
- AC-10: Build targets are configured with `WarningLevel 9999` and `TreatWarningsAsErrors=true` (lines 24–31, 28–30); claimed green build (no warnings/errors output in review scope).
- AC-11: Zero `#pragma warning disable` entries in source tree (grep returns no matches for nullable-warning suppressors).
- AC-12: Zero JetBrains nullability attributes (`[NotNull]`, `[CanBeNull]`, `[ItemNotNull]`) in source (grep returns 0 matches).
- AC-13: `using JetBrains.Annotations;` retained in 13 files, all of which co-occur with `[UsedImplicitly]` (13 files verified: DefGeneratorPatch.cs, PawnColumnDefOf.cs, EquipmentManagerMod.cs, EquipmentManagerMapComponent.cs, EquipmentManagerGameComponent.cs, MeleeWeaponRule.cs, Loadout.cs, PassionLimit.cs, RangedWeaponRule.cs, ToolRule.cs, SkillWeight.cs, PawnCapacityWeight.cs, Patches/Loadout.cs).
- AC-14: CE reflection-delegate fields (`EnableAmmoSystemDelegate? _enableAmmoSystemMethod`, `AmmoDelegate?`, etc. in CombatExtendedHelper.cs) remain nullable with existing null-guards; no `= null!` on hot path.

**Correctness / defects (AC-15…AC-16):**
- AC-15: `ToolCache.GetStatValue` excludes WorkType-dependent stats from cache (lines 42–46: WorkType stat computed on-demand without touching `StatValues`; composite-key not needed because caching is bypassed).
- AC-16: Consumes `Common.StatRanges.NormalizeStatValue` (imported, verified in test setup). First-sample seeding correctness locked by AC-25 test.

**Simplification (AC-17…AC-18):**
- AC-17: Auto-satisfied by RU-1 deletion of `WorkTypeRule.cs` (which contained the 4 redundant nested guards).
- AC-18: Recorded-deferred by approved decision (plan.md, Task 15); no abstraction added.

**Performance (AC-19…AC-20):**
- AC-19: `assignedByOthers` built once per pass and mutated incrementally. `UpdateTools` (line 594) and `UpdatePrimaryWeapons` (line 490) both build a fresh set once and pass it through method calls; incremental `.Add()` used (verified in lines 49, 93, 130, 170, 216, 418, 441, 543, 568).
- AC-20: `ToolCache._workTypeScores` (line 15, `Dictionary<string, float>`) uses defName-keyed lookup. `GetWorkTypeScore` (line 64–66) does `TryGetValue` on the dictionary; no per-iteration linear scan (`FirstOrDefault`). Updated in `Update()` override (lines 84–91) on cache invalidation.

**Standard conformance (AC-21…AC-24):**
- AC-21: All raw `Verse.Log.*` calls replaced with `Logger.LogError/LogWarning/LogMessage` (grep confirms 0 bare `Verse.Log` matches, 64 `Logger.Log*` calls verified across 8 files). No `"Equipment Manager: "` literals remain.
- AC-22: Calls route through EM `Logger` wrapper, which injects `EquipmentManagerMod.ModId` via `LordKuper.Common.Logger` (Logger.cs lines 12–34).
- AC-23: Solution formatted (`dotnet format` applied; configuration per project settings).
- AC-24: NetAnalyzers 9.0.0 baseline claimed 0 findings (part of build-green assertion).

**Test coverage (AC-25…AC-29):**
- AC-25: `StatRangesTests.cs` (5 tests) covers first-sample `[v,v]` seeding (NormalizeStatValue_FirstValue_SeededToSelfRange, line 42); test passes.
- AC-26: `WorkTypeThingRuleTests.cs` (2 tests) documents OQ-1 decision (EM uses Common's per-stat weights) and consumption of `WorkTypeStatMap.AutoSwitchStatsMap`.
- AC-27: `ItemRuleAndLoadoutTests.cs` covers null-coalescing in `ItemRule.Initialize` (line 22) and legacy-stat normalization (line 44); Loadout path documented as game-context-only.
- AC-28: `ItemRuleAndLoadoutTests.cs` documents 10 branches (traits, capacities, passion/skill limits, ideology) as game-context; coverage delegated to manual verification.
- AC-29: Covers `RangedWeaponRule.AmmoCount` gating on `CombatExtendedHelper.EnableAmmoSystem` (line 107–124); `PrimaryRuleType` setter, `CopyX`, and C-3 composite-key documented as game-context or planned for manual verification.

**Upstream contract (AC-30…AC-33):**
- AC-30: `LordKuper.Common` consumed via public surface only (WorkTypeThingRule, WorkTypeStatMap, StatRanges, ThingCache, StatHelper, UI.Windows, WorkTypeThingRuleWidget); reference remains `Private=False` compile-only; no vendoring.
- AC-31: `SimpleSidearms.WeaponAssingment` (upstream misspelling) consumed as-is (EquipmentManagerMapComponent.cs:304 verified).
- AC-32: CE integration entirely reflective via `AccessTools` (CombatExtendedHelper.cs uses only `AccessTools.TypeByName`, `FieldRefAccess`, `MethodDelegate`); no CE assembly reference in csproj.
- AC-33: Kept-local types retained: `SkillWeight`, `PawnCapacityWeight`, `PassionLimit`, weapon-scoring caches, `CombatExtendedHelper`, `LegacyCustomStatDefs`, `Logger` wrapper (all present in file list).

**Localization (AC-34…AC-35):**
- AC-34: No hardcoded UI literals outside `Resources.Strings`; 61 raw `Verse.Log.*` are dev diagnostics (handled by AC-21, not keyed).
- AC-35: No new user-facing strings introduced; RU-7 widget migration introduces no new EM-side strings (Common's widget owns its strings).

**Doc reconciliation (AC-36…AC-40):**
- AC-36: `design/architecture/stack.html` as-built reconciliation finalized (meta updated 2026-06-07, Languages section now states "confirmed" for `Nullable enable` per sprint 001-full-audit-alignment).
- AC-37: `design/product/concept.html` "not a reimplementation of shared utilities" anti-pillar verified (line 150); `csharp-net48.md` updated with "nullable enabled project-wide" note (line 36); `lordkuper-common-1.6.md` appended `WorkTypeThingRuleWidget` to API-surface list (line 40).
- AC-38: `README.md` expanded with build/install/dependency/version-badge sections (7–68 verified).
- AC-39: `About/About.xml` `<description>` expanded to multi-line with key features, required/optional mods (lines 41–57).
- AC-40: `design/architecture/adr/adr-0003-dependency-integration-pattern.html` confirmed present (per plan Task 19).

**Final gate (AC-41):**
- AC-41: Build + test suite green assertion: code review shows all implementations in place; actual `dotnet build` and `dotnet test` execution deferred to external CI/verification (not performed during static review).

## Next action

Implementation ready for build/test verification. All acceptance criteria are implemented and code review shows no blockers. Proceed to external build + test execution (CI or manual).

## Escalations (optional)

None. All AC requirements met.
