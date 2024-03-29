﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EquipmentManager.CustomWidgets;
using UnityEngine;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules;

namespace EquipmentManager.Windows
{
    internal partial class ManageWeaponRulesDialog
    {
        private readonly List<Thing> _currentlyAvailableTools = new List<Thing>();
        private readonly List<ThingDef> _globallyAvailableTools = new List<ThingDef>();
        private ToolRule _selectedToolRule;

        private ToolRule SelectedToolRule
        {
            get => _selectedToolRule;
            set
            {
                CheckSelectedItemRuleHasName(_selectedToolRule);
                _selectedToolRule?.UpdateGloballyAvailableItems();
                _selectedToolRule = value;
                UpdateAvailableItems_Tools();
            }
        }

        private void DoButtonRow_Tools(Rect rect)
        {
            const int buttonCount = 4;
            var buttonWidth = (rect.width - (UiHelpers.ButtonGap * (buttonCount - 1))) / buttonCount;
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, buttonWidth, UiHelpers.ButtonHeight), Strings.SelectRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetToolRules().Select(rule =>
                    new FloatMenuOption(rule.Label, () => SelectedToolRule = rule)).ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + buttonWidth + UiHelpers.ButtonGap, rect.y, buttonWidth, UiHelpers.ButtonHeight),
                    Strings.AddRule)) { SelectedToolRule = EquipmentManager.AddToolRule(); }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 2), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.CopyRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetToolRules().Select(rule =>
                        new FloatMenuOption(rule.Label, () => SelectedToolRule = EquipmentManager.CopyToolRule(rule)))
                    .ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 3), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.DeleteRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetToolRules().Where(rule => !rule.Protected)
                    .Select(rule => new FloatMenuOption(rule.Label, () =>
                    {
                        EquipmentManager.DeleteToolRule(rule);
                        if (rule == SelectedToolRule) { SelectedToolRule = null; }
                    })).ToList()));
            }
        }

        private void DoItemProperties_Tools(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.ItemProperties);
            Text.Font = font;
            Text.Anchor = anchor;
            var itemPropertiesRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                rect.yMax - (labelRect.yMax + UiHelpers.ElementGap));
            var columnWidth =
                (itemPropertiesRect.width - (UiHelpers.ElementGap * (UiHelpers.BoolSettingsColumnCount - 1))) /
                UiHelpers.BoolSettingsColumnCount;
            for (var i = 1; i < UiHelpers.BoolSettingsColumnCount; i++)
            {
                UiHelpers.DoGapLineVertical(new Rect(
                    itemPropertiesRect.x + (i * (columnWidth + UiHelpers.ElementGap)) - UiHelpers.ElementGap,
                    itemPropertiesRect.y, UiHelpers.ElementGap, itemPropertiesRect.height));
            }
            DoRuleSetting(UiHelpers.GetBoolSettingRect(itemPropertiesRect, 0, columnWidth),
                () => SelectedToolRule.Ranged, value =>
                {
                    SelectedToolRule.Ranged = value;
                    UpdateAvailableItems_Tools();
                }, Strings.Tools.Ranged, Strings.Tools.RangedTooltip);
        }

        private void DoTab_Tools(Rect rect)
        {
            const int ruleSettingsCount = 0;
            const int itemPropertiesCount = 1;
            GetWeaponRuleTabRects(rect, ruleSettingsCount, itemPropertiesCount, out var buttonRowRect,
                out var labelRect, out var equipModeRect, out _, out var itemPropertiesRect, out var availableItemsRect,
                out var exclusiveItemsRect, out var statsRect);
            DoButtonRow_Tools(buttonRowRect);
            UiHelpers.DoGapLineHorizontal(new Rect(rect.x, buttonRowRect.yMax, rect.width, UiHelpers.ElementGap));
            if (SelectedToolRule == null) { LabelInput.DoLabelWithoutInput(labelRect, Strings.NoRuleSelected); }
            else
            {
                LabelInput.DoLabelInput(labelRect, Strings.RuleLabel, ref SelectedToolRule.Label);
                UiHelpers.DoGapLineVertical(new Rect(rect.center.x - (UiHelpers.ElementGap / 2f), labelRect.y,
                    UiHelpers.ElementGap, labelRect.height));
                DoToolRuleEquipMode(equipModeRect, () => SelectedToolRule.EquipMode,
                    mode => SelectedToolRule.EquipMode = mode);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, labelRect.yMax, rect.width, UiHelpers.ElementGap));
                DoItemProperties_Tools(itemPropertiesRect);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, itemPropertiesRect.yMax, rect.width,
                    UiHelpers.ElementGap));
                DoRuleStats(statsRect, StatHelper.ToolStatDefs, SelectedToolRule.GetStatWeights(), def =>
                {
                    SelectedToolRule.SetStatWeight(def, 0f, false);
                    UpdateAvailableItems_Tools();
                }, statDefName =>
                {
                    SelectedToolRule.DeleteStatWeight(statDefName);
                    UpdateAvailableItems_Tools();
                }, SelectedToolRule.GetStatLimits(), def =>
                {
                    SelectedToolRule.SetStatLimit(def, null, null);
                    UpdateAvailableItems_Tools();
                }, statDefName =>
                {
                    SelectedToolRule.DeleteStatLimit(statDefName);
                    UpdateAvailableItems_Tools();
                });
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, statsRect.yMax, rect.width, UiHelpers.ElementGap));
                DoExclusiveItems(exclusiveItemsRect, ToolRule.AllRelevantThings, SelectedToolRule.GetWhitelistedItems(),
                    def =>
                    {
                        SelectedToolRule.DeleteWhitelistedItem(def.defName);
                        UpdateAvailableItems_Tools();
                    }, def =>
                    {
                        SelectedToolRule.AddWhitelistedItem(def);
                        UpdateAvailableItems_Tools();
                    }, SelectedToolRule.GetBlacklistedItems(), def =>
                    {
                        SelectedToolRule.DeleteBlacklistedItem(def.defName);
                        UpdateAvailableItems_Tools();
                    }, def =>
                    {
                        SelectedToolRule.AddBlacklistedItem(def);
                        UpdateAvailableItems_Tools();
                    }, def => GetToolDefTooltip(def, SelectedToolRule));
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, exclusiveItemsRect.yMax, rect.width,
                    UiHelpers.ElementGap));
                DoAvailableItems(availableItemsRect, _globallyAvailableTools, def =>
                {
                    SelectedToolRule.AddBlacklistedItem(def);
                    UpdateAvailableItems_Tools();
                }, def => GetToolDefTooltip(def, SelectedToolRule), _currentlyAvailableTools, thing =>
                {
                    SelectedToolRule.AddBlacklistedItem(thing.def);
                    UpdateAvailableItems_Tools();
                }, thing => GetToolTooltip(thing, SelectedToolRule), UpdateAvailableItems_Tools);
            }
        }

        private static void DoToolRuleEquipMode(Rect rect, Func<ItemRule.ToolEquipMode> getter,
            Action<ItemRule.ToolEquipMode> setter)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleRight;
            var labelRect = new Rect(rect.x, rect.y, rect.width / 3f, rect.height);
            Widgets.Label(labelRect, Strings.RuleEquipModeLabel);
            Text.Font = GameFont.Small;
            var inputRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, rect.height);
            if (Widgets.ButtonText(inputRect, Strings.GetToolEquipModeLabel(getter())))
            {
                Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(ItemRule.ToolEquipMode))
                    .OfType<ItemRule.ToolEquipMode>().Select(mode =>
                        new FloatMenuOption(Strings.GetToolEquipModeLabel(mode), () => setter(mode))).ToList()));
            }
            Text.Font = font;
            Text.Anchor = anchor;
        }

        private string GetToolDefTooltip(ThingDef def, ItemRule rule)
        {
            var stringBuilder = new StringBuilder();
            _ = stringBuilder.AppendLine(def.LabelCap);
            var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
                .Union(rule.GetStatLimits().Where(sl => sl.StatDef != null).Select(sl => sl.StatDef)).ToHashSet();
            if (!stats.Any()) { return stringBuilder.ToString(); }
            var cache = EquipmentManager.GetToolDefCache(def, RimworldTime.GetMapTime(Find.CurrentMap));
            _ = stringBuilder.AppendLine();
            foreach (var stat in stats)
            {
                _ = stringBuilder.AppendLine(
                    $"- {stat.LabelCap} = {cache.GetStatValue(stat, WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList()):N2}");
            }
            return stringBuilder.ToString();
        }

        private string GetToolTooltip(Thing thing, ItemRule rule)
        {
            var stringBuilder = new StringBuilder();
            _ = stringBuilder.AppendLine(thing.LabelCapNoCount);
            var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
                .Union(rule.GetStatLimits().Where(sl => sl.StatDef != null).Select(sl => sl.StatDef)).ToHashSet();
            if (!stats.Any()) { return stringBuilder.ToString(); }
            var cache = EquipmentManager.GetToolCache(thing, RimworldTime.GetMapTime(Find.CurrentMap));
            _ = stringBuilder.AppendLine();
            foreach (var stat in stats)
            {
                _ = stringBuilder.AppendLine(
                    $"- {stat.LabelCap} = {cache.GetStatValue(stat, WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList()):N2}");
            }
            return stringBuilder.ToString();
        }

        private void PreClose_Tools()
        {
            CheckSelectedItemRuleHasName(_selectedToolRule);
            _selectedToolRule?.UpdateGloballyAvailableItems();
        }

        private void UpdateAvailableItems_Tools()
        {
            _globallyAvailableTools.Clear();
            _currentlyAvailableTools.Clear();
            if (SelectedToolRule == null) { return; }
            var map = Find.CurrentMap;
            SelectedToolRule.UpdateGloballyAvailableItems();
            _globallyAvailableTools.AddRange(SelectedToolRule.GetGloballyAvailableItemsSorted(
                WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList(), RimworldTime.GetMapTime(map)));
            _currentlyAvailableTools.AddRange(SelectedToolRule.GetCurrentlyAvailableItemsSorted(map,
                WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList(), RimworldTime.GetMapTime(map)));
        }
    }
}