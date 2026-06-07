using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal class SkillWeight : IExposable
{
    public const float WeightCap = 2f;
    private bool _isInitialized;
    // _skillDef is resolved lazily from _skillDefName; may be null if the def is missing.
    private SkillDef? _skillDef;
    // _skillDefName is populated by Scribe on load (IExposable lifecycle); null until Scribe
    // populates it or the parametrised constructor sets it.
    private string? _skillDefName;
    public float Weight;

    [UsedImplicitly]
    public SkillWeight() { }

    public SkillWeight(string skillDefName)
    {
        _skillDefName = skillDefName;
    }

    public SkillWeight(string skillDefName, float weight)
    {
        _skillDefName = skillDefName;
        Weight = weight;
    }

    public SkillDef? SkillDef
    {
        get
        {
            Initialize();
            return _skillDef;
        }
    }

    public string SkillDefName => _skillDefName ?? string.Empty;

    public void ExposeData()
    {
        Scribe_Values.Look(ref _skillDefName, nameof(SkillDefName));
        Scribe_Values.Look(ref Weight, nameof(Weight));
    }

    private void Initialize()
    {
        if (_isInitialized) { return; }
        _isInitialized = true;
        if (_skillDefName == null) { return; }
        _skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(_skillDefName);
    }
}