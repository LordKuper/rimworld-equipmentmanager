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

internal class RangedWeaponRule : ItemRule
{
    // Reset to null by ResetCache(); lazily rebuilt on first access.
    private static HashSet<ThingDef>? _allRelevantThings;
    private int _ammoCount;
    private bool? _explosive;
    private bool? _manualCast;
    public WeaponEquipMode EquipMode = WeaponEquipMode.BestOne;
    public RangedWeaponRule(int id, bool isProtected) : base(id, isProtected) { }

    [UsedImplicitly]
    public RangedWeaponRule() { }

    public RangedWeaponRule(int id, string label, bool isProtected, List<StatWeight> statWeights,
        List<StatLimit> statLimits, HashSet<string> whitelistedItemsDefNames, HashSet<string> blacklistedItemsDefNames,
        WeaponEquipMode equipMode, bool? explosive, bool? manualCast, int ammoCount) : base(id, label, isProtected,
        statWeights, statLimits, whitelistedItemsDefNames, blacklistedItemsDefNames)
    {
        EquipMode = equipMode;
        _explosive = explosive;
        _manualCast = manualCast;
        _ammoCount = ammoCount;
    }

    public static HashSet<ThingDef> AllRelevantThings
    {
        get
        {
            if (_allRelevantThings == null || _allRelevantThings.Count == 0)
            {
                _allRelevantThings = new HashSet<ThingDef>(
                    DefDatabase<ThingDef>.AllDefs.Where(def => def.IsRangedWeapon && !def.destroyOnDrop));
            }
            return _allRelevantThings;
        }
    }

    public int AmmoCount
    {
        get => CombatExtendedHelper.EnableAmmoSystem ? _ammoCount : 0;
        set => _ammoCount = CombatExtendedHelper.EnableAmmoSystem ? value : 0;
    }

    public static IEnumerable<string> DefaultBlacklist =>
    [
        "Weapon_GrenadeEMP", "Gun_SmokeLauncher", "Gun_EmpLauncher", "VWE_Gun_FireExtinguisher",
        "VWE_SmokeGrenade", "VWE_TearGasGrenade", "VWE_ToxicGrenade", "VWE_FlashGrenade"
    ];

    public static IEnumerable<RangedWeaponRule> DefaultRules
    {
        get
        {
            var rule0 = new RangedWeaponRule(0, true)
            {
                Label = Resources.Strings.WeaponRules.RangedWeapons.Default.HighestDpsa,
                EquipMode = WeaponEquipMode.BestOne,
                ManualCast = false,
                BlacklistedItemsDefNames = [.. DefaultBlacklist],
                AmmoCount = 100
            };
            rule0.StatWeights =
            [
                ..rule0.GetDefaultStatWeights().Union([
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 2.0f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Range), 0.2f, false)
                ])
            ];
            var rule1 = new RangedWeaponRule(1, false)
            {
                Label = Resources.Strings.WeaponRules.RangedWeapons.Default.LowWarmupTime,
                EquipMode = WeaponEquipMode.BestOne,
                Explosive = false,
                ManualCast = false,
                BlacklistedItemsDefNames = [.. DefaultBlacklist],
                AmmoCount = 50
            };
            rule1.StatWeights =
            [
                ..rule1.GetDefaultStatWeights().Union([
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Warmup), -2.0f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.DpsaShort), 1.0f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 0.5f, false)
                ])
            ];
            var rule2 = new RangedWeaponRule(2, false)
            {
                Label = Resources.Strings.WeaponRules.RangedWeapons.Default.HighRof,
                EquipMode = WeaponEquipMode.BestOne,
                Explosive = false,
                ManualCast = false,
                BlacklistedItemsDefNames = [.. DefaultBlacklist],
                AmmoCount = 200
            };
            rule2.StatWeights =
            [
                ..rule2.GetDefaultStatWeights().Union([
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.BurstShotCount), 2.0f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.TicksBetweenBurstShots), -2.0f,
                        false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Warmup), -0.5f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 1.0f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Range), 0.2f, false),
                    new StatWeight("RangedWeapon_Cooldown", -1.5f, false)
                ])
            ];
            var rule3 = new RangedWeaponRule(3, false)
            {
                Label = Resources.Strings.WeaponRules.RangedWeapons.Default.LongRangeHeavyHitter,
                EquipMode = WeaponEquipMode.BestOne,
                Explosive = false,
                ManualCast = false,
                BlacklistedItemsDefNames = [.. DefaultBlacklist],
                AmmoCount = 30
            };
            rule3.StatWeights =
            [
                ..rule3.GetDefaultStatWeights().Union([
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Range), 2.0f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Damage), 1.5f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.DpsaLong), 1.0f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.Dpsa), 0.5f, false),
                    new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.StoppingPower), 0.5f, false)
                ])
            ];
            return [rule0, rule1, rule2, rule3];
        }
    }

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

    public IEnumerable<Thing> GetCurrentlyAvailableItems(Map? map, RimWorldTime time)
    {
        Initialize();
        return (map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon) ?? [])
            .Where(thing => IsAvailable(thing, time)).ToList();
    }

    public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map, RimWorldTime time)
    {
        return GetCurrentlyAvailableItems(map, time).OrderByDescending(thing => GetThingScore(thing, time));
    }

    protected internal override IEnumerable<StatWeight> GetDefaultStatWeights()
    {
        return new[]
        {
            new StatWeight(RangedWeaponStats.GetStatDefName(RangedWeaponStat.ArmorPenetration), 0.2f, false)
        }.Union(base.GetDefaultStatWeights());
    }

    private IEnumerable<ThingDef> GetGloballyAvailableItems()
    {
        Initialize();
        return GloballyAvailableItems!;
    }

    public IEnumerable<ThingDef> GetGloballyAvailableItemsSorted(RimWorldTime time)
    {
        return GetGloballyAvailableItems().OrderByDescending(def => GetThingDefScore(def, time));
    }

    private static float GetStatValue(Thing thing, StatDef statDef, RimWorldTime time)
    {
        return thing == null ? throw new ArgumentNullException(nameof(thing)) :
            statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
            EquipmentManager.GetRangedWeaponCache(thing, time).GetStatValue(statDef);
    }

    private float GetThingDefScore(ThingDef def, RimWorldTime time)
    {
        if (def == null) { throw new ArgumentNullException(nameof(def)); }
        Initialize();
        var cache = EquipmentManager.GetRangedWeaponDefCache(def, time);
        // StatDef is non-null here: filtered by Where(statWeight => statWeight.StatDef != null).
        return StatWeights!.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
            StatRanges.NormalizeStatValue(statWeight.StatDef!, cache.GetStatValueDeviation(statWeight.StatDef!)) *
            statWeight.Weight);
    }

    public float GetThingScore(Thing thing, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        Initialize();
        var cache = EquipmentManager.GetRangedWeaponCache(thing, time);
        // StatDef is non-null here: filtered by Where(sw => sw.StatDef != null).
        var score = StatWeights!.Where(sw => sw.StatDef != null).Sum(statWeight =>
            StatRanges.NormalizeStatValue(statWeight.StatDef!, cache.GetStatValueDeviation(statWeight.StatDef!)) *
            statWeight.Weight);
        if (thing.def.useHitPoints) { score *= HitPointsCurve.Evaluate((float)thing.HitPoints / thing.MaxHitPoints); }
        return score;
    }

    public bool IsAvailable(Thing thing, RimWorldTime time)
    {
        Initialize();
        var comp = thing.TryGetComp<CompForbiddable>();
        return comp is not { Forbidden: true } && (GetWhitelistedItems().Contains(thing.def) ||
            (GetGloballyAvailableItems().Contains(thing.def) && SatisfiesLimits(thing, time)));
    }

    public static void ResetCache()
    {
        _allRelevantThings = null;
        ResetEquipmentManagerCache();
    }

    private bool SatisfiesLimits(Thing thing, RimWorldTime time)
    {
        if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
        // StatDef is non-null here: filtered by Where(limit => limit.StatDef != null).
        // StatLimits is guaranteed non-null: SatisfiesLimits is only called from IsAvailable(),
        // which calls Initialize() first.
        foreach (var statLimit in StatLimits!.Where(limit => limit.StatDef != null))
        {
            var value = GetStatValue(thing, statLimit.StatDef!, time);
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
        if (Explosive != null)
        {
            _ = GloballyAvailableItems!.RemoveWhere(def => def.Verbs.Any(verb =>
                verb?.defaultProjectile != null && verb.defaultProjectile.projectile.explosionRadius > 0) != Explosive);
        }
        if (ManualCast != null)
        {
            _ = GloballyAvailableItems!.RemoveWhere(def => def.Verbs.Any(verb => verb.onlyManualCast) != ManualCast);
        }
        _ = GloballyAvailableItems!.RemoveWhere(def => GetBlacklistedItems().Contains(def));
        foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems!.Add(def); }
    }
}