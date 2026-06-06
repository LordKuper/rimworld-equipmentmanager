---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# SimpleSidearms @ 1.6 (game-bound mod; petetimessix.simplesidearms)

## Canonical source
- Steam Workshop id 927155256 (`petetimessix.simplesidearms`); source: https://github.com/PeteTimesSix/SimpleSidearms
- Last verified: 2026-06-06
- MEDIUM risk: a community mod API not present in LLM training data. Members below were confirmed by grepping EquipmentManager's source; verify signatures against the decompiled `SimpleSidearms.dll` or the GitHub source.

## Reference nature (compile-only; hard runtime dependency)
- Referenced by `HintPath` `$(SimpleSidearmsDir)\SimpleSidearms.dll`, `Private=False`. `SimpleSidearmsDir` defaults to the Workshop path `...\workshop\content\294100\927155256\v1.6\Assemblies` (overridable via `SIMPLE_SIDEARMS_DIR`).
- **Hard runtime dependency** (unlike CombatExtended/VFE which are optional + reflective): declared in `About/About.xml` as a `modDependency` and `loadAfter`. SimpleSidearms is the integration target for sidearm loadouts, so it is referenced directly (not via `AccessTools`).

## API surface used in project (confirmed from source; all in `EquipmentManagerMapComponent.cs` + `WorkTypeRule.cs`)
- Namespaces: `PeteTimesSix.SimpleSidearms`, `PeteTimesSix.SimpleSidearms.Utilities`, `SimpleSidearms.rimworld`.
- `CompSidearmMemory.GetMemoryCompForPawn(pawn)` — fetch the pawn's sidearm memory component; primary integration entry point.
- `CompSidearmMemory.RememberedWeapons` — collection of remembered sidearms; queried with `.Contains(...)`. The mod reads/checks this to decide what to remember/forget when applying a loadout.
- `StatCalculator.CanPickupSidearmInstance((ThingWithComps)weapon, pawn, out _)` — eligibility check before assigning a weapon as a sidearm.
- Extension `thing.toThingDefStuffDefPair()` (from `Utilities`) → `ThingDefStuffDefPair`, the key type used in `RememberedWeapons`. Used pervasively to translate `Thing` ↔ memory entries.

## Version-specific notes
- Pinned to the v1.6 Workshop build (`\v1.6\Assemblies`). SimpleSidearms is game-version-bound; the v1.6 assembly is the supported target.
- `RememberedWeapons` is keyed by `ThingDefStuffDefPair` (def + stuff), not by `Thing` instance — equality is by def/material, so instance identity is irrelevant when matching.

## Deprecations and breaking changes from prior version
- SimpleSidearms internals (the `CompSidearmMemory` / `StatCalculator` surface) have shifted across RimWorld versions without published changelogs; re-verify these members when bumping the supported game version.

## Project conventions
- Because SimpleSidearms is a hard dependency, it is referenced directly (typed calls), in contrast to optional mods (CE/VFE) which are bound reflectively via Harmony `AccessTools`.
- All sidearm interaction is centralized in `EquipmentManagerMapComponent`; convert `Thing` to `ThingDefStuffDefPair` via `toThingDefStuffDefPair()` before touching `RememberedWeapons`.
- Always gate assignment on `StatCalculator.CanPickupSidearmInstance` before mutating sidearm memory.

## Known issues and workarounds
- API not in training data → confirm against decompiled `SimpleSidearms.dll` / GitHub source before relying on any member.
- Recent fix in this area: `EquipSecondary` job is skipped for sidearms a pawn already carries (commit 25bda62) — keep new assignment paths consistent with that guard to avoid redundant jobs.
