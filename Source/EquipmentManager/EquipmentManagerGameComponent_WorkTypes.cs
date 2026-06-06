using System.Collections.Generic;
using JetBrains.Annotations;
using LordKuper.Common;
using Verse;

namespace EquipmentManager;

internal partial class EquipmentManagerGameComponent
{
    private List<WorkTypeThingRule> _workTypeRules;

    public void AddWorkTypeRule([NotNull] WorkTypeThingRule workTypeRule)
    {
        var existingRule =
            _workTypeRules.FirstOrDefault(rule =>
                rule.WorkTypeDefName == workTypeRule.WorkTypeDefName);
        if (existingRule != null) { _ = _workTypeRules.Remove(existingRule); }
        _workTypeRules.Add(workTypeRule);
    }

    public void DeleteWorkTypeRule(WorkTypeThingRule workTypeRule)
    {
        _ = _workTypeRules.Remove(workTypeRule);
    }

    private void ExposeData_WorkTypes()
    {
        Scribe_Collections.Look(ref _workTypeRules, "WorkTypeRules", LookMode.Deep);
    }

    [NotNull]
    public IEnumerable<WorkTypeThingRule> GetWorkTypeRules()
    {
        if (_workTypeRules == null || _workTypeRules.Count == 0)
        {
            _workTypeRules = new List<WorkTypeThingRule>(WorkTypeThingRule.DefaultRules);
        }
        return _workTypeRules;
    }
}