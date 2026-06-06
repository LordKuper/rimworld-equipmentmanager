---
responsibility:
  owns: project-owner custom rules read during design and design-review phases
  excludes: universal rules, code/test rules
  delegates_to: custom-common-rules.md (all phases), custom-coding-rules.md (impl/impl-review)
---

# Custom Design Rules

Inherited from the `LordKuper.Common` parent library and adapted to EquipmentManager.

## Modding & patchability

- Harmony-patchable: prefer small methods, stable public entry points, predictable side effects.
- Don't seal mod extension points without strong reason.
- No static constructors with heavy side effects.
- EquipmentManager patches RimWorld def generation and the pawn-table loadout column (see `Source/EquipmentManager/Patches/`, `PawnColumnWorkers/`) — patch targets are an integration surface. Prefer narrow, stable patch points; document why each patch exists.

## Data-driven over hardcoded

- Stat / balance / tuning values come from RimWorld `Def`s or mod settings, never hardcoded literals in code. ADRs/PRDs introducing new tunables MUST specify the Def/settings surface, not literal constants.
- Custom stat defs are generated (`EquipmentManagerStatDefs`, `DefGeneratorPatch`); new tunables should follow that generation pattern rather than ad-hoc constants.

## Determinism

- Loadout assignment, weapon/tool rule evaluation, scoring, filtering, and caching logic (`ItemCache`, `PawnCache`, `RangedWeaponCache`, `MeleeWeaponCache`, `ToolCache`): same inputs → same outputs.
- No time- or order-dependent behavior in core logic unless explicitly required.

## Compatibility

- EquipmentManager integrates with other mods: CombatExtended (`CombatExtendedHelper`), SimpleSidearms (sidearm equipping), and VanillaFactionsExpanded.Core (shield usability). Detection happens at mod init (`EquipmentManagerMod`). Design changes touching weapon/tool selection, equipping jobs, or stat surfaces MUST consider these compatibility shims and call out impact in the ADR.
