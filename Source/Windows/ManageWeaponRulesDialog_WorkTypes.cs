using System.Collections.Generic;
using System.Linq;
using System.Text;
using EquipmentManager.CustomWidgets;
using RimWorld;
using UnityEngine;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules;

namespace EquipmentManager.Windows
{
    internal partial class ManageWeaponRulesDialog
    {
        private readonly List<Thing> _currentlyAvailableWorkTypes = new List<Thing>();
        private readonly List<ThingDef> _globallyAvailableWorkTypes = new List<ThingDef>();
        private WorkTypeRule _selectedWorkTypeRule;

        private WorkTypeRule SelectedWorkTypeRule
        {
            get => _selectedWorkTypeRule;
            set
            {
                _selectedWorkTypeRule = value;
                UpdateAvailableItems_WorkTypes();
            }
        }

        private void DoButtonRow_WorkTypes(Rect rect)
        {
            const int buttonCount = 1;
            var buttonWidth = (rect.width - (UiHelpers.ButtonGap * (buttonCount - 1))) / buttonCount;
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, buttonWidth, UiHelpers.ButtonHeight), Strings.SelectRule))
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
            var availableItemsBoxHeight = (ItemIconSize * AvailableItemIconsRowCount) +
                (ItemIconGap * (AvailableItemIconsRowCount + 1));
            var availableItemsRect = new Rect(rect.x, rect.yMax - availableItemsBoxHeight - sectionHeaderHeight,
                rect.width, availableItemsBoxHeight + sectionHeaderHeight);
            var statsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                availableItemsRect.y - UiHelpers.ElementGap - labelRect.yMax - UiHelpers.ElementGap);
            DoButtonRow_WorkTypes(buttonRowRect);
            UiHelpers.DoGapLineHorizontal(new Rect(rect.x, buttonRowRect.yMax, rect.width, UiHelpers.ElementGap));
            if (SelectedWorkTypeRule == null) { LabelInput.DoLabelWithoutInput(labelRect, Strings.NoRuleSelected); }
            else
            {
                LabelInput.DoLabelInputReadOnly(labelRect, Strings.RuleLabel, SelectedWorkTypeRule.Label);
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, labelRect.yMax, rect.width, UiHelpers.ElementGap));
                DoRuleStatWeights(statsRect, StatHelper.WorkTypeStatDefs, SelectedWorkTypeRule.GetStatWeights(), def =>
                {
                    SelectedWorkTypeRule.SetStatWeight(def, 0f);
                    UpdateAvailableItems_WorkTypes();
                }, statDefName =>
                {
                    SelectedWorkTypeRule.DeleteStatWeight(statDefName);
                    UpdateAvailableItems_WorkTypes();
                });
                UiHelpers.DoGapLineHorizontal(new Rect(rect.x, statsRect.yMax, rect.width, UiHelpers.ElementGap));
                DoAvailableItems(availableItemsRect, _globallyAvailableWorkTypes, def => { },
                    def => GetWorkTypeDefTooltip(def, SelectedWorkTypeRule), _currentlyAvailableWorkTypes, thing => { },
                    thing => GetWorkTypeTooltip(thing, SelectedWorkTypeRule), UpdateAvailableItems_WorkTypes);
            }
        }

        private string GetWorkTypeDefTooltip(ThingDef def, WorkTypeRule rule)
        {
            var stringBuilder = new StringBuilder();
            _ = stringBuilder.AppendLine(def.LabelCap);
            var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef).ToHashSet();
            if (!stats.Any()) { return stringBuilder.ToString(); }
            _ = stringBuilder.AppendLine();
            var thing = def.MadeFromStuff
                ? ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def))
                : ThingMaker.MakeThing(def);
            foreach (var stat in stats)
            {
                _ = stringBuilder.AppendLine($"- {stat.LabelCap} = {StatHelper.GetStatValue(thing, stat):N2}");
            }
            return stringBuilder.ToString();
        }

        private string GetWorkTypeTooltip(Thing thing, WorkTypeRule rule)
        {
            var stringBuilder = new StringBuilder();
            _ = stringBuilder.AppendLine(thing.LabelCapNoCount);
            var stats = rule.GetStatWeights().Where(sw => sw.StatDef != null).Select(sw => sw.StatDef).ToHashSet();
            if (!stats.Any()) { return stringBuilder.ToString(); }
            _ = stringBuilder.AppendLine();
            foreach (var stat in stats)
            {
                _ = stringBuilder.AppendLine($"- {stat.LabelCap} = {StatHelper.GetStatValue(thing, stat):N2}");
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
}