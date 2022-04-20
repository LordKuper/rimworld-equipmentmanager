using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal static class StatHelper
    {
        internal const string CustomStatPrefix = "EM";
        private static IEnumerable<StatDef> AllStatsDefs => DefaultStatDefs.Union(CustomStatsDefs);

        private static IEnumerable<StatDef> CustomStatsDefs =>
            CustomMeleeWeaponStats.StatDefs.Union(CustomRangedWeaponStats.StatDefs).Union(CustomToolStats.StatDefs);

        private static IEnumerable<StatDef> DefaultStatDefs => DefDatabase<StatDef>.AllDefs;

        public static IReadOnlyList<StatDef> MeleeWeaponStatDefs { get; } =
            new List<StatDef>(CustomMeleeWeaponStats.StatDefs.Union(DefaultStatDefs));

        public static IReadOnlyList<StatDef> RangedWeaponStatDefs { get; } = new List<StatDef>(
            CustomRangedWeaponStats.StatDefs.Union(DefaultStatDefs));

        public static IReadOnlyList<StatDef> ToolStatDefs { get; } =
            new List<StatDef>(CustomToolStats.StatDefs.Union(DefaultStatDefs));

        public static IEnumerable<StatDef> WorkTypeStatDefs { get; } = DefaultStatDefs;

        public static StatDef GetStatDef(string defName)
        {
            return AllStatsDefs.FirstOrDefault(def => def.defName.Equals(defName, StringComparison.OrdinalIgnoreCase));
        }

        public static float GetStatValue([NotNull] Thing thing, [NotNull] StatDef statDef)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
            try
            {
                return thing.GetStatValue(statDef) +
                    (thing.def.equippedStatOffsets?.Find(modifier => modifier.stat == statDef)?.value ?? 0f);
            }
            catch (Exception exception)
            {
                Log.Warning(
                    $"Equipment Manager: Could not evaluate stat '{statDef.LabelCap}' of {thing.LabelCapNoCount}: {exception.Message}");
                return 0f;
            }
        }

        private static float GetStatValue([NotNull] ThingDef def, [NotNull] StatDef statDef)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
            try
            {
                return (def.statBases?.Find(modifier => modifier.stat == statDef)?.value ?? 0f) +
                    (def.equippedStatOffsets?.Find(modifier => modifier.stat == statDef)?.value ?? 0f);
            }
            catch (Exception exception)
            {
                Log.Warning(
                    $"Equipment Manager: Could not evaluate stat '{statDef.LabelCap}' of {def.LabelCap}: {exception.Message}");
                return 0f;
            }
        }

        public static float GetStatValueDeviation([NotNull] ThingDef def, [NotNull] StatDef statDef)
        {
            return def == null ? throw new ArgumentNullException(nameof(def)) :
                statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                GetStatValue(def, statDef) - statDef.defaultBaseValue;
        }

        public static float GetStatValueDeviation([NotNull] Thing thing, [NotNull] StatDef statDef)
        {
            return thing == null ? throw new ArgumentNullException(nameof(thing)) :
                statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                GetStatValue(thing, statDef) - statDef.defaultBaseValue;
        }
    }
}