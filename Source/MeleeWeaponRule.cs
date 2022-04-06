using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules.MeleeWeapons;

namespace EquipmentManager
{
    internal class MeleeWeaponRule : ItemRule
    {
        private static HashSet<ThingDef> _allRelevantThings;
        public static MethodInfo UsableWithShieldsMethod = null;
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
                new StatWeight("MeleeWeapon_AverageArmorPenetration", false) {Weight = 0.5f}
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

        public IEnumerable<Thing> GetCurrentlyAvailableItems(Map map)
        {
            Initialize();
            return (map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon) ?? new List<Thing>()).Where(IsAvailable)
                .ToList();
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
            if (UsableWithShields != null && UsableWithShieldsMethod != null)
            {
                _ = GloballyAvailableItems.RemoveWhere(def =>
                    (bool) UsableWithShieldsMethod.Invoke(null, new object[] {def}) != UsableWithShields);
            }
            if (Rottable != null)
            {
                _ = GloballyAvailableItems.RemoveWhere(def =>
                    def.GetCompProperties<CompProperties_Rottable>() != null != Rottable);
            }
            _ = GloballyAvailableItems.RemoveWhere(def => GetBlacklistedItems().Contains(def));
            foreach (var def in GetWhitelistedItems()) { _ = GloballyAvailableItems.Add(def); }
        }
    }
}