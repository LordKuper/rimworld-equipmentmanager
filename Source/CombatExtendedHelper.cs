using System;
using HarmonyLib;
using Verse;

namespace EquipmentManager
{
    internal static class CombatExtendedHelper
    {
        private static EnableAmmoSystemDelegate _enableAmmoSystemMethod;
        public static AccessTools.FieldRef<object, ThingDef> AmmoDelegate;
        public static AccessTools.FieldRef<CompProperties, Def> AmmoSetDelegate;
        public static AccessTools.FieldRef<Def, object> AmmoTypesDelegate;
        public static AccessTools.FieldRef<ProjectileProperties, float> ArmorPenetrationBluntDelegate;
        public static AccessTools.FieldRef<ProjectileProperties, float> ArmorPenetrationSharpDelegate;
        public static bool CombatExtended;
        public static Type CompAmmoUserType;
        public static Type ProjectilePropertiesType;
        public static bool EnableAmmoSystem => _enableAmmoSystemMethod != null && _enableAmmoSystemMethod();

        public static void Initialize()
        {
            ProjectilePropertiesType = AccessTools.TypeByName("CombatExtended.ProjectilePropertiesCE");
            ArmorPenetrationSharpDelegate =
                AccessTools.FieldRefAccess<float>(ProjectilePropertiesType, "armorPenetrationSharp");
            ArmorPenetrationBluntDelegate =
                AccessTools.FieldRefAccess<float>(ProjectilePropertiesType, "armorPenetrationBlunt");
            _enableAmmoSystemMethod = AccessTools.MethodDelegate<EnableAmmoSystemDelegate>(
                AccessTools.PropertyGetter(AccessTools.TypeByName("CombatExtended.Settings"), "EnableAmmoSystem"),
                AccessTools.Field(AccessTools.TypeByName("CombatExtended.Controller"), "settings").GetValue(null));
            CompAmmoUserType = AccessTools.TypeByName("CombatExtended.CompAmmoUser");
            AmmoSetDelegate =
                AccessTools.FieldRefAccess<Def>(AccessTools.TypeByName("CombatExtended.CompProperties_AmmoUser"),
                    "ammoSet");
            AmmoTypesDelegate =
                AccessTools.FieldRefAccess<object>(AccessTools.TypeByName("CombatExtended.AmmoSetDef"), "ammoTypes");
            AmmoDelegate =
                AccessTools.FieldRefAccess<ThingDef>(AccessTools.TypeByName("CombatExtended.AmmoLink"), "ammo");
        }

        private delegate bool EnableAmmoSystemDelegate();
    }
}