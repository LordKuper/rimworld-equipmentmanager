using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common;
using LordKuper.Common.Filters.Limits;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal class Loadout : IExposable
{
    public enum PrimaryWeaponType
    {
        None,
        RangedWeapon,
        MeleeWeapon
    }

    public static readonly IEnumerable<Loadout> DefaultLoadouts =
    [
        new(1)
        {
            Label = Resources.Strings.Loadouts.Default.Assault,
            Priority = 5,
            PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
            PrimaryRangedWeaponRuleId = 0,
            RangedSidearmRules = { 1 },
            MeleeSidearmRules = { 0 },
            ToolRuleId = 0,
            PawnTraits = { { "Brawler", false } },
            PawnWorkCapacities = { { "Violent", true } },
            DropUnassignedWeapons = true
        },
        new(2)
        {
            Label = Resources.Strings.Loadouts.Default.Sniper,
            Priority = 3,
            PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
            PrimaryRangedWeaponRuleId = 3,
            RangedSidearmRules = { 1 },
            MeleeSidearmRules = { 0 },
            ToolRuleId = 0,
            PawnTraits = { { "Brawler", false } },
            PawnWorkCapacities = { { "Violent", true } },
            DropUnassignedWeapons = true,
            SkillWeights = { new SkillWeight("Shooting", 1f) },
            StatWeights = { new StatWeight("ShootingAccuracyPawn", 2f, false) }
        },
        new(3)
        {
            Label = Resources.Strings.Loadouts.Default.Support,
            Priority = 2,
            PrimaryRuleType = PrimaryWeaponType.RangedWeapon,
            PrimaryRangedWeaponRuleId = 2,
            RangedSidearmRules = { 1 },
            MeleeSidearmRules = { 0 },
            ToolRuleId = 0,
            PawnTraits = { { "Brawler", false } },
            PawnWorkCapacities = { { "Violent", true } },
            DropUnassignedWeapons = true,
            SkillWeights = { new SkillWeight("Shooting", -1f) },
            StatWeights = { new StatWeight("ShootingAccuracyPawn", -2f, false) }
        },
        new(4)
        {
            Label = Resources.Strings.Loadouts.Default.Slasher,
            Priority = 5,
            PrimaryRuleType = PrimaryWeaponType.MeleeWeapon,
            PrimaryMeleeWeaponRuleId = 1,
            ToolRuleId = 0,
            PawnTraits = { { "Brawler", true } },
            PawnWorkCapacities = { { "Violent", true } },
            DropUnassignedWeapons = true
        },
        new(5)
        {
            Label = Resources.Strings.Loadouts.Default.Crusher,
            Priority = 5,
            PrimaryRuleType = PrimaryWeaponType.MeleeWeapon,
            PrimaryMeleeWeaponRuleId = 2,
            ToolRuleId = 0,
            PawnTraits = { { "Brawler", true } },
            PawnWorkCapacities = { { "Violent", true } },
            DropUnassignedWeapons = true
        },
        new(6)
        {
            Label = Resources.Strings.Loadouts.Default.Pacifist,
            Priority = 5,
            PrimaryRuleType = PrimaryWeaponType.None,
            ToolRuleId = 0,
            PawnWorkCapacities = { { "Violent", false } },
            DropUnassignedWeapons = true
        }
    ];

    private int _id;
    private bool _initialized;
    private List<int> _meleeSidearmRules = [];
    private List<PassionLimit> _passionLimits = [];
    private List<PawnCapacityLimit> _pawnCapacityLimits = [];
    private List<PawnCapacityWeight> _pawnCapacityWeights = [];
    private Dictionary<string, bool> _pawnTraits = new();
    private Dictionary<string, bool> _pawnWorkCapacities = new();
    private PrimaryWeaponType _primaryRuleType = PrimaryWeaponType.None;
    private List<int> _rangedSidearmRules = [];
    private List<PawnSkillLimit> _skillLimits = [];
    private List<SkillWeight> _skillWeights = [];
    private List<StatLimit> _statLimits = [];
    private List<StatWeight> _statWeights = [];
    private int _scoreContextTick = -1;
    private List<Pawn> _scoreContextPawns;
    private readonly Dictionary<StatDef, FloatRange> _scoreStatRanges = new();
    private readonly Dictionary<SkillDef, FloatRange> _scoreSkillRanges = new();
    private readonly Dictionary<PawnCapacityDef, FloatRange> _scoreCapacityRanges = new();
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
        Dictionary<string, bool> pawnWorkCapacities, bool dropUnassignedWeapons,
        List<PassionLimit> passionLimits, List<PawnCapacityLimit> pawnCapacityLimits,
        List<PawnCapacityWeight> pawnCapacityWeights, List<PawnSkillLimit> skillLimits,
        List<SkillWeight> skillWeights, List<StatLimit> statLimits, List<StatWeight> statWeights)
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
        _passionLimits = passionLimits;
        _pawnCapacityLimits = pawnCapacityLimits;
        _pawnCapacityWeights = pawnCapacityWeights;
        _skillLimits = skillLimits;
        _skillWeights = skillWeights;
        _statLimits = statLimits;
        _statWeights = statWeights;
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

    public List<PawnSkillLimit> SkillLimits
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
        Scribe_Collections.Look(ref _pawnCapacityWeights, nameof(PawnCapacityWeights),
            LookMode.Deep);
        Scribe_Collections.Look(ref _skillLimits, nameof(SkillLimits), LookMode.Deep);
        Scribe_Collections.Look(ref _skillWeights, nameof(SkillWeights), LookMode.Deep);
        Scribe_Collections.Look(ref _statLimits, nameof(StatLimits), LookMode.Deep);
        Scribe_Collections.Look(ref _statWeights, nameof(StatWeights), LookMode.Deep);
    }

    [NotNull]
    public IReadOnlyList<Pawn> GetAvailablePawnsOrdered()
    {
        Initialize();
        return new List<Pawn>(PawnsFinder.AllMaps_FreeColonistsSpawned.Where(IsAvailable)
            .OrderByDescending(GetScore));
    }

    public float GetScore(Pawn pawn)
    {
        Initialize();
        EnsureScoreContext();
        var score = 0f;
        foreach (var statWeight in _statWeights.Where(sw => sw.StatDef != null))
        {
            if (statWeight.StatDef.Worker?.IsDisabledFor(pawn) ?? false) { continue; }
            if (!_scoreStatRanges.TryGetValue(statWeight.StatDef, out var range)) { continue; }
            var normalizedValue = MathHelper.NormalizeValue(
                StatHelper.GetStatValue(pawn, statWeight.StatDef), range);
            score += normalizedValue * statWeight.Weight;
        }
        foreach (var skillWeight in _skillWeights.Where(sw => sw.SkillDef != null))
        {
            if (!_scoreSkillRanges.TryGetValue(skillWeight.SkillDef, out var range)) { continue; }
            var normalizedValue = MathHelper.NormalizeValue(
                pawn.skills.GetSkill(skillWeight.SkillDef).Level, range);
            score += normalizedValue * skillWeight.Weight;
        }
        foreach (var pawnCapacityWeight in _pawnCapacityWeights.Where(pcw =>
                     pcw.PawnCapacityDef != null))
        {
            if (!_scoreCapacityRanges.TryGetValue(pawnCapacityWeight.PawnCapacityDef, out var range))
            {
                continue;
            }
            var normalizedValue = MathHelper.NormalizeValue(
                pawn.health.capacities.GetLevel(pawnCapacityWeight.PawnCapacityDef), range);
            score += normalizedValue * pawnCapacityWeight.Weight;
        }
        return score;
    }

    private void EnsureScoreContext()
    {
        var tick = Find.TickManager.TicksGame;
        if (_scoreContextTick == tick) { return; }
        _scoreContextTick = tick;
        _scoreContextPawns = PawnsFinder.AllMaps_FreeColonistsSpawned.ToList();
        _scoreStatRanges.Clear();
        foreach (var statWeight in _statWeights.Where(sw => sw.StatDef != null))
        {
            var eligible = _scoreContextPawns
                .Where(p => !(statWeight.StatDef.Worker?.IsDisabledFor(p) ?? false)).ToList();
            if (!eligible.Any()) { continue; }
            var values = eligible.Select(p => StatHelper.GetStatValue(p, statWeight.StatDef)).ToList();
            _scoreStatRanges[statWeight.StatDef] = new FloatRange(values.Min(), values.Max());
        }
        _scoreSkillRanges.Clear();
        foreach (var skillWeight in _skillWeights.Where(sw => sw.SkillDef != null))
        {
            var values = _scoreContextPawns.Select(p => (float)p.skills.GetSkill(skillWeight.SkillDef).Level).ToList();
            if (!values.Any()) { continue; }
            _scoreSkillRanges[skillWeight.SkillDef] = new FloatRange(values.Min(), values.Max());
        }
        _scoreCapacityRanges.Clear();
        foreach (var pawnCapacityWeight in _pawnCapacityWeights.Where(pcw =>
                     pcw.PawnCapacityDef != null))
        {
            var values = _scoreContextPawns
                .Select(p => p.health.capacities.GetLevel(pawnCapacityWeight.PawnCapacityDef)).ToList();
            if (!values.Any()) { continue; }
            _scoreCapacityRanges[pawnCapacityWeight.PawnCapacityDef] =
                new FloatRange(values.Min(), values.Max());
        }
    }

    private void Initialize()
    {
        if (_initialized) { return; }
        _initialized = true;
        if (_meleeSidearmRules == null) { _meleeSidearmRules = []; }
        if (_rangedSidearmRules == null) { _rangedSidearmRules = []; }
        if (_pawnTraits == null) { _pawnTraits = new Dictionary<string, bool>(); }
        if (_pawnWorkCapacities == null) { _pawnWorkCapacities = new Dictionary<string, bool>(); }
        if (_passionLimits == null) { _passionLimits = []; }
        if (_pawnCapacityLimits == null) { _pawnCapacityLimits = []; }
        if (_pawnCapacityWeights == null) { _pawnCapacityWeights = []; }
        if (_skillLimits == null) { _skillLimits = []; }
        if (_skillWeights == null) { _skillWeights = []; }
        if (_statLimits == null) { _statLimits = []; }
        if (_statWeights == null) { _statWeights = []; }
        NormalizeLegacyCustomStatDefNames();
    }

    internal void NormalizeLegacyCustomStatDefNames()
    {
        _statLimits = _statLimits?.Select(LegacyCustomStatDefs.NormalizeStatLimit).ToList() ?? [];
        _statWeights = _statWeights?.Select(LegacyCustomStatDefs.NormalizeStatWeight).ToList() ?? [];
    }

    public bool IsAvailable(Pawn pawn)
    {
        Initialize();
        if (ModsConfig.IdeologyActive && pawn.Ideo != null)
        {
            var role = pawn.Ideo.GetRole(pawn);
            if (role?.def?.roleEffects != null)
            {
                if (PrimaryRuleType == PrimaryWeaponType.RangedWeapon &&
                    PrimaryRangedWeaponRuleId != null &&
                    role.def.roleEffects.Any(effect => effect is RoleEffect_NoRangedWeapons))
                {
                    return false;
                }
                if (PrimaryRuleType == PrimaryWeaponType.MeleeWeapon &&
                    PrimaryMeleeWeaponRuleId != null &&
                    role.def.roleEffects.Any(effect => effect is RoleEffect_NoMeleeWeapons))
                {
                    return false;
                }
            }
        }
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
        foreach (var pawnCapacityLimit in _pawnCapacityLimits.Where(pcl =>
                     pcl.PawnCapacityDef != null))
        {
            var capacity = pawn.health.capacities.GetLevel(pawnCapacityLimit.PawnCapacityDef);
            if ((pawnCapacityLimit.MinValue != null && capacity < pawnCapacityLimit.MinValue) ||
                (pawnCapacityLimit.MaxValue != null && capacity > pawnCapacityLimit.MaxValue))
            {
                return false;
            }
        }
        foreach (var statLimit in _statLimits.Where(sl => sl.StatDef != null))
        {
            if (statLimit.StatDef.Worker?.IsDisabledFor(pawn) ?? false) { return false; }
            var statValue = StatHelper.GetStatValue(pawn, statLimit.StatDef);
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
