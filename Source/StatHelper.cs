using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace EquipmentManager
{
    internal static class StatHelper
    {
        internal const string CustomStatPrefix = "EM";
        private static IEnumerable<StatDef> AllStatsDefs => DefaultStatDefs.Union(CustomStatsDefs);

        private static IEnumerable<StatDef> CustomStatsDefs =>
            CustomMeleeWeaponStats.StatDefs.Union(CustomRangedWeaponStats.StatDefs).Union(CustomToolStats.StatDefs);

        public static IEnumerable<StatDef> DefaultPawnStatDefs =>
            DefaultStatDefs.Where(def => PawnCategories.Contains(def.category.defName))
                .OrderBy(def => def.category?.defName ?? string.Empty).ThenBy(def => def.label);

        private static IEnumerable<StatDef> DefaultStatDefs => DefDatabase<StatDef>.AllDefs;

        private static IEnumerable<StatDef> DefaultWeaponStatDefs =>
            DefaultStatDefs.Where(def => WeaponCategories.Contains(def.category.defName));

        public static IReadOnlyList<StatDef> MeleeWeaponStatDefs { get; } = new List<StatDef>(CustomMeleeWeaponStats
            .StatDefs.Union(DefaultWeaponStatDefs).OrderBy(def => def.category?.defName ?? string.Empty)
            .ThenBy(def => def.label));

        private static IEnumerable<string> PawnCategories =>
            new[]
            {
                "Basics", "BasicsImportant", "BasicsPawnImportant", "BasicsPawn", "PawnCombat", "PawnSocial",
                "PawnMisc", "PawnWork"
            };

        public static IReadOnlyList<StatDef> RangedWeaponStatDefs { get; } = new List<StatDef>(CustomRangedWeaponStats
            .StatDefs.Union(DefaultWeaponStatDefs).OrderBy(def => def.category?.defName ?? string.Empty)
            .ThenBy(def => def.label));

        public static IReadOnlyList<StatDef> ToolStatDefs { get; } = new List<StatDef>(CustomToolStats.StatDefs
            .Union(DefaultWeaponStatDefs).OrderBy(def => def.category?.defName ?? string.Empty)
            .ThenBy(def => def.label));

        private static IEnumerable<string> WeaponCategories =>
            new[]
            {
                "Basics", "BasicsImportant", "BasicsNonPawnImportant", "BasicsNonPawn", "Weapon", "Weapon_Ranged",
                "Weapon_Melee", "PawnWork"
            };

        private static IEnumerable<string> WorkCategories => new[] {"PawnWork"};

        public static IEnumerable<StatDef> WorkTypeStatDefs { get; } = DefaultStatDefs
            .Where(def => WorkCategories.Contains(def.category.defName))
            .OrderBy(def => def.category?.defName ?? string.Empty).ThenBy(def => def.label);

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

        public static float NormalizeValue(float value, FloatRange range)
        {
            value = Mathf.Clamp(value, range.min, range.max);
            var valueRange = range.max - range.min;
            if (Math.Abs(valueRange) < 0.001f) { return 0f; }
            var normalizedValue = (value - range.min) / valueRange;
            return range.min < 0 && range.max < 0 ? -1 + normalizedValue :
                range.min < 0 && range.max > 0 ? -1 + (2 * normalizedValue) : normalizedValue;
        }
    }
}