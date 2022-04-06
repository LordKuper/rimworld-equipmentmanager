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
                    var relevantStats = EquipmentManager.GetWorkTypeRules().SelectMany(rule =>
                            rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)).Distinct()
                        .ToList();
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
                new StatWeight(StatHelper.GetCustomToolStatDefName(CustomToolStat.WorkType), true) {Weight = 2.0f},
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

        public IEnumerable<Thing> GetCurrentlyAvailableItems(Map map, IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            Initialize();
            return !workTypeDefs.Any()
                ? throw new ArgumentException("At least one work type must be passed", nameof(workTypeDefs))
                : map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon)
                    .Where(thing => IsAvailable(thing, workTypeDefs));
        }

        public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map,
            IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            return !workTypeDefs.Any()
                ? throw new ArgumentException("At least one work type must be passed", nameof(workTypeDefs))
                : GetCurrentlyAvailableItems(map, workTypeDefs)
                    .OrderByDescending(thing => GetStatScore(thing, workTypeDefs));
        }

        private IEnumerable<ThingDef> GetGloballyAvailableItems(IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            Initialize();
            var relevantStats = EquipmentManager.GetWorkTypeRules()
                .Where(wtr => workTypeDefs.Any(wtd => wtd.defName == wtr.WorkTypeDefName)).SelectMany(rule =>
                    rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)).Distinct().ToList();
            return GloballyAvailableItems.Where(def => (def.statBases ?? new List<StatModifier>())
                .Union(def.equippedStatOffsets ?? new List<StatModifier>()).Any(sm => relevantStats.Contains(sm.stat)));
        }

        public IEnumerable<ThingDef> GetGloballyAvailableItemsSorted(IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            return GetGloballyAvailableItems(workTypeDefs).OrderByDescending(def => GetStatScore(def, workTypeDefs));
        }

        public bool IsAvailable(Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            Initialize();
            if (!workTypeDefs.Any())
            {
                throw new ArgumentException("At least one work type must be passed", nameof(workTypeDefs));
            }
            var comp = thing.TryGetComp<CompForbiddable>();
            return (comp == null || !comp.Forbidden) && (GetWhitelistedItems().Contains(thing.def) ||
                (GetGloballyAvailableItems(workTypeDefs).Contains(thing.def) && SatisfiesLimits(thing, workTypeDefs)));
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
    }
}