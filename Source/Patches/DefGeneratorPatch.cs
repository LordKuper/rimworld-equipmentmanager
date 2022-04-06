using System;
using EquipmentManager.DefOfs;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;

namespace EquipmentManager.Patches
{
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve)), UsedImplicitly]
    internal static class DefGeneratorPatch
    {
        [UsedImplicitly]
        public static void Postfix()
        {
            PawnTableDefOf.Assign.columns.Insert(
                PawnTableDefOf.Assign.columns.FindIndex(x => x.defName.Equals("Outfit", StringComparison.Ordinal)) + 1,
                PawnColumnDefOf.Loadout);
        }
    }
}