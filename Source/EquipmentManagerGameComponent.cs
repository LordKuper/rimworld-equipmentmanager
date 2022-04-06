using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Verse;

namespace EquipmentManager
{
    [UsedImplicitly]
    internal partial class EquipmentManagerGameComponent : GameComponent
    {
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Game API")]
        public EquipmentManagerGameComponent(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            ExposeData_Loadouts();
            ExposeData_MeleeWeaponRules();
            ExposeData_RangedWeaponRules();
            ExposeData_ToolRules();
            ExposeData_StatRanges();
            ExposeData_WorkTypes();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            foreach (var rule in GetMeleeWeaponRules()) { rule.UpdateGloballyAvailableItems(); }
            foreach (var rule in GetRangedWeaponRules()) { rule.UpdateGloballyAvailableItems(); }
            foreach (var rule in GetToolRules()) { rule.UpdateGloballyAvailableItems(); }
        }
    }
}