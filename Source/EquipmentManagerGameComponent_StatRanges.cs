using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private const float StatTolerance = 0.001f;
        private readonly HashSet<Thing> _cachedWeapons = new HashSet<Thing>();
        private Dictionary<string, FloatRange> _statRanges;

        private void ExposeData_StatRanges()
        {
            Scribe_Collections.Look(ref _statRanges, "StatRanges", LookMode.Value, LookMode.Value);
        }

        public float NormalizeStatValue(StatDef stat, float deviation)
        {
            UpdateStatRange(stat, deviation);
            var range = _statRanges[stat.defName];
            var valueRange = range.max - range.min;
            if (Math.Abs(valueRange) < StatTolerance) { return 0f; }
            var normalizedValue = (deviation - range.min) / valueRange;
            return range.min < 0 && range.max < 0 ? -1 + normalizedValue :
                range.min < 0 && range.max > 0 ? -1 + (2 * normalizedValue) : normalizedValue;
        }

        public void UpdateStatRange(StatDef stat, float deviation)
        {
            if (_statRanges == null) { _statRanges = new Dictionary<string, FloatRange>(); }
            if (!_statRanges.ContainsKey(stat.defName))
            {
                _statRanges[stat.defName] = new FloatRange(deviation, deviation);
            }
            var range = _statRanges[stat.defName];
            if (range.min > deviation)
            {
                range.min = deviation;
                _statRanges[stat.defName] = range;
            }
            if (range.max < deviation)
            {
                range.max = deviation;
                _statRanges[stat.defName] = range;
            }
        }

        public void UpdateStatRanges([NotNull] Thing weapon, RimworldTime time)
        {
            if (weapon == null) { throw new ArgumentNullException(nameof(weapon)); }
            if (!_cachedWeapons.Add(weapon)) { return; }
            if (weapon.def.IsRangedWeapon)
            {
                foreach (var rule in GetRangedWeaponRules()) { rule.UpdateStatRanges(weapon, time); }
            }
            else
            {
                foreach (var rule in GetMeleeWeaponRules()) { rule.UpdateStatRanges(weapon, time); }
            }
            foreach (var rule in GetToolRules())
            {
                rule.UpdateStatRanges(weapon, WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList(), time);
            }
        }
    }
}