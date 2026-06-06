using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common;
using LordKuper.Common.CustomStats;
using LordKuper.Common.Filters.Limits;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal class MeleeWeaponRule : ItemRule
{
    public delegate bool UsableWithShieldsDelegate(ThingDef thing);

    public static UsableWithShieldsDelegate UsableWithShieldsMethod;
    private bool? _rottable;
    private bool? _usableWithShields;
    public WeaponEquipMode EquipMode = WeaponEquipMode.BestOne;
    public MeleeWeaponRule(int id, bool isProtected) : base(id, isProtected) { }

    [UsedImplicitly]
    public MeleeWeaponRule() { }

    public MeleeWeaponRule(int id, string label, bool isProtected, List<StatWeight> statWeights,
        List<StatLimit> statLimits, HashSet<string> whitelistedItemsDefNames,
        HashSet<string> blacklistedItemsDefNames, WeaponEquipMode equipMode,
        bool? usableWithShields, bool? rottable) : base(id, label, isProtected, statWeights,
        statLimits, whitelistedItemsDefNames, blacklistedItemsDefNames)
    {
        EquipMode = equipMode;
        _usableWithShields = usableWithShields;
        _rottable = rottable;
    }

    private static HashSet<ThingDef> _allRelevantThings;

    [NotNull]
    public static HashSet<ThingDef> AllRelevantThings
    {
        get
        {
            if (_allRelevantThings == null || _allRelevantThings.Count == 0)
            {
                _allRelevantThings = new HashSet<ThingDef>(
                    DefDatabase<ThingDef>.AllDefs.Where(def =>
                        def.IsMeleeWeapon && !def.destroyOnDrop));
            }
            return _allRelevantThings;
        }
    }

    public static void ResetCache()
    {
        _allRelevantThings = null;
        ResetEquipmentManagerCache();
    }

    [NotNull] public static IEnumerable<string> DefaultBlacklist => ["WoodLog", "Beer"];

    [NotNull]
    public static IEnumerable<MeleeWeaponRule> DefaultRules =>
    [
        new(0, true)
        {
            Label = Resources.Strings.WeaponRules.MeleeWeapons.Default.HighestDps,
            EquipMode = WeaponEquipMode.BestOne,
            Rottable = false,
            StatWeights =
            [
                ..DefaultStatWeights.Where(sw => !new[] { "Mass" }.Contains(sw.StatDefName))
                    .Union([new StatWeight("Mass", -1.0f, false)])
            ],
            BlacklistedItemsDefNames = [..DefaultBlacklist]
        },
        new(1, false)
        {
            Label = Resources.Strings.WeaponRules.MeleeWeapons.Default.Sharpest,
            EquipMode = WeaponEquipMode.BestOne,
            Rottable = false,
            StatWeights =
            [
                ..DefaultStatWeights
                    .Where(sw => !new[] { "MeleeWeapon_AverageDPS" }.Contains(sw.StatDefName))
                    .Union([
                        new StatWeight(MeleeWeaponStats.GetStatDefName(MeleeWeaponStat.DpsSharp),
                            2.0f, false),
                        new StatWeight("MeleeWeapon_AverageDPS", 0.5f, false)
                    ])
            ],
            BlacklistedItemsDefNames = [..DefaultBlacklist]
        },
        new(2, false)
        {
            Label = Resources.Strings.WeaponRules.MeleeWeapons.Default.Bluntest,
            EquipMode = WeaponEquipMode.BestOne,
            Rottable = false,
            StatWeights =
            [
                ..DefaultStatWeights
                    .Where(sw => !new[] { "MeleeWeapon_AverageDPS" }.Contains(sw.StatDefName))
                    .Union([
                        new StatWeight(MeleeWeaponStats.GetStatDefName(MeleeWeaponStat.DpsBlunt),
                            2.0f, false),
                        new StatWeight("MeleeWeapon_AverageDPS", 0.5f, false)
                    ])
            ],
            BlacklistedItemsDefNames = [..DefaultBlacklist]
        }
    ];

    [NotNull]
    public new static IEnumerable<StatWeight> DefaultStatWeights =>
        new[]
        {
            new StatWeight("MeleeWeapon_AverageDPS", 2.0f, false),
            new StatWeight(MeleeWeaponStats.GetStatDefName(MeleeWeaponStat.ArmorPenetration),
                0.5f, false)
        }.Union(ItemRule.DefaultStatWeights);

    public bool? Rottable
    {
        get => _rottable;
        set => _rottable = value;
    }

    public bool? UsableWithShields
    {
        get => _usableWithShields;
        set => _usableWithShields = value;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref EquipMode, nameof(EquipMode));
        Scribe_Values.Look(ref _usableWithShields, nameof(UsableWithShields));
        Scribe_Values.Look(ref _rottable, nameof(Rottable));
    }

    [NotNull]
    public IEnumerable<Thing> GetCurrentlyAvailableItems([CanBeNull] Map map, RimWorldTime time)
    {
        Initialize();
        return (map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon) ?? [])
            .Where(thing => IsAvailable(thing, time)).ToList();
    }

    [NotNull]
    public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map, RimWorldTime time)
    {
        return GetCurrentlyAvailableItems(map, time)
            .OrderByDescending(thing => GetThingScore(thing, time));
    }

    private HashSet<ThingDef> GetGloballyAvailableItems()
    {
        Initialize();
        return GloballyAvailableItems;
    }

    [NotNull]
    public IEnumerable<ThingDef> GetGloballyAvailableItemsSorted(RimWorldTime time)
    {
        return GetGloballyAvailableItems().OrderByDescending(def => GetThingDefScore(def, time));
    }

    private static float GetStatValue([NotNull] Thing thing, [NotNull] StatDef statDef,
        RimWorldTime time)
    {
        return thing == null ? throw new ArgumentNullException(nameof(thing)) :
            statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
            EquipmentManager.GetMeleeWeaponCache(thing, time).GetStatValue(statDef);
    }

    private float GetThingDefScore([NotNull] ThingDef def, RimWorldTime time)
    {
        if (def == null) { throw new ArgumentNullException(nameof(def)); }
        var cache = EquipmentManager.GetMeleeWeaponDefCache(def, time);
        return StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
            EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                cache.GetStatValueDeviation(statWeight.StatDef)) * statWeight.Weight);
    }

    public float GetThingScore([NotNull] Thing thing, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        var cache = EquipmentManager.GetMeleeWeaponCache(thing, time);
        var score = StatWeights.Where(sw => sw.StatDef != null).Sum(statWeight =>
            EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                cache.GetStatValueDeviation(statWeight.StatDef)) * statWeight.Weight);
        if (thing.def.useHitPoints)
        {
            score *= HitPointsCurve.Evaluate((float)thing.HitPoints / thing.MaxHitPoints);
        }
        return score;
    }

    public bool IsAvailable(Thing thing, RimWorldTime time)
    {
        Initialize();
        var comp = thing.TryGetComp<CompForbiddable>();
        return (comp == null || !comp.Forbidden) && (GetWhitelistedItems().Contains(thing.def) ||
            (GetGloballyAvailableItems().Contains(thing.def) && SatisfiesLimits(thing, time)));
    }

    private bool SatisfiesLimits([NotNull] Thing thing, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        foreach (var statLimit in StatLimits.Where(limit => limit.StatDef != null))
        {
            var value = GetStatValue(thing, statLimit.StatDef, time);
            if ((statLimit.MinValue != null && value < statLimit.MinValue) ||
                (statLimit.MaxValue != null && value > statLimit.MaxValue)) { return false; }
        }
        return true;
    }

    public void UpdateGloballyAvailableItems()
    {
        Initialize();
        GloballyAvailableItems.Clear();
        foreach (var def in AllRelevantThings) { _ = GloballyAvailableItems.Add(def); }
        if (UsableWithShields != null && UsableWithShieldsMethod != null)
        {
            _ = GloballyAvailableItems.RemoveWhere(def =>
                UsableWithShieldsMethod(def) != UsableWithShields);
        }
        if (Rottable != null)
        {
            _ = GloballyAvailableItems.RemoveWhere(def =>
                def.GetCompProperties<CompProperties_Rottable>() != null != Rottable.Value);
        }
        _ = GloballyAvailableItems.RemoveWhere(def => GetBlacklistedItems().Contains(def));
        foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems.Add(def); }
    }

    public void UpdateStatRanges([NotNull] Thing thing, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        var cache = EquipmentManager.GetMeleeWeaponCache(thing, time);
        var stats = StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
            .Union(StatLimits.Where(sl => sl.StatDef != null).Select(sl => sl.StatDef));
        foreach (var stat in stats)
        {
            EquipmentManager.UpdateStatRange(stat, cache.GetStatValueDeviation(stat));
        }
    }
}