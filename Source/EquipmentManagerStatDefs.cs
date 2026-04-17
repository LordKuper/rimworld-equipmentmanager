using System.Collections.Generic;
using LordKuper.Common;
using LordKuper.Common.Helpers;
using RimWorld;

namespace EquipmentManager;

internal static class EquipmentManagerStatDefs
{
    public static IReadOnlyList<StatDef> DefaultPawnStatDefs { get; } =
    [
        ..StatHelper.GetStatsByCategory(StatCategory.Pawn)
    ];

    public static IReadOnlyList<StatDef> MeleeWeaponStatDefs { get; } =
    [
        ..StatHelper.GetStatsByCategory(StatCategory.WeaponMelee)
    ];

    public static IReadOnlyList<StatDef> RangedWeaponStatDefs { get; } =
    [
        ..StatHelper.GetStatsByCategory(StatCategory.WeaponRanged)
    ];

    public static IReadOnlyList<StatDef> ToolStatDefs { get; } =
    [
        ..StatHelper.GetStatsByCategory(StatCategory.Tool)
    ];

    public static IReadOnlyList<StatDef> WorkTypeStatDefs { get; } =
    [
        ..StatHelper.GetStatsByCategory(StatCategory.Work)
    ];
}
