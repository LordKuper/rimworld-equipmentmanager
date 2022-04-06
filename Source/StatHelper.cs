using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Strings = EquipmentManager.Resources.Strings.Stats;

namespace EquipmentManager
{
    internal static class StatHelper
    {
        private const string CustomRangedWeaponStatCategory = "RangedWeapons";
        private const string CustomStatPrefix = "EM";
        private const string CustomToolStatCategory = "Tools";
        private static EquipmentManagerGameComponent _equipmentManager;
        private static IEnumerable<StatDef> AllStatsDefs => DefaultStatDefs.Union(CustomStatsDefs);

        private static IEnumerable<StatDef> CustomRangedWeaponStatDefs { get; } =
            CustomRangedWeaponStatsDefNames.Select(defName => new StatDef
            {
                defName = defName,
                label = Strings.GetStatLabel(defName),
                description = Strings.GetStatDescription(defName),
                category = RangedWeaponCategoryDef
            });

        private static IEnumerable<string> CustomRangedWeaponStatsDefNames =>
            Enum.GetValues(typeof(CustomRangedWeaponStat)).OfType<CustomRangedWeaponStat>()
                .Select(GetCustomRangedWeaponStatDefName);

        private static IEnumerable<string> CustomStatsDefNames =>
            CustomRangedWeaponStatsDefNames.Union(CustomToolStatsDefNames);

        private static IEnumerable<StatDef> CustomStatsDefs => CustomRangedWeaponStatDefs.Union(CustomToolStatDefs);

        private static IEnumerable<StatDef> CustomToolStatDefs { get; } = CustomToolStatsDefNames.Select(defName =>
            new StatDef
            {
                defName = defName,
                label = Strings.GetStatLabel(defName),
                description = Strings.GetStatDescription(defName),
                category = ToolCategoryDef
            });

        private static IEnumerable<string> CustomToolStatsDefNames =>
            Enum.GetValues(typeof(CustomToolStat)).OfType<CustomToolStat>().Select(GetCustomToolStatDefName);

        private static IEnumerable<StatDef> DefaultStatDefs => DefDatabase<StatDef>.AllDefs;

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public static IReadOnlyList<StatDef> MeleeWeaponStatDefs { get; } = new List<StatDef>(DefaultStatDefs);

        private static StatCategoryDef RangedWeaponCategoryDef { get; } = new StatCategoryDef
        {
            defName = $"{CustomStatPrefix}_{CustomRangedWeaponStatCategory}",
            label = $"{CustomStatPrefix}_{CustomRangedWeaponStatCategory}"
        };

        public static IReadOnlyList<StatDef> RangedWeaponStatDefs { get; } = new List<StatDef>(
            CustomRangedWeaponStatDefs.Union(DefaultStatDefs));

        private static StatCategoryDef ToolCategoryDef { get; } = new StatCategoryDef
        {
            defName = $"{CustomStatPrefix}_{CustomToolStatCategory}",
            label = $"{CustomStatPrefix}_{CustomToolStatCategory}"
        };

        public static IReadOnlyList<StatDef> ToolStatDefs { get; } =
            new List<StatDef>(CustomToolStatDefs.Union(DefaultStatDefs));

        public static IEnumerable<StatDef> WorkTypeStatDefs { get; } = DefaultStatDefs;

        public static string GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat stat)
        {
            return $"{CustomStatPrefix}_{CustomRangedWeaponStatCategory}_{stat}";
        }

        private static string GetCustomRangedWeaponStatName(string defName)
        {
            var categoryPrefix = $"{CustomStatPrefix}_{CustomRangedWeaponStatCategory}_";
            return defName.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase)
                ? defName.Substring(categoryPrefix.Length)
                : null;
        }

        private static float GetCustomStatValue([NotNull] ThingDef def, [NotNull] StatDef statDef,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimworldTime time)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
            if (Enum.TryParse(GetCustomRangedWeaponStatName(statDef.defName),
                    out CustomRangedWeaponStat rangedWeaponStat))
            {
                var cache = EquipmentManager.GetRangedWeaponDefCache(def, time);
                switch (rangedWeaponStat)
                {
                    case CustomRangedWeaponStat.Dpsa:
                        return cache.Dpsa;
                    case CustomRangedWeaponStat.DpsaClose:
                        return cache.DpsaClose;
                    case CustomRangedWeaponStat.DpsaShort:
                        return cache.DpsaShort;
                    case CustomRangedWeaponStat.DpsaMedium:
                        return cache.DpsaMedium;
                    case CustomRangedWeaponStat.DpsaLong:
                        return cache.DpsaLong;
                    case CustomRangedWeaponStat.Range:
                        return cache.MaxRange;
                    case CustomRangedWeaponStat.Warmup:
                        return cache.Warmup;
                    case CustomRangedWeaponStat.BurstShotCount:
                        return cache.BurstShotCount;
                    case CustomRangedWeaponStat.TicksBetweenBurstShots:
                        return cache.TicksBetweenBurstShots;
                    case CustomRangedWeaponStat.ArmorPenetration:
                        return cache.ArmorPenetration;
                    case CustomRangedWeaponStat.StoppingPower:
                        return cache.StoppingPower;
                    case CustomRangedWeaponStat.Damage:
                        return cache.Damage;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(statDef));
                }
            }
            if (Enum.TryParse(GetCustomToolStatName(statDef.defName), out CustomToolStat toolStat))
            {
                var cache = EquipmentManager.GetToolDefCache(def, time);
                switch (toolStat)
                {
                    case CustomToolStat.WorkType:
                        return workTypeDefs == null
                            ? 0f
                            : cache.GetWorkTypesScore(workTypeDefs.Select(workTypeDef => workTypeDef.defName));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(statDef));
                }
            }
            Log.Error($"Equipment Manager: Tried to evaluate unknown custom stat ({statDef.defName})");
            return 0f;
        }

        private static float GetCustomStatValue([NotNull] Thing thing, [NotNull] StatDef statDef,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
            if (Enum.TryParse(GetCustomRangedWeaponStatName(statDef.defName), out CustomRangedWeaponStat stat))
            {
                var cache = EquipmentManager.GetRangedWeaponCache(thing, time);
                switch (stat)
                {
                    case CustomRangedWeaponStat.Dpsa:
                        return cache.Dpsa;
                    case CustomRangedWeaponStat.DpsaClose:
                        return cache.DpsaClose;
                    case CustomRangedWeaponStat.DpsaShort:
                        return cache.DpsaShort;
                    case CustomRangedWeaponStat.DpsaMedium:
                        return cache.DpsaMedium;
                    case CustomRangedWeaponStat.DpsaLong:
                        return cache.DpsaLong;
                    case CustomRangedWeaponStat.Range:
                        return cache.MaxRange;
                    case CustomRangedWeaponStat.Warmup:
                        return cache.Warmup;
                    case CustomRangedWeaponStat.BurstShotCount:
                        return cache.BurstShotCount;
                    case CustomRangedWeaponStat.TicksBetweenBurstShots:
                        return cache.TicksBetweenBurstShots;
                    case CustomRangedWeaponStat.ArmorPenetration:
                        return cache.ArmorPenetration;
                    case CustomRangedWeaponStat.StoppingPower:
                        return cache.StoppingPower;
                    case CustomRangedWeaponStat.Damage:
                        return cache.Damage;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(statDef));
                }
            }
            if (Enum.TryParse(GetCustomToolStatName(statDef.defName), out CustomToolStat toolStat))
            {
                var cache = EquipmentManager.GetToolCache(thing, time);
                switch (toolStat)
                {
                    case CustomToolStat.WorkType:
                        if (!workTypeDefs.Any())
                        {
                            throw new ArgumentException("At least one work type must be passed", nameof(workTypeDefs));
                        }
                        return cache.GetWorkTypesScore(workTypeDefs.Select(workTypeDef => workTypeDef.defName));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(statDef));
                }
            }
            Log.Error($"Equipment Manager: Tried to evaluate unknown custom stat ({statDef.defName})");
            return 0f;
        }

        public static string GetCustomToolStatDefName(CustomToolStat stat)
        {
            return $"{CustomStatPrefix}_{CustomToolStatCategory}_{stat}";
        }

        private static string GetCustomToolStatName(string defName)
        {
            var categoryPrefix = $"{CustomStatPrefix}_{CustomToolStatCategory}_";
            return defName.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase)
                ? defName.Substring(categoryPrefix.Length)
                : null;
        }

        public static StatDef GetStatDef(string defName)
        {
            return AllStatsDefs.FirstOrDefault(def => def.defName.Equals(defName, StringComparison.OrdinalIgnoreCase));
        }

        private static float GetStatValue([NotNull] ThingDef def, [NotNull] StatDef statDef, RimworldTime time,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs = null)
        {
            try
            {
                return def == null ? throw new ArgumentNullException(nameof(def)) :
                    statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                    IsCustomStat(statDef.defName) ? GetCustomStatValue(def, statDef, workTypeDefs, time) :
                    (def.statBases?.Find(modifier => modifier.stat == statDef)?.value ?? 0f) +
                    (def.equippedStatOffsets?.Find(modifier => modifier.stat == statDef)?.value ?? 0f);
            }
            catch { return 0f; }
        }

        public static float GetStatValue([NotNull] Thing thing, [NotNull] StatDef statDef, RimworldTime time,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs = null)
        {
            try
            {
                return thing == null ? throw new ArgumentNullException(nameof(thing)) :
                    statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                    IsCustomStat(statDef.defName) ? GetCustomStatValue(thing, statDef, workTypeDefs, time) :
                    thing.GetStatValue(statDef) +
                    (thing.def.equippedStatOffsets?.Find(modifier => modifier.stat == statDef)?.value ?? 0f);
            }
            catch { return 0f; }
        }

        public static float GetStatValueDeviation([NotNull] ThingDef def, [NotNull] StatDef statDef, RimworldTime time,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs = null)
        {
            return def == null ? throw new ArgumentNullException(nameof(def)) :
                statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                IsCustomStat(statDef.defName) ? GetCustomStatValue(def, statDef, workTypeDefs, time) :
                GetStatValue(def, statDef, time, workTypeDefs) - statDef.defaultBaseValue;
        }

        public static float GetStatValueDeviation([NotNull] Thing thing, [NotNull] StatDef statDef, RimworldTime time,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs = null)
        {
            return thing == null ? throw new ArgumentNullException(nameof(thing)) :
                statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                IsCustomStat(statDef.defName) ? GetCustomStatValue(thing, statDef, workTypeDefs, time) :
                GetStatValue(thing, statDef, time, workTypeDefs) - statDef.defaultBaseValue;
        }

        private static bool IsCustomStat(string defName)
        {
            return CustomStatsDefNames.Contains(defName);
        }
    }
}