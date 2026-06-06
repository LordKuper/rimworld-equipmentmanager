---
responsibility:
  owns: project-owner custom rules read by all agents in all phases
  excludes: phase-specific rules (design-only, coding-only)
  delegates_to: custom-design-rules.md (design/design-review), custom-coding-rules.md (impl/impl-review), .asd/rules/ (workflow rules)
---

# Custom Common Rules

## What this project is

EquipmentManager is a RimWorld mod that automatically assigns weapons, tools, and loadouts to pawns based on configurable rules (ranged/melee weapon rules, tool rules, work-type rules, stat ranges, passions, skills). It is a downstream consumer of the shared `LordKuper.Common` library (`..\rimworld-common`). Rules here are inherited from that parent library and adapted to EquipmentManager's actual setup; where EquipmentManager diverges, the EquipmentManager wording wins.

## Project layout

- All source lives under `Source/`: the solution `Source/EquipmentManager.slnx`, the shared `Source/Directory.Build.props`, and one folder per project.
- **Production**: `Source/EquipmentManager/` (`EquipmentManager.csproj`). Target framework `net48`. `LangVersion latest`. `Nullable` is NOT yet enabled on the production csproj (it is the target standard — see `custom-coding-rules.md`; the test project already enables it). References RimWorld `Assembly-CSharp` + Unity modules (via `$(RimWorldManagedDir)`), `Lib.Harmony` 2.4.2 (compile-only: `PrivateAssets=all`, `ExcludeAssets=runtime`), `Microsoft.CodeAnalysis.NetAnalyzers` 9.0.0, `LordKuper.Common.dll` and `SimpleSidearms.dll` (compile-only references, `Private=False`). Build output goes to `1.6/Assemblies/`. `InternalsVisibleTo` exposes internals to the test project.
- **Tests**: `Source/EquipmentManager.Tests/` (`EquipmentManager.Tests.csproj`). NUnit 4.x + NUnit3TestAdapter + Microsoft.NET.Test.Sdk + **FluentAssertions 7.x**, `net48`, `Nullable enable`. Real tests already exist (e.g. `SkillWeightTests`, `PassionLimitTests`) with static-state isolation infrastructure (`StateIsolationTestBase`, `RimWorldAssemblyResolverFixture`).

## Mod identity

- ModId / Harmony id / `About` packageId is `LordKuper.EquipmentManager`. Use the `EquipmentManagerMod.ModId` constant, never a bare string literal.

## Upstream dependency

- `LordKuper.Common` is an upstream integration contract, not editable from here. Consume its public surface; do not fork or reimplement what it already provides. Common is resolved at compile time from `$(LordKuperCommonDir)\1.6\Assemblies` (defaults to `..\..\rimworld-common`, overridable via `LORDKUPER_COMMON_DIR`).
- RimWorld build requires `RimWorldManagedDir` / `RIMWORLD_DIR` pointing at RimWorld's `Managed` dir.
- SimpleSidearms build reference resolved via `$(SimpleSidearmsDir)` / `SIMPLE_SIDEARMS_DIR`.
