using Verse;

namespace EquipmentManager
{
    internal class PawnLoadout : IExposable
    {
        public bool Automatic;
        public int LoadoutId;
        public Pawn Pawn;

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, nameof(Pawn), true);
            Scribe_Values.Look(ref LoadoutId, nameof(LoadoutId));
            Scribe_Values.Look(ref Automatic, nameof(Automatic));
        }
    }
}