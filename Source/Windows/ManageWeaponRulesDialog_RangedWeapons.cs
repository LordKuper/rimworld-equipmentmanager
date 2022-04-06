using System.Collections.Generic;
using System.Linq;
using EquipmentManager.CustomWidgets;
using UnityEngine;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules;

namespace EquipmentManager.Windows
{
    internal partial class ManageWeaponRulesDialog
    {
        private readonly List<Thing> _currentlyAvailableRangedWeapons = new List<Thing>();
        private readonly List<ThingDef> _globallyAvailableRangedWeapons = new List<ThingDef>();
        private RangedWeaponRule _selectedRangedWeaponRule;

        private RangedWeaponRule SelectedRangedWeaponRule
        {
            get => _selectedRangedWeaponRule;
            set
            {
                CheckSelectedItemRuleHasName(_selectedRangedWeaponRule);
                _selectedRangedWeaponRule?.UpdateGloballyAvailableItems();
                _selectedRangedWeaponRule = value;
                UpdateAvailableItems_RangedWeapons();
            }
        }

        private void DoButtonRow_RangedWeapons(Rect rect)
        {
            const int buttonCount = 4;
            var buttonWidth = (rect.width - (UiHelpers.ButtonGap * (buttonCount - 1))) / buttonCount;
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, buttonWidth, UiHelpers.ButtonHeight), Strings.SelectRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetRangedWeaponRules().Select(rule =>
                    new FloatMenuOption(rule.Label, () => SelectedRangedWeaponRule = rule)).ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + buttonWidth + UiHelpers.ButtonGap, rect.y, buttonWidth, UiHelpers.ButtonHeight),
                    Strings.AddRule)) { SelectedRangedWeaponRule = EquipmentManager.AddRangedWeaponRule(); }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 2), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.CopyRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetRangedWeaponRules().Select(rule =>
                    new FloatMenuOption(rule.Label,
                        () => SelectedRangedWeaponRule = EquipmentManager.CopyRangedWeaponRule(rule))).ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 3), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.DeleteRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetRangedWeaponRules()
                    .Where(rule => !rule.Protected).Select(rule => new FloatMenuOption(rule.Label, () =>
                    {
                        EquipmentManager.DeleteRangedWeaponRule(rule);
                        if (rule == SelectedRangedWeaponRule) { SelectedRangedWeaponRule = null; }
                    })).ToList()));
            }
        }

        private void DoRuleSettings_RangedWeapons(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.ItemProperties);
            Text.Font = font;
            Text.Anchor = anchor;
            var settingsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                rect.yMax - (labelRect.yMax + UiHelpers.ElementGap));
            var columnWidth = (settingsRect.width - (UiHelpers.ElementGap * (SettingsColumnCount - 1))) /
                SettingsColumnCount;
            for (var i = 1; i < SettingsColumnCount; i++)
            {
                UiHelpers.DoGapLineVertical(new Rect(
                    settingsRect.x + (i * (columnWidth + UiHelpers.ElementGap)) - UiHelpers.ElementGap, settingsRect.y,
                    UiHelpers.ElementGap, settingsRect.height));
            }
            DoRuleSetting(GetSettingRect(settingsRect, columnWidth, 0), () => SelectedRangedWeaponRule.Explosive,
                value =>
                {
                    SelectedRangedWeaponRule.Explosive = value;
                    UpdateAvailableItems_RangedWeapons();
                }, Strings.RangedWeapons.Explosive, Strings.RangedWeapons.ExplosiveTooltip);
            DoRuleSetting(GetSettingRect(settingsRect, columnWidth, 1), () => SelectedRangedWeaponRule.ManualCast,
                value =>
                {
                    SelectedRangedWeaponRule.ManualCast = value;
                    UpdateAvailableItems_RangedWeapons();
                }, Strings.RangedWeapons.ManualCast, Strings.RangedWeapons.ManualCastTooltip);
        }

        private void DoTab_RangedWeapons(Rect rect)
        {
            const int settingsCount = 2;
            GetWeaponRuleTabRects(rect, settingsCount, out var buttonRowRect, out var labelRect, out var equipModeRect,
                out var settingsRect, out var availableItemsRect, out var exclusiveItemsRect, out var statsRect);
            DoButtonRow_RangedWeapons(buttonRowRect);
            UiHelpers.DoGapLineHorizontal(new Rect(rect.x, buttonRowRect.yMax, rect.width, UiHelpers.ElementGap));
            if (SelectedRangedWeaponRule == null) { LabelInput.DoLabelWithoutInput(labelRect, Strings.NoRuleSelected); }
            else
            {
                LabelInput.DoLabelInput(labelRect, Strings.RuleLabel, ref SelectedRangedWeaponRule.Label);
                UiHelpers.DoGapLineVertical(new Rect(rect.center.x - (UiHelpers.ElementGap / 2f), labelRect.y,
                    UiHelpers.ElementGap, labelRect.height));
                DoWeaponRuleEquipMode(equipModeRect, () => SelectedRangedWeaponRule.EquipMode,
                    mode => SelectedRangedWeaponRule.EquipMode = mode);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, labelRect.yMax, rect.width, UiHelpers.ElementGap));
                DoRuleSettings_RangedWeapons(settingsRect);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, settingsRect.yMax, rect.width, UiHelpers.ElementGap));
                DoRuleStats(statsRect, StatHelper.RangedWeaponStatDefs, SelectedRangedWeaponRule.GetStatWeights(),
                    def =>
                    {
                        SelectedRangedWeaponRule.SetStatWeight(def, 0f, false);
                        UpdateAvailableItems_RangedWeapons();
                    }, statDefName =>
                    {
                        SelectedRangedWeaponRule.DeleteStatWeight(statDefName);
                        UpdateAvailableItems_RangedWeapons();
                    }, SelectedRangedWeaponRule.GetStatLimits(), def =>
                    {
                        SelectedRangedWeaponRule.SetStatLimit(def, null, null);
                        UpdateAvailableItems_RangedWeapons();
                    }, statDefName =>
                    {
                        SelectedRangedWeaponRule.DeleteStatLimit(statDefName);
                        UpdateAvailableItems_RangedWeapons();
                    });
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, statsRect.yMax, rect.width, UiHelpers.ElementGap));
                DoExclusiveItems(exclusiveItemsRect, RangedWeaponRule.AllRelevantThings,
                    SelectedRangedWeaponRule.GetWhitelistedItems(), def =>
                    {
                        SelectedRangedWeaponRule.DeleteWhitelistedItem(def.defName);
                        UpdateAvailableItems_RangedWeapons();
                    }, def =>
                    {
                        SelectedRangedWeaponRule.AddWhitelistedItem(def);
                        UpdateAvailableItems_RangedWeapons();
                    }, SelectedRangedWeaponRule.GetBlacklistedItems(), def =>
                    {
                        SelectedRangedWeaponRule.DeleteBlacklistedItem(def.defName);
                        UpdateAvailableItems_RangedWeapons();
                    }, def =>
                    {
                        SelectedRangedWeaponRule.AddBlacklistedItem(def);
                        UpdateAvailableItems_RangedWeapons();
                    });
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, exclusiveItemsRect.yMax, rect.width,
                    UiHelpers.ElementGap));
                DoAvailableItems(availableItemsRect, _globallyAvailableRangedWeapons, def =>
                {
                    SelectedRangedWeaponRule.AddBlacklistedItem(def);
                    UpdateAvailableItems_RangedWeapons();
                }, _currentlyAvailableRangedWeapons, thing =>
                {
                    SelectedRangedWeaponRule.AddBlacklistedItem(thing.def);
                    UpdateAvailableItems_RangedWeapons();
                }, UpdateAvailableItems_RangedWeapons);
            }
        }

        private void PreClose_RangedWeapons()
        {
            CheckSelectedItemRuleHasName(_selectedRangedWeaponRule);
            _selectedRangedWeaponRule?.UpdateGloballyAvailableItems();
        }

        private void UpdateAvailableItems_RangedWeapons()
        {
            _globallyAvailableRangedWeapons.Clear();
            _currentlyAvailableRangedWeapons.Clear();
            if (SelectedRangedWeaponRule == null) { return; }
            SelectedRangedWeaponRule.UpdateGloballyAvailableItems();
            _globallyAvailableRangedWeapons.AddRange(SelectedRangedWeaponRule.GetGloballyAvailableItemsSorted());
            _currentlyAvailableRangedWeapons.AddRange(
                SelectedRangedWeaponRule.GetCurrentlyAvailableItemsSorted(Find.CurrentMap));
        }
    }
}