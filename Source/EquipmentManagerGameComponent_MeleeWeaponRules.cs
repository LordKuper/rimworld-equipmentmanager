﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private readonly Dictionary<ThingDef, MeleeWeaponCache> _meleeWeaponDefsCache =
            new Dictionary<ThingDef, MeleeWeaponCache>();

        private readonly Dictionary<Thing, MeleeWeaponCache> _meleeWeaponsCache =
            new Dictionary<Thing, MeleeWeaponCache>();

        private List<MeleeWeaponRule> _meleeWeaponRules;

        public MeleeWeaponRule AddMeleeWeaponRule()
        {
            var id = _meleeWeaponRules.Any() ? _meleeWeaponRules.Max(l => l.Id) + 1 : 0;
            var meleeWeaponRule = new MeleeWeaponRule(id, false) {Label = $"{id}"};
            foreach (var statWeight in MeleeWeaponRule.DefaultStatWeights)
            {
                if (statWeight.StatDef == null) { continue; }
                meleeWeaponRule.SetStatWeight(statWeight.StatDef, statWeight.Weight, statWeight.Protected);
            }
            foreach (var defName in MeleeWeaponRule.DefaultBlacklist)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def != null) { meleeWeaponRule.AddBlacklistedItem(def); }
            }
            meleeWeaponRule.UpdateGloballyAvailableItems();
            _meleeWeaponRules.Add(meleeWeaponRule);
            return meleeWeaponRule;
        }

        public void AddMeleeWeaponRule(MeleeWeaponRule meleeWeaponRule)
        {
            var existingRule = _meleeWeaponRules.FirstOrDefault(rule => rule.Id == meleeWeaponRule.Id);
            if (existingRule != null) { _ = _meleeWeaponRules.Remove(existingRule); }
            _meleeWeaponRules.Add(meleeWeaponRule);
        }

        public MeleeWeaponRule CopyMeleeWeaponRule(MeleeWeaponRule meleeWeaponRule)
        {
            var newMeleeWeaponRule = AddMeleeWeaponRule();
            newMeleeWeaponRule.Label = $"{meleeWeaponRule.Label} 2";
            newMeleeWeaponRule.UsableWithShields = meleeWeaponRule.UsableWithShields;
            newMeleeWeaponRule.Rottable = meleeWeaponRule.Rottable;
            foreach (var statWeight in meleeWeaponRule.GetStatWeights())
            {
                newMeleeWeaponRule.SetStatWeight(statWeight.StatDef, statWeight.Weight, statWeight.Protected);
            }
            foreach (var statLimit in meleeWeaponRule.GetStatLimits())
            {
                newMeleeWeaponRule.SetStatLimit(statLimit.StatDef, statLimit.MinValue, statLimit.MaxValue);
            }
            foreach (var def in meleeWeaponRule.GetWhitelistedItems()) { newMeleeWeaponRule.AddWhitelistedItem(def); }
            foreach (var def in meleeWeaponRule.GetBlacklistedItems()) { newMeleeWeaponRule.AddBlacklistedItem(def); }
            newMeleeWeaponRule.UpdateGloballyAvailableItems();
            return newMeleeWeaponRule;
        }

        public void DeleteMeleeWeaponRule(MeleeWeaponRule meleeWeaponRule)
        {
            foreach (var loadout in GetLoadouts())
            {
                if (loadout.PrimaryMeleeWeaponRuleId == meleeWeaponRule.Id) { loadout.PrimaryMeleeWeaponRuleId = null; }
                if (loadout.MeleeSidearmRules.Contains(meleeWeaponRule.Id))
                {
                    _ = loadout.MeleeSidearmRules.Remove(meleeWeaponRule.Id);
                }
            }
            _ = _meleeWeaponRules.Remove(meleeWeaponRule);
        }

        private void ExposeData_MeleeWeaponRules()
        {
            Scribe_Collections.Look(ref _meleeWeaponRules, "MeleeWeaponRules", LookMode.Deep);
        }

        public MeleeWeaponCache GetMeleeWeaponCache(Thing thing, RimworldTime time)
        {
            if (!_meleeWeaponsCache.TryGetValue(thing, out var cache))
            {
                cache = new MeleeWeaponCache(thing);
                _meleeWeaponsCache[thing] = cache;
            }
            _ = cache.Update(time);
            return cache;
        }

        public MeleeWeaponCache GetMeleeWeaponDefCache(ThingDef thingDef, RimworldTime time)
        {
            if (!_meleeWeaponDefsCache.TryGetValue(thingDef, out var cache))
            {
                var thing = thingDef.MadeFromStuff
                    ? ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef))
                    : ThingMaker.MakeThing(thingDef);
                cache = new MeleeWeaponCache(thing);
                _meleeWeaponDefsCache[thingDef] = cache;
            }
            _ = cache.Update(time);
            return cache;
        }

        public MeleeWeaponRule GetMeleeWeaponRule(int id)
        {
            return GetMeleeWeaponRules().FirstOrDefault(rule => rule.Id == id);
        }

        public IEnumerable<MeleeWeaponRule> GetMeleeWeaponRules()
        {
            if (_meleeWeaponRules == null || _meleeWeaponRules.Count == 0)
            {
                _meleeWeaponRules = new List<MeleeWeaponRule>(MeleeWeaponRule.DefaultRules);
            }
            return _meleeWeaponRules;
        }
    }
}