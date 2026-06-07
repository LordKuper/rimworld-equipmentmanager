using System;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace EquipmentManager;

[UsedImplicitly]
public class EquipmentManagerMod : Mod
{
    internal const string ModId = "LordKuper.EquipmentManager";

    public EquipmentManagerMod(ModContentPack content) : base(content)
    {
        Logger.LogMessage(
            $"Initializing (v.{Assembly.GetExecutingAssembly().GetName().Version})...");
        var harmony = new Harmony(ModId);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        DetectVanillaFactionsExpandedCore();
        DetectCombatExtended();
    }

    private static void DetectCombatExtended()
    {
        if (!LoadedModManager.RunningModsListForReading.Any(m =>
                "CETeam.CombatExtended".Equals(m.PackageId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        Logger.LogMessage("CombatExtended detected.");
        CombatExtendedHelper.CombatExtended = true;
        CombatExtendedHelper.Initialize();
    }

    private static void DetectVanillaFactionsExpandedCore()
    {
        if (!LoadedModManager.RunningModsListForReading.Any(m =>
                "OskarPotocki.VanillaFactionsExpanded.Core".Equals(m.PackageId,
                    StringComparison.OrdinalIgnoreCase))) { return; }
        Logger.LogMessage("VanillaFactionsExpanded.Core detected.");
        MeleeWeaponRule.UsableWithShieldsMethod =
            AccessTools.MethodDelegate<MeleeWeaponRule.UsableWithShieldsDelegate>(
                AccessTools.Method(AccessTools.TypeByName("VEF.Apparels.ShieldUtility"),
                    "UsableWithShields"));
    }
}