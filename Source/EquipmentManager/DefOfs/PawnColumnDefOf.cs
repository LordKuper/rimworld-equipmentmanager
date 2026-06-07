using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RimWorld;

namespace EquipmentManager.DefOfs;

[DefOf, UsedImplicitly, SuppressMessage("ReSharper", "UnassignedField.Global"),
 SuppressMessage("ReSharper", "InconsistentNaming")]
public static class PawnColumnDefOf
{
    // Populated by RimWorld's [DefOf] reflection injection before any game code runs.
    // The field is contractually non-null at every read site after startup completes.
    [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
    public static PawnColumnDef EM_Loadout = null!;
}