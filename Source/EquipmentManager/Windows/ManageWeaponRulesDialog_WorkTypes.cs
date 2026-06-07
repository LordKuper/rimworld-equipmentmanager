using System.Collections.Generic;
using System.Linq;
using LordKuper.Common;
using LordKuper.Common.UI.Widgets;
using RimWorld;
using UnityEngine;
using Verse;

namespace EquipmentManager.Windows;

internal partial class ManageWeaponRulesDialog
{
    private readonly List<Thing> _currentlyAvailableMapThings = new();
    private readonly List<ThingDef> _globallyAvailableWorkTypes = new();
    private Vector2 _workTypesMapThingIconBoxScrollPosition;
    private Vector2 _workTypesScrollPosition;
    private float _workTypesScrollableContentHeight;
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
        WorkTypeThingRuleWidget.DoWidgetTab(rect, ref _workTypesScrollableContentHeight, ref _workTypesScrollPosition,
            AvailableItemIconsRowCount, EquipmentManager.GetWorkTypeRules().ToList(), SelectedWorkTypeRule,
            rule => SelectedWorkTypeRule = rule, UpdateAvailableItems_WorkTypes,
            ref _workTypesThingIconBoxScrollPosition, _globallyAvailableWorkTypes,
            ref _workTypesMapThingIconBoxScrollPosition, _currentlyAvailableMapThings);
    }

    private void UpdateAvailableItems_WorkTypes()
    {
        _globallyAvailableWorkTypes.Clear();
        _currentlyAvailableMapThings.Clear();
        if (SelectedWorkTypeRule == null) { return; }
        _globallyAvailableWorkTypes.AddRange(SelectedWorkTypeRule.GetGloballyAvailableItems());
        var globalDefs = new HashSet<ThingDef>(_globallyAvailableWorkTypes);
        var mapThings = Find.CurrentMap?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon);
        if (mapThings == null) { return; }
        _currentlyAvailableMapThings.AddRange(mapThings
            .Where(thing => globalDefs.Contains(thing.def) &&
                !(thing.TryGetComp<CompForbiddable>() is { Forbidden: true }))
            .OrderByDescending(thing => SelectedWorkTypeRule.GetThingScore(thing)));
    }
}