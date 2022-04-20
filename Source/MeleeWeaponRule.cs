using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules.MeleeWeapons;

namespace EquipmentManager
{
    internal class MeleeWeaponRule : ItemRule
    {
        public delegate bool UsableWithShieldsDelegate(ThingDef thing);

        private static HashSet<ThingDef> _allRelevantThings;
        public static UsableWithShieldsDelegate UsableWithShieldsMethod;
        private bool? _rottable;
        private bool? _usableWithShields;
        public WeaponEquipMode EquipMode = WeaponEquipMode.BestOne;
        public MeleeWeaponRule(int id, bool isProtected) : base(id, isProtected) { }

        [UsedImplicitly]
        public MeleeWeaponRule() { }

        public MeleeWeaponRule(int id, string label, bool isProtected, List<StatWeight> statWeights,
            List<StatLimit> statLimits, HashSet<string> whitelistedItemsDefNames,
            HashSet<string> blacklistedItemsDefNames, WeaponEquipMode equipMode, bool? usableWithShields,
            bool? rottable) : base(id, label, isProtected, statWeights, statLimits, whitelistedItemsDefNames,
            blacklistedItemsDefNames)
        {
            EquipMode = equipMode;
            _usableWithShields = usableWithShields;
            _rottable = rottable;
        }

        public static HashSet<ThingDef> AllRelevantThings
        {
            get
            {
                if (_allRelevantThings == null || _allRelevantThings.Count == 0)
                {
                    _allRelevantThings = new HashSet<ThingDef>(
                        DefDatabase<ThingDef>.AllDefs.Where(def => def.IsMeleeWeapon && !def.destroyOnDrop));
                }
                return _allRelevantThings;
            }
        }

        public static IEnumerable<string> DefaultBlacklist => new[] {"WoodLog", "Beer"};

        public static IEnumerable<MeleeWeaponRule> DefaultRules =>
            new[]
            {
                new MeleeWeaponRule(0, true)
                {
                    Label = Strings.Default.HighestDps,
                    EquipMode = WeaponEquipMode.BestOne,
                    Rottable = false,
                    StatWeights = new List<StatWeight>(DefaultStatWeights),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
                },
                new MeleeWeaponRule(1, false)
                {
                    Label = Strings.Default.OneHandHighestDps,
                    EquipMode = WeaponEquipMode.BestOne,
                    Rottable = false,
                    UsableWithShields = true,
                    StatWeights = new List<StatWeight>(DefaultStatWeights),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
                }
            };

        public new static IEnumerable<StatWeight> DefaultStatWeights =>
            new[]
            {
                new StatWeight("MeleeWeapon_AverageDPS", false) {Weight = 2.0f},
                new StatWeight(CustomMeleeWeaponStats.GetStatDefName(CustomMeleeWeaponStat.ArmorPenetration), false)
                {
                    Weight = 0.5f
                }
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
                EquipmentManager.GetMeleeWeaponCache(thing, time).GetStatValue(statDef);
        }

        private float GetThingDefScore([NotNull] ThingDef def, RimworldTime time)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            var cache = EquipmentManager.GetMeleeWeaponDefCache(def, time);
            return StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                    cache.GetStatValueDeviation(statWeight.StatDef)) * statWeight.Weight);
        }

        public float GetThingScore([NotNull] Thing thing, RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            var cache = EquipmentManager.GetMeleeWeaponCache(thing, time);
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
            if (UsableWithShields != null && UsableWithShieldsMethod != null)
            {
                _ = GloballyAvailableItems.RemoveWhere(def => UsableWithShieldsMethod(def) != UsableWithShields);
            }
            if (Rottable != null)
            {
                _ = GloballyAvailableItems.RemoveWhere(def =>
                    def.GetCompProperties<CompProperties_Rottable>() != null != Rottable);
            }
            _ = GloballyAvailableItems.RemoveWhere(def => GetBlacklistedItems().Contains(def));
            foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems.Add(def); }
        }

        public void UpdateStatRanges([NotNull] Thing thing, RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            var cache = EquipmentManager.GetMeleeWeaponCache(thing, time);
            var stats = StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
                .Union(StatLimits.Where(sl => sl.StatDef != null).Select(sl => sl.StatDef));
            foreach (var stat in stats) { EquipmentManager.UpdateStatRange(stat, cache.GetStatValueDeviation(stat)); }
        }
    }
}