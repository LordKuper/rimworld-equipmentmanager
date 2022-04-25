using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules.Tools;

namespace EquipmentManager
{
    internal class ToolRule : ItemRule
    {
        private static HashSet<ThingDef> _allRelevantThings;
        private bool? _ranged;
        public ToolEquipMode EquipMode = ToolEquipMode.OneForEveryAssignedWorkType;
        public ToolRule(int id, bool isProtected) : base(id, isProtected) { }

        [UsedImplicitly]
        public ToolRule() { }

        public ToolRule(int id, string label, bool isProtected, List<StatWeight> statWeights,
            List<StatLimit> statLimits, HashSet<string> whitelistedItemsDefNames,
            HashSet<string> blacklistedItemsDefNames, ToolEquipMode equipMode, bool? ranged) : base(id, label,
            isProtected, statWeights, statLimits, whitelistedItemsDefNames, blacklistedItemsDefNames)
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
                    var relevantStats = EquipmentManager.GetWorkTypeRules().SelectMany(rule => rule.RequiredStats)
                        .ToHashSet();
                    _allRelevantThings = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where(def =>
                        def.IsWeapon && !def.destroyOnDrop && (def.statBases ?? new List<StatModifier>())
                        .Union(def.equippedStatOffsets ?? new List<StatModifier>())
                        .Any(sm => relevantStats.Contains(sm.stat))));
                }
                return _allRelevantThings;
            }
        }

        public static IEnumerable<string> DefaultBlacklist => Array.Empty<string>();

        public static IEnumerable<ToolRule> DefaultRules =>
            new[]
            {
                new ToolRule(0, true)
                {
                    Label = Strings.Default.AssignedWorkTypes,
                    EquipMode = ToolEquipMode.OneForEveryAssignedWorkType,
                    StatWeights = new List<StatWeight>(DefaultStatWeights),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
                },
                new ToolRule(1, true)
                {
                    Label = Strings.Default.AllWorkTypes,
                    EquipMode = ToolEquipMode.OneForEveryWorkType,
                    StatWeights = new List<StatWeight>(DefaultStatWeights),
                    BlacklistedItemsDefNames = new HashSet<string>(DefaultBlacklist)
                }
            };

        public new static IEnumerable<StatWeight> DefaultStatWeights =>
            new[]
            {
                new StatWeight(CustomToolStats.GetStatDefName(CustomToolStat.WorkType), true) {Weight = 2.0f},
                new StatWeight("MoveSpeed", false) {Weight = 1.0f}
            }.Union(ItemRule.DefaultStatWeights);

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

        public IEnumerable<Thing> GetCurrentlyAvailableItems(Map map, IReadOnlyCollection<WorkTypeDef> workTypeDefs,
            RimworldTime time)
        {
            Initialize();
            return (map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon) ?? new List<Thing>())
                .Where(thing => IsAvailable(thing, workTypeDefs, time)).ToList();
        }

        public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimworldTime time)
        {
            return !workTypeDefs.Any()
                ? throw new ArgumentException("At least one work type must be passed", nameof(workTypeDefs))
                : GetCurrentlyAvailableItems(map, workTypeDefs, time)
                    .OrderByDescending(thing => GetThingScore(thing, workTypeDefs, time));
        }

        private IEnumerable<ThingDef> GetGloballyAvailableItems(IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            Initialize();
            var relevantStats = EquipmentManager.GetWorkTypeRules()
                .Where(wtr => workTypeDefs.Any(wtd => wtd.defName == wtr.WorkTypeDefName))
                .SelectMany(rule => rule.RequiredStats).ToHashSet();
            return GloballyAvailableItems.Where(def => (def.statBases ?? new List<StatModifier>())
                .Union(def.equippedStatOffsets ?? new List<StatModifier>()).Any(sm => relevantStats.Contains(sm.stat)));
        }

        public IEnumerable<ThingDef> GetGloballyAvailableItemsSorted(IReadOnlyCollection<WorkTypeDef> workTypeDefs,
            RimworldTime time)
        {
            return GetGloballyAvailableItems(workTypeDefs)
                .OrderByDescending(def => GetThingDefScore(def, workTypeDefs, time));
        }

        private static float GetStatValue([NotNull] Thing thing, [NotNull] StatDef statDef,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimworldTime time)
        {
            return thing == null ? throw new ArgumentNullException(nameof(thing)) :
                statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                EquipmentManager.GetToolCache(thing, time).GetStatValue(statDef, workTypeDefs);
        }

        private float GetThingDefScore([NotNull] ThingDef def, IReadOnlyCollection<WorkTypeDef> workTypeDefs,
            RimworldTime time)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            var cache = EquipmentManager.GetToolDefCache(def, time);
            return StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                    cache.GetStatValueDeviation(statWeight.StatDef, workTypeDefs)) * statWeight.Weight);
        }

        public float GetThingScore([NotNull] Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs,
            RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            var cache = EquipmentManager.GetToolCache(thing, time);
            var score = StatWeights.Where(sw => sw.StatDef != null).Sum(statWeight =>
                EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                    cache.GetStatValueDeviation(statWeight.StatDef, workTypeDefs)) * statWeight.Weight);
            if (thing.def.useHitPoints)
            {
                score *= HitPointsCurve.Evaluate((float) thing.HitPoints / thing.MaxHitPoints);
            }
            return score;
        }

        public bool IsAvailable(Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs, RimworldTime time)
        {
            Initialize();
            var comp = thing.TryGetComp<CompForbiddable>();
            return (comp == null || !comp.Forbidden) && (GetWhitelistedItems().Contains(thing.def) ||
                (GetGloballyAvailableItems(workTypeDefs).Contains(thing.def) &&
                    SatisfiesLimits(thing, workTypeDefs, time)));
        }

        private bool SatisfiesLimits([NotNull] Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs,
            RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            foreach (var statLimit in StatLimits.Where(limit => limit.StatDef != null))
            {
                var value = GetStatValue(thing, statLimit.StatDef, workTypeDefs, time);
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
            if (Ranged != null) { _ = GloballyAvailableItems.RemoveWhere(def => def.IsRangedWeapon != Ranged); }
            _ = GloballyAvailableItems.RemoveWhere(def => GetBlacklistedItems().Contains(def));
            foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems.Add(def); }
        }

        public void UpdateStatRanges([NotNull] Thing thing, [NotNull] IReadOnlyCollection<WorkTypeDef> workTypeDefs,
            RimworldTime time)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            if (workTypeDefs == null) { throw new ArgumentNullException(nameof(workTypeDefs)); }
            var cache = EquipmentManager.GetToolCache(thing, time);
            var stats = StatWeights.Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
                .Union(StatLimits.Where(sl => sl.StatDef != null).Select(sl => sl.StatDef));
            foreach (var stat in stats)
            {
                EquipmentManager.UpdateStatRange(stat, cache.GetStatValueDeviation(stat, workTypeDefs));
            }
        }
    }
}