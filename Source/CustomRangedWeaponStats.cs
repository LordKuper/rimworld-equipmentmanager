using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;

namespace EquipmentManager;

internal enum CustomRangedWeaponStat
{
    Dpsa,
    DpsaClose,
    DpsaShort,
    DpsaMedium,
    DpsaLong,
    Range,
    Warmup,
    BurstShotCount,
    TicksBetweenBurstShots,
    ArmorPenetration,
    StoppingPower,
    Damage,
    TechLevel
}

internal static class CustomRangedWeaponStats
{
    private const string Category = "RangedWeapons";

    private static StatCategoryDef CategoryDef { get; } = new()
    {
        defName = $"{StatHelper.CustomStatPrefix}_{Category}",
        label = $"{StatHelper.CustomStatPrefix}_{Category}"
    };

    [NotNull]
    private static IEnumerable<string> StatDefNames =>
        Enum.GetValues(typeof(CustomRangedWeaponStat)).OfType<CustomRangedWeaponStat>()
            .Select(GetStatDefName);

    public static IEnumerable<StatDef> StatDefs { get; } = StatDefNames.Select(defName =>
        new StatDef
        {
            defName = defName,
            label = Resources.Strings.Stats.GetStatLabel(defName),
            description = Resources.Strings.Stats.GetStatDescription(defName),
            category = CategoryDef
        });

    [NotNull]
    public static string GetStatDefName(CustomRangedWeaponStat stat)
    {
        return $"{StatHelper.CustomStatPrefix}_{Category}_{stat}";
    }

    [CanBeNull]
    public static string GetStatName([NotNull] string defName)
    {
        const string categoryPrefix = $"{StatHelper.CustomStatPrefix}_{Category}_";
        return defName.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase)
            ? defName.Substring(categoryPrefix.Length)
            : null;
    }

    public static bool IsCustomStat(string defName)
    {
        return StatDefNames.Contains(defName);
    }
}