using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private const float StatTolerance = 0.001f;
        private Dictionary<string, FloatRange> _statRanges = new Dictionary<string, FloatRange>();

        private void ExposeData_StatRanges()
        {
            Scribe_Collections.Look(ref _statRanges, "StatRanges", LookMode.Value, LookMode.Value);
        }

        public float NormalizeStatValue(StatDef stat, float value)
        {
            if (_statRanges == null) { _statRanges = new Dictionary<string, FloatRange>(); }
            if (!_statRanges.ContainsKey(stat.defName))
            {
                _statRanges[stat.defName] = new FloatRange(value, value);
                return 0f;
            }
            var range = _statRanges[stat.defName];
            if (range.min > value)
            {
                range.min = value;
                _statRanges[stat.defName] = range;
            }
            if (range.max < value)
            {
                range.max = value;
                _statRanges[stat.defName] = range;
            }
            var valueRange = range.max - range.min;
            if (Math.Abs(valueRange) < StatTolerance) { return 0f; }
            var normalizedValue = (value - range.min) / valueRange;
            return range.min < 0 && range.max < 0 ? -1 + normalizedValue :
                range.min < 0 && range.max > 0 ? -1 + (2 * normalizedValue) : normalizedValue;
        }
    }
}