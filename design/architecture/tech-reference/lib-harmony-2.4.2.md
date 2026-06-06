---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# Lib.Harmony @ 2.4.2

## Canonical source
- Official docs: https://harmony.pardeike.net/
- Last verified: 2026-06-06
- 2.4.2 confirmed latest stable (published 2025-11-13).

## Reference nature (compile-only)
- Referenced as a NuGet `PackageReference Include="Lib.Harmony" Version="2.4.2"` with `PrivateAssets=all`, `ExcludeAssets=runtime`, `IncludeAssets=compile; build; native; contentfiles; analyzers; buildtransitive`.
- **Compile-time only.** The runtime Harmony assembly is provided by the `brrainz.harmony` mod (declared in `About/About.xml` as a `modDependency`, `loadAfter`). The mod must NOT ship its own Harmony DLL — doing so would conflict with the shared runtime instance every RimWorld mod uses.

## API surface used in project
- `HarmonyLib.Harmony` (ctor + `PatchAll`): `new Harmony(EquipmentManagerMod.ModId)` then `harmony.PatchAll(Assembly.GetExecutingAssembly())` in the `Mod` ctor. Single id, single PatchAll call. (`EquipmentManagerMod.cs`)
- `[HarmonyPatch]` attribute + `Postfix`: `Patches/DefGeneratorPatch.cs` declares `[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]` with a `public static void Postfix()` that injects the loadout column.
- `HarmonyLib.AccessTools` — used heavily for reflective binding to optional mods (CombatExtended, VanillaFactionsExpanded.Core) so they are never hard-referenced:
  - `AccessTools.TypeByName("...")` — resolve types from optional mods (`CombatExtended.ProjectilePropertiesCE`, `VEF.Apparels.ShieldUtility`, `CombatExtended.CompAmmoUser`, etc.).
  - `AccessTools.Method(...)`, `AccessTools.PropertyGetter(...)`, `AccessTools.Field(...)`.
  - `AccessTools.FieldRef<TObject,TField>` / `AccessTools.FieldRefAccess<T>(type, name)` — cached field accessors (`CombatExtendedHelper`, `MeleeWeaponCache`, `RangedWeaponCache`).
- `AccessTools.MethodDelegate<TDelegate>(method, ...)` — bind a strongly-typed delegate to a reflected method:
  - `MeleeWeaponRule.UsableWithShieldsDelegate` ← `VEF.Apparels.ShieldUtility.UsableWithShields`.
  - `CombatExtendedHelper.EnableAmmoSystemDelegate` ← CE settings property getter.
  - `RangedWeaponCache.AmmoUserPropsDelegate` ← CE `CompAmmoUser` props getter.

## Version-specific notes
- 2.4.x is the current major line; `Lib.Harmony` package merges dependencies into a self-contained assembly (relevant only at compile; runtime comes from brrainz.harmony).
- Targets net48 / Mono — compatible with RimWorld's runtime.

## Deprecations and breaking changes from prior version
- No project-affecting breaks observed within the 2.x line for the APIs used here (`Harmony`, `PatchAll`, `[HarmonyPatch]`, `AccessTools`, `MethodDelegate`). Keep the compile-time package version aligned with the Harmony runtime shipped by brrainz.harmony to avoid signature drift.

## Project conventions
- One Harmony instance, id = `EquipmentManagerMod.ModId`; one `PatchAll(Assembly.GetExecutingAssembly())` at boot.
- Patch classes live under `Patches/`, are `internal static`, and carry `[UsedImplicitly]` so analyzers don't flag them.
- **Optional-mod integration is always reflective** via `AccessTools` + `MethodDelegate`/`FieldRef`, gated by `LoadedModManager` package-id detection. Never add a hard assembly reference to CombatExtended or VFE.
- Reflected delegates/field-refs are resolved once during `Initialize` and cached in static fields, never per-call.

## Known issues and workarounds
- Shipping a runtime Harmony copy breaks the shared instance → `ExcludeAssets=runtime` enforces compile-only; runtime stays with brrainz.harmony.
- `AccessTools.TypeByName` returns null when the optional mod is absent → all reflective binding is behind `PackageId` detection so missing mods are a no-op, not a crash.
