using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal partial class EquipmentManagerGameComponent
{
    private readonly Dictionary<Thing, ToolCache> _toolCache = new();
    private readonly Dictionary<ThingDef, ToolCache> _toolDefsCache = new();
    // Populated by Scribe on load (IExposable lifecycle); = null! asserts the field is always
    // set before any read, consistent with the RimWorld load contract.
    private List<ToolRule> _toolRules = null!;

    public ToolRule AddToolRule()
    {
        var id = _toolRules.Any() ? _toolRules.Max(l => l.Id) + 1 : 0;
        var toolRule = new ToolRule(id, false) { Label = $"{id}" };
        foreach (var statWeight in ToolRule.DefaultStatWeights)
        {
            if (statWeight.StatDef == null) { continue; }
            toolRule.SetStatWeight(statWeight.StatDef, statWeight.Weight, statWeight.Protected);
        }
        foreach (var defName in ToolRule.DefaultBlacklist)
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def != null) { toolRule.AddBlacklistedItem(def); }
        }
        toolRule.UpdateGloballyAvailableItems();
        _toolRules.Add(toolRule);
        return toolRule;
    }

    public void AddToolRule(ToolRule toolRule)
    {
        toolRule.NormalizeLegacyCustomStatDefNames();
        var existingRule = _toolRules.FirstOrDefault(rule => rule.Id == toolRule.Id);
        if (existingRule != null) { _ = _toolRules.Remove(existingRule); }
        _toolRules.Add(toolRule);
    }

    public ToolRule CopyToolRule(ToolRule toolRule)
    {
        var newToolRule = AddToolRule();
        newToolRule.Label = $"{toolRule.Label} 2";
        newToolRule.Ranged = toolRule.Ranged;
        foreach (var statWeight in toolRule.GetStatWeights())
        {
            // StatDef may be null on loaded items from older saves; guard before calling.
            if (statWeight.StatDef == null) { continue; }
            newToolRule.SetStatWeight(statWeight.StatDef, statWeight.Weight, statWeight.Protected);
        }
        foreach (var statLimit in toolRule.GetStatLimits())
        {
            if (statLimit.StatDef == null) { continue; }
            newToolRule.SetStatLimit(statLimit.StatDef, statLimit.MinValue, statLimit.MaxValue);
        }
        foreach (var def in toolRule.GetWhitelistedItems()) { newToolRule.AddWhitelistedItem(def); }
        foreach (var def in toolRule.GetBlacklistedItems()) { newToolRule.AddBlacklistedItem(def); }
        newToolRule.UpdateGloballyAvailableItems();
        return newToolRule;
    }

    public void DeleteToolRule(ToolRule toolRule)
    {
        foreach (var loadout in GetLoadouts())
        {
            if (loadout.ToolRuleId == toolRule.Id) { loadout.ToolRuleId = null; }
        }
        _ = _toolRules.Remove(toolRule);
    }

    private void ExposeData_ToolRules()
    {
        Scribe_Collections.Look(ref _toolRules, "ToolRules", LookMode.Deep);
    }

    public ToolCache GetToolCache(Thing thing, RimWorldTime time)
    {
        if (!_toolCache.TryGetValue(thing, out var cache))
        {
            cache = new ToolCache(thing);
            _toolCache[thing] = cache;
        }
        _ = cache.Update(time);
        return cache;
    }

    public ToolCache GetToolDefCache(ThingDef thingDef, RimWorldTime time)
    {
        if (!_toolDefsCache.TryGetValue(thingDef, out var cache))
        {
            var thing = thingDef.MadeFromStuff
                ? ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef))
                : ThingMaker.MakeThing(thingDef);
            cache = new ToolCache(thing);
            _toolDefsCache[thingDef] = cache;
        }
        _ = cache.Update(time);
        return cache;
    }

    public ToolRule? GetToolRule(int id)
    {
        return GetToolRules().FirstOrDefault(rule => rule.Id == id);
    }

    public IEnumerable<ToolRule> GetToolRules()
    {
        if (_toolRules == null || _toolRules.Count == 0)
        {
            _toolRules = new List<ToolRule>(ToolRule.DefaultRules);
        }
        return _toolRules;
    }
}
