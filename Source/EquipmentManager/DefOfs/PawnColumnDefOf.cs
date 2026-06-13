using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RimWorld;

namespace LordKuper.EquipmentManager.DefOfs;

/// <summary>
///     Holds references to the mod's <see cref="PawnColumnDef" />s, injected by RimWorld's
///     <see cref="DefOf" /> reflection at startup.
/// </summary>
[DefOf, UsedImplicitly, SuppressMessage("ReSharper", "UnassignedField.Global"),
 SuppressMessage("ReSharper", "InconsistentNaming")]
public static class PawnColumnDefOf
{
    /// <summary>The loadout-selection column shown in the Assign tab.</summary>
    // Populated by RimWorld's [DefOf] reflection injection before any game code runs.
    // The field is contractually non-null at every read site after startup completes.
    // FieldCanBeMadeReadOnly.Global suppressed: [DefOf] reflection writes after the class
    // initializer — readonly is not applicable.
    [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible"),
     SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public static PawnColumnDef EM_Loadout = null!;
}