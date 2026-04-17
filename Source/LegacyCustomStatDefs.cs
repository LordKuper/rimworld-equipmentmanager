using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LordKuper.Common;
using LordKuper.Common.CustomStats;
using LordKuper.Common.Filters.Limits;
using Verse;

namespace EquipmentManager;

internal static class LegacyCustomStatDefs
{
    private const string LegacyPrefix = "EM_";

    [NotNull]
    public static string NormalizeStatDefName([CanBeNull] string statDefName)
    {
        if (string.IsNullOrEmpty(statDefName))
        {
            throw new ArgumentNullException(nameof(statDefName));
        }
        if (!statDefName.StartsWith(LegacyPrefix, StringComparison.Ordinal)) { return statDefName; }
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
            Enum.TryParse(toolStatName, out ToolStat toolStat))
        {
            return ToolStats.GetStatDefName(toolStat);
        }
        return statDefName;
    }

    [NotNull]
    public static StatLimit NormalizeStatLimit([NotNull] StatLimit statLimit)
    {
        var normalizedName = NormalizeStatDefName(statLimit.StatDefName);
        return normalizedName == statLimit.StatDefName
            ? statLimit
            : new StatLimit(normalizedName, statLimit.MinValue, statLimit.MaxValue);
    }

    [NotNull]
    public static Dictionary<string, FloatRange> NormalizeStatRanges(
        [NotNull] Dictionary<string, FloatRange> statRanges)
    {
        var normalized = new Dictionary<string, FloatRange>();
        foreach ((var key, var value) in statRanges)
        {
            var normalizedKey = NormalizeStatDefName(key);
            if (!normalized.TryGetValue(normalizedKey, out var current))
            {
                normalized[normalizedKey] = value;
                continue;
            }
            normalized[normalizedKey] = new FloatRange(Math.Min(current.min, value.min),
                Math.Max(current.max, value.max));
        }
        return normalized;
    }

    [NotNull]
    public static StatWeight NormalizeStatWeight([NotNull] StatWeight statWeight)
    {
        var normalizedName = NormalizeStatDefName(statWeight.StatDefName);
        return normalizedName == statWeight.StatDefName
            ? statWeight
            : new StatWeight(normalizedName, statWeight.Weight, statWeight.Protected);
    }

    private static bool TryNormalizeStatName([NotNull] string statDefName, [NotNull] string prefix,
        [CanBeNull] out string statName)
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