using UnityEngine;
using Verse;

namespace EquipmentManager
{
    internal static class Resources
    {
        internal static class Strings
        {
            internal static readonly string Add = $"EquipmentManager.{nameof(Add)}".Translate();

            internal static class Loadouts
            {
                internal static readonly string AddLoadout =
                    $"EquipmentManager.Loadouts.{nameof(AddLoadout)}".Translate();

                internal static readonly string AutoSelect =
                    $"EquipmentManager.Loadouts.{nameof(AutoSelect)}".Translate();

                internal static readonly string AvailablePawns =
                    $"EquipmentManager.Loadouts.{nameof(AvailablePawns)}".Translate();

                internal static readonly string CancelDataImport =
                    $"EquipmentManager.Loadouts.{nameof(CancelDataImport)}".Translate();

                internal static readonly string CopyLoadout =
                    $"EquipmentManager.Loadouts.{nameof(CopyLoadout)}".Translate();

                internal static readonly string DeleteLoadout =
                    $"EquipmentManager.Loadouts.{nameof(DeleteLoadout)}".Translate();

                internal static readonly string ImportData =
                    $"EquipmentManager.Loadouts.{nameof(ImportData)}".Translate();

                internal static readonly string ImportLoadouts =
                    $"EquipmentManager.Loadouts.{nameof(ImportLoadouts)}".Translate();

                internal static readonly string LoadoutLabel =
                    $"EquipmentManager.Loadouts.{nameof(LoadoutLabel)}".Translate();

                internal static readonly string LoadoutListHeader =
                    $"EquipmentManager.Loadouts.{nameof(LoadoutListHeader)}".Translate();

                internal static readonly string ManageLoadouts =
                    $"EquipmentManager.Loadouts.{nameof(ManageLoadouts)}".Translate();

                internal static readonly string ManageWeaponRules =
                    $"EquipmentManager.Loadouts.{nameof(ManageWeaponRules)}".Translate();

                internal static readonly string MeleeSidearmRulesLabel =
                    $"EquipmentManager.Loadouts.{nameof(MeleeSidearmRulesLabel)}".Translate();

                internal static readonly string NoLoadoutSelected =
                    $"EquipmentManager.Loadouts.{nameof(NoLoadoutSelected)}".Translate();

                internal static readonly string PawnCapacities =
                    $"EquipmentManager.Loadouts.{nameof(PawnCapacities)}".Translate();

                internal static readonly string PawnSkills =
                    $"EquipmentManager.Loadouts.{nameof(PawnSkills)}".Translate();

                internal static readonly string PawnTraits =
                    $"EquipmentManager.Loadouts.{nameof(PawnTraits)}".Translate();

                internal static readonly string PreferredSkillsLabel =
                    $"EquipmentManager.Loadouts.{nameof(PreferredSkillsLabel)}".Translate();

                internal static readonly string PreferredSkillsTooltip =
                    $"EquipmentManager.Loadouts.{nameof(PreferredSkillsTooltip)}".Translate();

                internal static readonly string PrimaryWeaponLabel =
                    $"EquipmentManager.Loadouts.{nameof(PrimaryWeaponLabel)}".Translate();

                internal static readonly string PriorityLabel =
                    $"EquipmentManager.Loadouts.{nameof(PriorityLabel)}".Translate();

                internal static readonly string PriorityTooltip =
                    $"EquipmentManager.Loadouts.{nameof(PriorityTooltip)}".Translate();

                internal static readonly string RangedSidearmRulesLabel =
                    $"EquipmentManager.Loadouts.{nameof(RangedSidearmRulesLabel)}".Translate();

                internal static readonly string Rules = $"EquipmentManager.Loadouts.{nameof(Rules)}".Translate();

                internal static readonly string SavedGamesListHeader =
                    $"EquipmentManager.Loadouts.{nameof(SavedGamesListHeader)}".Translate();

                internal static readonly string SelectLoadout =
                    $"EquipmentManager.Loadouts.{nameof(SelectLoadout)}".Translate();

                internal static readonly string ToolsLabel =
                    $"EquipmentManager.Loadouts.{nameof(ToolsLabel)}".Translate();

                internal static readonly string UndesirableSkillsLabel =
                    $"EquipmentManager.Loadouts.{nameof(UndesirableSkillsLabel)}".Translate();

                internal static readonly string UndesirableSkillsTooltip =
                    $"EquipmentManager.Loadouts.{nameof(UndesirableSkillsTooltip)}".Translate();

                internal static string GetPrimaryWeaponTypeLabel(Loadout.PrimaryWeaponType primaryWeaponType)
                {
                    return $"EquipmentManager.Loadouts.PrimaryWeaponTypes.{primaryWeaponType}".Translate();
                }

                internal static class Default
                {
                    internal static readonly string Assault =
                        $"EquipmentManager.Loadouts.Default.{nameof(Assault)}".Translate();

                    internal static readonly string Brawler =
                        $"EquipmentManager.Loadouts.Default.{nameof(Brawler)}".Translate();

                    internal static readonly string NoLoadout =
                        $"EquipmentManager.Loadouts.Default.{nameof(NoLoadout)}".Translate();

                    internal static readonly string Pacifist =
                        $"EquipmentManager.Loadouts.Default.{nameof(Pacifist)}".Translate();

                    internal static readonly string Sniper =
                        $"EquipmentManager.Loadouts.Default.{nameof(Sniper)}".Translate();

                    internal static readonly string Support =
                        $"EquipmentManager.Loadouts.Default.{nameof(Support)}".Translate();
                }
            }

            internal static class Stats
            {
                internal static string GetStatDescription(string defName)
                {
                    return $"EquipmentManager.Stats.{defName}.Description".Translate();
                }

                internal static string GetStatLabel(string defName)
                {
                    return $"EquipmentManager.Stats.{defName}.Label".Translate();
                }
            }

            internal static class WeaponRules
            {
                internal static readonly string AddRule = $"EquipmentManager.WeaponRules.{nameof(AddRule)}".Translate();

                internal static readonly string BlacklistedItems =
                    $"EquipmentManager.WeaponRules.{nameof(BlacklistedItems)}".Translate();

                internal static readonly string BlacklistedItemsTooltip =
                    $"EquipmentManager.WeaponRules.{nameof(BlacklistedItemsTooltip)}".Translate();

                internal static readonly string CopyRule =
                    $"EquipmentManager.WeaponRules.{nameof(CopyRule)}".Translate();

                internal static readonly string CurrentlyAvailableItems =
                    $"EquipmentManager.WeaponRules.{nameof(CurrentlyAvailableItems)}".Translate();

                internal static readonly string CurrentlyAvailableItemsTooltip =
                    $"EquipmentManager.WeaponRules.{nameof(CurrentlyAvailableItemsTooltip)}".Translate();

                internal static readonly string DeleteRule =
                    $"EquipmentManager.WeaponRules.{nameof(DeleteRule)}".Translate();

                internal static readonly string GloballyAvailableItems =
                    $"EquipmentManager.WeaponRules.{nameof(GloballyAvailableItems)}".Translate();

                internal static readonly string GloballyAvailableItemsTooltip =
                    $"EquipmentManager.WeaponRules.{nameof(GloballyAvailableItemsTooltip)}".Translate();

                internal static readonly string ItemProperties =
                    $"EquipmentManager.WeaponRules.{nameof(ItemProperties)}".Translate();

                internal static readonly string NoRuleSelected =
                    $"EquipmentManager.WeaponRules.{nameof(NoRuleSelected)}".Translate();

                internal static readonly string Refresh = $"EquipmentManager.WeaponRules.{nameof(Refresh)}".Translate();

                internal static readonly string RuleEquipModeLabel =
                    $"EquipmentManager.WeaponRules.{nameof(RuleEquipModeLabel)}".Translate();

                internal static readonly string RuleLabel =
                    $"EquipmentManager.WeaponRules.{nameof(RuleLabel)}".Translate();

                internal static readonly string SelectRule =
                    $"EquipmentManager.WeaponRules.{nameof(SelectRule)}".Translate();

                internal static readonly string StatLimits =
                    $"EquipmentManager.WeaponRules.{nameof(StatLimits)}".Translate();

                internal static readonly string StatWeights =
                    $"EquipmentManager.WeaponRules.{nameof(StatWeights)}".Translate();

                internal static readonly string WhitelistedItems =
                    $"EquipmentManager.WeaponRules.{nameof(WhitelistedItems)}".Translate();

                internal static readonly string WhitelistedItemsTooltip =
                    $"EquipmentManager.WeaponRules.{nameof(WhitelistedItemsTooltip)}".Translate();

                internal static string GetToolEquipModeLabel(ItemRule.ToolEquipMode equipMode)
                {
                    return $"EquipmentManager.WeaponRules.ToolEquipModes.{equipMode}".Translate();
                }

                internal static string GetWeaponEquipModeLabel(ItemRule.WeaponEquipMode equipMode)
                {
                    return $"EquipmentManager.WeaponRules.WeaponEquipModes.{equipMode}".Translate();
                }

                internal static class MeleeWeapons
                {
                    internal static readonly string Rottable =
                        $"EquipmentManager.WeaponRules.MeleeWeapons.{nameof(Rottable)}".Translate();

                    internal static readonly string RottableTooltip =
                        $"EquipmentManager.WeaponRules.MeleeWeapons.{nameof(RottableTooltip)}".Translate();

                    internal static readonly string Title =
                        $"EquipmentManager.WeaponRules.MeleeWeapons.{nameof(Title)}".Translate();

                    internal static readonly string UsableWithShields =
                        $"EquipmentManager.WeaponRules.MeleeWeapons.{nameof(UsableWithShields)}".Translate();

                    internal static readonly string UsableWithShieldsTooltip =
                        $"EquipmentManager.WeaponRules.MeleeWeapons.{nameof(UsableWithShieldsTooltip)}".Translate();

                    internal static class Default
                    {
                        internal static readonly string HighestDps =
                            $"EquipmentManager.WeaponRules.MeleeWeapons.Default.{nameof(HighestDps)}".Translate();

                        internal static readonly string OneHandHighestDps =
                            $"EquipmentManager.WeaponRules.MeleeWeapons.Default.{nameof(OneHandHighestDps)}"
                                .Translate();
                    }
                }

                internal static class RangedWeapons
                {
                    internal static readonly string Explosive =
                        $"EquipmentManager.WeaponRules.RangedWeapons.{nameof(Explosive)}".Translate();

                    internal static readonly string ExplosiveTooltip =
                        $"EquipmentManager.WeaponRules.RangedWeapons.{nameof(ExplosiveTooltip)}".Translate();

                    internal static readonly string ManualCast =
                        $"EquipmentManager.WeaponRules.RangedWeapons.{nameof(ManualCast)}".Translate();

                    internal static readonly string ManualCastTooltip =
                        $"EquipmentManager.WeaponRules.RangedWeapons.{nameof(ManualCastTooltip)}".Translate();

                    internal static readonly string Title =
                        $"EquipmentManager.WeaponRules.RangedWeapons.{nameof(Title)}".Translate();

                    internal static class Default
                    {
                        internal static readonly string HighestDpsa =
                            $"EquipmentManager.WeaponRules.RangedWeapons.Default.{nameof(HighestDpsa)}".Translate();

                        internal static readonly string HighRof =
                            $"EquipmentManager.WeaponRules.RangedWeapons.Default.{nameof(HighRof)}".Translate();

                        internal static readonly string LongRangeHeavyHitter =
                            $"EquipmentManager.WeaponRules.RangedWeapons.Default.{nameof(LongRangeHeavyHitter)}"
                                .Translate();

                        internal static readonly string LowWarmupTime =
                            $"EquipmentManager.WeaponRules.RangedWeapons.Default.{nameof(LowWarmupTime)}".Translate();
                    }
                }

                internal static class Tools
                {
                    internal static readonly string Ranged =
                        $"EquipmentManager.WeaponRules.Tools.{nameof(Ranged)}".Translate();

                    internal static readonly string RangedTooltip =
                        $"EquipmentManager.WeaponRules.Tools.{nameof(RangedTooltip)}".Translate();

                    internal static readonly string Title =
                        $"EquipmentManager.WeaponRules.Tools.{nameof(Title)}".Translate();

                    internal static class Default
                    {
                        internal static readonly string AllWorkTypes =
                            $"EquipmentManager.WeaponRules.Tools.Default.{nameof(AllWorkTypes)}".Translate();

                        internal static readonly string AssignedWorkTypes =
                            $"EquipmentManager.WeaponRules.Tools.Default.{nameof(AssignedWorkTypes)}".Translate();
                    }
                }

                internal static class WorkTypes
                {
                    internal static readonly string Title =
                        $"EquipmentManager.WeaponRules.WorkTypes.{nameof(Title)}".Translate();
                }
            }
        }

        [StaticConstructorOnStartup]
        internal static class Textures
        {
            internal static readonly Texture2D Delete = ContentFinder<Texture2D>.Get("equipment-manager-delete");
        }
    }
}