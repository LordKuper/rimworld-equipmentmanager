using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PeteTimesSix.SimpleSidearms;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class WorkTypeRule : IExposable
    {
        private static HashSet<ThingDef> _allRelevantThings;
        private static EquipmentManagerGameComponent _equipmentManager;

        private static readonly Dictionary<string, IEnumerable<string>> DefaultWorkTypeStats =
            new Dictionary<string, IEnumerable<string>>
            {
                {
                    "Cooking",
                    new[]
                    {
                        "FoodPoisonChance", "DrugCookingSpeed", "ButcheryFleshSpeed", "ButcheryFleshEfficiency",
                        "CookSpeed"
                    }
                },
                {"Hunting", new[] {"HuntingStealth"}},
                {"Doctor", new[] {"MedicalTendQualityOffset", "MedicalPotency"}}
            };

        private readonly HashSet<StatDef> _requiredStats = new HashSet<StatDef>();
        private List<StatWeight> _defaultStatWeights;
        private bool _isInitialized;
        private List<StatWeight> _statWeights = new List<StatWeight>();
        private WorkTypeDef _workTypeDef;
        private string _workTypeDefName;

        [UsedImplicitly]
        public WorkTypeRule() { }

        public WorkTypeRule(string workTypeDefName)
        {
            _workTypeDefName = workTypeDefName;
        }

        public WorkTypeRule(string workTypeDefName, List<StatWeight> statWeights)
        {
            _workTypeDefName = workTypeDefName;
            _statWeights = statWeights;
        }

        private static IEnumerable<ThingDef> AllRelevantThings
        {
            get
            {
                if (_allRelevantThings == null || _allRelevantThings.Count == 0)
                {
                    _allRelevantThings = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where(def =>
                        def.IsWeapon && !def.destroyOnDrop &&
                        (def.statBases != null || def.equippedStatOffsets != null)));
                }
                return _allRelevantThings;
            }
        }

        public static IEnumerable<WorkTypeRule> DefaultRules
        {
            get
            {
                var rules = new List<WorkTypeRule>();
                foreach (var workTypeDef in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
                {
                    var rule = new WorkTypeRule(workTypeDef.defName);
                    foreach (var statWeight in rule.DefaultStatWeights.Where(sw => sw.StatDef != null))
                    {
                        rule.SetStatWeight(statWeight.StatDef, statWeight.Weight);
                    }
                    rules.Add(rule);
                }
                return rules;
            }
        }

        private IEnumerable<StatWeight> DefaultStatWeights
        {
            get
            {
                Initialize();
                return _defaultStatWeights;
            }
        }

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public string Label =>
            WorkTypeDef != null
                ? WorkTypeDef.labelShort.NullOrEmpty() ? WorkTypeDefName : WorkTypeDef.labelShort.CapitalizeFirst()
                : WorkTypeDefName;

        public IEnumerable<StatDef> RequiredStats
        {
            get
            {
                Initialize();
                return _requiredStats.Intersect(_statWeights.Select(sw => sw.StatDef));
            }
        }

        private WorkTypeDef WorkTypeDef
        {
            get
            {
                Initialize();
                return _workTypeDef;
            }
        }

        public string WorkTypeDefName => _workTypeDefName;

        public void ExposeData()
        {
            Scribe_Values.Look(ref _workTypeDefName, nameof(WorkTypeDefName));
            Scribe_Collections.Look(ref _statWeights, "StatWeights", LookMode.Deep);
        }

        public void DeleteStatWeight(string statDefName)
        {
            _ = _statWeights.RemoveAll(weight => weight.StatDefName == statDefName);
        }

        public IEnumerable<Thing> GetCurrentlyAvailableItemsSorted(Map map)
        {
            var items = new List<Thing>();
            var globalItemDefs = new HashSet<ThingDef>(GetGloballyAvailableItems());
            foreach (var thing in map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon).Where(thing =>
                         globalItemDefs.Contains(thing.def)) ?? Array.Empty<Thing>())
            {
                var comp = thing.TryGetComp<CompForbiddable>();
                if (comp != null && comp.Forbidden) { continue; }
                items.Add(thing);
            }
            items.SortByDescending(GetThingScore);
            return items;
        }

        public IEnumerable<ThingDef> GetGloballyAvailableItems()
        {
            var items = new List<ThingDef>();
            items.AddRange(AllRelevantThings.Where(def =>
                (def.statBases ?? new List<StatModifier>()).Union(def.equippedStatOffsets ?? new List<StatModifier>())
                .Any(sm => RequiredStats.Any(statDef => statDef == sm.stat))));
            items.SortByDescending(GetThingDefScore);
            return items;
        }

        public IReadOnlyList<StatWeight> GetStatWeights()
        {
            Initialize();
            return _statWeights;
        }

        private float GetThingDefScore([NotNull] ThingDef def)
        {
            return def == null
                ? throw new ArgumentNullException(nameof(def))
                : _statWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                    EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                        StatHelper.GetStatValueDeviation(def, statWeight.StatDef)) * statWeight.Weight);
        }

        public float GetThingScore([NotNull] Thing thing)
        {
            return thing == null
                ? throw new ArgumentNullException(nameof(thing))
                : _statWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                    EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                        StatHelper.GetStatValueDeviation(thing, statWeight.StatDef)) * statWeight.Weight);
        }

        private void Initialize()
        {
            if (_isInitialized) { return; }
            _isInitialized = true;
            _workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(_workTypeDefName);
            if (WorkTypeDef == null) { return; }
            _defaultStatWeights = new List<StatWeight>();
            if (DefaultWorkTypeStats.ContainsKey(WorkTypeDefName))
            {
                foreach (var statDefName in DefaultWorkTypeStats[WorkTypeDefName])
                {
                    if (!_defaultStatWeights.Any(sw => sw.StatDefName == statDefName))
                    {
                        _defaultStatWeights.Add(new StatWeight(statDefName, false) {Weight = 2f});
                    }
                }
            }
            foreach (var statDef in WorkTypeDef.relevantSkills.Where(skill => SkillStatMap.Map.ContainsKey(skill))
                         .Select(skill => SkillStatMap.Map[skill]).SelectMany(stats => stats))
            {
                _ = _requiredStats.Add(statDef);
                if (!_defaultStatWeights.Any(sw => sw.StatDefName == statDef.defName))
                {
                    _defaultStatWeights.Add(new StatWeight(statDef.defName, false) {Weight = 1f});
                }
            }
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefs.Where(recipeDef =>
                         recipeDef.requiredGiverWorkType == WorkTypeDef))
            {
                if (recipe.efficiencyStat != null &&
                    !_defaultStatWeights.Any(sw => sw.StatDefName == recipe.efficiencyStat.defName))
                {
                    if (!_defaultStatWeights.Any(sw => sw.StatDefName == recipe.efficiencyStat.defName))
                    {
                        _defaultStatWeights.Add(new StatWeight(recipe.efficiencyStat.defName, false) {Weight = 0.8f});
                    }
                }
                if (recipe.workSpeedStat != null)
                {
                    _ = _requiredStats.Add(recipe.workSpeedStat);
                    if (!_defaultStatWeights.Any(sw => sw.StatDefName == recipe.workSpeedStat.defName))
                    {
                        if (!_defaultStatWeights.Any(sw => sw.StatDefName == recipe.workSpeedStat.defName))
                        {
                            _defaultStatWeights.Add(
                                new StatWeight(recipe.workSpeedStat.defName, false) {Weight = 0.5f});
                        }
                    }
                }
                if (recipe.workTableEfficiencyStat != null &&
                    !_defaultStatWeights.Any(sw => sw.StatDefName == recipe.workTableEfficiencyStat.defName))
                {
                    if (!_defaultStatWeights.Any(sw => sw.StatDefName == recipe.workTableEfficiencyStat.defName))
                    {
                        _defaultStatWeights.Add(
                            new StatWeight(recipe.workTableEfficiencyStat.defName, false) {Weight = 0.8f});
                    }
                }
                if (recipe.workTableSpeedStat != null &&
                    !_defaultStatWeights.Any(sw => sw.StatDefName == recipe.workTableSpeedStat.defName))
                {
                    if (!_defaultStatWeights.Any(sw => sw.StatDefName == recipe.workTableSpeedStat.defName))
                    {
                        _defaultStatWeights.Add(
                            new StatWeight(recipe.workTableSpeedStat.defName, false) {Weight = 0.5f});
                    }
                }
            }
            _ = _defaultStatWeights.RemoveAll(sw => !StatHelper.WorkTypeStatDefs.Contains(sw.StatDef));
            _ = _requiredStats.RemoveWhere(statDef => !StatHelper.WorkTypeStatDefs.Contains(statDef));
        }

        public void SetStatWeight([NotNull] StatDef statDef, float weight)
        {
            if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
            var statWeight = _statWeights.FirstOrDefault(sw => sw.StatDef == statDef);
            if (statWeight == null)
            {
                statWeight = new StatWeight(statDef.defName, false);
                _statWeights.Add(statWeight);
            }
            statWeight.Weight = weight;
        }
    }
}