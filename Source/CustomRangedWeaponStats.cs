using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace EquipmentManager
{
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

        private static StatCategoryDef CategoryDef { get; } = new StatCategoryDef
        {
            defName = $"{StatHelper.CustomStatPrefix}_{Category}",
            label = $"{StatHelper.CustomStatPrefix}_{Category}"
        };

        private static IEnumerable<string> StatDefNames =>
            Enum.GetValues(typeof(CustomRangedWeaponStat)).OfType<CustomRangedWeaponStat>().Select(GetStatDefName);

        public static IEnumerable<StatDef> StatDefs { get; } = StatDefNames.Select(defName =>
            new StatDef
            {
                defName = defName,
                label = Resources.Strings.Stats.GetStatLabel(defName),
                description = Resources.Strings.Stats.GetStatDescription(defName),
                category = CategoryDef
            });

        public static string GetStatDefName(CustomRangedWeaponStat stat)
        {
            return $"{StatHelper.CustomStatPrefix}_{Category}_{stat}";
        }

        public static string GetStatName(string defName)
        {
            var categoryPrefix = $"{StatHelper.CustomStatPrefix}_{Category}_";
            return defName.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase)
                ? defName.Substring(categoryPrefix.Length)
                : null;
        }

        public static bool IsCustomStat(string defName)
        {
            return StatDefNames.Contains(defName);
        }
    }
}