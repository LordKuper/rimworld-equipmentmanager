using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace EquipmentManager
{
    internal class ToolCache : ItemCache
    {
        private static EquipmentManagerGameComponent _equipmentManager;
        private readonly Dictionary<string, float> _workTypeScores = new Dictionary<string, float>();

        public ToolCache([NotNull] Thing thing)
        {
            Thing = thing ?? throw new ArgumentNullException(nameof(thing));
        }

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        private Thing Thing { get; }

        private float GetCustomStatValue([NotNull] StatDef statDef, IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            if (Enum.TryParse(CustomToolStats.GetStatName(statDef.defName), out CustomToolStat toolStat))
            {
                switch (toolStat)
                {
                    case CustomToolStat.WorkType:
                        if (!workTypeDefs.Any())
                        {
                            throw new ArgumentException("At least one work type must be passed", nameof(workTypeDefs));
                        }
                        return GetWorkTypesScore(workTypeDefs.Select(workTypeDef => workTypeDef.defName));
                    case CustomToolStat.TechLevel:
                        return (float) Thing.def.techLevel;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(statDef));
                }
            }
            Log.Error($"Equipment Manager: Tried to evaluate unknown custom tool stat ({statDef.defName})");
            return 0f;
        }

        public float GetStatValue(StatDef statDef, IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            if (!StatValues.TryGetValue(statDef, out var value))
            {
                value = CustomToolStats.IsCustomStat(statDef.defName)
                    ? GetCustomStatValue(statDef, workTypeDefs)
                    : StatHelper.GetStatValue(Thing, statDef);
                StatValues.Add(statDef, value);
            }
            return value;
        }

        public float GetStatValueDeviation([NotNull] StatDef statDef, IReadOnlyCollection<WorkTypeDef> workTypeDefs)
        {
            return statDef == null ? throw new ArgumentNullException(nameof(statDef)) :
                CustomToolStats.IsCustomStat(statDef.defName) ? GetCustomStatValue(statDef, workTypeDefs) :
                StatHelper.GetStatValueDeviation(Thing, statDef);
        }

        private float GetWorkTypeScore(string workTypeDefName)
        {
            return _workTypeScores.ContainsKey(workTypeDefName) ? _workTypeScores[workTypeDefName] : 0f;
        }

        private float GetWorkTypesScore(IEnumerable<string> workTypeDefNames)
        {
            return workTypeDefNames.Average(GetWorkTypeScore);
        }

        public override bool Update(RimworldTime time)
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
                throw;
            }
            return true;
        }
    }
}