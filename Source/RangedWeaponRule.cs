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
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Dpsa), false)
                        {
                            Weight = 2.0f
                        },
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Range),
                            false) {Weight = 0.5f}
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
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
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Warmup),
                            false) {Weight = -2.0f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.DpsaShort),
                            false) {Weight = 1.0f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Dpsa), false)
                        {
                            Weight = 0.5f
                        }
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
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
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat
                                .BurstShotCount), false) {Weight = 2.0f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat
                                .TicksBetweenBurstShots), false) {Weight = -2.0f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Warmup),
                            false) {Weight = -0.5f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Dpsa), false)
                        {
                            Weight = 1.0f
                        },
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Range),
                            false) {Weight = 0.5f},
                        new StatWeight("RangedWeapon_Cooldown", false) {Weight = -1.5f}
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
                },
                new RangedWeaponRule(3, false)
                {
                    Label = Strings.Default.LongRangeHeavyHitter,
                    EquipMode = WeaponEquipMode.BestOne,
                    Explosive = false,
                    ManualCast = false,
                    StatWeights = new List<StatWeight>(DefaultStatWeights.Union(new[]
                    {
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Range),
                            false) {Weight = 2.0f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Damage),
                            false) {Weight = 1.5f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.DpsaLong),
                            false) {Weight = 1.0f},
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.Dpsa), false)
                        {
                            Weight = 0.5f
                        },
                        new StatWeight(
                            StatHelper.GetCustomRangedWeaponStatDefName(
                                CustomRangedWeaponStat.StoppingPower), false) {Weight = 0.5f}
                    })),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
                }
            };

        public new static IEnumerable<StatWeight> DefaultStatWeights =>
            new[]
            {
                new StatWeight(StatHelper.GetCustomRangedWeaponStatDefName(CustomRangedWeaponStat.ArmorPenetration),
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
        }

        public IEnumerable<Thing> GetCurrentlyAvailableItems(Map map)
        {
            Initialize();
            return (map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon).Where(thing => !thing.Fogged()) ??
                new List<Thing>()).Where(IsAvailable).ToList();
        }

        public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map)
        {
            return GetCurrentlyAvailableItems(map).OrderByDescending(thing => GetStatScore(thing));
        }

        private IEnumerable<ThingDef> GetGloballyAvailableItems()
        {
            Initialize();
            return GloballyAvailableItems;
        }

        public IEnumerable<ThingDef> GetGloballyAvailableItemsSorted()
        {
            return GetGloballyAvailableItems().OrderByDescending(def => GetStatScore(def));
        }

        public bool IsAvailable(Thing thing)
        {
            Initialize();
            var comp = thing.TryGetComp<CompForbiddable>();
            return (comp == null || !comp.Forbidden) && (GetWhitelistedItems().Contains(thing.def) ||
                (GetGloballyAvailableItems().Contains(thing.def) && SatisfiesLimits(thing, null)));
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
    }
}