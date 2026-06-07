using JetBrains.Annotations;
using Verse;

namespace EquipmentManager;

internal class PawnCapacityWeight : IExposable
{
    public const float WeightCap = 2f;
    private bool _isInitialized;
    // _pawnCapacityDef is resolved lazily from _pawnCapacityDefName; may be null if def is missing.
    private PawnCapacityDef? _pawnCapacityDef;
    // _pawnCapacityDefName is populated by Scribe on load (IExposable lifecycle); null until
    // Scribe populates it or the parametrised constructor sets it.
    private string? _pawnCapacityDefName;
    public float Weight;

    [UsedImplicitly]
    public PawnCapacityWeight() { }

    public PawnCapacityWeight(string pawnCapacityDefName)
    {
        _pawnCapacityDefName = pawnCapacityDefName;
    }

    public PawnCapacityWeight(string pawnCapacityDefName, float weight)
    {
        _pawnCapacityDefName = pawnCapacityDefName;
        Weight = weight;
    }

    public PawnCapacityDef? PawnCapacityDef
    {
        get
        {
            Initialize();
            return _pawnCapacityDef;
        }
    }

    public string PawnCapacityDefName => _pawnCapacityDefName ?? string.Empty;

    public void ExposeData()
    {
        Scribe_Values.Look(ref _pawnCapacityDefName, nameof(PawnCapacityDefName));
        Scribe_Values.Look(ref Weight, nameof(Weight));
    }

    private void Initialize()
    {
        if (_isInitialized) { return; }
        _isInitialized = true;
        if (_pawnCapacityDefName == null) { return; }
        _pawnCapacityDef = DefDatabase<PawnCapacityDef>.GetNamedSilentFail(_pawnCapacityDefName);
    }
}