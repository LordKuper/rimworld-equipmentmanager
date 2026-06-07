using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common;
using LordKuper.Common.CustomStats;
using LordKuper.Common.Filters.Limits;
using RimWorld;
using Verse;

namespace LordKuper.EquipmentManager;

internal class ToolRule : ItemRule
{
    // Reset to null by ResetCache(); lazily rebuilt on first access.
    private static HashSet<ThingDef>? _allRelevantThings;
    private bool? _ranged;
    public ToolEquipMode EquipMode = ToolEquipMode.OneForEveryAssignedWorkType;
    public ToolRule(int id, bool isProtected) : base(id, isProtected) { }

    [UsedImplicitly]
    public ToolRule() { }

    public ToolRule(int id, string label, bool isProtected, List<StatWeight> statWeights, List<StatLimit> statLimits,
        HashSet<string> whitelistedItemsDefNames, HashSet<string> blacklistedItemsDefNames, ToolEquipMode equipMode,
        bool? ranged) : base(id, label, isProtected, statWeights, statLimits, whitelistedItemsDefNames,
        blacklistedItemsDefNames)
    {
        EquipMode = equipMode;
        _ranged = ranged;
    }

    public static HashSet<ThingDef> AllRelevantThings
    {
        get
        {
            if (_allRelevantThings == null || _allRelevantThings.Count == 0)
            {
                var relevantStats =
                    (WorkTypeStatMap.AutoSwitchStatsMap?.Values.SelectMany(s => s) ?? Enumerable.Empty<StatDef>())
                    .ToHashSet();
                _allRelevantThings = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where(def =>
                    def.IsWeapon && !def.destroyOnDrop && (def.statBases ?? []).Union(def.equippedStatOffsets ?? [])
                    .Any(sm => relevantStats.Contains(sm.stat))));
            }
            return _allRelevantThings;
        }
    }

    public static IEnumerable<string> DefaultBlacklist => [];

    public static IEnumerable<ToolRule> DefaultRules
    {
        get
        {
            var rule0 = new ToolRule(0, true)
            {
                Label = Resources.Strings.WeaponRules.Tools.Default.AssignedWorkTypes,
                EquipMode = ToolEquipMode.OneForEveryAssignedWorkType,
                BlacklistedItemsDefNames = [.. DefaultBlacklist]
            };
            rule0.StatWeights = [.. rule0.GetDefaultStatWeights()];
            var rule1 = new ToolRule(1, true)
            {
                Label = Resources.Strings.WeaponRules.Tools.Default.AllWorkTypes,
                EquipMode = ToolEquipMode.OneForEveryWorkType,
                BlacklistedItemsDefNames = [.. DefaultBlacklist]
            };
            rule1.StatWeights = [.. rule1.GetDefaultStatWeights()];
            return [rule0, rule1];
        }
    }

    public bool? Ranged
    {
        get => _ranged;
        set => _ranged = value;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref EquipMode, nameof(EquipMode));
        Scribe_Values.Look(ref _ranged, nameof(Ranged));
    }

    public IEnumerable<Thing> GetCurrentlyAvailableItems(Map? map, IReadOnlyCollection<WorkTypeDef> workTypeDefs,
        RimWorldTime time)
    {
        Initialize();
        return (map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon) ?? [])
            .Where(thing => IsAvailable(thing, workTypeDefs, time)).ToList();
    }

    public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map, IReadOnlyCollection<WorkTypeDef> workTypeDefs,
        RimWorldTime time)
    {
        return !workTypeDefs.Any()
            ? throw new ArgumentException("At least one work type must be passed", nameof(workTypeDefs))
            : GetCurrentlyAvailableItems(map, workTypeDefs, time)
                .OrderByDescending(thing => GetThingScore(thing, workTypeDefs, time));
    }

    protected internal override IEnumerable<StatWeight> GetDefaultStatWeights()
    {
        return new[]
        {
            new StatWeight(ToolStats.GetStatDefName(ToolStat.WorkType), 2.0f, true),
            new StatWeight("MoveSpeed", 1.0f, false)
        }.Union(base.GetDefaultStatWeights());
    }

    private IEnumerable<ThingDef> GetGloballyAvailableItems(IReadOnlyCollection<WorkTypeDef> workTypeDefs)
    {
        Initialize();
        var autoSwitchMap = WorkTypeStatMap.AutoSwitchStatsMap;
        var relevantStats = workTypeDefs.SelectMany(wtd =>
            autoSwitchMap != null && autoSwitchMap.TryGetValue(wtd, out var stats)
                ? stats
                : Enumerable.Empty<StatDef>()).ToHashSet();
        return GloballyAvailableItems!.Where(def => (def.statBases ?? [])
            .Union(def.equippedStatOffsets ?? []).Any(sm => relevantStats.Contains(sm.stat)));
    }

    public IEnumerable<ThingDef> GetGloballyAvailableItemsSorted(IReadOnlyCollection<WorkTypeDef> workTypeDefs,
        RimWorldTime time)
    {
        return GetGloballyAvailableItems(workTypeDefs)
            .OrderByDescending(def => GetThingDefScore(def, workTypeDefs, time));
    }

    private static float GetStatValue(Thing thing, StatDef statDef, IReadOnlyCollection<WorkTypeDef> workTypeDefs,
        RimWorldTime time)
    {
        return thing == null ? throw new ArgumentNullException(nameof(thing)) :
            statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
            EquipmentManager.GetToolCache(thing, time).GetStatValue(statDef, workTypeDefs);
    }

    private float GetThingDefScore(ThingDef def, IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimWorldTime time)
    {
        if (def == null) { throw new ArgumentNullException(nameof(def)); }
        Initialize();
        var cache = EquipmentManager.GetToolDefCache(def, time);
        // StatDef is non-null here: filtered by Where(statWeight => statWeight.StatDef != null).
        return StatWeights!.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
            StatRanges.NormalizeStatValue(statWeight.StatDef!,
                cache.GetStatValueDeviation(statWeight.StatDef!, workTypeDefs)) * statWeight.Weight);
    }

    public float GetThingScore(Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        Initialize();
        var cache = EquipmentManager.GetToolCache(thing, time);
        // StatDef is non-null here: filtered by Where(sw => sw.StatDef != null).
        var score = StatWeights!.Where(sw => sw.StatDef != null).Sum(statWeight =>
            StatRanges.NormalizeStatValue(statWeight.StatDef!,
                cache.GetStatValueDeviation(statWeight.StatDef!, workTypeDefs)) * statWeight.Weight);
        if (thing.def.useHitPoints) { score *= HitPointsCurve.Evaluate((float)thing.HitPoints / thing.MaxHitPoints); }
        return score;
    }

    public bool IsAvailable(Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimWorldTime time)
    {
        Initialize();
        var comp = thing.TryGetComp<CompForbiddable>();
        return comp is not { Forbidden: true } && (GetWhitelistedItems().Contains(thing.def) ||
            (GetGloballyAvailableItems(workTypeDefs).Contains(thing.def) &&
                SatisfiesLimits(thing, workTypeDefs, time)));
    }

    public static void ResetCache()
    {
        _allRelevantThings = null;
        ResetEquipmentManagerCache();
    }

    private bool SatisfiesLimits(Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        // StatDef is non-null here: filtered by Where(limit => limit.StatDef != null).
        // StatLimits is guaranteed non-null: SatisfiesLimits is only called from IsAvailable(),
        // which calls Initialize() first.
        foreach (var statLimit in StatLimits!.Where(limit => limit.StatDef != null))
        {
            var value = GetStatValue(thing, statLimit.StatDef!, workTypeDefs, time);
            if ((statLimit.MinValue != null && value < statLimit.MinValue) ||
                (statLimit.MaxValue != null && value > statLimit.MaxValue)) { return false; }
        }
        return true;
    }

    public void UpdateGloballyAvailableItems()
    {
        Initialize();
        GloballyAvailableItems!.Clear();
        foreach (var def in AllRelevantThings) { _ = GloballyAvailableItems!.Add(def); }
        if (Ranged != null) { _ = GloballyAvailableItems!.RemoveWhere(def => def.IsRangedWeapon != Ranged); }
        _ = GloballyAvailableItems!.RemoveWhere(def => GetBlacklistedItems().Contains(def));
        foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems!.Add(def); }
    }
}