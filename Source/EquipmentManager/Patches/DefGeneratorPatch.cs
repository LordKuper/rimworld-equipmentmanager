using System;
using HarmonyLib;
using JetBrains.Annotations;
using LordKuper.EquipmentManager.DefOfs;
using RimWorld;

namespace LordKuper.EquipmentManager.Patches;

[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve)), UsedImplicitly]
internal static class DefGeneratorPatch
{
    [UsedImplicitly]
    public static void Postfix()
    {
        if (PawnTableDefOf.Assign.columns.Contains(PawnColumnDefOf.EM_Loadout)) { return; }
        var outfitIndex = PawnTableDefOf.Assign.columns.FindIndex(x =>
            x.defName.Equals("Outfit", StringComparison.Ordinal));
        if (outfitIndex < 0)
        {
            Logger.LogWarning("Could not find 'Outfit' column in Assign table.");
            return;
        }
        PawnTableDefOf.Assign.columns.Insert(outfitIndex + 1, PawnColumnDefOf.EM_Loadout);
    }
}