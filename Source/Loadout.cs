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
            new Loadout(0, true)
            {
                Label = Strings.Default.NoLoadout,
                Priority = 0,
                _primaryRuleType = PrimaryWeaponType.None,
                DropUnassignedWeapons = false
            },
            new Loadout(1, false)
            {
                Label = Strings.Default.Sniper,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
                PrimaryRangedWeaponRuleId = 3,
                RangedSidearmRules = {1},
                MeleeSidearmRules = {0},
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", false}},
                PawnCapacities = {{"Violent", true}},
                PreferredSkills = {"Shooting"},
                DropUnassignedWeapons = true
            },
            new Loadout(2, false)
            {
                Label = Strings.Default.Assault,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
                PrimaryRangedWeaponRuleId = 0,
                RangedSidearmRules = {1},
                MeleeSidearmRules = {0},
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", false}},
                PawnCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true
            },
            new Loadout(3, false)
            {
                Label = Strings.Default.Support,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
                PrimaryRangedWeaponRuleId = 2,
                RangedSidearmRules = {1},
                MeleeSidearmRules = {0},
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", false}},
                PawnCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true
            },
            new Loadout(4, false)
            {
                Label = Strings.Default.Brawler,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.MeleeWeapon,
                PrimaryMeleeWeaponRuleId = 0,
                ToolRuleId = 0,
                PawnTraits = {{"Brawler", true}},
                PawnCapacities = {{"Violent", true}},
                DropUnassignedWeapons = true
            },
            new Loadout(5, false)
            {
                Label = Strings.Default.Pacifist,
                Priority = 5,
                PrimaryRuleType = PrimaryWeaponType.None,
                ToolRuleId = 0,
                PawnCapacities = {{"Violent", false}},
                DropUnassignedWeapons = true
            }
        };

        private int _id;
        private bool _initialized;
        private List<int> _meleeSidearmRules = new List<int>();
        private Dictionary<string, bool> _pawnCapacities = new Dictionary<string, bool>();
        private Dictionary<string, bool> _pawnTraits = new Dictionary<string, bool>();
        private HashSet<string> _preferredSkills = new HashSet<string>();
        private PrimaryWeaponType _primaryRuleType = PrimaryWeaponType.None;
        private bool _protected;
        private List<int> _rangedSidearmRules = new List<int>();
        private HashSet<string> _undesirableSkills = new HashSet<string>();
        public bool DropUnassignedWeapons = true;
        public string Label;
        public int? PrimaryMeleeWeaponRuleId;
        public int? PrimaryRangedWeaponRuleId;
        public int Priority;
        public int? ToolRuleId;

        [UsedImplicitly]
        public Loadout() { }

        public Loadout(int id, bool isProtected)
        {
            _id = id;
            _protected = isProtected;
        }

        public Loadout(int id, string label, bool isProtected, int priority, PrimaryWeaponType primaryRuleType,
            int? primaryRangedWeaponRuleId, int? primaryMeleeWeaponRuleId, List<int> rangedSidearmRules,
            List<int> meleeSidearmRules, int? toolRuleId, Dictionary<string, bool> pawnTraits,
            Dictionary<string, bool> pawnCapacities, HashSet<string> preferredSkills, HashSet<string> undesirableSkills,
            bool dropUnassignedWeapons)
        {
            _id = id;
            Label = label;
            _protected = isProtected;
            Priority = priority;
            _primaryRuleType = primaryRuleType;
            PrimaryRangedWeaponRuleId = primaryRangedWeaponRuleId;
            PrimaryMeleeWeaponRuleId = primaryMeleeWeaponRuleId;
            _rangedSidearmRules = rangedSidearmRules;
            _meleeSidearmRules = meleeSidearmRules;
            ToolRuleId = toolRuleId;
            _pawnTraits = pawnTraits;
            _pawnCapacities = pawnCapacities;
            _preferredSkills = preferredSkills;
            _undesirableSkills = undesirableSkills;
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

        public Dictionary<string, bool> PawnCapacities
        {
            get
            {
                Initialize();
                return _pawnCapacities;
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

        public HashSet<string> PreferredSkills
        {
            get
            {
                Initialize();
                return _preferredSkills;
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

        public bool Protected => _protected;

        public List<int> RangedSidearmRules
        {
            get
            {
                Initialize();
                return _rangedSidearmRules;
            }
        }

        public HashSet<string> UndesirableSkills
        {
            get
            {
                Initialize();
                return _undesirableSkills;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _id, nameof(Id));
            Scribe_Values.Look(ref Label, nameof(Label));
            Scribe_Values.Look(ref _protected, nameof(Protected));
            Scribe_Values.Look(ref Priority, nameof(Priority));
            Scribe_Values.Look(ref _primaryRuleType, nameof(PrimaryRuleType));
            Scribe_Values.Look(ref PrimaryRangedWeaponRuleId, nameof(PrimaryRangedWeaponRuleId));
            Scribe_Values.Look(ref PrimaryMeleeWeaponRuleId, nameof(PrimaryMeleeWeaponRuleId));
            Scribe_Collections.Look(ref _rangedSidearmRules, nameof(RangedSidearmRules));
            Scribe_Collections.Look(ref _meleeSidearmRules, nameof(MeleeSidearmRules));
            Scribe_Values.Look(ref ToolRuleId, nameof(ToolRuleId));
            Scribe_Collections.Look(ref _pawnTraits, nameof(PawnTraits));
            Scribe_Collections.Look(ref _pawnCapacities, nameof(PawnCapacities));
            Scribe_Collections.Look(ref _preferredSkills, nameof(PreferredSkills));
            Scribe_Collections.Look(ref _undesirableSkills, nameof(UndesirableSkills));
            Scribe_Values.Look(ref DropUnassignedWeapons, nameof(DropUnassignedWeapons), true);
        }

        public IReadOnlyList<Pawn> GetAvailablePawns()
        {
            Initialize();
            return new List<Pawn>(PawnsFinder.AllMaps_FreeColonistsSpawned.Where(IsAvailable));
        }

        public float GetScore(Pawn pawn)
        {
            Initialize();
            var score = 0f;
            foreach (var preferredSkill in _preferredSkills)
            {
                var skill = DefDatabase<SkillDef>.GetNamedSilentFail(preferredSkill);
                if (skill == null) { continue; }
                score += pawn.skills.GetSkill(skill).Level;
            }
            foreach (var undesirableSkill in _undesirableSkills)
            {
                var skill = DefDatabase<SkillDef>.GetNamedSilentFail(undesirableSkill);
                if (skill == null) { continue; }
                score -= pawn.skills.GetSkill(skill).Level;
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
            if (_pawnCapacities == null) { _pawnCapacities = new Dictionary<string, bool>(); }
            if (_preferredSkills == null) { _preferredSkills = new HashSet<string>(); }
            if (_undesirableSkills == null) { _undesirableSkills = new HashSet<string>(); }
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
            foreach (var pawnCapacity in _pawnCapacities)
            {
                if (!Enum.TryParse<WorkTags>(pawnCapacity.Key, out var tag)) { continue; }
                if (pawn.WorkTagIsDisabled(tag) == pawnCapacity.Value) { return false; }
            }
            return true;
        }
    }
}