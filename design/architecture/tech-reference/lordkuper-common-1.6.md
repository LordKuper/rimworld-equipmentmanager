---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# LordKuper.Common @ 1.6 (upstream shared library; local sibling repo)

## Canonical source
- Local upstream repo: `..\..\rimworld-common` (sibling working copy; the authoritative source). Steam Workshop id 3531352422.
- Last verified: 2026-06-07 (updated for sprint 001-full-audit-alignment API-surface expansion)
- HIGH knowledge-gap risk: a private/community library with **no LLM training coverage**. Every member below was confirmed by grepping EquipmentManager's source; do NOT invent additional API. To verify signatures, read the sibling repo or decompile `..\..\rimworld-common\1.6\Assemblies\LordKuper.Common.dll`.

## Reference nature (compile-only, upstream contract)
- Referenced by `HintPath` `$(LordKuperCommonAssembliesDir)\LordKuper.Common.dll`, `Private=False`. `LordKuperCommonAssembliesDir` defaults to `..\..\rimworld-common\1.6\Assemblies` (overridable via `LORDKUPER_COMMON_DIR`).
- Declared a runtime `modDependency` **only for v1.6** in `About/About.xml` (`modDependenciesByVersion` → `<v1.6>`), `loadAfter`. At runtime the Common mod supplies the assembly.
- **Upstream integration contract — do not fork or reimplement.** Consume only the public surface; if a needed capability is missing, the change belongs upstream in `rimworld-common`, not here.

## API surface used in project (confirmed from source)
- `LordKuper.Common.Logger` — static logging keyed by mod id. Wrapped locally by `EquipmentManager.Logger`:
  `Logger.LogError(EquipmentManagerMod.ModId, message, exception)`, `Logger.LogMessage(ModId, message)`, `Logger.LogWarning(ModId, message, exception)`.
- `LordKuper.Common.RimWorldTime` — value type for in-game time, threaded through all cache/score APIs:
  - ctor `new RimWorldTime(year, day, hour)` (used as `new RimWorldTime(0,0,0)` for stat-range seeding).
  - static `RimWorldTime.GetMapTime(map)` — current map time, passed to caches and rule scoring.
- `LordKuper.Common.Cache.TimedCache` — base class for time-bucketed caches. `MeleeWeaponCache`, `RangedWeaponCache`, `ToolCache` extend `ThingCache` (which extends `TimedCache`). Ctor `base(thing, 24f)`, `override bool Update(RimWorldTime time)`.
- `LordKuper.Common.ThingCache` — concrete base for thing caches; `MeleeWeaponCache`, `RangedWeaponCache`, `ToolCache` extend it (consumed since RU-3 / sprint 001).
- `LordKuper.Common.Helpers.StatHelper` — stat access/aggregation:
  - `StatHelper.GetStatValue(thing|pawn|def, statDef)`
  - `StatHelper.GetStatValueDeviation(thing|def, statDef)` — normalized deviation used in weighted scoring.
  - `StatHelper.GetStatsByCategory(StatCategory.X)` — returns `IReadOnlyCollection<StatDef>` filtered by category; replaced `EquipmentManagerStatDefs` (RU-4 / sprint 001).
- `LordKuper.Common.Helpers.StatCategory` (enum) — `Pawn`, `WeaponMelee`, `WeaponRanged`, `Tool`, `Work`.
- `LordKuper.Common.StatRanges` — process-global ephemeral stat-range tracking; provides `StatRanges.NormalizeStatValue(statDef, value)` and `InitializeStatRanges`. Replaced `EquipmentManagerGameComponent_StatRanges` (RU-2 / sprint 001).
- `LordKuper.Common.WorkTypeThingRule` — work-type scoring rule (replaced EM's local `WorkTypeRule`; RU-1 / sprint 001). Key members: `DefaultRules` (static, seeds default rules), `StatWeights`, `SetStatWeight(StatDef, float)`, `DeleteStatWeight(string)`, `GetGloballyAvailableItems()`, `GetThingScore(Thing)`, `Label`.
- `LordKuper.Common.WorkTypeStatMap` — per-work-type stat weight map used by `WorkTypeThingRule`. Default stat weights are seeded via `WorkTypeStatMap.AutoSwitchStatsMap` (internal) through `WorkTypeThingRule.DefaultRules`. EM does not call `WorkTypeStatMap` directly; it is consumed indirectly. `SkillStatMap.Map` (internal in Common) is used internally by `WorkTypeStatMap` to derive skill-to-stat mappings — not directly accessible from EM.
- `LordKuper.Common.CustomStats` — custom stat-def infrastructure consumed by `ToolCache`, weapon caches, weapon/tool rules, and `LegacyCustomStatDefs` (legacy name normalization).
- `LordKuper.Common.Filters.Limits` — limit/range model reused across rules and loadouts: `StatLimit` (with `StatDef`, `StatDefName`, `MinValue`, `MaxValue`), and the pawn-side limits `PawnCapacityLimit`, `PawnSkillLimit`, used by `ItemRule`, `Loadout`, weapon/tool rules.
- `LordKuper.Common.UI.Windows.GetWindowSize(preferred, max)` — window-size helper; replaced EM's local `UiHelpers.GetWindowSize` (RU-6 / sprint 001).
- `LordKuper.Common.UI.Widgets.ThingIconBox` — scrollable thing/ThingDef icon grid used by weapon-rule dialogs.
- `LordKuper.Common.UI.Widgets.WorkTypeThingRuleWidget` — WorkType tab UI widget; `DoWidgetTab(rect, ...)` drives the complete WorkType rule editor tab. Consumed since RU-7 / sprint 001 (replaced EM's inline tab rendering).
- `LordKuper.Common.UI` / other widget types — `Labels`, `Fields`, `Buttons`, `Sections`, `Tabs`, `Layout`, `ScrollView` used by `ManageLoadoutsDialog` and `ManageWeaponRulesDialog`.

## Version-specific notes
- The mod pins the 1.6 Common build (`\1.6\Assemblies`). The runtime dependency is version-scoped to v1.6 in About.xml, so the Common contract is a 1.6-only expectation; older game versions of this mod did not depend on Common.

## Deprecations and breaking changes from prior version
- Common's public surface is the contract; a breaking change upstream surfaces here as a compile error. `LegacyCustomStatDefs.NormalizeStatLimit` exists to migrate legacy custom-stat names that predate the current Common `Filters.Limits` model.

## Project conventions
- Treat Common as read-only upstream: consume `Logger`, `RimWorldTime`, `TimedCache`/`ThingCache`, `StatHelper`, `StatRanges`, `WorkTypeThingRule`, `WorkTypeStatMap`, `Filters.Limits`, `CustomStats`, `UI.Windows`, `UI.Widgets.WorkTypeThingRuleWidget` as-is. Do not reimplement what Common already provides.
- Local `EquipmentManager.Logger` thinly wraps `LordKuper.Common.Logger`, always passing `EquipmentManagerMod.ModId`.
- All caches derive from `ThingCache` (extends `TimedCache`) and take a `RimWorldTime` in `Update`; never roll a bespoke time/cache mechanism.

## Known issues and workarounds
- No training coverage / private API → confirm every member against the sibling repo before relying on it; this doc lists only grep-confirmed usage.
- Resolution depends on the sibling checkout being present at `..\..\rimworld-common` (or `LORDKUPER_COMMON_DIR` set) → build fails fast if the path is missing.
