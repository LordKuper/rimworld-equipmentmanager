using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager;

public enum PassionValue
{
    None,
    Minor,
    Major,
    Any
}

internal class PassionLimit : IExposable
{
    public PassionValue Value = PassionValue.None;
    private bool _isInitialized;

    // _skillDef is resolved lazily from _skillDefName; may be null if the def is missing.
    private SkillDef? _skillDef;

    // _skillDefName is populated by Scribe on load (IExposable lifecycle); null until Scribe
    // populates it or the parametrised constructor sets it.
    private string? _skillDefName;

    [UsedImplicitly]
    public PassionLimit() { }

    public PassionLimit(string skillDefName)
    {
        _skillDefName = skillDefName;
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
        Scribe_Values.Look(ref Value, nameof(Value));
    }

    private void Initialize()
    {
        if (_isInitialized) { return; }
        _isInitialized = true;
        if (_skillDefName == null) { return; }
        _skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(_skillDefName);
    }
}