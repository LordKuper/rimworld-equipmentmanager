using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Verse;

namespace EquipmentManager;

[UsedImplicitly]
internal partial class EquipmentManagerGameComponent : GameComponent
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Game API")]
    public EquipmentManagerGameComponent(Game game) { }

    public override void ExposeData()
    {
        base.ExposeData();
        ExposeData_WorkTypes();
        ExposeData_ToolRules();
        ExposeData_MeleeWeaponRules();
        ExposeData_RangedWeaponRules();
        ExposeData_Loadouts();
        ExposeData_StatRanges();
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        MeleeWeaponRule.ResetCache();
        RangedWeaponRule.ResetCache();
        ToolRule.ResetCache();
        WorkTypeRule.ResetCache();
        PawnCache.ResetCache();
        ToolCache.ResetCache();
        PawnColumnWorkers.Loadout.ResetEquipmentManagerCache();
        foreach (var loadout in GetLoadouts()) { loadout.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetWorkTypeRules()) { rule.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetMeleeWeaponRules()) { rule.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetRangedWeaponRules()) { rule.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetToolRules()) { rule.NormalizeLegacyCustomStatDefNames(); }
        if (_statRanges != null) { _statRanges = LegacyCustomStatDefs.NormalizeStatRanges(_statRanges); }
        foreach (var rule in GetMeleeWeaponRules()) { rule.UpdateGloballyAvailableItems(); }
        foreach (var rule in GetRangedWeaponRules()) { rule.UpdateGloballyAvailableItems(); }
        foreach (var rule in GetToolRules()) { rule.UpdateGloballyAvailableItems(); }
    }
}
