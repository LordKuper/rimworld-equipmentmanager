using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using LordKuper.Common.UI.Widgets;
using UnityEngine;
using Verse;

namespace EquipmentManager.Windows;

internal partial class ManageWeaponRulesDialog
{
    private readonly List<ThingDef> _globallyAvailableWorkTypes = new();
    private float _workTypesScrollableContentHeight;
    private Vector2 _workTypesScrollPosition;
    private Vector2 _workTypesThingIconBoxScrollPosition;

    private WorkTypeThingRule? SelectedWorkTypeRule
    {
        get;
        set
        {
            field = value;
            UpdateAvailableItems_WorkTypes();
        }
    }

    private void DoTab_WorkTypes(Rect rect)
    {
        WorkTypeThingRuleWidget.DoWidgetTab(rect, ref _workTypesScrollableContentHeight,
            ref _workTypesScrollPosition, AvailableItemIconsRowCount,
            EquipmentManager.GetWorkTypeRules().ToList(),
            SelectedWorkTypeRule, rule => SelectedWorkTypeRule = rule,
            UpdateAvailableItems_WorkTypes, ref _workTypesThingIconBoxScrollPosition,
            _globallyAvailableWorkTypes);
    }

    private void UpdateAvailableItems_WorkTypes()
    {
        _globallyAvailableWorkTypes.Clear();
        if (SelectedWorkTypeRule == null) { return; }
        _globallyAvailableWorkTypes.AddRange(SelectedWorkTypeRule.GetGloballyAvailableItems());
    }
}
