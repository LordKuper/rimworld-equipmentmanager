using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using RimWorld;
using Verse;

namespace LordKuper.EquipmentManager;

internal partial class EquipmentManagerGameComponent
{
    private readonly Dictionary<ThingDef, MeleeWeaponCache> _meleeWeaponDefsCache = new();
    private readonly Dictionary<Thing, MeleeWeaponCache> _meleeWeaponsCache = new();

    // Populated by Scribe on load (IExposable lifecycle); null when no saved data exists.
    // GetMeleeWeaponRules() guards with ??= to restore default rules.
    private List<MeleeWeaponRule>? _meleeWeaponRules;

    public MeleeWeaponRule AddMeleeWeaponRule()
    {
        _ = GetMeleeWeaponRules(); // ensure list is initialized
        var id = _meleeWeaponRules!.Any() ? _meleeWeaponRules!.Max(l => l.Id) + 1 : 0;
        var meleeWeaponRule = new MeleeWeaponRule(id, false) { Label = $"{id}" };
        foreach (var statWeight in meleeWeaponRule.GetDefaultStatWeights())
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
        _meleeWeaponRules!.Add(meleeWeaponRule);
        return meleeWeaponRule;
    }

    public void AddMeleeWeaponRule(MeleeWeaponRule meleeWeaponRule)
    {
        meleeWeaponRule.NormalizeLegacyCustomStatDefNames();
        _ = GetMeleeWeaponRules(); // ensure list is initialized
        var existingRule = _meleeWeaponRules!.FirstOrDefault(rule => rule.Id == meleeWeaponRule.Id);
        if (existingRule != null) { _ = _meleeWeaponRules!.Remove(existingRule); }
        _meleeWeaponRules!.Add(meleeWeaponRule);
    }

    public MeleeWeaponRule CopyMeleeWeaponRule(MeleeWeaponRule meleeWeaponRule)
    {
        var newMeleeWeaponRule = AddMeleeWeaponRule();
        newMeleeWeaponRule.Label = $"{meleeWeaponRule.Label} 2";
        newMeleeWeaponRule.UsableWithShields = meleeWeaponRule.UsableWithShields;
        newMeleeWeaponRule.Rottable = meleeWeaponRule.Rottable;
        foreach (var statWeight in meleeWeaponRule.GetStatWeights())
        {
            // StatDef may be null on loaded items from older saves; guard before calling.
            if (statWeight.StatDef == null) { continue; }
            newMeleeWeaponRule.SetStatWeight(statWeight.StatDef, statWeight.Weight, statWeight.Protected);
        }
        foreach (var statLimit in meleeWeaponRule.GetStatLimits())
        {
            if (statLimit.StatDef == null) { continue; }
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
        _ = GetMeleeWeaponRules(); // ensure list is initialized
        _ = _meleeWeaponRules!.Remove(meleeWeaponRule);
    }

    private void ExposeData_MeleeWeaponRules()
    {
        Scribe_Collections.Look(ref _meleeWeaponRules, "MeleeWeaponRules", LookMode.Deep);
    }

    public MeleeWeaponCache GetMeleeWeaponCache(Thing thing, RimWorldTime time)
    {
        if (!_meleeWeaponsCache.TryGetValue(thing, out var cache))
        {
            cache = new MeleeWeaponCache(thing);
            _meleeWeaponsCache[thing] = cache;
        }
        _ = cache.Update(time);
        return cache;
    }

    public MeleeWeaponCache GetMeleeWeaponDefCache(ThingDef thingDef, RimWorldTime time)
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

    public MeleeWeaponRule? GetMeleeWeaponRule(int id)
    {
        return GetMeleeWeaponRules().FirstOrDefault(rule => rule.Id == id);
    }

    public IEnumerable<MeleeWeaponRule> GetMeleeWeaponRules()
    {
        if (_meleeWeaponRules == null || _meleeWeaponRules.Count == 0)
        {
            _meleeWeaponRules = [.. MeleeWeaponRule.DefaultRules];
        }
        return _meleeWeaponRules;
    }
}