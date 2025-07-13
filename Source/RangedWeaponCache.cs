using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class RangedWeaponCache : ItemCache
    {
        private AmmoUserPropsDelegate _ammoUserPropsMethod;
        private bool _initialized;
        private bool _isAmmo;

        public RangedWeaponCache([NotNull] Thing thing)
        {
            Thing = thing ?? throw new ArgumentNullException(nameof(thing));
        }

        private float AccuracyClose { get; set; }
        private float AccuracyLong { get; set; }
        private float AccuracyMedium { get; set; }
        private float AccuracyShort { get; set; }

        public IEnumerable<ThingDef> AmmoTypes
        {
            get
            {
                Initialize();
                var ammoTypes = new HashSet<ThingDef>();
                if (_isAmmo)
                {
                    _ = ammoTypes.Add(Thing.def);
                    return ammoTypes;
                }
                if (_ammoUserPropsMethod == null) { return ammoTypes; }
                var ammoUserProps = _ammoUserPropsMethod();
                if (ammoUserProps == null)
                {
                    Log.Error($"Equipment Manager: CompProperties_AmmoUser was not found for {Thing.LabelCapNoCount}");
                    return ammoTypes;
                }
                var ammoSet = CombatExtendedHelper.AmmoSetDelegate(ammoUserProps);
                if (ammoSet == null)
                {
                    Log.Error($"Equipment Manager: Ammo set was not found for {Thing.LabelCapNoCount}");
                    return ammoTypes;
                }
                if (!(CombatExtendedHelper.AmmoTypesDelegate(ammoSet) is IEnumerable<object> ammoLinks))
                {
                    Log.Error($"Equipment Manager: Could not get ammo links for {Thing.LabelCapNoCount}");
                    return ammoTypes;
                }
                ammoTypes.AddRange(ammoLinks.Select(ammoLink => CombatExtendedHelper.AmmoDelegate(ammoLink))
                    .Where(ammoType => ammoType != null));
                return ammoTypes;
            }
        }

        private ThingComp AmmoUserComp =>
            !(Thing is ThingWithComps thingWithComps)
                ? null
                : thingWithComps.AllComps.FirstOrDefault(comp =>
                    comp.GetType() == CombatExtendedHelper.CompAmmoUserType);

        private float ArmorPenetration { get; set; }
        private int BurstShotCount { get; set; }
        private float Cooldown { get; set; }
        private float Damage { get; set; }
        private float Dps { get; set; }
        private float Dpsa { get; set; }
        private float DpsaClose { get; set; }
        private float DpsaLong { get; set; }
        private float DpsaMedium { get; set; }
        private float DpsaShort { get; set; }

        public bool IsAmmo
        {
            get
            {
                Initialize();
                return _isAmmo;
            }
        }

        private float MaxRange { get; set; }
        private float MinRange { get; set; }
        private float SightsEfficiency { get; set; }
        private float StoppingPower { get; set; }
        private Thing Thing { get; }
        private int TicksBetweenBurstShots { get; set; }
        private float Warmup { get; set; }

        private float GetCustomStatValue([NotNull] StatDef statDef)
        {
            if (Enum.TryParse(CustomRangedWeaponStats.GetStatName(statDef.defName),
                    out CustomRangedWeaponStat rangedWeaponStat))
            {
                switch (rangedWeaponStat)
                {
                    case CustomRangedWeaponStat.Dpsa:
                        return Dpsa;
                    case CustomRangedWeaponStat.DpsaClose:
                        return DpsaClose;
                    case CustomRangedWeaponStat.DpsaShort:
                        return DpsaShort;
                    case CustomRangedWeaponStat.DpsaMedium:
                        return DpsaMedium;
                    case CustomRangedWeaponStat.DpsaLong:
                        return DpsaLong;
                    case CustomRangedWeaponStat.Range:
                        return MaxRange;
                    case CustomRangedWeaponStat.Warmup:
                        return Warmup;
                    case CustomRangedWeaponStat.BurstShotCount:
                        return BurstShotCount;
                    case CustomRangedWeaponStat.TicksBetweenBurstShots:
                        return TicksBetweenBurstShots;
                    case CustomRangedWeaponStat.ArmorPenetration:
                        return ArmorPenetration;
                    case CustomRangedWeaponStat.StoppingPower:
                        return StoppingPower;
                    case CustomRangedWeaponStat.Damage:
                        return Damage;
                    case CustomRangedWeaponStat.TechLevel:
                        return (float) Thing.def.techLevel;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(statDef));
                }
            }
            Log.Error($"Equipment Manager: Tried to evaluate unknown custom ranged stat ({statDef.defName})");
            return 0f;
        }

        public float GetStatValue(StatDef statDef)
        {
            if (!StatValues.TryGetValue(statDef, out var value))
            {
                value = CustomRangedWeaponStats.IsCustomStat(statDef.defName)
                    ? GetCustomStatValue(statDef)
                    : StatHelper.GetStatValue(Thing, statDef);
                StatValues.Add(statDef, value);
            }
            return value;
        }

        public float GetStatValueDeviation([NotNull] StatDef statDef)
        {
            return statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                CustomRangedWeaponStats.IsCustomStat(statDef.defName) ? GetCustomStatValue(statDef) :
                StatHelper.GetStatValueDeviation(Thing, statDef);
        }

        private void Initialize()
        {
            if (_initialized) { return; }
            _initialized = true;
            if (!CombatExtendedHelper.CombatExtended) { return; }
            try
            {
                if (AmmoUserComp == null)
                {
                    if (Thing.def.Verbs.Any(properties => string.Equals(properties.verbClass.FullName,
                            "CombatExtended.Verb_ShootCEOneUse", StringComparison.OrdinalIgnoreCase)))
                    {
                        _isAmmo = true;
                    }
                }
                else
                {
                    var ammoUserPropsMethod =
                        AccessTools.PropertyGetter(CombatExtendedHelper.CompAmmoUserType, "Props");
                    if (ammoUserPropsMethod == null)
                    {
                        Log.Error("Equipment Manager: Could not find 'CombatExtended.CompAmmoUser.Props'");
                    }
                    else
                    {
                        _ammoUserPropsMethod =
                            AccessTools.MethodDelegate<AmmoUserPropsDelegate>(ammoUserPropsMethod, AmmoUserComp);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(
                    $"Equipment Manager: Could not create Combat Extended delegates for {Thing.LabelCapNoCount}: {exception.Message}");
                _ammoUserPropsMethod = null;
            }
        }

        private void ReadProjectileProperties(ProjectileProperties projectileProperties)
        {
            if (projectileProperties == null) { throw new ArgumentNullException(nameof(projectileProperties)); }
            try { Damage = projectileProperties.GetDamageAmount(Thing); }
            catch (Exception e)
            {
                Log.Warning(
                    $"Equipment Manager: Could not get projectile damage for {Thing.LabelCapNoCount}: {e.Message}");
                Damage = 0;
            }
            StoppingPower = projectileProperties.stoppingPower;
            try { ArmorPenetration = projectileProperties.GetArmorPenetration(Thing); }
            catch (Exception e)
            {
                Log.Warning(
                    $"Equipment Manager: Could not get projectile armor penetration for {Thing.LabelCapNoCount}: {e.Message}");
                ArmorPenetration = 0;
            }
        }

        private void ReadProjectilePropertiesCombatExtended(ProjectileProperties projectileProperties)
        {
            Damage = projectileProperties.GetDamageAmount(Thing);
            StoppingPower = projectileProperties.stoppingPower;
            if (projectileProperties.GetType() != CombatExtendedHelper.ProjectilePropertiesType)
            {
                Log.Warning(
                    $"Equipment Manager: {Thing.LabelCapNoCount}'s projectile type is not CombatExtended-compatible");
                ReadProjectileProperties(projectileProperties);
            }
            else
            {
                if (CombatExtendedHelper.ArmorPenetrationSharpDelegate != null &&
                    CombatExtendedHelper.ArmorPenetrationBluntDelegate != null)
                {
                    ArmorPenetration = CombatExtendedHelper.ArmorPenetrationSharpDelegate(projectileProperties) +
                        CombatExtendedHelper.ArmorPenetrationBluntDelegate(projectileProperties);
                }
            }
        }

        public override bool Update(RimworldTime time)
        {
            if (!base.Update(time)) { return false; }
            try
            {
                if (Thing.def?.Verbs != null)
                {
                    var verb = Thing.def.Verbs.FirstOrDefault(vp => vp.range > 0);
                    if (verb == null)
                    {
                        Log.Warning(
                            $"Equipment Manager: Could not find correct ranged weapon verb on the first try for weapon '{Thing.LabelCapNoCount}' ({Thing.def.defName})");
                        verb = Thing.def.Verbs.FirstOrDefault();
                    }
                    if (verb == null)
                    {
                        Log.Error(
                            $"Equipment Manager: Could not find ranged weapon verb for weapon '{Thing.LabelCapNoCount}' ({Thing.def.defName})");
                        return true;
                    }
                    if (verb.defaultProjectile?.projectile != null)
                    {
                        if (CombatExtendedHelper.CombatExtended)
                        {
                            ReadProjectilePropertiesCombatExtended(verb.defaultProjectile.projectile);
                        }
                        else { ReadProjectileProperties(verb.defaultProjectile.projectile); }
                    }
                    SightsEfficiency = CombatExtendedHelper.CombatExtended
                        ? Thing.GetStatValue(StatDef.Named("SightsEfficiency"))
                        : 1f;
                    BurstShotCount = verb.burstShotCount <= 0 ? 1 : verb.burstShotCount;
                    TicksBetweenBurstShots = verb.ticksBetweenBurstShots <= 0 ? 10 : verb.ticksBetweenBurstShots;
                    Warmup = verb.warmupTime;
                    MinRange = verb.minRange;
                    MaxRange = verb.range;
                    Cooldown = Thing.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
                    Dps = (float) Math.Round(
                        Damage * (double) BurstShotCount / ((((Cooldown + (double) Warmup) * 60f) +
                            (BurstShotCount * TicksBetweenBurstShots)) / 60f), 2);
                }
                if (MinRange <= 3f && MaxRange >= 3f)
                {
                    AccuracyClose = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyTouch) * 100f, 2);
                }
                if (MinRange <= 12f && MaxRange >= 12f)
                {
                    AccuracyShort = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyShort) * 100f, 2);
                }
                if (MinRange <= 25f && MaxRange >= 25f)
                {
                    AccuracyMedium = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyMedium) * 100f, 2);
                }
                if (MinRange <= 40f && MaxRange >= 40f)
                {
                    AccuracyLong = (float) Math.Round(Thing.GetStatValue(StatDefOf.AccuracyLong) * 100f, 2);
                }
                var totalAccuracy = 0f;
                var rangeCount = 0;
                if (AccuracyClose > 0f)
                {
                    DpsaClose = Dps * AccuracyClose / 100f;
                    totalAccuracy += AccuracyClose;
                    rangeCount++;
                }
                if (AccuracyShort > 0f)
                {
                    DpsaShort = Dps * AccuracyShort / 100f;
                    totalAccuracy += AccuracyShort;
                    rangeCount++;
                }
                if (AccuracyMedium > 0f)
                {
                    DpsaMedium = Dps * AccuracyMedium / 100f;
                    totalAccuracy += AccuracyMedium;
                    rangeCount++;
                }
                if (AccuracyLong > 0f)
                {
                    DpsaLong = Dps * AccuracyLong / 100f;
                    totalAccuracy += AccuracyLong;
                    rangeCount++;
                }
                Dpsa = rangeCount == 0 ? 0f : Dps * (totalAccuracy * SightsEfficiency / rangeCount) / 100f;
            }
            catch (Exception exception)
            {
                Log.Error(
                    $"Equipment Manager: Could not update cache of '{Thing.LabelCapNoCount}' ({Thing.def?.defName}): {exception.Message}");
            }
            return true;
        }

        private delegate CompProperties AmmoUserPropsDelegate();
    }
}