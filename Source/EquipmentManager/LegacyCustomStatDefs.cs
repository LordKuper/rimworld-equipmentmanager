using System;
using LordKuper.Common;
using LordKuper.Common.CustomStats;
using LordKuper.Common.Filters.Limits;

namespace EquipmentManager;

internal static class LegacyCustomStatDefs
{
    private const string LegacyPrefix = "EM_";

    public static string NormalizeStatDefName(string? statDefName)
    {
        if (string.IsNullOrEmpty(statDefName)) { throw new ArgumentNullException(nameof(statDefName)); }
        if (!statDefName!.StartsWith(LegacyPrefix, StringComparison.Ordinal)) { return statDefName; }
        if (TryNormalizeStatName(statDefName, "EM_MeleeWeapons_", out var meleeWeaponStatName) &&
            Enum.TryParse(meleeWeaponStatName, out MeleeWeaponStat meleeWeaponStat))
        {
            return MeleeWeaponStats.GetStatDefName(meleeWeaponStat);
        }
        if (TryNormalizeStatName(statDefName, "EM_RangedWeapons_", out var rangedWeaponStatName) &&
            Enum.TryParse(rangedWeaponStatName, out RangedWeaponStat rangedWeaponStat))
        {
            return RangedWeaponStats.GetStatDefName(rangedWeaponStat);
        }
        if (TryNormalizeStatName(statDefName, "EM_Tools_", out var toolStatName) &&
            Enum.TryParse(toolStatName, out ToolStat toolStat)) { return ToolStats.GetStatDefName(toolStat); }
        return statDefName;
    }

    public static StatLimit NormalizeStatLimit(StatLimit statLimit)
    {
        var normalizedName = NormalizeStatDefName(statLimit.StatDefName);
        return normalizedName == statLimit.StatDefName
            ? statLimit
            : new StatLimit(normalizedName, statLimit.MinValue, statLimit.MaxValue);
    }

    public static StatWeight NormalizeStatWeight(StatWeight statWeight)
    {
        var normalizedName = NormalizeStatDefName(statWeight.StatDefName);
        return normalizedName == statWeight.StatDefName
            ? statWeight
            : new StatWeight(normalizedName, statWeight.Weight, statWeight.Protected);
    }

    private static bool TryNormalizeStatName(string statDefName, string prefix, out string? statName)
    {
        if (!statDefName.StartsWith(prefix, StringComparison.Ordinal))
        {
            statName = null;
            return false;
        }
        statName = statDefName.Substring(prefix.Length);
        return true;
    }
}