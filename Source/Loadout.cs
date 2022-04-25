using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Strings = EquipmentManager.Resources.Strings.Loadouts;

namespace EquipmentManager
{
    internal class Loadout : IExposable
    {
        public enum PrimaryWeaponType
        {
            None,
            RangedWeapon,
            MeleeWeapon
        }

        public static readonly IEnumerable<Loadout> DefaultLoadouts = new[]
        {
            new Loadout(1)
            {
                Label = Strings.Default.Assault,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
                PrimaryRangedWeaponRuleId = 0,
                RangedSidearmRules = {1},
                MeleeSidearmRules = {0},
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", false}},
                PawnWorkCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true
            },
            new Loadout(2)
            {
                Label = Strings.Default.Sniper,
                Priority = 3,
                PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
                PrimaryRangedWeaponRuleId = 3,
                RangedSidearmRules = {1},
                MeleeSidearmRules = {0},
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", false}},
                PawnWorkCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true,
                SkillWeights = {new SkillWeight("Shooting", 1f)},
                StatWeights = {new StatWeight("ShootingAccuracyPawn", 2f, false)}
            },
            new Loadout(3)
            {
                Label = Strings.Default.Support,
                Priority = 2,
                PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
                PrimaryRangedWeaponRuleId = 2,
                RangedSidearmRules = {1},
                MeleeSidearmRules = {0},
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", false}},
                PawnWorkCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true,
                SkillWeights = {new SkillWeight("Shooting", -1f)},
                StatWeights = {new StatWeight("ShootingAccuracyPawn", -2f, false)}
            },
            new Loadout(4)
            {
                Label = Strings.Default.Slasher,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.MeleeWeapon,
                PrimaryMeleeWeaponRuleId = 1,
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", true}},
                PawnWorkCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true
            },
            new Loadout(5)
            {
                Label = Strings.Default.Crusher,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.MeleeWeapon,
                PrimaryMeleeWeaponRuleId = 2,
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", true}},
                PawnWorkCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true
            },
            new Loadout(6)
            {
                Label = Strings.Default.Pacifist,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.None,
                ToolRuleId = 0,
                PawnWorkCapacities = {{"Violent", false}},
                DropUnassignedWeapons = true
            }
        };

        private int _id;
        private bool _initialized;
        private List<int> _meleeSidearmRules = new List<int>();
        private List<PassionLimit> _passionLimits = new List<PassionLimit>();
        private List<PawnCapacityLimit> _pawnCapacityLimits = new List<PawnCapacityLimit>();
        private List<PawnCapacityWeight> _pawnCapacityWeights = new List<PawnCapacityWeight>();
        private Dictionary<string, bool> _pawnTraits = new Dictionary<string, bool>();
        private Dictionary<string, bool> _pawnWorkCapacities = new Dictionary<string, bool>();
        private PrimaryWeaponType _primaryRuleType = PrimaryWeaponType.None;
        private List<int> _rangedSidearmRules = new List<int>();
        private List<SkillLimit> _skillLimits = new List<SkillLimit>();
        private List<SkillWeight> _skillWeights = new List<SkillWeight>();
        private List<StatLimit> _statLimits = new List<StatLimit>();
        private List<StatWeight> _statWeights = new List<StatWeight>();
        public bool DropUnassignedWeapons = true;
        public string Label;
        public int? PrimaryMeleeWeaponRuleId;
        public int? PrimaryRangedWeaponRuleId;
        public int Priority;
        public int? ToolRuleId;

        [UsedImplicitly]
        public Loadout() { }

        public Loadout(int id)
        {
            _id = id;
        }

        public Loadout(int id, string label, int priority, PrimaryWeaponType primaryRuleType,
            int? primaryRangedWeaponRuleId, int? primaryMeleeWeaponRuleId, List<int> rangedSidearmRules,
            List<int> meleeSidearmRules, int? toolRuleId, Dictionary<string, bool> pawnTraits,
            Dictionary<string, bool> pawnWorkCapacities, bool dropUnassignedWeapons)
        {
            _id = id;
            Label = label;
            Priority = priority;
            _primaryRuleType = primaryRuleType;
            PrimaryRangedWeaponRuleId = primaryRangedWeaponRuleId;
            PrimaryMeleeWeaponRuleId = primaryMeleeWeaponRuleId;
            _rangedSidearmRules = rangedSidearmRules;
            _meleeSidearmRules = meleeSidearmRules;
            ToolRuleId = toolRuleId;
            _pawnTraits = pawnTraits;
            _pawnWorkCapacities = pawnWorkCapacities;
            DropUnassignedWeapons = dropUnassignedWeapons;
        }

        public int Id => _id;

        public List<int> MeleeSidearmRules
        {
            get
            {
                Initialize();
                return _meleeSidearmRules;
            }
        }

        public List<PassionLimit> PassionLimits
        {
            get
            {
                Initialize();
                return _passionLimits;
            }
        }

        public List<PawnCapacityLimit> PawnCapacityLimits
        {
            get
            {
                Initialize();
                return _pawnCapacityLimits;
            }
        }

        public List<PawnCapacityWeight> PawnCapacityWeights
        {
            get
            {
                Initialize();
                return _pawnCapacityWeights;
            }
        }

        public Dictionary<string, bool> PawnTraits
        {
            get
            {
                Initialize();
                return _pawnTraits;
            }
        }

        public Dictionary<string, bool> PawnWorkCapacities
        {
            get
            {
                Initialize();
                return _pawnWorkCapacities;
            }
        }

        public PrimaryWeaponType PrimaryRuleType
        {
            get => _primaryRuleType;
            set
            {
                _primaryRuleType = value;
                switch (_primaryRuleType)
                {
                    case PrimaryWeaponType.None:
                        PrimaryMeleeWeaponRuleId = null;
                        PrimaryRangedWeaponRuleId = null;
                        break;
                    case PrimaryWeaponType.RangedWeapon:
                        PrimaryMeleeWeaponRuleId = null;
                        break;
                    case PrimaryWeaponType.MeleeWeapon:
                        PrimaryRangedWeaponRuleId = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public List<int> RangedSidearmRules
        {
            get
            {
                Initialize();
                return _rangedSidearmRules;
            }
        }

        public List<SkillLimit> SkillLimits
        {
            get
            {
                Initialize();
                return _skillLimits;
            }
        }

        public List<SkillWeight> SkillWeights
        {
            get
            {
                Initialize();
                return _skillWeights;
            }
        }

        public List<StatLimit> StatLimits
        {
            get
            {
                Initialize();
                return _statLimits;
            }
        }

        public List<StatWeight> StatWeights
        {
            get
            {
                Initialize();
                return _statWeights;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _id, nameof(Id));
            Scribe_Values.Look(ref Label, nameof(Label));
            Scribe_Values.Look(ref Priority, nameof(Priority));
            Scribe_Values.Look(ref _primaryRuleType, nameof(PrimaryRuleType));
            Scribe_Values.Look(ref PrimaryRangedWeaponRuleId, nameof(PrimaryRangedWeaponRuleId));
            Scribe_Values.Look(ref PrimaryMeleeWeaponRuleId, nameof(PrimaryMeleeWeaponRuleId));
            Scribe_Collections.Look(ref _rangedSidearmRules, nameof(RangedSidearmRules));
            Scribe_Collections.Look(ref _meleeSidearmRules, nameof(MeleeSidearmRules));
            Scribe_Values.Look(ref ToolRuleId, nameof(ToolRuleId));
            Scribe_Collections.Look(ref _pawnTraits, nameof(PawnTraits));
            Scribe_Collections.Look(ref _pawnWorkCapacities, nameof(PawnWorkCapacities));
            Scribe_Values.Look(ref DropUnassignedWeapons, nameof(DropUnassignedWeapons));
            Scribe_Collections.Look(ref _passionLimits, nameof(PassionLimits), LookMode.Deep);
            Scribe_Collections.Look(ref _pawnCapacityLimits, nameof(PawnCapacityLimits), LookMode.Deep);
            Scribe_Collections.Look(ref _pawnCapacityWeights, nameof(PawnCapacityWeights), LookMode.Deep);
            Scribe_Collections.Look(ref _skillLimits, nameof(SkillLimits), LookMode.Deep);
            Scribe_Collections.Look(ref _skillWeights, nameof(SkillWeights), LookMode.Deep);
            Scribe_Collections.Look(ref _statLimits, nameof(StatLimits), LookMode.Deep);
            Scribe_Collections.Look(ref _statWeights, nameof(StatWeights), LookMode.Deep);
        }

        public IReadOnlyList<Pawn> GetAvailablePawnsOrdered()
        {
            Initialize();
            return new List<Pawn>(PawnsFinder.AllMaps_FreeColonistsSpawned.Where(IsAvailable)
                .OrderByDescending(GetScore));
        }

        public float GetScore(Pawn pawn)
        {
            Initialize();
            var pawns = PawnsFinder.AllMaps_FreeColonistsSpawned;
            var score = 0f;
            foreach (var statWeight in _statWeights.Where(sw => sw.StatDef != null))
            {
                var pawnValues = pawns.Select(p => p.GetStatValue(statWeight.StatDef)).ToList();
                var normalizedValue = StatHelper.NormalizeValue(pawn.GetStatValue(statWeight.StatDef),
                    new FloatRange(pawnValues.Min(), pawnValues.Max()));
                score += normalizedValue * statWeight.Weight;
            }
            foreach (var skillWeight in _skillWeights.Where(sw => sw.SkillDef != null))
            {
                var pawnValues = pawns.Select(p => p.skills.GetSkill(skillWeight.SkillDef).Level).ToList();
                var normalizedValue = StatHelper.NormalizeValue(pawn.skills.GetSkill(skillWeight.SkillDef).Level,
                    new FloatRange(pawnValues.Min(), pawnValues.Max()));
                score += normalizedValue * skillWeight.Weight;
            }
            foreach (var pawnCapacityWeight in _pawnCapacityWeights.Where(pcw => pcw.PawnCapacityDef != null))
            {
                var pawnValues = pawns.Select(p => p.health.capacities.GetLevel(pawnCapacityWeight.PawnCapacityDef))
                    .ToList();
                var normalizedValue = StatHelper.NormalizeValue(
                    pawn.health.capacities.GetLevel(pawnCapacityWeight.PawnCapacityDef),
                    new FloatRange(pawnValues.Min(), pawnValues.Max()));
                score += normalizedValue * pawnCapacityWeight.Weight;
            }
            return score;
        }

        private void Initialize()
        {
            if (_initialized) { return; }
            _initialized = true;
            if (_meleeSidearmRules == null) { _meleeSidearmRules = new List<int>(); }
            if (_rangedSidearmRules == null) { _rangedSidearmRules = new List<int>(); }
            if (_pawnTraits == null) { _pawnTraits = new Dictionary<string, bool>(); }
            if (_pawnWorkCapacities == null) { _pawnWorkCapacities = new Dictionary<string, bool>(); }
            if (_passionLimits == null) { _passionLimits = new List<PassionLimit>(); }
            if (_pawnCapacityLimits == null) { _pawnCapacityLimits = new List<PawnCapacityLimit>(); }
            if (_pawnCapacityWeights == null) { _pawnCapacityWeights = new List<PawnCapacityWeight>(); }
            if (_skillLimits == null) { _skillLimits = new List<SkillLimit>(); }
            if (_skillWeights == null) { _skillWeights = new List<SkillWeight>(); }
            if (_statLimits == null) { _statLimits = new List<StatLimit>(); }
            if (_statWeights == null) { _statWeights = new List<StatWeight>(); }
        }

        public bool IsAvailable(Pawn pawn)
        {
            Initialize();
            foreach (var pawnTrait in _pawnTraits)
            {
                var trait = DefDatabase<TraitDef>.GetNamedSilentFail(pawnTrait.Key);
                if (trait == null) { continue; }
                if (pawn.story.traits.HasTrait(trait) != pawnTrait.Value) { return false; }
            }
            foreach (var pawnCapacity in _pawnWorkCapacities)
            {
                if (!Enum.TryParse<WorkTags>(pawnCapacity.Key, out var tag)) { continue; }
                if (pawn.WorkTagIsDisabled(tag) == pawnCapacity.Value) { return false; }
            }
            foreach (var passionLimit in _passionLimits.Where(pl => pl.SkillDef != null))
            {
                var passion = pawn.skills.GetSkill(passionLimit.SkillDef).passion;
                switch (passionLimit.Value)
                {
                    case PassionValue.None:
                        if (passion != Passion.None) { return false; }
                        break;
                    case PassionValue.Minor:
                        if (passion != Passion.Minor) { return false; }
                        break;
                    case PassionValue.Major:
                        if (passion != Passion.Major) { return false; }
                        break;
                    case PassionValue.Any:
                        if (passion == Passion.None) { return false; }
                        break;
                }
            }
            foreach (var pawnCapacityLimit in _pawnCapacityLimits.Where(pcl => pcl.PawnCapacityDef != null))
            {
                var capacity = pawn.health.capacities.GetLevel(pawnCapacityLimit.PawnCapacityDef);
                if ((pawnCapacityLimit.MinValue != null && capacity < pawnCapacityLimit.MinValue) ||
                    (pawnCapacityLimit.MaxValue != null && capacity > pawnCapacityLimit.MaxValue)) { return false; }
            }
            foreach (var statLimit in _statLimits.Where(sl => sl.StatDef != null))
            {
                var statValue = pawn.GetStatValue(statLimit.StatDef);
                if ((statLimit.MinValue != null && statValue < statLimit.MinValue) ||
                    (statLimit.MaxValue != null && statValue > statLimit.MaxValue)) { return false; }
            }
            foreach (var skillLimit in _skillLimits.Where(sl => sl.SkillDef != null))
            {
                var skillValue = pawn.skills.GetSkill(skillLimit.SkillDef).Level;
                if ((skillLimit.MinValue != null && skillValue < skillLimit.MinValue) ||
                    (skillLimit.MaxValue != null && skillValue > skillLimit.MaxValue)) { return false; }
            }
            return true;
        }
    }
}