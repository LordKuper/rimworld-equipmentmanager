using System.Collections.Generic;
using LordKuper.Common;
using Verse;

namespace LordKuper.EquipmentManager;

internal partial class EquipmentManagerGameComponent
{
    // Populated by Scribe on load (IExposable lifecycle); null when no saved data exists.
    // GetWorkTypeRules() guards with ??= to restore default rules.
    private List<WorkTypeThingRule>? _workTypeRules;

    // Keyed lookup built on first access after any rule edit; null signals a stale cache.
    private Dictionary<string, WorkTypeThingRule>? _workTypeRulesByDefName;

    public void AddWorkTypeRule(WorkTypeThingRule workTypeRule)
    {
        _ = GetWorkTypeRules(); // ensure list is initialized
        var existingRule = _workTypeRules!.FirstOrDefault(rule => rule.WorkTypeDefName == workTypeRule.WorkTypeDefName);
        if (existingRule != null) { _ = _workTypeRules!.Remove(existingRule); }
        _workTypeRules!.Add(workTypeRule);
        _workTypeRulesByDefName = null;
    }

    public void DeleteWorkTypeRule(WorkTypeThingRule workTypeRule)
    {
        _ = GetWorkTypeRules(); // ensure list is initialized
        _ = _workTypeRules!.Remove(workTypeRule);
        _workTypeRulesByDefName = null;
    }

    private void ExposeData_WorkTypes()
    {
        Scribe_Collections.Look(ref _workTypeRules, "WorkTypeRules", LookMode.Deep);
        _workTypeRulesByDefName = null;
    }

    /// <summary>
    ///     Returns the <see cref="WorkTypeThingRule" /> for the given work-type defName, or
    ///     <c>null</c> if no rule exists for it. Uses a defName-keyed dictionary built once per
    ///     rule-list version, avoiding a per-call linear scan.
    /// </summary>
    public WorkTypeThingRule? GetWorkTypeRuleByDefName(string workTypeDefName)
    {
        if (_workTypeRulesByDefName == null)
        {
            _ = GetWorkTypeRules(); // ensure list is initialized
            _workTypeRulesByDefName = new Dictionary<string, WorkTypeThingRule>(_workTypeRules!.Count);
            foreach (var rule in _workTypeRules!)
            {
                if (rule.WorkTypeDefName != null) { _workTypeRulesByDefName[rule.WorkTypeDefName] = rule; }
            }
        }
        _ = _workTypeRulesByDefName.TryGetValue(workTypeDefName, out var result);
        return result;
    }

    public IEnumerable<WorkTypeThingRule> GetWorkTypeRules()
    {
        if (_workTypeRules == null || _workTypeRules.Count == 0)
        {
            _workTypeRules = [.. WorkTypeThingRule.DefaultRules];
            _workTypeRulesByDefName = null;
        }
        return _workTypeRules;
    }
}