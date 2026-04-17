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

internal class RangedWeaponRule : ItemRule
{
    private int _ammoCount;
    private bool? _explosive;
    private bool? _manualCast;
    public WeaponEquipMode EquipMode = WeaponEquipMode.BestOne;
    public RangedWeaponRule(int id, bool isProtected) : base(id, isProtected) { }

    [UsedImplicitly]
    public RangedWeaponRule() { }

    public RangedWeaponRule(int id, string label, bool isProtected, List<StatWeight> statWeights,
        List<StatLimit> statLimits, HashSet<string> whitelistedItemsDefNames,
        HashSet<string> blacklistedItemsDefNames, WeaponEquipMode equipMode, bool? explosive,
        bool? manualCast, int ammoCount) : base(id, label, isProtected, statWeights, statLimits,
        whitelistedItemsDefNames, blacklistedItemsDefNames)
    {
        EquipMode = equipMode;
        _explosive = explosive;
        _manualCast = manualCast;
        _ammoCount = ammoCount;
    }

    [NotNull]
    public static HashSet<ThingDef> AllRelevantThings
    {
        get
        {
            if (field == null || field.Count == 0)
            {
                field = new HashSet<ThingDef>(
                    DefDatabase<ThingDef>.AllDefs.Where(def =>
                        def.IsRangedWeapon && !def.destroyOnDrop));
            }
            return field;
        }
    }

    public int AmmoCount
    {
        get => CombatExtendedHelper.EnableAmmoSystem ? _ammoCount : 0;
        set => _ammoCount = CombatExtendedHelper.EnableAmmoSystem ? value : 0;
    }

    [NotNull]
    public static IEnumerable<string> DefaultBlacklist =>
    [
        "Weapon_GrenadeEMP", "Gun_SmokeLauncher", "Gun_EmpLauncher", "VWE_Gun_FireExtinguisher",
        "VWE_SmokeGrenade", "VWE_TearGasGrenade", "VWE_ToxicGrenade", "VWE_FlashGrenade"
    ];

    [NotNull]
    public static IEnumerable<RangedWeaponRule> DefaultRules =>
    [
        new(0, true)
        {
            Label = Resources.Strings.WeaponRules.RangedWeapons.Default.HighestDpsa,
            EquipMode = WeaponEquipMode.BestOne,
            ManualCast = false,
            StatWeights =
            [
                ..DefaultStatWeights.Union([
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 2.0f,
                        false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Range), 0.5f,
                        false)
                ])
            ],
            BlacklistedItemsDefNames = [..DefaultBlacklist],
            AmmoCount = 100
        },
        new(1, false)
        {
            Label = Resources.Strings.WeaponRules.RangedWeapons.Default.LowWarmupTime,
            EquipMode = WeaponEquipMode.BestOne,
            Explosive = false,
            ManualCast = false,
            StatWeights =
            [
                ..DefaultStatWeights.Union([
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Warmup),
                        -2.0f, false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.DpsaShort),
                        1.0f, false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 0.5f,
                        false)
                ])
            ],
            BlacklistedItemsDefNames = [..DefaultBlacklist],
            AmmoCount = 50
        },
        new(2, false)
        {
            Label = Resources.Strings.WeaponRules.RangedWeapons.Default.HighRof,
            EquipMode = WeaponEquipMode.BestOne,
            Explosive = false,
            ManualCast = false,
            StatWeights =
            [
                ..DefaultStatWeights.Union([
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.BurstShotCount), 2.0f,
                        false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.TicksBetweenBurstShots),
                        -2.0f, false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Warmup),
                        -0.5f, false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 1.0f,
                        false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Range), 0.5f,
                        false),
                    new StatWeight("RangedWeapon_Cooldown", -1.5f, false)
                ])
            ],
            BlacklistedItemsDefNames = [..DefaultBlacklist],
            AmmoCount = 200
        },
        new(3, false)
        {
            Label = Resources.Strings.WeaponRules.RangedWeapons.Default.LongRangeHeavyHitter,
            EquipMode = WeaponEquipMode.BestOne,
            Explosive = false,
            ManualCast = false,
            StatWeights =
            [
                ..DefaultStatWeights.Union([
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Range), 2.0f,
                        false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Damage), 1.5f,
                        false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.DpsaLong),
                        1.0f, false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 0.5f,
                        false),
                    new StatWeight(
                        RangedWeaponStats.GetStatDefName(RangedWeaponStat.StoppingPower), 0.5f,
                        false)
                ])
            ],
            BlacklistedItemsDefNames = [..DefaultBlacklist],
            AmmoCount = 30
        }
    ];

    [NotNull]
    public new static IEnumerable<StatWeight> DefaultStatWeights =>
        new[]
        {
            new StatWeight(
                RangedWeaponStats.GetStatDefName(RangedWeaponStat.ArmorPenetration),
                0.2f, false)
        }.Union(ItemRule.DefaultStatWeights);

    public bool? Explosive
    {
        get => _explosive;
        set => _explosive = value;
    }

    public bool? ManualCast
    {
        get => _manualCast;
        set => _manualCast = value;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref EquipMode, nameof(EquipMode));
        Scribe_Values.Look(ref _explosive, nameof(Explosive));
        Scribe_Values.Look(ref _manualCast, nameof(ManualCast));
        Scribe_Values.Look(ref _ammoCount, nameof(AmmoCount));
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

    private IEnumerable<ThingDef> GetGloballyAvailableItems()
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
            EquipmentManager.GetRangedWeaponCache(thing, time).GetStatValue(statDef);
    }

    private float GetThingDefScore([NotNull] ThingDef def, RimWorldTime time)
    {
        if (def == null) { throw new ArgumentNullException(nameof(def)); }
        var cache = EquipmentManager.GetRangedWeaponDefCache(def, time);
        return StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
            EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                cache.GetStatValueDeviation(statWeight.StatDef)) * statWeight.Weight);
    }

    public float GetThingScore([NotNull] Thing thing, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        var cache = EquipmentManager.GetRangedWeaponCache(thing, time);
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
        if (Explosive != null)
        {
            _ = GloballyAvailableItems.RemoveWhere(def => def.Verbs.Any(verb =>
                verb?.defaultProjectile != null &&
                verb.defaultProjectile.projectile.explosionRadius > 0) != Explosive);
        }
        if (ManualCast != null)
        {
            _ = GloballyAvailableItems.RemoveWhere(def =>
                def.Verbs.Any(verb => verb.onlyManualCast) != ManualCast);
        }
        _ = GloballyAvailableItems.RemoveWhere(def => GetBlacklistedItems().Contains(def));
        foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems.Add(def); }
    }

    public void UpdateStatRanges([NotNull] Thing thing, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        var cache = EquipmentManager.GetRangedWeaponCache(thing, time);
        var stats = StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
            .Union(StatLimits.Where(sl => sl.StatDef != null).Select(sl => sl.StatDef));
        foreach (var stat in stats)
        {
            EquipmentManager.UpdateStatRange(stat, cache.GetStatValueDeviation(stat));
        }
    }
}
