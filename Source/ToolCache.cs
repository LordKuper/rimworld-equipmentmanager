using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

        private float GetWorkTypeScore(string workTypeDefName)
        {
            return _workTypeScores.ContainsKey(workTypeDefName) ? _workTypeScores[workTypeDefName] : 0f;
        }

        public float GetWorkTypesScore(IEnumerable<string> workTypeDefNames)
        {
            return workTypeDefNames.Average(GetWorkTypeScore);
        }

        public override void Update(RimworldTime time)
        {
            base.Update(time);
            var hoursPassed = ((time.Year - UpdateTime.Year) * 60 * 24) + ((time.Day - UpdateTime.Day) * 24) +
                time.Hour - UpdateTime.Hour;
            if (hoursPassed < UpdateTimer) { return; }
            UpdateTime.Year = time.Year;
            UpdateTime.Day = time.Day;
            UpdateTime.Hour = time.Hour;
            try
            {
                _workTypeScores.Clear();
                foreach (var workTypeDef in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
                {
                    var score = 0f;
                    var workTypeRule = EquipmentManager.GetWorkTypeRules()
                        .FirstOrDefault(rule => rule.WorkTypeDefName == workTypeDef.defName);
                    if (workTypeRule != null) { score += workTypeRule.GetStatScore(Thing); }
                    _workTypeScores.Add(workTypeDef.defName, score);
                }
            }
            catch (Exception exception)
            {
                Log.Error(
                    $"Equipment Manager: Could not update cache of '{Thing.LabelCap}' ({Thing.def?.defName}): {exception.Message}");
            }
        }
    }
}