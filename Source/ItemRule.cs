using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class ItemRule : IExposable
    {
        private static EquipmentManagerGameComponent _equipmentManager;
        private HashSet<ThingDef> _blacklistedItems = new HashSet<ThingDef>();
        private int _id;
        private bool _initialized;
        private bool _protected;
        private HashSet<ThingDef> _whitelistedItems = new HashSet<ThingDef>();
        protected HashSet<string> BlacklistedItemsDefNames = new HashSet<string>();
        protected HashSet<ThingDef> GloballyAvailableItems = new HashSet<ThingDef>();
        public string Label;
        protected List<StatLimit> StatLimits = new List<StatLimit>();
        protected List<StatWeight> StatWeights = new List<StatWeight>();
        protected HashSet<string> WhitelistedItemsDefNames = new HashSet<string>();

        protected ItemRule(int id, string label, bool isProtected, List<StatWeight> statWeights,
            List<StatLimit> statLimits, HashSet<string> whitelistedItemsDefNames,
            HashSet<string> blacklistedItemsDefNames)
        {
            _id = id;
            Label = label;
            _protected = isProtected;
            StatWeights = statWeights;
            StatLimits = statLimits;
            WhitelistedItemsDefNames = whitelistedItemsDefNames;
            BlacklistedItemsDefNames = blacklistedItemsDefNames;
        }

        protected ItemRule() { }

        protected ItemRule(int id, bool isProtected)
        {
            _id = id;
            _protected = isProtected;
        }

        protected static IEnumerable<StatWeight> DefaultStatWeights =>
            new[]
            {
                new StatWeight("Mass", false) {Weight = -0.1f}, new StatWeight("MarketValue", false) {Weight = 0.1f}
            };

        protected static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public int Id => _id;
        public bool Protected => _protected;

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref _id, nameof(Id));
            Scribe_Values.Look(ref Label, nameof(Label));
            Scribe_Values.Look(ref _protected, nameof(Protected));
            Scribe_Collections.Look(ref StatWeights, nameof(StatWeights), LookMode.Deep);
            Scribe_Collections.Look(ref StatLimits, nameof(StatLimits), LookMode.Deep);
            Scribe_Collections.Look(ref WhitelistedItemsDefNames, nameof(WhitelistedItemsDefNames), LookMode.Value);
            Scribe_Collections.Look(ref BlacklistedItemsDefNames, nameof(BlacklistedItemsDefNames), LookMode.Value);
        }

        public void AddBlacklistedItem([NotNull] ThingDef thingDef)
        {
            if (thingDef == null) { throw new ArgumentNullException(nameof(thingDef)); }
            if (!BlacklistedItemsDefNames.Add(thingDef.defName)) { return; }
            _ = WhitelistedItemsDefNames.Remove(thingDef.defName);
            UpdateExclusiveItems();
        }

        public void AddWhitelistedItem([NotNull] ThingDef thingDef)
        {
            if (thingDef == null) { throw new ArgumentNullException(nameof(thingDef)); }
            if (!WhitelistedItemsDefNames.Add(thingDef.defName)) { return; }
            _ = BlacklistedItemsDefNames.Remove(thingDef.defName);
            UpdateExclusiveItems();
        }

        public void DeleteBlacklistedItem(string defName)
        {
            _ = BlacklistedItemsDefNames.Remove(defName);
            UpdateExclusiveItems();
        }

        public void DeleteStatLimit(string statDefName)
        {
            _ = StatLimits.RemoveAll(limit => limit.StatDefName == statDefName);
        }

        public void DeleteStatWeight(string statDefName)
        {
            _ = StatWeights.RemoveAll(weight => weight.StatDefName == statDefName);
        }

        public void DeleteWhitelistedItem(string defName)
        {
            _ = WhitelistedItemsDefNames.Remove(defName);
            UpdateExclusiveItems();
        }

        public IReadOnlyCollection<ThingDef> GetBlacklistedItems()
        {
            Initialize();
            return _blacklistedItems;
        }

        public IReadOnlyList<StatLimit> GetStatLimits()
        {
            Initialize();
            return StatLimits;
        }

        protected float GetStatScore([NotNull] ThingDef def, IReadOnlyCollection<WorkTypeDef> workTypeDefs = null)
        {
            return def == null
                ? throw new ArgumentNullException(nameof(def))
                : StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                    EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                        StatHelper.GetStatValueDeviation(def, statWeight.StatDef, new RimworldTime(0, 0, 0f),
                            workTypeDefs)) * statWeight.Weight);
        }

        public float GetStatScore([NotNull] Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs = null)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            var score = StatWeights.Where(statWeight => statWeight.StatDef != null).Sum(statWeight =>
                EquipmentManager.NormalizeStatValue(statWeight.StatDef,
                    StatHelper.GetStatValueDeviation(thing, statWeight.StatDef, new RimworldTime(0, 0, 0f),
                        workTypeDefs)) * statWeight.Weight);
            if (thing.def.useHitPoints) { score *= (float) thing.HitPoints / thing.MaxHitPoints; }
            return score;
        }

        public IReadOnlyList<StatWeight> GetStatWeights()
        {
            Initialize();
            return StatWeights;
        }

        public IReadOnlyCollection<ThingDef> GetWhitelistedItems()
        {
            Initialize();
            return _whitelistedItems;
        }

        protected void Initialize()
        {
            if (_initialized) { return; }
            _initialized = true;
            if (StatWeights == null) { StatWeights = new List<StatWeight>(); }
            if (StatLimits == null) { StatLimits = new List<StatLimit>(); }
            if (_whitelistedItems == null) { _whitelistedItems = new HashSet<ThingDef>(); }
            if (WhitelistedItemsDefNames == null) { WhitelistedItemsDefNames = new HashSet<string>(); }
            if (_blacklistedItems == null) { _blacklistedItems = new HashSet<ThingDef>(); }
            if (BlacklistedItemsDefNames == null) { BlacklistedItemsDefNames = new HashSet<string>(); }
            if (GloballyAvailableItems == null) { GloballyAvailableItems = new HashSet<ThingDef>(); }
            UpdateExclusiveItems();
        }

        protected bool SatisfiesLimits([NotNull] Thing thing, IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            if (thing == null) { throw new ArgumentNullException(nameof(thing)); }
            foreach (var statLimit in StatLimits.Where(limit => limit.StatDef != null))
            {
                var value = StatHelper.GetStatValue(thing, statLimit.StatDef, new RimworldTime(0, 0, 0f), workTypeDefs);
                if ((statLimit.MinValue != null && value < statLimit.MinValue) ||
                    (statLimit.MaxValue != null && value > statLimit.MaxValue)) { return false; }
            }
            return true;
        }

        public void SetStatLimit([NotNull] StatDef statDef, float? min, float? max)
        {
            if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
            var statLimit = StatLimits.FirstOrDefault(limit => limit.StatDef == statDef);
            if (statLimit == null)
            {
                statLimit = new StatLimit(statDef.defName);
                StatLimits.Add(statLimit);
            }
            statLimit.MinValue = min;
            statLimit.MinValueBuffer = min.ToString();
            statLimit.MaxValue = max;
            statLimit.MaxValueBuffer = max.ToString();
        }

        public void SetStatWeight([NotNull] StatDef statDef, float weight, bool isProtected)
        {
            if (statDef == null) { throw new ArgumentNullException(nameof(statDef)); }
            var statWeight = StatWeights.FirstOrDefault(sw => sw.StatDef == statDef);
            if (statWeight == null)
            {
                statWeight = new StatWeight(statDef.defName, isProtected);
                StatWeights.Add(statWeight);
            }
            statWeight.Weight = weight;
        }

        private void UpdateExclusiveItems()
        {
            _whitelistedItems.Clear();
            foreach (var def in WhitelistedItemsDefNames.Select(DefDatabase<ThingDef>.GetNamedSilentFail)
                         .Where(def => def != null)) { _ = _whitelistedItems.Add(def); }
            _blacklistedItems.Clear();
            foreach (var def in BlacklistedItemsDefNames.Select(DefDatabase<ThingDef>.GetNamedSilentFail)
                         .Where(def => def != null)) { _ = _blacklistedItems.Add(def); }
        }

        internal enum ToolEquipMode
        {
            OneForEveryWorkType,
            OneForEveryAssignedWorkType,
            BestOne,
            AllAvailable
        }

        internal enum WeaponEquipMode
        {
            BestOne,
            AllAvailable
        }
    }
}