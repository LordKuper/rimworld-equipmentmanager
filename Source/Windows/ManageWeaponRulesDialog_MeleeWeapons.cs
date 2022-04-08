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
        private readonly List<Thing> _currentlyAvailableMeleeWeapons = new List<Thing>();
        private readonly List<ThingDef> _globallyAvailableMeleeWeapons = new List<ThingDef>();
        private MeleeWeaponRule _selectedMeleeWeaponRule;

        private MeleeWeaponRule SelectedMeleeWeaponRule
        {
            get => _selectedMeleeWeaponRule;
            set
            {
                CheckSelectedItemRuleHasName(_selectedMeleeWeaponRule);
                _selectedMeleeWeaponRule?.UpdateGloballyAvailableItems();
                _selectedMeleeWeaponRule = value;
                UpdateAvailableItems_MeleeWeapons();
            }
        }

        private void DoButtonRow_MeleeWeapons(Rect rect)
        {
            const int buttonCount = 4;
            var buttonWidth = (rect.width - (UiHelpers.ButtonGap * (buttonCount - 1))) / buttonCount;
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, buttonWidth, UiHelpers.ButtonHeight), Strings.SelectRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetMeleeWeaponRules().Select(rule =>
                    new FloatMenuOption(rule.Label, () => SelectedMeleeWeaponRule = rule)).ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + buttonWidth + UiHelpers.ButtonGap, rect.y, buttonWidth, UiHelpers.ButtonHeight),
                    Strings.AddRule)) { SelectedMeleeWeaponRule = EquipmentManager.AddMeleeWeaponRule(); }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 2), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.CopyRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetMeleeWeaponRules().Select(rule =>
                    new FloatMenuOption(rule.Label,
                        () => SelectedMeleeWeaponRule = EquipmentManager.CopyMeleeWeaponRule(rule))).ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 3), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.DeleteRule))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetMeleeWeaponRules().Where(rule => !rule.Protected)
                    .Select(rule => new FloatMenuOption(rule.Label, () =>
                    {
                        EquipmentManager.DeleteMeleeWeaponRule(rule);
                        if (rule == SelectedMeleeWeaponRule) { SelectedMeleeWeaponRule = null; }
                    })).ToList()));
            }
        }

        private void DoRuleSettings_MeleeWeapons(Rect rect)
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
            var columnWidth = (settingsRect.width - (UiHelpers.ElementGap * (UiHelpers.BoolSettingsColumnCount - 1))) /
                UiHelpers.BoolSettingsColumnCount;
            for (var i = 1; i < UiHelpers.BoolSettingsColumnCount; i++)
            {
                UiHelpers.DoGapLineVertical(new Rect(
                    settingsRect.x + (i * (columnWidth + UiHelpers.ElementGap)) - UiHelpers.ElementGap, settingsRect.y,
                    UiHelpers.ElementGap, settingsRect.height));
            }
            DoRuleSetting(UiHelpers.GetBoolSettingRect(settingsRect, 0, columnWidth),
                () => SelectedMeleeWeaponRule.UsableWithShields, value =>
                {
                    SelectedMeleeWeaponRule.UsableWithShields = value;
                    UpdateAvailableItems_MeleeWeapons();
                }, Strings.MeleeWeapons.UsableWithShields, Strings.MeleeWeapons.UsableWithShieldsTooltip);
            DoRuleSetting(UiHelpers.GetBoolSettingRect(settingsRect, 1, columnWidth),
                () => SelectedMeleeWeaponRule.Rottable, value =>
                {
                    SelectedMeleeWeaponRule.Rottable = value;
                    UpdateAvailableItems_MeleeWeapons();
                }, Strings.MeleeWeapons.Rottable, Strings.MeleeWeapons.RottableTooltip);
        }

        private void DoTab_MeleeWeapons(Rect rect)
        {
            const int settingsCount = 2;
            GetWeaponRuleTabRects(rect, settingsCount, out var buttonRowRect, out var labelRect, out var equipModeRect,
                out var settingsRect, out var availableItemsRect, out var exclusiveItemsRect, out var statsRect);
            DoButtonRow_MeleeWeapons(buttonRowRect);
            UiHelpers.DoGapLineHorizontal(new Rect(rect.x, buttonRowRect.yMax, rect.width, UiHelpers.ElementGap));
            if (SelectedMeleeWeaponRule == null) { LabelInput.DoLabelWithoutInput(labelRect, Strings.NoRuleSelected); }
            else
            {
                LabelInput.DoLabelInput(labelRect, Strings.RuleLabel, ref SelectedMeleeWeaponRule.Label);
                UiHelpers.DoGapLineVertical(new Rect(rect.center.x - (UiHelpers.ElementGap / 2f), labelRect.y,
                    UiHelpers.ElementGap, labelRect.height));
                DoWeaponRuleEquipMode(equipModeRect, () => SelectedMeleeWeaponRule.EquipMode,
                    mode => SelectedMeleeWeaponRule.EquipMode = mode);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, labelRect.yMax, rect.width, UiHelpers.ElementGap));
                DoRuleSettings_MeleeWeapons(settingsRect);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, settingsRect.yMax, rect.width, UiHelpers.ElementGap));
                DoRuleStats(statsRect, StatHelper.MeleeWeaponStatDefs, SelectedMeleeWeaponRule.GetStatWeights(), def =>
                {
                    SelectedMeleeWeaponRule.SetStatWeight(def, 0f, false);
                    UpdateAvailableItems_MeleeWeapons();
                }, statDefName =>
                {
                    SelectedMeleeWeaponRule.DeleteStatWeight(statDefName);
                    UpdateAvailableItems_MeleeWeapons();
                }, SelectedMeleeWeaponRule.GetStatLimits(), def =>
                {
                    SelectedMeleeWeaponRule.SetStatLimit(def, null, null);
                    UpdateAvailableItems_MeleeWeapons();
                }, statDefName =>
                {
                    SelectedMeleeWeaponRule.DeleteStatLimit(statDefName);
                    UpdateAvailableItems_MeleeWeapons();
                });
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, statsRect.yMax, rect.width, UiHelpers.ElementGap));
                DoExclusiveItems(exclusiveItemsRect, MeleeWeaponRule.AllRelevantThings,
                    SelectedMeleeWeaponRule.GetWhitelistedItems(), def =>
                    {
                        SelectedMeleeWeaponRule.DeleteWhitelistedItem(def.defName);
                        UpdateAvailableItems_MeleeWeapons();
                    }, def =>
                    {
                        SelectedMeleeWeaponRule.AddWhitelistedItem(def);
                        UpdateAvailableItems_MeleeWeapons();
                    }, SelectedMeleeWeaponRule.GetBlacklistedItems(), def =>
                    {
                        SelectedMeleeWeaponRule.DeleteBlacklistedItem(def.defName);
                        UpdateAvailableItems_MeleeWeapons();
                    }, def =>
                    {
                        SelectedMeleeWeaponRule.AddBlacklistedItem(def);
                        UpdateAvailableItems_MeleeWeapons();
                    });
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, exclusiveItemsRect.yMax, rect.width,
                    UiHelpers.ElementGap));
                DoAvailableItems(availableItemsRect, _globallyAvailableMeleeWeapons, def =>
                {
                    SelectedMeleeWeaponRule.AddBlacklistedItem(def);
                    UpdateAvailableItems_MeleeWeapons();
                }, _currentlyAvailableMeleeWeapons, thing =>
                {
                    SelectedMeleeWeaponRule.AddBlacklistedItem(thing.def);
                    UpdateAvailableItems_MeleeWeapons();
                }, UpdateAvailableItems_MeleeWeapons);
            }
        }

        private void PreClose_MeleeWeapons()
        {
            CheckSelectedItemRuleHasName(_selectedMeleeWeaponRule);
            _selectedMeleeWeaponRule?.UpdateGloballyAvailableItems();
        }

        private void UpdateAvailableItems_MeleeWeapons()
        {
            _globallyAvailableMeleeWeapons.Clear();
            _currentlyAvailableMeleeWeapons.Clear();
            if (SelectedMeleeWeaponRule == null) { return; }
            SelectedMeleeWeaponRule.UpdateGloballyAvailableItems();
            _globallyAvailableMeleeWeapons.AddRange(SelectedMeleeWeaponRule.GetGloballyAvailableItemsSorted());
            _currentlyAvailableMeleeWeapons.AddRange(
                SelectedMeleeWeaponRule.GetCurrentlyAvailableItemsSorted(Find.CurrentMap));
        }
    }
}