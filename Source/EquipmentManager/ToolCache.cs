using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LordKuper.Common;
using LordKuper.Common.CustomStats;
using LordKuper.Common.Helpers;
using RimWorld;
using Verse;

namespace EquipmentManager;

internal class ToolCache([NotNull] Thing thing) : ItemCache
{
    private static EquipmentManagerGameComponent _equipmentManager;
    private readonly Dictionary<string, float> _workTypeScores = new();

    private static EquipmentManagerGameComponent EquipmentManager =>
        _equipmentManager ??= Current.Game.GetComponent<EquipmentManagerGameComponent>();

    private Thing Thing { get; } = thing ?? throw new ArgumentNullException(nameof(thing));

    private float GetCustomStatValue([NotNull] StatDef statDef,
        IReadOnlyCollection<WorkTypeDef> workTypeDefs)
    {
        if (Enum.TryParse(ToolStats.GetStatName(statDef.defName), out ToolStat toolStat))
        {
            switch (toolStat)
            {
                case ToolStat.WorkType:
                    if (!workTypeDefs.Any())
                    {
                        throw new ArgumentException("At least one work type must be passed",
                            nameof(workTypeDefs));
                    }
                    return GetWorkTypesScore(
                        workTypeDefs.Select(workTypeDef => workTypeDef.defName));
                case ToolStat.TechLevel:
                    return (float)Thing.def.techLevel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statDef));
            }
        }
        Log.Error(
            $"Equipment Manager: Tried to evaluate unknown custom tool stat ({statDef.defName})");
        return 0f;
    }

    public float GetStatValue([NotNull] StatDef statDef,
        IReadOnlyCollection<WorkTypeDef> workTypeDefs)
    {
        if (!StatValues.TryGetValue(statDef, out var value))
        {
            value = ToolStats.IsCustomStat(statDef.defName)
                ? GetCustomStatValue(statDef, workTypeDefs)
                : StatHelper.GetStatValue(Thing, statDef);
            StatValues.Add(statDef, value);
        }
        return value;
    }

    public float GetStatValueDeviation([NotNull] StatDef statDef,
        IReadOnlyCollection<WorkTypeDef> workTypeDefs)
    {
        return statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
            ToolStats.IsCustomStat(statDef.defName) ? GetCustomStatValue(statDef, workTypeDefs) :
            StatHelper.GetStatValueDeviation(Thing, statDef);
    }

    private float GetWorkTypeScore([NotNull] string workTypeDefName)
    {
        return _workTypeScores.TryGetValue(workTypeDefName, out var score) ? score : 0f;
    }

    private float GetWorkTypesScore([NotNull] IEnumerable<string> workTypeDefNames)
    {
        return workTypeDefNames.Average(GetWorkTypeScore);
    }

    public static void ResetCache()
    {
        _equipmentManager = null;
    }

    public override bool Update(RimWorldTime time)
    {
        if (!base.Update(time)) { return false; }
        try
        {
            _workTypeScores.Clear();
            foreach (var workTypeDef in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
            {
                var score = 0f;
                var workTypeRule = EquipmentManager.GetWorkTypeRules()
                    .FirstOrDefault(rule => rule.WorkTypeDefName == workTypeDef.defName);
                if (workTypeRule != null) { score += workTypeRule.GetThingScore(Thing); }
                _workTypeScores.Add(workTypeDef.defName, score);
            }
        }
        catch (Exception exception)
        {
            Log.Error(
                $"Equipment Manager: Could not update cache of '{Thing.LabelCapNoCount}' ({Thing.def?.defName}): {exception.Message}");
        }
        return true;
    }
}