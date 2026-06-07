---
responsibility:
  owns: task breakdown, dod, task status (checkboxes)
  excludes: requirements, design decisions, code, review findings
  delegates_to: design/ docs (requirements/design), reviews/ (findings)
---

# Plan

<!--
Format rules (parser-critical):
- Overview, Context, Definition of Done — prose only, NO checkboxes
- Checkboxes (- [ ]/- [x]) appear ONLY inside `### Task N:` sections
- Checkboxes in any non-task section break orchestrator task parsing
- A subtask deferred for a manual action stays `- [ ]` and is suffixed ` — BLOCKED: MS-N` (see manual-steps.md)
-->

## Overview

Implementation plan for sprint `001-full-audit-alignment` — a full-audit-driven, end-to-end remediation of the EquipmentManager codebase, docs, and tests. The headline deliverable is the production nullable migration; remediation also covers reuse/consolidation onto `LordKuper.Common`, correctness defects, performance, standard conformance, tests, upstream-contract invariants, localization verification, and doc reconciliation. The work is ordered into six phases (A–F) that encode the load-bearing ADR-0002 sequencing constraint: all reuse deletes/migrations (RU-1, RU-2, RU-3, RU-4, RU-6) land and build green **before** the production nullable flip, so deleted code is never annotated. Each Task cites the PRD acceptance criteria (AC-N) it satisfies, an owner role, and its dependencies. No frontend work exists; the only UI touchpoint (RU-7 widget swap) is minor and assigned to backend-dev.

## Context

- [prd.html](./design/prd.html) — 3 goals · 9 stories · 41 AC · traceability table (AC ↔ audit finding)
- [adr-0001-production-nullable-migration.html](../../design/architecture/adr/adr-0001-production-nullable-migration.html)
- [adr-0002-consume-lordkuper-common.html](../../design/architecture/adr/adr-0002-consume-lordkuper-common.html) — the A→D sequencing constraint
- [adr-0003-dependency-integration-pattern.html](../../design/architecture/adr/adr-0003-dependency-integration-pattern.html) — CE reflective binding stays nullable
- [adr-0004-defect-performance-policy.html](../../design/architecture/adr/adr-0004-defect-performance-policy.html)
- [audit.md](./audit.md) — C-1…C-25, RU-1…RU-8, DM-1…DM-6; no related open stubs
- [sprint.md](./sprint.md) — goal, acceptance, nine workstreams, free-expansion ceiling

Commands (from `commands.yaml`): build `dotnet build Source/EquipmentManager.slnx -c Release`; test `dotnet test Source/EquipmentManager.slnx`; lint `dotnet format Source/EquipmentManager.slnx --verify-no-changes`; format `dotnet format Source/EquipmentManager.slnx`. ReSharper `jb-inspect` is not runnable against `.slnx` in this toolchain (R-C8); NetAnalyzers 9.0.0 covers the analyzer baseline.

## Definition of Done

- **Headline (nullable):** `Nullable enable` is set on `Source/EquipmentManager/EquipmentManager.csproj`; a clean build is green (0 warnings, 0 errors) under `WarningLevel 9999` + `TreatWarningsAsErrors`; all NRT sites are resolved by real annotations/guards (no nullable-warning `#pragma warning disable`); all 224 JetBrains nullability attributes (`[NotNull]`/`[CanBeNull]`/`[ItemNotNull]`) are removed in favor of real NRT; `using JetBrains.Annotations;` remains only in files still referencing a non-nullability JetBrains attribute (`[UsedImplicitly]`). (AC-9…AC-14)
- **Sequencing (ADR-0002):** RU-1, RU-2, RU-3, RU-4, RU-6 land and build green in commit(s) that precede the commit adding `<Nullable>enable</Nullable>`; no deleted file is annotated with NRT in the interim. (AC-1)
- **Reuse / consolidation:** `WorkTypeRule`, `EquipmentManagerGameComponent_StatRanges`, `ItemCache`, `EquipmentManagerStatDefs`, the local `UiHelpers.GetWindowSize`, and the EM-local `WorkTypeThingRuleWidget` reimplementation are deleted; EM consumes `Common.WorkTypeThingRule`/`WorkTypeStatMap`, `Common.StatRanges`, `Common.ThingCache`, `Common.Helpers.StatHelper.GetStatsByCategory`, `Common.UI.Windows.GetWindowSize`, and Common's `WorkTypeThingRuleWidget` (RU-7, migrated this sprint — UI pixel-parity verified); the `DefaultWorkTypeStats` literal no longer exists EM-side. (AC-2…AC-8)
- **Correctness:** tool-cache WorkType-stat composite-key defect fixed (AC-15); first-sample stat-range seeding correct via consumed `Common.StatRanges` (AC-16); lower-severity defects C-5/C-7 resolved by the nullable pass; C-6/C-8 addressed or recorded as deferred-awareness in this plan.
- **Performance:** `assignedByOthers` computed once per pass and mutated incrementally (AC-19); per-tool-cache-update linear rule scan replaced by a defName-keyed dictionary (AC-20).
- **Standard conformance:** all 61 raw `Verse.Log.*` calls routed through the EM `Logger` wrapper with literal prefixes dropped (AC-21, AC-22); solution consistently formatted (AC-23); NetAnalyzers reports 0 (AC-24).
- **Tests:** genuine unit tests using `StateIsolationTestBase`/`RimWorldAssemblyResolverFixture`, with the base's `CachingTypes`/fields extended where new static state is touched (R-C4); first-sample `[v,v]` assertion (AC-25), work-type default-weight assembly + dedup (AC-26), `ItemRule`/`Loadout` init + legacy-stat round-trip (AC-27), `Loadout.IsAvailable` table-driven (AC-28), `AmmoCount`/`PrimaryRuleType`/`CopyX` + C-3 composite-key (AC-29); full suite green (AC-41).
- **Upstream contract:** Common consumed only via public surface, compile-only `Private=False`, not vendored/forked (AC-30); SimpleSidearms `WeaponAssingment` typo consumed as-is, NOT corrected (AC-31); CE stays entirely reflective (AC-32); kept-local non-duplicate types retained (AC-33).
- **Localization:** verified clean — no hardcoded UI literals outside `Resources.Strings`; any new user-facing string keyed in `Resources.Strings` + Keyed XML (1.3–1.6) in the same change (AC-34, AC-35).
- **Doc reconciliation:** stack.html as-built nullable confirmation finalized during impl as a doc-to-code reconciliation now that code is flipped — validated by the impl-review documentation reviewer (impl may edit persistent docs to match as-built code) (AC-36, DM-1); concept + tech-reference re-verified (AC-37); README expanded and About.xml `<description>` completed — both MANDATED this sprint, not deferred (AC-38, AC-39); dependency-integration ADR exists — already promoted in design-promote (AC-40).
- **Gate:** full solution builds green and the entire test suite passes after all remediation (AC-41); all impl-review reviewers green.

**AC coverage note.** All 41 PRD acceptance criteria are covered by the Tasks below: AC-1 (Task 6), AC-2/AC-8/AC-7 (Tasks 1, 16), AC-3/AC-16 (Task 2), AC-4 (Task 3), AC-5 (Task 4), AC-6 (Task 5), AC-9…AC-14 (Task 10), AC-15 (Task 7), AC-17 (Task 1, auto-resolved), AC-18 (Task 15, **DEFERRED** — recorded-deferred, not implemented this sprint; see Task 15), AC-19 (Task 8), AC-20 (Task 9), AC-21/AC-22 (Task 11), AC-23 (Task 17), AC-24 (Task 17), AC-25…AC-29 (Tasks 12, 13, 14), AC-30…AC-33 (Task 18, verification), AC-34/AC-35 (Task 18, verification), AC-36 (Task 19), AC-37 (Task 19), AC-38/AC-39 (Task 16, **IN SCOPE** this sprint — mandated, not deferred), AC-40 (already satisfied by design-promote; verified in Task 19), AC-41 (Tasks 6, 17, 20).

**Test scope.** Tests are genuine unit tests over pure-logic units (stat-range math, work-type weight assembly, rule/loadout init, legacy-stat round-trips, loadout eligibility, ammo/primary-rule/copy getters, tool-cache composite key). No automated integration/game-context test is created. RU-1 changes WorkType scoring outputs (OQ-1 default-weight parity: EM flat `2f` vs Common `WorkTypeStatMap.DefaultWorkTypeStats` per-stat weights); the characterization test in Task 13 (AC-26) decides OQ-1, but any in-game assignment-decision shift (R-C2) is verified only by manual in-game observation — flagged below as a manual-verification item, not an automated test.

### Task 1: RU-1 — replace WorkTypeRule with Common.WorkTypeThingRule + WorkTypeStatMap
- [x] Delete `Source/EquipmentManager/WorkTypeRule.cs` (the 290-line reimplementation, incl. its `DefaultWorkTypeStats` literal and the four redundant nested guards).
- [x] Wire EM to consume `LordKuper.Common.WorkTypeThingRule` (+ `WorkTypeStatMap.Rebuild`) wherever `WorkTypeRule` was constructed/stored/scored (game component rule family, `ToolCache`, `ToolRule.GetThingScore`, dialog tab).
- [x] Do NOT add a Scribe save-migration shim (`backward_compat=none`); old saves silently lose persisted WorkType rules (approved).
- [x] Resolve OQ-1 default-weight parity per the Task 13 characterization test: if EM's flat `2f` default diverges intentionally from Common's per-stat weighted defaults, apply only the EM default-weight seed and still drop the duplicated recipe/skill/AllRelevantThings/scoring machinery (EM wording wins).
- [x] Confirm `grep -r "class WorkTypeRule"` under `Source/EquipmentManager/` returns nothing and `WorkTypeThingRule` is referenced.
- [x] Build green. Auto-resolves C-9 (AC-17), removes the 36-site nullable hotspot, and satisfies RU-5 (AC-8).
<!-- owner: backend-dev | AC: AC-2, AC-8, AC-17 | deps: none (Phase A, first) -->

### Task 2: RU-2 — delete EM stat-range duplicate; consume Common.StatRanges
- [x] Delete `Source/EquipmentManager/EquipmentManagerGameComponent_StatRanges.cs` (the `_statRanges` instance dict + `ExposeData_StatRanges`).
- [x] Delete `LegacyCustomStatDefs.NormalizeStatRanges` (the stat-range save-migration).
- [x] Replace EM `NormalizeStatValue`/`UpdateStatRange` usage with `Common.StatRanges` (public as of rimworld-common commit `17199b6`); accept process-global ephemeral stat ranges (not save-persisted), rebuilt via `InitializeStatRanges`.
- [x] Confirm `grep -r "NormalizeStatRanges\|ExposeData_StatRanges\|_statRanges"` returns nothing; `Common.StatRanges` referenced.
- [x] Build green. Auto-resolves the EM copy of C-4 (first-sample `[v,v]` seeding now correct upstream).
<!-- owner: backend-dev | AC: AC-3, AC-16 | deps: none (Phase A) -->

### Task 3: RU-3 — ItemCache → Common.ThingCache
- [x] Re-base `RangedWeaponCache`/`MeleeWeaponCache`/`ToolCache` on `LordKuper.Common.ThingCache` (or otherwise remove the duplicated `StatValues` dictionary + clear).
- [x] Delete `Source/EquipmentManager/ItemCache.cs`.
- [x] Confirm `grep -r "class ItemCache"` returns nothing; cache types reference `ThingCache`.
- [x] Build green. (Does NOT resolve C-3 — that EM-specific composite-key fix is Task 7.)
<!-- owner: backend-dev | AC: AC-4 | deps: coordinate with Task 7 (both touch ToolCache) -->

### Task 4: RU-4 — delete EquipmentManagerStatDefs; inline StatHelper.GetStatsByCategory
- [x] Inline the five category call-sites to `Common.Helpers.StatHelper.GetStatsByCategory(StatCategory.X)`.
- [x] Delete `Source/EquipmentManager/EquipmentManagerStatDefs.cs`.
- [x] Confirm `grep -r "EquipmentManagerStatDefs"` returns nothing.
- [x] Build green.
<!-- owner: backend-dev | AC: AC-5 | deps: none (Phase A) -->

### Task 5: RU-6 — UiHelpers.GetWindowSize → Common.UI.Windows.GetWindowSize
- [x] Delete the local `GetWindowSize` body in `Source/EquipmentManager/CustomWidgets/UiHelpers.cs`; route call-sites to `Common.UI.Windows.GetWindowSize`.
- [x] Retain EM-specific `UiHelpers` members (`CycleSettingValue`, `GetSettingCheckboxState`, `ValidNameRegex`, gap/label helpers).
- [x] Confirm no local `GetWindowSize` definition remains; `Windows.GetWindowSize` referenced.
- [x] Build green.
<!-- owner: backend-dev | AC: AC-6 | deps: none (Phase A) -->

### Task 6: Phase A green gate — reuse deletes/migrations complete before nullable flip
- [x] Confirm Tasks 1–5 (RU-1, RU-2, RU-3, RU-4, RU-6) are merged and the solution builds green BEFORE any `<Nullable>enable</Nullable>` is set.
- [x] Confirm in git history that the reuse-delete/migrate commit(s) precede the nullable-flip commit (Task 10) and that no deleted file was annotated with NRT in the interim.
- [x] Run `dotnet build Source/EquipmentManager.slnx -c Release` → 0 warnings, 0 errors.
<!-- owner: backend-dev | AC: AC-1 (also feeds AC-41) | deps: Task 1, Task 2, Task 3, Task 4, Task 5 -->

### Task 7: C-3 — ToolCache composite cache key including workTypeDefs
- [x] Key the cache entry by `(statDef, workTypeDefs)` for WorkType-dependent stats in `ToolCache.GetStatValue` (or do not cache WorkType-dependent stats), so differing work-type sets no longer return a stale first-set score within one cache window.
- [x] Verify against the migrated `WorkTypeThingRule` scoring path (Task 1) and the re-based cache (Task 3).
- [x] Build green; correctness asserted by the AC-29 test (Task 14).
<!-- owner: backend-dev | AC: AC-15 | deps: Task 1, Task 3 (Phase B) -->

### Task 8: C-12 — assignedByOthers single-build-per-pass optimization
- [x] In `EquipmentManagerMapComponent`, compute the `assignedByOthers` set once per assignment pass and mutate it incrementally (`assignedByOthers.Add(...)`), removing the per-pawn `_pawnCache.Values.SelectMany(...)` rebuild (O(pawns²×weapons)) from `AssignAllTools`/`AssignBestTool`/primary melee/ranged passes; extend the pattern the sidearm passes already use.
- [x] Confirm `grep -n "SelectMany" EquipmentManagerMapComponent.cs` no longer shows the per-pawn rebuild in those methods.
- [x] Build green.
<!-- owner: backend-dev | AC: AC-19 | deps: Task 6 (after Phase A) -->

### Task 9: C-13 — defName-keyed WorkType-rule lookup (replace O(workTypes×rules) scan)
- [x] Build a `Dictionary<string, WorkTypeThingRule>` once (on the game component, invalidated on rule edit) and look up by defName in `ToolCache.Update`, replacing the per-iteration `GetWorkTypeRules().FirstOrDefault(...)` linear scan.
- [x] Confirm `grep -n "FirstOrDefault" ToolCache.cs` no longer shows the per-iteration scan.
- [x] Build green.
<!-- owner: backend-dev | AC: AC-20 | deps: Task 1 (uses WorkTypeThingRule), Task 6 -->

### Task 10: C-1/C-2 — production nullable migration (headline)
- [x] Set `<Nullable>enable</Nullable>` on `Source/EquipmentManager/EquipmentManager.csproj` (commit AFTER Phase A per AC-1).
- [x] Resolve remaining NRT sites (~131 minus those deleted in Phase A) with real annotations/guards: `IExposable`/Scribe-populated fields use the `= null!` pattern; immutable ctor-set fields use `required`/real `?`; dereferenced hot-path sites prefer real `?` + guard over `null!` (R-C1).
- [x] Keep CE reflection-delegate fields (`CombatExtendedHelper.cs` and weapon caches) nullable `T?` with existing null-guards — do NOT convert to non-null or `= null!` on the hot path (AC-14, R-C3, ADR-0003).
- [x] Resolve C-5 (`UpdateAmmo` null `map`) and C-7 (`pawn.skills`/`pawn.health` guards) as they surface as CS86xx during this pass.
- [x] Remove all 224 JetBrains nullability attributes (`[NotNull]`/`[CanBeNull]`/`[ItemNotNull]`), pairing each removal with the matching real NRT annotation in the same edit (AC-12).
- [x] Remove `using JetBrains.Annotations;` only from files that no longer reference any JetBrains attribute; retain it where `[UsedImplicitly]` (or other non-nullability attribute) is still used (AC-13).
- [x] Use no nullable-warning `#pragma warning disable` to mask sites (AC-11).
- [x] Build clean: 0 warnings, 0 errors under `TreatWarningsAsErrors` (AC-10).
<!-- owner: backend-dev | AC: AC-9, AC-10, AC-11, AC-12, AC-13, AC-14 | deps: Task 6 (HARD: must follow Phase A) -->

### Task 11: C-16 — route 61 raw Verse.Log.* through the project Logger wrapper
- [x] Replace the 61 raw `Verse.Log.Error/Warning/Message` calls across the 7 files (`CombatExtendedHelper.cs`, `RangedWeaponCache.cs`, `MeleeWeaponCache.cs`, `ImportLoadoutsDialog.cs`, `ToolCache.cs`, `EquipmentManagerMod.cs`, `DefGeneratorPatch.cs`) with `Logger.LogError/LogWarning/LogMessage`.
- [x] Drop the hand-typed `"Equipment Manager: "` literal prefixes (the wrapper prefixes via `EquipmentManagerMod.ModId`).
- [x] Route through the EM `Logger` wrapper, NOT bare `Common.Logger.Log*` (AC-22, kept-local Logger non-finding).
- [x] Confirm `grep -rn` for raw `Verse.Log`/`Log.Error`/`Log.Warning`/`Log.Message` returns only the `Logger.cs` wrapper body; no `"Equipment Manager: "` literals remain.
- [x] Build green.
<!-- owner: backend-dev | AC: AC-21, AC-22 | deps: Task 6 (after Phase A; independent of nullable) -->

### Task 12: C-18 — stat-range / NormalizeStatValue unit test (covers C-4)
- [x] Add a unit test (using the isolation infra) over the consumed `Common.StatRanges` behavior that explicitly asserts the first observed value `v` yields range `[v,v]` (the C-4 correctness the migration fixes).
- [x] Extend `StateIsolationTestBase` `CachingTypes`/fields if the test touches additional static state (R-C4).
- [x] Test passes.
<!-- owner: test-engineer | AC: AC-25 | deps: Task 2 -->

### Task 13: C-19 — work-type default-weight assembly + dedup test (decides OQ-1)
- [x] Add a characterization test over the migrated `WorkTypeThingRule`/`WorkTypeStatMap` path asserting the dedup invariant (no duplicate StatWeights; required-stats intersection) and the WorkType default-weight values — this test decides OQ-1 (EM flat `2f` vs Common per-stat weights); record the chosen parity in Task 1.
- [x] Use the resolver fixture or refactor the pure part as needed; extend isolation base if required (R-C4).
- [x] Test passes.
<!-- owner: test-engineer | AC: AC-26 | deps: Task 1 -->

### Task 14: C-20/C-21/C-22 + C-3 — rule/loadout/cache unit tests
- [x] AC-27: test `ItemRule`/`Loadout` `Initialize` null-coalescing and `NormalizeLegacyCustomStatDefNames` legacy-stat round-trips.
- [x] AC-28: table-driven tests for `Loadout.IsAvailable` predicate branches (traits, work capacities, passion/capacity/stat/skill limits).
- [x] AC-29: tests for `RangedWeaponRule.AmmoCount` gating (`CombatExtendedHelper.EnableAmmoSystem`), `Loadout.PrimaryRuleType` setter-clear logic, `*Rule.CopyX` deep-copy completeness, AND the C-3 tool-cache composite-key correctness (differing work-type sets yield differing scores).
- [x] Extend `StateIsolationTestBase` `CachingTypes`/fields where new static state is touched (R-C4).
- [x] All tests pass.
<!-- owner: test-engineer | AC: AC-27, AC-28, AC-29 | deps: Task 7, Task 10 -->

### Task 15: C-10 — assignment-pipeline helper extraction (DEFERRED — NOT implemented this sprint)
- [x] DECIDED: DEFERRED (user decision 2026-06-07). C-10 is recorded-deferred and NOT implemented this sprint. No shared helper/abstraction is added.
- [x] Rationale: Simplicity Default — extracting an assignment-pipeline helper would add a new abstraction/layer; no Complication Approval (What / Why / Justification / Alternatives) was sought or granted, so the abstraction is not introduced (R-C5).
- [ ] Carry-forward note for PR: C-10 (assignment-pipeline duplication — the area-restriction `Predicate<Thing>` factory duplicated 6+ times verbatim, and the candidate-filter/commit-job repetition) remains a recorded-deferred simplification candidate for a future sprint, to be reconsidered only via a Complication Approval.
<!-- owner: backend-dev | AC: AC-18 (DEFERRED — not satisfied this sprint by design) | deps: n/a (deferred) -->

### Task 16: RU-7 + DM-3/DM-4 — widget migration to Common and doc completion (all MANDATED this sprint)
- [ ] AC-7 (RU-7, MIGRATE — depends on RU-1 / Task 1): migrate the EM `WorkTypeThingRuleWidget` to Common — replace EM's local widget reimplementation with consumption of `LordKuper.Common`'s `WorkTypeThingRuleWidget` per Common's actual public API. Do NOT defer.
  - Note on "follow workmanager's approach": rimworld-workmanager has NO analog of this widget (different domain — work priorities, not equipment/weapons). So "follow workmanager" here means mirror its general consolidation philosophy — consume Common's public surface rather than retain a local reimplementation — not copy a specific WorkManager widget call.
  - Reference Common source at `D:\Storage\Projects\RimWorld\rimworld-common` for the actual `WorkTypeThingRuleWidget` API (entry method, parameters, draw signature); wire `ManageWeaponRulesDialog_WorkTypes.cs` (and any other EM call-site) to Common's widget.
  - Delete the EM-local widget reimplementation once the swap compiles green; confirm `grep -r` shows EM no longer defines its own copy and references the Common type.
- [ ] AC-7 subtask — VERIFY UI pixel-parity: after the swap, verify the rendered tab matches the current EM widget (layout, columns, spacing, controls). Use a manual in-game check if needed; record the parity outcome (and any intentional cosmetic delta) in the PR.
- [ ] AC-38 (DM-3) — MANDATED: expand `README.md` from the current stub to include build/install instructions, dependency info (LordKuper.Common, SimpleSidearms, RimWorld 1.6), and a version badge. Not optional; not deferred.
- [ ] AC-39 (DM-4) — MANDATED: expand the bare `About/About.xml` `<description>` one-liner into a proper mod description (what the mod does, key features, dependencies). Not optional; not deferred.
- [ ] If the widget swap or doc changes adopt/add a user-facing string, key it in `Resources.Strings` + Keyed XML (1.3–1.6) in the same change (AC-35).
- [ ] Build green (RU-7 widget migration must compile clean).
<!-- owner: backend-dev | AC: AC-7, AC-38, AC-39 | deps: Task 1 (RU-7 widget migration); DM-3/DM-4 independent -->

### Task 17: Formatting + analyzer conformance
- [ ] Run `dotnet format Source/EquipmentManager.slnx` across the solution; ensure `dotnet format Source/EquipmentManager.slnx --verify-no-changes` exits 0 (per-`.csproj` if `.slnx` cannot run it) (AC-23).
- [ ] Confirm NetAnalyzers 9.0.0 reports 0 findings on the green build (AC-24); C-17 is satisfied by the clean post-nullable build.
- [ ] Run after all code-changing tasks (A–E) are in.
<!-- owner: backend-dev | AC: AC-23, AC-24 | deps: Task 10, Task 11, Task 14 -->

### Task 18: Upstream-contract + localization invariant verification
- [ ] AC-30: confirm `LordKuper.Common` consumed only via public surface (now incl. `WorkTypeThingRule`, `WorkTypeStatMap`, `StatRanges`, `ThingCache`, `StatHelper`, `UI.Windows`); reference stays compile-only `Private=False`; no Common source vendored/forked.
- [ ] AC-31: confirm SimpleSidearms `WeaponAssingment` (upstream's own misspelling) consumed as-is; NO rename attempted.
- [ ] AC-32: confirm CE integration stays entirely reflective (`AccessTools`); no compile-time CE reference introduced.
- [ ] AC-33: confirm kept-local non-duplicate types retained (`SkillWeight`, `PawnCapacityWeight`, `PassionLimit`, weapon-scoring caches, `CombatExtendedHelper`, `LegacyCustomStatDefs`, the `Logger` wrapper).
- [ ] AC-34/AC-35: confirm no hardcoded UI literals outside `Resources.Strings`; no new `.Translate()` key lacks a Keyed entry; no new raw UI literal introduced.
<!-- owner: backend-dev | AC: AC-30, AC-31, AC-32, AC-33, AC-34, AC-35 | deps: Task 1, Task 2, Task 3, Task 4, Task 5, Task 11 -->

### Task 19: Doc finalization — stack.html as-built + concept/tech-reference re-verify
- [ ] AC-36 (DM-1): finalize `design/architecture/stack.html` Languages section to the as-built state now that Nullable is enabled and JetBrains nullability attributes are removed (replace the in-progress wording with the completed confirmation; keep the ADR-0001 cross-link). This is a **doc-to-code reconciliation finalized during impl** — for AC-36 specifically, impl IS permitted to edit this persistent doc to match as-built code, and the change is **validated by the impl-review documentation reviewer**. (Decision 2026-06-07.)
- [ ] AC-37 (DM-2): re-verify `design/product/concept.html` (the "not a reimplementation of shared utilities" anti-pillar now holds more strongly post-RU) and `design/architecture/tech-reference/*.md`; add a "nullable enabled project-wide" note to `csharp-net48.md`; append `Helpers.SkillStatMap.Map` to `lordkuper-common-1.6.md` API-surface list.
- [ ] AC-40: verify the dependency-integration ADR exists (already promoted in design-promote as `adr-0003`); no new authoring needed.
- [ ] NOTE: persistent `design/` writes are generally owned by the design-promote skill, NOT impl. The DM-1/AC-36 stack.html as-built reconciliation is the sanctioned exception (doc-to-code reconciliation, validated by the impl-review documentation reviewer). For other persistent-doc edits (AC-37), flag to the orchestrator if a separate doc-reconciliation pass is required after impl lands.
<!-- owner: backend-dev | AC: AC-36, AC-37, AC-40 | deps: Task 10 (nullable must be live before as-built confirmation) -->

### Task 20: Final build + suite gate
- [ ] Run `dotnet build Source/EquipmentManager.slnx -c Release` → green (0 warnings, 0 errors).
- [ ] Run `dotnet test Source/EquipmentManager.slnx` → entire suite passes.
- [ ] Run `dotnet format Source/EquipmentManager.slnx --verify-no-changes` → exits 0.
- [ ] All impl-review reviewers green (verified in the impl-review phase).
<!-- owner: backend-dev | AC: AC-41 | deps: ALL prior tasks -->

## Risks
- **R-C1 nullable scope creep** — 131 NRT sites + 224 attribute edits; `= null!` can mask real null-deref. Mitigation: per-hotspot-file migration, prefer real `?` + guard on hot paths, lean on Tasks 12–14 tests, keep `TreatWarningsAsErrors` on.
- **R-C2 behavioral change from defect fixes** — C-3 (Task 7) and the C-4/RU-2 stat-range fix (Task 2) shift scoring → assignment decisions change. Mitigation: characterization tests (Tasks 12–14); note in PR; this is correct behavior. Manual in-game verification needed for assignment-decision shift (no automated game-context test).
- **R-C3 CE reflection fragility** — nullable migration must keep CE delegate fields `T?` with guards (Task 10, AC-14, ADR-0003).
- **R-C4 static-state test isolation drift** — extend `StateIsolationTestBase` `CachingTypes`/fields when new static-touching tests are added (Tasks 12–14).
- **R-C5 simplification approval gate** — C-10 (Task 15) is DEFERRED this sprint (no Complication Approval sought; Simplicity Default). Recorded-deferred only; no abstraction added.
- **R-C8 jb-inspect unavailable on `.slnx`** — rely on NetAnalyzers + `dotnet format --verify-no-changes` for the lint dimension.
- **OQ-1 WorkType default-weight parity** — decided by the Task 13 characterization test; the Task 1 migration encodes the chosen default-weight behavior.

## Dependencies
- HARD sequencing (AC-1, ADR-0002): Phase A (Tasks 1–5, gated by Task 6) precedes the nullable flip (Task 10).
- Task 6 depends on Tasks 1, 2, 3, 4, 5.
- Task 7 depends on Tasks 1, 3. Task 9 depends on Task 1. Tasks 8, 9, 11 depend on Task 6 (Phase B/C, after Phase A).
- Task 10 depends on Task 6 (HARD). Task 14 depends on Tasks 7, 10. Task 12 depends on Task 2. Task 13 depends on Task 1.
- Task 15 (C-10) is DEFERRED — not implemented this sprint; no remaining dependency. Task 16 (RU-7 widget migration) depends on Task 1; DM-3/DM-4 are independent and mandated.
- Task 17 depends on Tasks 10, 11, 14. Task 18 depends on Tasks 1–5, 11. Task 19 depends on Task 10. Task 20 depends on all prior tasks.

## Out of scope
- No save-migration shim for RU-1 (`backward_compat=none`; old saves lose persisted WorkType rules).
- No "fix" of SimpleSidearms' upstream `WeaponAssingment` typo; no forking/reimplementation of `LordKuper.Common` or SimpleSidearms.
- No new features, behavior, or scope beyond the audit findings; C-10 (new abstraction) is DEFERRED — recorded-deferred this sprint, not implemented (Simplicity Default; no Complication Approval sought).
- No persistent `design/product/requirements.html` reverse-engineering (alignment sprint).
- No edits to workflow infrastructure (`.asd/rules/`, `.asd/templates/`, `.claude/`).
- Lower-severity awareness items with no standalone AC, decided here: **C-6** (RangedWeaponCache accuracy-field reset at `Update` entry) — LOW likelihood, recommend a cheap zero-at-entry fix if the nullable pass touches the file, else defer-awareness; **C-8/R-C6** (unbounded per-Thing session cache growth) — defer unless measured to matter; **C-11, C-14, C-15** (low-priority smells/transients) — defer unless naturally touched.
