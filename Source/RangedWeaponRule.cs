using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules.RangedWeapons;

namespace EquipmentManager
{
    internal class RangedWeaponRule : ItemRule
    {
        private static HashSet<ThingDef> _allRelevantThings;
        private int _ammoCount;
        private bool? _explosive;
        private bool? _manualCast;
        public WeaponEquipMode EquipMode = WeaponEquipMode.BestOne;
        public RangedWeaponRule(int id, bool isProtected) : base(id, isProtected) { }

        [UsedImplicitly]
        public RangedWeaponRule() { }

        public RangedWeaponRule(int id, string label, bool isProtected, List<StatWeight> statWeights,
            List<StatLimit> statLimits, HashSet<string> whitelistedItemsDefNames,
            HashSet<string> blacklistedItemsDefNames, WeaponEquipMode equipMode, bool? explosive, bool? manualCast) :
            base(id, label, isProtected, statWeights, statLimits, whitelistedItemsDefNames, blacklistedItemsDefNames)
        {
            EquipMode = equipMode;
            _explosive = explosive;
            _manualCast = manualCast;
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
            new[]
            {
                "Weapon_GrenadeEMP", "Gun_SmokeLauncher", "Gun_EmpLauncher", "VWE_Gun_FireExtinguisher",
                "VWE_SmokeGrenade", "VWE_TearGasGrenade", "VWE_ToxicGrenade", "VWE_FlashGrenade"
            };

        public static IEnumerable<RangedWeaponRule> DefaultRules =>
            new[]
            {
                new RangedWeaponRule(0, true)
                {
                    Label = Strings.Default.HighestDpsa,
                    EquipMode = WeaponEquipMode.BestOne,
                    ManualCast = false,
                    StatWeights = new List<StatWeight>(DefaultStatWeights.Union(new[]
                    {
                        new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Dpsa),
                            false) {Weight = 2.0f},
                        new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Range),
                            false) {Weight = 0.5f}
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist),
                    AmmoCount = 100
                },
                new RangedWeaponRule(1, false)
                {
                    Label = Strings.Default.LowWarmupTime,
                    EquipMode = WeaponEquipMode.BestOne,
                    Explosive = false,
                    ManualCast = false,
                    StatWeights = new List<StatWeight>(DefaultStatWeights.Union(new[]
                    {
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Warmup), false)
                        {
                            Weight = -2.0f
                        },
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.DpsaShort), false)
                        {
                            Weight = 1.0f
                        },
                        new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Dpsa),
                            false) {Weight = 0.5f}
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist),
                    AmmoCount = 50
                },
                new RangedWeaponRule(2, false)
                {
                    Label = Strings.Default.HighRof,
                    EquipMode = WeaponEquipMode.BestOne,
                    Explosive = false,
                    ManualCast = false,
                    StatWeights = new List<StatWeight>(DefaultStatWeights.Union(new[]
                    {
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.BurstShotCount),
                            false) {Weight = 2.0f},
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat
                                .TicksBetweenBurstShots), false) {Weight = -2.0f},
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Warmup), false)
                        {
                            Weight = -0.5f
                        },
                        new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Dpsa),
                            false) {Weight = 1.0f},
                        new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Range),
                            false) {Weight = 0.5f},
                        new StatWeight("RangedWeapon_Cooldown", false) {Weight = -1.5f}
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist),
                    AmmoCount = 200
                },
                new RangedWeaponRule(3, false)
                {
                    Label = Strings.Default.LongRangeHeavyHitter,
                    EquipMode = WeaponEquipMode.BestOne,
                    Explosive = false,
                    ManualCast = false,
                    StatWeights = new List<StatWeight>(DefaultStatWeights.Union(new[]
                    {
                        new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Range),
                            false) {Weight = 2.0f},
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Damage), false)
                        {
                            Weight = 1.5f
                        },
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.DpsaLong), false)
                        {
                            Weight = 1.0f
                        },
                        new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.Dpsa),
                            false) {Weight = 0.5f},
                        new StatWeight(
                            CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.StoppingPower),
                            false) {Weight = 0.5f}
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist),
                    AmmoCount = 30
                }
            };

        public new static IEnumerable<StatWeight> DefaultStatWeights =>
            new[]
            {
                new StatWeight(CustomRangedWeaponStats.GetStatDefName(CustomRangedWeaponStat.ArmorPenetration),
                    false) {Weight = 0.2f}
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

        public IEnumerable<Thing> GetCurrentlyAvailableItems(Map map, RimworldTime time)
        {
            Initialize();
            return (map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon) ?? new List<Thing>())
                .Where(thing => IsAvailable(thing, time)).ToList();
        }

        public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map, RimworldTime time)
        {
            return GetCurrentlyAvailableItems(map, time).OrderByDescending(thing => GetThingScore(thing, time));
        }

        private IEnumerable<ThingDef> GetGloballyAvailableItems()
        {
            Initialize();
            return GloballyAvailableItems;
        }

        public IEnumerable<ThingDef> GetGloballyAvailableItemsSorted(RimworldTime time)
        {
            return GetGloballyAvailableItems().OrderByDescending(def => GetThingDefScore(def, time));
        }

        private static float GetStatValue([NotNull] Thing thing, [NotNull] StatDef statDef, RimworldTime time)
        {
            return thing == null ? throw new ArgumentNullException(nameof(thing)) :
                statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                EquipmentManager.GetRangedWeaponCache(thing, time).GetStatValue(statDef);
        }

        private float GetThingDefScore([NotNull] ThingDef def, RimworldTime time)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            var cache = EquipmentManager.GetRangedWeaponDefCache(def, time);
            return StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                    cache.GetStatValueDeviation(statWeight.StatDef)) * statWeight.Weight);
        }

        public float GetThingScore([NotNull] Thing thing, RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            var cache = EquipmentManager.GetRangedWeaponCache(thing, time);
            var score = StatWeights.Where(sw => sw.StatDef != null).Sum(statWeight =>
                EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                    cache.GetStatValueDeviation(statWeight.StatDef)) * statWeight.Weight);
            if (thing.def.useHitPoints)
            {
                score *= HitPointsCurve.Evaluate((float) thing.HitPoints / thing.MaxHitPoints);
            }
            return score;
        }

        public bool IsAvailable(Thing thing, RimworldTime time)
        {
            Initialize();
            var comp = thing.TryGetComp<CompForbiddable>();
            return (comp == null || !comp.Forbidden) && (GetWhitelistedItems().Contains(thing.def) ||
                (GetGloballyAvailableItems().Contains(thing.def) && SatisfiesLimits(thing, time)));
        }

        private bool SatisfiesLimits([NotNull] Thing thing, RimworldTime time)
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
                        verb?.defaultProjectile != null && verb.defaultProjectile.projectile.explosionRadius > 0) !=
                    Explosive);
            }
            if (ManualCast != null)
            {
                _ = GloballyAvailableItems.RemoveWhere(def => def.Verbs.Any(verb => verb.onlyManualCast) != ManualCast);
            }
            _ = GloballyAvailableItems.RemoveWhere(def => GetBlacklistedItems().Contains(def));
            foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems.Add(def); }
        }

        public void UpdateStatRanges([NotNull] Thing thing, RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            var cache = EquipmentManager.GetRangedWeaponCache(thing, time);
            var stats = StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
                .Union(StatLimits.Where(sl => sl.StatDef != null).Select(sl => sl.StatDef));
            foreach (var stat in stats) { EquipmentManager.UpdateStatRange(stat, cache.GetStatValueDeviation(stat)); }
        }
    }
}