using System;
using HarmonyLib;
using Verse;

namespace LordKuper.EquipmentManager;

internal static class CombatExtendedHelper
{
    // These fields are CE reflection-delegate / soft-dependency fields populated by Initialize()
    // when Combat Extended is present at runtime. They are legitimately null when CE is absent,
    // so they are declared nullable with existing null-guards rather than using = null!.
    private static EnableAmmoSystemDelegate? _enableAmmoSystemMethod;
    public static AccessTools.FieldRef<object, ThingDef>? AmmoDelegate;
    public static AccessTools.FieldRef<CompProperties, Def>? AmmoSetDelegate;
    public static AccessTools.FieldRef<Def, object>? AmmoTypesDelegate;
    public static AccessTools.FieldRef<ProjectileProperties, float>? ArmorPenetrationBluntDelegate;
    public static AccessTools.FieldRef<ProjectileProperties, float>? ArmorPenetrationSharpDelegate;
    public static bool CombatExtended;
    public static Type? CompAmmoUserType;
    public static Type? ProjectilePropertiesType;
    public static bool EnableAmmoSystem => _enableAmmoSystemMethod != null && _enableAmmoSystemMethod();

    public static void Initialize()
    {
        ProjectilePropertiesType = AccessTools.TypeByName("CombatExtended.ProjectilePropertiesCE");
        if (ProjectilePropertiesType != null)
        {
            ArmorPenetrationSharpDelegate =
                AccessTools.FieldRefAccess<float>(ProjectilePropertiesType, "armorPenetrationSharp");
            if (ArmorPenetrationSharpDelegate == null)
            {
                Logger.LogError("Could not find 'CombatExtended.ProjectilePropertiesCE.armorPenetrationSharp'");
            }
            ArmorPenetrationBluntDelegate =
                AccessTools.FieldRefAccess<float>(ProjectilePropertiesType, "armorPenetrationBlunt");
            if (ArmorPenetrationBluntDelegate == null)
            {
                Logger.LogError("Could not find 'CombatExtended.ProjectilePropertiesCE.armorPenetrationBlunt'");
            }
        }
        else { Logger.LogError("Could not find 'CombatExtended.ProjectilePropertiesCE'"); }
        var settingsType = AccessTools.TypeByName("CombatExtended.Settings");
        var controllerType = AccessTools.TypeByName("CombatExtended.Controller");
        if (settingsType != null && controllerType != null)
        {
            var enableAmmoGetter = AccessTools.PropertyGetter(settingsType, "EnableAmmoSystem");
            var settingsField = AccessTools.Field(controllerType, "settings");
            if (enableAmmoGetter != null && settingsField != null)
            {
                _enableAmmoSystemMethod =
                    AccessTools.MethodDelegate<EnableAmmoSystemDelegate>(enableAmmoGetter,
                        settingsField.GetValue(null));
                if (_enableAmmoSystemMethod == null)
                {
                    Logger.LogError("Could not create delegate for 'CombatExtended.Settings.EnableAmmoSystem'");
                }
            }
            else
            {
                Logger.LogError(
                    "Could not find 'CombatExtended.Settings.EnableAmmoSystem' or 'CombatExtended.Controller.settings'");
            }
        }
        else { Logger.LogError("Could not find 'CombatExtended.Settings' or 'CombatExtended.Controller'"); }
        CompAmmoUserType = AccessTools.TypeByName("CombatExtended.CompAmmoUser");
        if (CompAmmoUserType == null) { Logger.LogError("Could not find 'CombatExtended.CompAmmoUser'"); }
        var compPropsAmmoUserType = AccessTools.TypeByName("CombatExtended.CompProperties_AmmoUser");
        if (compPropsAmmoUserType != null)
        {
            AmmoSetDelegate = AccessTools.FieldRefAccess<Def>(compPropsAmmoUserType, "ammoSet");
            if (AmmoSetDelegate == null)
            {
                Logger.LogError("Could not find 'CombatExtended.CompProperties_AmmoUser.ammoSet'");
            }
        }
        else { Logger.LogError("Could not find 'CombatExtended.CompProperties_AmmoUser'"); }
        var ammoSetDefType = AccessTools.TypeByName("CombatExtended.AmmoSetDef");
        if (ammoSetDefType != null)
        {
            AmmoTypesDelegate = AccessTools.FieldRefAccess<object>(ammoSetDefType, "ammoTypes");
            if (AmmoTypesDelegate == null) { Logger.LogError("Could not find 'CombatExtended.AmmoSetDef.ammoTypes'"); }
        }
        else { Logger.LogError("Could not find 'CombatExtended.AmmoSetDef'"); }
        var ammoLinkType = AccessTools.TypeByName("CombatExtended.AmmoLink");
        if (ammoLinkType != null)
        {
            AmmoDelegate = AccessTools.FieldRefAccess<ThingDef>(ammoLinkType, "ammo");
            if (AmmoDelegate == null) { Logger.LogError("Could not find 'CombatExtended.AmmoLink.ammo'"); }
        }
        else { Logger.LogError("Could not find 'CombatExtended.AmmoLink'"); }
    }

    private delegate bool EnableAmmoSystemDelegate();
}