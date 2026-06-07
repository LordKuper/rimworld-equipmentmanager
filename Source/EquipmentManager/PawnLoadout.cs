using Verse;

namespace EquipmentManager;

internal class PawnLoadout : IExposable
{
    public bool Automatic;
    public int? LoadoutId;

    // Populated by Scribe_References.Look on load (IExposable lifecycle); null when the pawn
    // was destroyed since the last save. Callers must guard against null.
    public Pawn? Pawn;

    public void ExposeData()
    {
        Scribe_References.Look(ref Pawn, nameof(Pawn));
        Scribe_Values.Look(ref LoadoutId, nameof(LoadoutId));
        Scribe_Values.Look(ref Automatic, nameof(Automatic));
    }
}