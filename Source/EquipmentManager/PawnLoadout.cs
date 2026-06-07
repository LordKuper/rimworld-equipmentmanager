using Verse;

namespace EquipmentManager;

internal class PawnLoadout : IExposable
{
    public bool Automatic;
    public int? LoadoutId;
    // Populated by Scribe_References.Look on load (IExposable lifecycle); = null! asserts
    // the field is always set before any read, consistent with the RimWorld load contract.
    public Pawn Pawn = null!;

    public void ExposeData()
    {
        Scribe_References.Look(ref Pawn, nameof(Pawn));
        Scribe_Values.Look(ref LoadoutId, nameof(LoadoutId));
        Scribe_Values.Look(ref Automatic, nameof(Automatic));
    }
}