using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private readonly Dictionary<ThingDef, RangedWeaponCache> _rangedWeaponDefsCache =
            new Dictionary<ThingDef, RangedWeaponCache>();

        private readonly Dictionary<Thing, RangedWeaponCache> _rangedWeaponsCache =
            new Dictionary<Thing, RangedWeaponCache>();

        private List<RangedWeaponRule> _rangedWeaponRules;

        public RangedWeaponRule AddRangedWeaponRule()
        {
            var id = _rangedWeaponRules.Any() ? _rangedWeaponRules.Max(l => l.Id) + 1 : 0;
            var rangedWeaponRule = new RangedWeaponRule(id, false) {Label = $"{id}"};
            foreach (var statWeight in RangedWeaponRule.DefaultStatWeights)
            {
                if (statWeight.StatDef == null) { continue; }
                rangedWeaponRule.SetStatWeight(statWeight.StatDef, statWeight.Weight, statWeight.Protected);
            }
            foreach (var defName in RangedWeaponRule.DefaultBlacklist)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def != null) { rangedWeaponRule.AddBlacklistedItem(def); }
            }
            rangedWeaponRule.UpdateGloballyAvailableItems();
            _rangedWeaponRules.Add(rangedWeaponRule);
            return rangedWeaponRule;
        }

        public void AddRangedWeaponRule(RangedWeaponRule rangedWeaponRule)
        {
            var existingRule = _rangedWeaponRules.FirstOrDefault(rule => rule.Id == rangedWeaponRule.Id);
            if (existingRule != null) { _ = _rangedWeaponRules.Remove(existingRule); }
            _rangedWeaponRules.Add(rangedWeaponRule);
        }

        public RangedWeaponRule CopyRangedWeaponRule(RangedWeaponRule rangedWeaponRule)
        {
            var newRangedWeaponRule = AddRangedWeaponRule();
            newRangedWeaponRule.Label = $"{rangedWeaponRule.Label} 2";
            newRangedWeaponRule.Explosive = rangedWeaponRule.Explosive;
            newRangedWeaponRule.ManualCast = rangedWeaponRule.ManualCast;
            foreach (var statWeight in rangedWeaponRule.GetStatWeights())
            {
                newRangedWeaponRule.SetStatWeight(statWeight.StatDef, statWeight.Weight, statWeight.Protected);
            }
            foreach (var statLimit in rangedWeaponRule.GetStatLimits())
            {
                newRangedWeaponRule.SetStatLimit(statLimit.StatDef, statLimit.MinValue, statLimit.MaxValue);
            }
            foreach (var def in rangedWeaponRule.GetWhitelistedItems()) { newRangedWeaponRule.AddWhitelistedItem(def); }
            foreach (var def in rangedWeaponRule.GetBlacklistedItems()) { newRangedWeaponRule.AddBlacklistedItem(def); }
            newRangedWeaponRule.UpdateGloballyAvailableItems();
            return newRangedWeaponRule;
        }

        public void DeleteRangedWeaponRule(RangedWeaponRule rangedWeaponRule)
        {
            foreach (var loadout in GetLoadouts())
            {
                if (loadout.PrimaryRangedWeaponRuleId == rangedWeaponRule.Id)
                {
                    loadout.PrimaryRangedWeaponRuleId = null;
                }
                if (loadout.RangedSidearmRules.Contains(rangedWeaponRule.Id))
                {
                    _ = loadout.RangedSidearmRules.Remove(rangedWeaponRule.Id);
                }
            }
            _ = _rangedWeaponRules.Remove(rangedWeaponRule);
        }

        private void ExposeData_RangedWeaponRules()
        {
            Scribe_Collections.Look(ref _rangedWeaponRules, "RangedWeaponRules", LookMode.Deep);
        }

        public RangedWeaponCache GetRangedWeaponCache(Thing thing, RimworldTime time)
        {
            if (!_rangedWeaponsCache.TryGetValue(thing, out var cache))
            {
                cache = new RangedWeaponCache(thing);
                _rangedWeaponsCache[thing] = cache;
            }
            _ = cache.Update(time);
            return cache;
        }

        public RangedWeaponCache GetRangedWeaponDefCache(ThingDef thingDef, RimworldTime time)
        {
            if (!_rangedWeaponDefsCache.TryGetValue(thingDef, out var cache))
            {
                var thing = thingDef.MadeFromStuff
                    ? ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef))
                    : ThingMaker.MakeThing(thingDef);
                cache = new RangedWeaponCache(thing);
                _rangedWeaponDefsCache[thingDef] = cache;
            }
            _ = cache.Update(time);
            return cache;
        }

        public RangedWeaponRule GetRangedWeaponRule(int id)
        {
            return GetRangedWeaponRules().FirstOrDefault(rule => rule.Id == id);
        }

        public IEnumerable<RangedWeaponRule> GetRangedWeaponRules()
        {
            if (_rangedWeaponRules == null || _rangedWeaponRules.Count == 0)
            {
                _rangedWeaponRules = new List<RangedWeaponRule>(RangedWeaponRule.DefaultRules);
            }
            return _rangedWeaponRules;
        }
    }
}