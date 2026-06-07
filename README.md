# Equipment Manager

![RimWorld 1.6](https://img.shields.io/badge/RimWorld-1.6-blue)
![Steam Workshop](https://img.shields.io/badge/Steam-Workshop-blue?logo=steam)

Automatically assigns weapons, tools, and loadouts to colonists based on configurable scoring rules. Supports ranged weapons, melee weapons, multi-tool assignments, work-type scoring, and sidearm management via Simple Sidearms.

## What it does

- **Loadouts** — define named equipment sets (primary weapon, sidearms, tools) and auto-assign them to colonists by trait, skill, passion, work capacity, or stat thresholds.
- **Ranged / melee weapon rules** — score weapons by stat weights and limits; auto-equip the best available weapon per colonist.
- **Tool rules** — assign work-type tools per work type (e.g. a mining pick for Miners, a chef's knife for Cooks), with equip-mode control.
- **Work-type rules** — configure per-work-type stat weights that drive tool scoring.
- **Simple Sidearms integration** — assign sidearms alongside primary weapons using the same rule system.
- **Combat Extended integration** — optional reflective integration; ammo count targets work when CE is loaded.

## Dependencies

| Dependency | Required | Notes |
|---|---|---|
| **RimWorld** | Yes | Version 1.3–1.6 (game host) |
| **Harmony** (`brrainz.harmony`) | Yes | Hard runtime dependency; load before this mod |
| **Simple Sidearms** (`petetimessix.simplesidearms`) | Yes | Hard runtime dependency; sidearm assignment requires it |
| **LordKuper.Common** (`LordKuper.Common`) | Yes (1.6 only) | Shared stat/filter/UI library; auto-dependency for RimWorld 1.6 |
| **Combat Extended** | No | Optional soft integration via reflection; load after this mod if used |

## Installing

Install via the [Steam Workshop](https://steamcommunity.com/workshop/) — subscribe to Equipment Manager. Workshop automatically resolves the required dependencies (Harmony, Simple Sidearms, LordKuper.Common).

For manual installation, copy the mod folder into your RimWorld `Mods/` directory and ensure all required dependencies are also installed and load-ordered before Equipment Manager.

## Building from source

### Prerequisites

- .NET SDK 10.x (`dotnet` on PATH)
- RimWorld installed; set `RIMWORLD_DIR` to the game's `Managed/` directory
- `rimworld-common` sibling checkout at `..\rimworld-common` (or set `LORDKUPER_COMMON_DIR`)
- Simple Sidearms DLL; set `SIMPLE_SIDEARMS_DIR` to the folder containing `SimpleSidearms.dll`

### Build

```shell
dotnet build Source/EquipmentManager.slnx -c Release
```

Output goes to `1.6/Assemblies/EquipmentManager.dll`.

### Test

```shell
dotnet test Source/EquipmentManager.slnx
```

### Lint / format

```shell
# Check formatting
dotnet format Source/EquipmentManager.slnx --verify-no-changes

# Apply formatting
dotnet format Source/EquipmentManager.slnx
```

## Version

Current version: `1.6.0.2` (RimWorld 1.6 build).

## License

Source code is provided for reference. All rights reserved by LordKuper unless otherwise stated.
