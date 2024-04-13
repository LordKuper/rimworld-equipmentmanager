using System;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class MeleeWeaponCache : ItemCache
    {
        private AccessTools.FieldRef<Tool, float> _armorPenetrationBluntDelegate;
        private AccessTools.FieldRef<Tool, float> _armorPenetrationSharpDelegate;
        private bool _initialized;
        private Type _toolType;

        public MeleeWeaponCache([NotNull] Thing thing)
        {
            Thing = thing ?? throw new ArgumentNullException(nameof(thing));
        }

        private float ArmorPenetration { get; set; }

        private AccessTools.FieldRef<Tool, float> ArmorPenetrationBluntDelegate
        {
            get
            {
                Initialize();
                return _armorPenetrationBluntDelegate;
            }
        }

        private AccessTools.FieldRef<Tool, float> ArmorPenetrationSharpDelegate
        {
            get
            {
                Initialize();
                return _armorPenetrationSharpDelegate;
            }
        }

        private Thing Thing { get; }

        private Type ToolType
        {
            get
            {
                Initialize();
                return _toolType;
            }
        }

        private float GetCustomStatValue([NotNull] StatDef statDef)
        {
            try
            {
                if (Enum.TryParse(CustomMeleeWeaponStats.GetStatName(statDef.defName),
                        out CustomMeleeWeaponStat meleeWeaponStat))
                {
                    switch (meleeWeaponStat)
                    {
                        case CustomMeleeWeaponStat.ArmorPenetration:
                            return ArmorPenetration;
                        case CustomMeleeWeaponStat.DpsSharp:
                            var sharpVerbProperties =
                                VerbUtility.GetAllVerbProperties(Thing.def.Verbs, Thing.def.tools);
                            if (sharpVerbProperties == null) { return 0f; }
                            var sharpVerbs = sharpVerbProperties.Where(vp =>
                                (vp.verbProps?.IsMeleeAttack ?? false) && "Sharp".Equals(
                                    vp.maneuver?.verb?.meleeDamageDef?.armorCategory?.defName,
                                    StringComparison.OrdinalIgnoreCase)).ToList();
                            if (!sharpVerbs.Any()) { return 0f; }
                            var sharpDamage = sharpVerbs.AverageWeighted(
                                vp => vp.verbProps.AdjustedMeleeSelectionWeight(vp.tool, null, Thing, null, false),
                                vp => vp.verbProps.AdjustedMeleeDamageAmount(vp.tool, null, Thing, null));
                            var sharpCooldown = sharpVerbs.AverageWeighted(
                                vp => vp.verbProps.AdjustedMeleeSelectionWeight(vp.tool, null, Thing, null, false),
                                vp => vp.verbProps.AdjustedCooldown(vp.tool, null, Thing));
                            return sharpCooldown == 0f ? 0f : sharpDamage / sharpCooldown;
                        case CustomMeleeWeaponStat.DpsBlunt:
                            var bluntVerbProperties =
                                VerbUtility.GetAllVerbProperties(Thing.def.Verbs, Thing.def.tools);
                            if (bluntVerbProperties == null) { return 0f; }
                            var bluntVerbs = bluntVerbProperties.Where(vp =>
                                (vp.verbProps?.IsMeleeAttack ?? false) && "Blunt".Equals(
                                    vp.maneuver?.verb?.meleeDamageDef?.armorCategory?.defName,
                                    StringComparison.OrdinalIgnoreCase)).ToList();
                            if (!bluntVerbs.Any()) { return 0f; }
                            var bluntDamage = bluntVerbs.AverageWeighted(
                                vp => vp.verbProps.AdjustedMeleeSelectionWeight(vp.tool, null, Thing, null, false),
                                vp => vp.verbProps.AdjustedMeleeDamageAmount(vp.tool, null, Thing, null));
                            var bluntCooldown = bluntVerbs.AverageWeighted(
                                vp => vp.verbProps.AdjustedMeleeSelectionWeight(vp.tool, null, Thing, null, false),
                                vp => vp.verbProps.AdjustedCooldown(vp.tool, null, Thing));
                            return bluntCooldown == 0f ? 0f : bluntDamage / bluntCooldown;
                        case CustomMeleeWeaponStat.TechLevel:
                            return (float) Thing.def.techLevel;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(statDef));
                    }
                }
                Log.Error($"Equipment Manager: Tried to evaluate unknown custom melee stat ({statDef.defName})");
                return 0f;
            }
            catch (Exception e)
            {
                Log.Error(
                    $"Equipment Manager: An error occured while evaluating custom melee stat '{statDef.defName}' of '{Thing.def.defName}':\n{e.Message}\n{e.StackTrace}");
                return 0f;
            }
        }

        public float GetStatValue(StatDef statDef)
        {
            if (!StatValues.TryGetValue(statDef, out var value))
            {
                value = CustomMeleeWeaponStats.IsCustomStat(statDef.defName)
                    ? GetCustomStatValue(statDef)
                    : StatHelper.GetStatValue(Thing, statDef);
                StatValues.Add(statDef, value);
            }
            return value;
        }

        public float GetStatValueDeviation([NotNull] StatDef statDef)
        {
            return statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                CustomMeleeWeaponStats.IsCustomStat(statDef.defName) ? GetCustomStatValue(statDef) :
                StatHelper.GetStatValueDeviation(Thing, statDef);
        }

        private void Initialize()
        {
            if (_initialized) { return; }
            _initialized = true;
            if (!CombatExtendedHelper.CombatExtended) { return; }
            _toolType = AccessTools.TypeByName("CombatExtended.ToolCE");
            if (_toolType == null) { Log.Error("Equipment Manager: Could not find 'CombatExtended.ToolCE'"); }
            _armorPenetrationSharpDelegate = AccessTools.FieldRefAccess<float>(ToolType, "armorPenetrationSharp");
            if (_armorPenetrationSharpDelegate == null)
            {
                Log.Error("Equipment Manager: Could not find 'CombatExtended.ToolCE.armorPenetrationSharp'");
            }
            _armorPenetrationBluntDelegate = AccessTools.FieldRefAccess<float>(ToolType, "armorPenetrationBlunt");
            if (_armorPenetrationBluntDelegate == null)
            {
                Log.Error("Equipment Manager: Could not find 'CombatExtended.ToolCE.armorPenetrationBlunt'");
            }
        }

        public override bool Update(RimworldTime time)
        {
            if (!base.Update(time)) { return false; }
            try
            {
                if (CombatExtendedHelper.CombatExtended && ToolType != null)
                {
                    var tools = Thing.def.tools.Where(tool => tool.power > 0f).ToList();
                    if (!tools.Any())
                    {
                        Log.Error(
                            $"Equipment Manager: Could not find any melee tools of '{Thing.LabelCapNoCount}' ({Thing.def?.defName})");
                    }
                    else
                    {
                        foreach (var tool in tools)
                        {
                            if (tool.GetType() != ToolType)
                            {
                                Log.Warning(
                                    $"Equipment Manager: {Thing.LabelCapNoCount}'s tool '{tool.label}' is not CombatExtended-compatible");
                                ArmorPenetration += tool.armorPenetration;
                            }
                            else
                            {
                                if (ArmorPenetrationSharpDelegate != null && ArmorPenetrationBluntDelegate != null)
                                {
                                    ArmorPenetration = ArmorPenetrationSharpDelegate(tool) +
                                        ArmorPenetrationBluntDelegate(tool);
                                }
                                else { ArmorPenetration += tool.armorPenetration; }
                            }
                        }
                        ArmorPenetration /= tools.Count;
                    }
                }
                else { ArmorPenetration = 0; }
            }
            catch (Exception exception)
            {
                Log.Error(
                    $"Equipment Manager: Could not update cache of '{Thing.LabelCapNoCount}' ({Thing.def?.defName}): {exception.Message}");
            }
            return true;
        }
    }
}