using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RimWorld;

namespace EquipmentManager.DefOfs
{
    [DefOf, UsedImplicitly, SuppressMessage("ReSharper", "UnassignedField.Global")]
    public static class PawnColumnDefOf
    {
        [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
        public static PawnColumnDef Loadout;
    }
}