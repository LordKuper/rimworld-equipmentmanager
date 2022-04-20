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

        private void DoItemProperties_MeleeWeapons(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.ItemProperties);
            Text.Font = font;
            Text.Anchor = anchor;
            var propertiesRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                rect.yMax - (labelRect.yMax + UiHelpers.ElementGap));
            var columnWidth =
                (propertiesRect.width - (UiHelpers.ElementGap * (UiHelpers.BoolSettingsColumnCount - 1))) /
                UiHelpers.BoolSettingsColumnCount;
            for (var i = 1; i < UiHelpers.BoolSettingsColumnCount; i++)
            {
                UiHelpers.DoGapLineVertical(new Rect(
                    propertiesRect.x + (i * (columnWidth + UiHelpers.ElementGap)) - UiHelpers.ElementGap,
                    propertiesRect.y, UiHelpers.ElementGap, propertiesRect.height));
            }
            DoRuleSetting(UiHelpers.GetBoolSettingRect(propertiesRect, 0, columnWidth),
                () => SelectedMeleeWeaponRule.UsableWithShields, value =>
                {
                    SelectedMeleeWeaponRule.UsableWithShields = value;
                    UpdateAvailableItems_MeleeWeapons();
                }, Strings.MeleeWeapons.UsableWithShields, Strings.MeleeWeapons.UsableWithShieldsTooltip);
            DoRuleSetting(UiHelpers.GetBoolSettingRect(propertiesRect, 1, columnWidth),
                () => SelectedMeleeWeaponRule.Rottable, value =>
                {
                    SelectedMeleeWeaponRule.Rottable = value;
                    UpdateAvailableItems_MeleeWeapons();
                }, Strings.MeleeWeapons.Rottable, Strings.MeleeWeapons.RottableTooltip);
        }

        private void DoTab_MeleeWeapons(Rect rect)
        {
            const int ruleSettingsCount = 0;
            const int itemPropertiesCount = 2;
            GetWeaponRuleTabRects(rect, ruleSettingsCount, itemPropertiesCount, out var buttonRowRect,
                out var labelRect, out var equipModeRect, out _, out var itemPropertiesRect, out var availableItemsRect,
                out var exclusiveItemsRect, out var statsRect);
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
                DoItemProperties_MeleeWeapons(itemPropertiesRect);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, itemPropertiesRect.yMax, rect.width,
                    UiHelpers.ElementGap));
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
                    }, def => GetMeleeWeaponDefTooltip(def, SelectedMeleeWeaponRule));
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, exclusiveItemsRect.yMax, rect.width,
                    UiHelpers.ElementGap));
                DoAvailableItems(availableItemsRect, _globallyAvailableMeleeWeapons, def =>
                    {
                        SelectedMeleeWeaponRule.AddBlacklistedItem(def);
                        UpdateAvailableItems_MeleeWeapons();
                    }, def => GetMeleeWeaponDefTooltip(def, SelectedMeleeWeaponRule), _currentlyAvailableMeleeWeapons,
                    thing =>
                    {
                        SelectedMeleeWeaponRule.AddBlacklistedItem(thing.def);
                        UpdateAvailableItems_MeleeWeapons();
                    }, thing => GetMeleeWeaponTooltip(thing, SelectedMeleeWeaponRule),
                    UpdateAvailableItems_MeleeWeapons);
            }
        }

        private string GetMeleeWeaponDefTooltip(ThingDef def, ItemRule rule)
        {
            var stringBuilder = new StringBuilder();
            _ = stringBuilder.AppendLine(def.LabelCap);
            var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
                .Union(rule.GetStatLimits().Where(sl => sl.StatDef != null).Select(sl => sl.StatDef)).ToHashSet();
            if (!stats.Any()) { return stringBuilder.ToString(); }
            var cache = EquipmentManager.GetMeleeWeaponDefCache(def, RimworldTime.GetMapTime(Find.CurrentMap));
            _ = stringBuilder.AppendLine();
            foreach (var stat in stats)
            {
                _ = stringBuilder.AppendLine($"- {stat.LabelCap} = {cache.GetStatValue(stat):N2}");
            }
            return stringBuilder.ToString();
        }

        private string GetMeleeWeaponTooltip(Thing thing, ItemRule rule)
        {
            var stringBuilder = new StringBuilder();
            _ = stringBuilder.AppendLine(thing.LabelCapNoCount);
            var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef)
                .Union(rule.GetStatLimits().Where(sl => sl.StatDef != null).Select(sl => sl.StatDef)).ToHashSet();
            if (!stats.Any()) { return stringBuilder.ToString(); }
            var cache = EquipmentManager.GetMeleeWeaponCache(thing, RimworldTime.GetMapTime(Find.CurrentMap));
            _ = stringBuilder.AppendLine();
            foreach (var stat in stats)
            {
                _ = stringBuilder.AppendLine($"- {stat.LabelCap} = {cache.GetStatValue(stat):N2}");
            }
            return stringBuilder.ToString();
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
            var map = Find.CurrentMap;
            SelectedMeleeWeaponRule.UpdateGloballyAvailableItems();
            _globallyAvailableMeleeWeapons.AddRange(
                SelectedMeleeWeaponRule.GetGloballyAvailableItemsSorted(RimworldTime.GetMapTime(map)));
            _currentlyAvailableMeleeWeapons.AddRange(
                SelectedMeleeWeaponRule.GetCurrentlyAvailableItemsSorted(map, RimworldTime.GetMapTime(map)));
        }
    }
}