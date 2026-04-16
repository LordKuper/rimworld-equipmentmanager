using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;

namespace EquipmentManager;

internal enum CustomMeleeWeaponStat
{
    ArmorPenetration,
    DpsSharp,
    DpsBlunt,
    TechLevel
}

internal static class CustomMeleeWeaponStats
{
    private const string Category = "MeleeWeapons";

    private static StatCategoryDef CategoryDef { get; } = new()
    {
        defName = $"{StatHelper.CustomStatPrefix}_{Category}",
        label = $"{StatHelper.CustomStatPrefix}_{Category}"
    };

    [NotNull]
    private static IEnumerable<string> StatDefNames =>
        Enum.GetValues(typeof(CustomMeleeWeaponStat)).OfType<CustomMeleeWeaponStat>()
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
    public static string GetStatDefName(CustomMeleeWeaponStat stat)
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