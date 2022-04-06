using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EquipmentManager
{
    internal partial class EquipmentManagerGameComponent
    {
        private List<WorkTypeRule> _workTypeRules = new List<WorkTypeRule>();

        public void AddWorkTypeRule(WorkTypeRule workTypeRule)
        {
            var existingRule =
                _workTypeRules.FirstOrDefault(rule => rule.WorkTypeDefName == workTypeRule.WorkTypeDefName);
            if (existingRule != null) { _ = _workTypeRules.Remove(existingRule); }
            _workTypeRules.Add(workTypeRule);
        }

        public void DeleteWorkTypeRule(WorkTypeRule workTypeRule)
        {
            _ = _workTypeRules.Remove(workTypeRule);
        }

        private void ExposeData_WorkTypes()
        {
            Scribe_Collections.Look(ref _workTypeRules, "WorkTypeRules", LookMode.Deep);
        }

        public IEnumerable<WorkTypeRule> GetWorkTypeRules()
        {
            if (_workTypeRules == null || _workTypeRules.Count == 0)
            {
                _workTypeRules = new List<WorkTypeRule>(WorkTypeRule.DefaultRules);
            }
            return _workTypeRules;
        }
    }
}