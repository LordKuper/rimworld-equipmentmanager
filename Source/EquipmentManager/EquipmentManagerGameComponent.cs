using System.Collections.Generic;
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
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        MeleeWeaponRule.ResetCache();
        RangedWeaponRule.ResetCache();
        ToolRule.ResetCache();
        PawnCache.ResetCache();
        ToolCache.ResetCache();
        PawnColumnWorkers.Loadout.ResetEquipmentManagerCache();
        foreach (var loadout in GetLoadouts()) { loadout.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetMeleeWeaponRules()) { rule.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetRangedWeaponRules()) { rule.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetToolRules()) { rule.NormalizeLegacyCustomStatDefNames(); }
        foreach (var rule in GetMeleeWeaponRules()) { rule.UpdateGloballyAvailableItems(); }
        foreach (var rule in GetRangedWeaponRules()) { rule.UpdateGloballyAvailableItems(); }
        foreach (var rule in GetToolRules()) { rule.UpdateGloballyAvailableItems(); }
    }

    /// <summary>
    ///     Removes per-Thing cache entries whose <see cref="Thing" /> key has been destroyed or
    ///     discarded. Called once per assignment pass to prevent unbounded dictionary growth over a
    ///     long session. Def-keyed caches (<c>_rangedWeaponDefsCache</c>, etc.) are bounded by the
    ///     number of <see cref="ThingDef" />s in the game and are not pruned here.
    /// </summary>
    internal void PruneDestroyedThingCaches()
    {
        var destroyedRanged = new List<Thing>();
        foreach (var key in _rangedWeaponsCache.Keys)
        {
            if (key.Destroyed) { destroyedRanged.Add(key); }
        }
        foreach (var key in destroyedRanged) { _ = _rangedWeaponsCache.Remove(key); }

        var destroyedMelee = new List<Thing>();
        foreach (var key in _meleeWeaponsCache.Keys)
        {
            if (key.Destroyed) { destroyedMelee.Add(key); }
        }
        foreach (var key in destroyedMelee) { _ = _meleeWeaponsCache.Remove(key); }

        var destroyedTools = new List<Thing>();
        foreach (var key in _toolCache.Keys)
        {
            if (key.Destroyed) { destroyedTools.Add(key); }
        }
        foreach (var key in destroyedTools) { _ = _toolCache.Remove(key); }
    }
}
