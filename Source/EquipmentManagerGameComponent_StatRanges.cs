using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private Dictionary<string, FloatRange> _statRanges;

        private void ExposeData_StatRanges()
        {
            Scribe_Collections.Look(ref _statRanges, "StatRanges", LookMode.Value, LookMode.Value);
        }

        private void InitializeStatRanges()
        {
            _statRanges = new Dictionary<string, FloatRange>();
            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(def => def.IsWeapon))
            {
                var weapon = def.MadeFromStuff
                    ? ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def))
                    : ThingMaker.MakeThing(def);
                var time = new RimworldTime(0, 0, 0);
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

        public float NormalizeStatValue(StatDef stat, float value)
        {
            UpdateStatRange(stat, value);
            return StatHelper.NormalizeValue(value, _statRanges[stat.defName]);
        }

        public void UpdateStatRange(StatDef stat, float value)
        {
            if (_statRanges == null) { InitializeStatRanges(); }
            if (!_statRanges.ContainsKey(stat.defName)) { _statRanges[stat.defName] = new FloatRange(value, value); }
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
        }
    }
}