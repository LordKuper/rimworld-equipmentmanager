using System;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace EquipmentManager
{
    [UsedImplicitly]
    public class EquipmentManagerMod : Mod
    {
        public EquipmentManagerMod(ModContentPack content) : base(content)
        {
            if (Prefs.DevMode)
            {
                Log.Message(
                    $"Equipment Manager: Initializing (v.{Assembly.GetExecutingAssembly().GetName().Version})...");
            }
            var harmony = new Harmony("LordKuper.EquipmentManager");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if (LoadedModManager.RunningModsListForReading.Any(m =>
                    "OskarPotocki.VanillaFactionsExpanded.Core".Equals(m.PackageId,
                        StringComparison.OrdinalIgnoreCase)))
            {
                if (Prefs.DevMode) { Log.Message("Equipment Manager: VanillaFactionsExpanded.Core detected."); }
                MeleeWeaponRule.UsableWithShieldsMethod =
                    AccessTools.Method(AccessTools.TypeByName("VFECore.ShieldUtility"), "UsableWithShields");
            }
        }
    }
}