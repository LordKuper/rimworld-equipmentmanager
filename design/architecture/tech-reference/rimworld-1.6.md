---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# RimWorld game API (Assembly-CSharp) @ 1.6 (game-bound; supported 1.3–1.6)

## Canonical source
- Official docs: https://rimworldwiki.com/wiki/Modding (modding wiki); decompiled `Assembly-CSharp.dll` is the authoritative API surface
- Last verified: 2026-06-06
- Note: the wiki returned HTTP 403 on automated fetch at verification time; API facts below are extracted from this repo's actual usages, not from the wiki. Decompile `$(RimWorldManagedDir)\Assembly-CSharp.dll` to confirm signatures.

## Reference nature (game-bound, not a package)
- `Assembly-CSharp.dll` is RimWorld itself. There is **no NuGet package and no semantic version**; the "version" tracks the game build (1.6). It is referenced by `HintPath` from `$(RimWorldManagedDir)` (`$(RimWorldDir)\RimWorldWin64_Data\Managed`), `Private=False` (never copied to output — the game already loads it).
- The mod's `About/About.xml` declares `supportedVersions` 1.3, 1.4, 1.5, 1.6. Source targets `net48` to match RimWorld's Mono runtime.
- This API is **not reliably present in LLM training data** and changes between game versions without published changelogs. Treat any recalled signature as unverified until checked against the decompiled assembly.

## API surface used in project
- `Verse.Mod` / `Verse.ModContentPack`: mod entry point. `EquipmentManagerMod : Mod` ctor receives `ModContentPack`, constructs Harmony and runs `PatchAll`. (`EquipmentManagerMod.cs`)
- `Verse.GameComponent`: per-save component. `EquipmentManagerGameComponent : GameComponent` holds all rules/loadouts and overrides `ExposeData()`. Retrieved via `Current.Game.GetComponent<EquipmentManagerGameComponent>()`.
- `Verse.MapComponent`: per-map component. `EquipmentManagerMapComponent(Map map) : MapComponent(map)` drives the equip/assign loop.
- Save/load (Scribe): `ExposeData()` overrides; `Scribe.mode == LoadSaveMode.Saving`; `Scribe_Collections.Look(ref list, name, LookMode.Deep)`. Every persisted rule/loadout type implements `ExposeData`.
- Def system: `Verse.Def`, `RimWorld.StatDef`, `Verse.ThingDef`, `Verse.WorkTypeDef`, `RimWorld.SkillDef`, `Verse.Tool`. Custom `PawnColumnDef` (`EM_Loadout`) defined via `DefOfs/PawnColumnDef.cs` and injected into `PawnTableDefOf.Assign.columns`.
- `Verse.DefGenerator.GenerateImpliedDefs_PreResolve`: patched (postfix) to insert the loadout column after the vanilla Outfit column. (`Patches/DefGeneratorPatch.cs`)
- Pawn/Thing: `Verse.Pawn`, `Verse.Thing`, `Verse.ThingWithComps`, `Pawn.LabelShortCap`. Used throughout assignment logic in `EquipmentManagerMapComponent`.
- `Verse.PawnColumnWorker`: `PawnColumnWorkers/Loadout : PawnColumnWorker` renders the Assign-tab loadout column.
- `Verse.Window`: all dialogs (`ManageLoadoutsDialog`, `ManageWeaponRulesDialog`, `LogDialog`, `ImportLoadoutsDialog`) derive from `Window`.
- `Verse.Log` (`Log.Message`, `Log.Warning`): boot diagnostics and patch warnings.
- `Verse.LoadedModManager.RunningModsListForReading`: runtime detection of CombatExtended and VanillaFactionsExpanded.Core by `PackageId`.
- `Find.CurrentMap` / `Find.CurrentMap` map context for cache time lookups.

## Version-specific notes
- Targets game 1.6; same source compiles for 1.3–1.6. Build output lands in `1.6/Assemblies/`.
- `LordKuper.Common` dependency is declared **only for v1.6** in `About.xml` (`modDependenciesByVersion`), so the Common-backed surface is a 1.6 expectation.
- Mono/.NET Framework 4.8 runtime: no `Span<T>`-heavy or modern-BCL-only APIs at runtime even though `LangVersion latest` is set (collection expressions like `[]` are compiler features, fine on net48).

## Deprecations and breaking changes from prior version
- Cross-version Def renames and method-signature changes occur between 1.3→1.6 without published notes; verify each patched target (`DefGenerator.GenerateImpliedDefs_PreResolve`, `PawnTableDefOf.Assign`) against the target game build before bumping.
- `PawnColumnDef` injection relies on a vanilla column named `Outfit`; if the vanilla table layout changes, the postfix logs a warning and no-ops rather than crashing.

## Project conventions
- Never hardcode the mod id; use `EquipmentManagerMod.ModId` (`"LordKuper.EquipmentManager"`).
- Game-API access to per-save state always goes through `Current.Game.GetComponent<EquipmentManagerGameComponent>()`, cached in a `??=` field.
- Optional integrations (CE, VFE) are detected by `PackageId` at boot and bound reflectively (see `lib-harmony-2.4.2.md`), never hard-referenced.
- All save/load goes through `ExposeData` + `Scribe_*`; legacy stat-def names are normalized on load (`LegacyCustomStatDefs.NormalizeStatLimit`).

## Known issues and workarounds
- API surface not in training data and wiki not machine-fetchable → confirm signatures by decompiling the local `Assembly-CSharp.dll`; do not trust recalled API.
- Static game state makes unit testing fragile → tests use `StateIsolationTestBase` / `RimWorldAssemblyResolverFixture` to isolate and resolve RimWorld assemblies.
