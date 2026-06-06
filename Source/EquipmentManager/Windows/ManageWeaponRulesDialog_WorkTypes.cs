using System.Collections.Generic;
using System.Linq;
using System.Text;
using EquipmentManager.CustomWidgets;
using JetBrains.Annotations;
using LordKuper.Common.Helpers;
using LordKuper.Common.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace EquipmentManager.Windows;

internal partial class ManageWeaponRulesDialog
{
    private readonly List<Thing> _currentlyAvailableWorkTypes = new();
    private readonly List<ThingDef> _globallyAvailableWorkTypes = new();

    private WorkTypeRule SelectedWorkTypeRule
    {
        get;
        set
        {
            field = value;
            UpdateAvailableItems_WorkTypes();
        }
    }

    private void DoButtonRow_WorkTypes(Rect rect)
    {
        const int buttonCount = 1;
        var buttonWidth = (rect.width - UiHelpers.ButtonGap * (buttonCount - 1)) / buttonCount;
        if (Widgets.ButtonText(new Rect(rect.x, rect.y, buttonWidth, UiHelpers.ButtonHeight),
                Resources.Strings.WeaponRules.SelectRule))
        {
            Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetWorkTypeRules().Select(rule =>
                new FloatMenuOption(rule.Label, () => SelectedWorkTypeRule = rule)).ToList()));
        }
    }

    private void DoTab_WorkTypes(Rect rect)
    {
        var sectionHeaderHeight = Text.LineHeightOf(GameFont.Medium) + UiHelpers.ElementGap;
        var buttonRowRect = new Rect(rect.x, rect.y, rect.width, UiHelpers.ButtonHeight);
        var labelRect = new Rect(rect.x, buttonRowRect.yMax + UiHelpers.ElementGap, rect.width,
            UiHelpers.LabelHeight);
        var availableItemsBoxHeight = ItemIconSize * AvailableItemIconsRowCount +
            ItemIconGap * (AvailableItemIconsRowCount + 1);
        var availableItemsRect = new Rect(rect.x,
            rect.yMax - availableItemsBoxHeight - sectionHeaderHeight, rect.width,
            availableItemsBoxHeight + sectionHeaderHeight);
        var statsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
            availableItemsRect.y - UiHelpers.ElementGap - labelRect.yMax - UiHelpers.ElementGap);
        DoButtonRow_WorkTypes(buttonRowRect);
        UiHelpers.DoGapLineHorizontal(new Rect(rect.x, buttonRowRect.yMax, rect.width,
            UiHelpers.ElementGap));
        if (SelectedWorkTypeRule == null)
        {
            Labels.DoLabel(labelRect, Resources.Strings.WeaponRules.NoRuleSelected,
                TextAnchor.MiddleLeft);
        }
        else
        {
            UiHelpers.DoLabeledText(labelRect, Resources.Strings.WeaponRules.RuleLabel,
                SelectedWorkTypeRule.Label);
            UiHelpers.DoGapLineHorizontal(new Rect(rect.x, labelRect.yMax, rect.width,
                UiHelpers.ElementGap));
            DoRuleStatWeights(statsRect, EquipmentManagerStatDefs.WorkTypeStatDefs,
                SelectedWorkTypeRule.GetStatWeights(), def =>
                {
                    SelectedWorkTypeRule.SetStatWeight(def, 0f);
                    UpdateAvailableItems_WorkTypes();
                }, statDefName =>
                {
                    SelectedWorkTypeRule.DeleteStatWeight(statDefName);
                    UpdateAvailableItems_WorkTypes();
                });
            UiHelpers.DoGapLineHorizontal(new Rect(rect.x, statsRect.yMax, rect.width,
                UiHelpers.ElementGap));
            DoAvailableItems(availableItemsRect, _globallyAvailableWorkTypes, _ => { },
                def => GetWorkTypeDefTooltip(def, SelectedWorkTypeRule),
                _currentlyAvailableWorkTypes, _ => { },
                thing => GetWorkTypeTooltip(thing, SelectedWorkTypeRule),
                UpdateAvailableItems_WorkTypes);
        }
    }

    [NotNull]
    private string GetWorkTypeDefTooltip([NotNull] BuildableDef def, [NotNull] WorkTypeRule rule)
    {
        var stringBuilder = new StringBuilder();
        _ = stringBuilder.AppendLine(def.LabelCap);
        var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
            .ToHashSet();
        if (!stats.Any()) { return stringBuilder.ToString(); }
        _ = stringBuilder.AppendLine();
        var stuffDef = def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null;
        var request = StatRequest.For(def, stuffDef);
        foreach (var stat in stats)
        {
            _ = stringBuilder.AppendLine($"- {stat.LabelCap} = {stat.Worker.GetValue(request):N2}");
        }
        return stringBuilder.ToString();
    }

    [NotNull]
    private string GetWorkTypeTooltip([NotNull] Thing thing, [NotNull] WorkTypeRule rule)
    {
        var stringBuilder = new StringBuilder();
        _ = stringBuilder.AppendLine(thing.LabelCapNoCount);
        var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
            .ToHashSet();
        if (!stats.Any()) { return stringBuilder.ToString(); }
        _ = stringBuilder.AppendLine();
        foreach (var stat in stats)
        {
            _ = stringBuilder.AppendLine(
                $"- {stat.LabelCap} = {StatHelper.GetStatValue(thing, stat):N2}");
        }
        return stringBuilder.ToString();
    }

    private void UpdateAvailableItems_WorkTypes()
    {
        _globallyAvailableWorkTypes.Clear();
        _currentlyAvailableWorkTypes.Clear();
        if (SelectedWorkTypeRule == null) { return; }
        _globallyAvailableWorkTypes.AddRange(SelectedWorkTypeRule.GetGloballyAvailableItems());
        _currentlyAvailableWorkTypes.AddRange(
            SelectedWorkTypeRule.GetCurrentlyAvailableItemsSorted(Find.CurrentMap));
    }
}