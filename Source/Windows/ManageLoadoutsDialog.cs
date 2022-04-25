using System;
using System.Linq;
using EquipmentManager.CustomWidgets;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Strings = EquipmentManager.Resources.Strings.Loadouts;

namespace EquipmentManager.Windows
{
    internal class ManageLoadoutsDialog : Window
    {
        private static Vector2 _availablePawnsScrollPosition;
        private static EquipmentManagerGameComponent _equipmentManager;
        private static Vector2 _scrollPosition;
        private float _scrollViewHeight;
        private Loadout _selectedLoadout;

        public ManageLoadoutsDialog(Loadout selectedLoadout)
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            SelectedLoadout = selectedLoadout;
        }

        private int AvailablePawnsColumnCount => InitialSize.x < MaxSize.x ? 3 : 5;
        private int AvailablePawnsRowCount => InitialSize.y < MaxSize.y ? 2 : 3;
        private static float AvailablePawnsRowHeight => Text.LineHeightOf(GameFont.Small) + UiHelpers.ElementGap;

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public override Vector2 InitialSize => UiHelpers.GetWindowSize(new Vector2(850f, 650f), MaxSize);
        private int LabeledButtonListColumnCount => InitialSize.x < MaxSize.x ? 2 : 3;
        private static Vector2 MaxSize => new Vector2(1200f, 1000f);
        private int PawnSettingsColumnCount => InitialSize.x < MaxSize.x ? 3 : 4;

        private Loadout SelectedLoadout
        {
            get => _selectedLoadout;
            set
            {
                CheckSelectedLoadoutHasName();
                _selectedLoadout = value;
                ResetScrollPositions();
            }
        }

        private void CheckSelectedLoadoutHasName()
        {
            if (SelectedLoadout == null || !SelectedLoadout.Label.NullOrEmpty()) { return; }
            SelectedLoadout.Label = $"{SelectedLoadout.Id}";
        }

        private void DoAvailablePawns(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.AvailablePawns);
            Text.Font = font;
            Text.Anchor = anchor;
            PawnBox.DoPawnBox(
                new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                    rect.yMax - (labelRect.yMax + UiHelpers.ElementGap)), new Color(1f, 1f, 1f, 0.05f),
                new Color(1f, 1f, 1f, 0.4f), AvailablePawnsColumnCount, UiHelpers.ElementGap,
                ref _availablePawnsScrollPosition, SelectedLoadout.GetAvailablePawnsOrdered());
        }

        private void DoButtonRow(Rect rect)
        {
            const int buttonCount = 7;
            var buttonWidth = (rect.width - (UiHelpers.ButtonGap * (buttonCount - 1))) / buttonCount;
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, buttonWidth, UiHelpers.ButtonHeight),
                    Strings.SelectLoadout))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetLoadouts()
                    .Select(loadout => new FloatMenuOption(loadout.Label, () => SelectedLoadout = loadout)).ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + buttonWidth + UiHelpers.ButtonGap, rect.y, buttonWidth, UiHelpers.ButtonHeight),
                    Strings.AddLoadout)) { SelectedLoadout = EquipmentManager.AddLoadout(); }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 2), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.CopyLoadout))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetLoadouts().Select(loadout =>
                        new FloatMenuOption(loadout.Label,
                            () => SelectedLoadout = EquipmentManager.CopyLoadout(loadout)))
                    .ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 3), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.DeleteLoadout))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetLoadouts().Select(loadout =>
                    new FloatMenuOption(loadout.Label, () =>
                    {
                        EquipmentManager.DeleteLoadout(loadout);
                        if (loadout == SelectedLoadout) { SelectedLoadout = null; }
                    })).ToList()));
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 4), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.ManageWeaponRules))
            {
                Find.WindowStack.Add(new ManageWeaponRulesDialog());
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 5), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.ImportLoadouts))
            {
                Find.WindowStack.Add(new ImportLoadoutsDialog());
            }
            if (Widgets.ButtonText(
                    new Rect(rect.x + ((buttonWidth + UiHelpers.ButtonGap) * 6), rect.y, buttonWidth,
                        UiHelpers.ButtonHeight), Strings.Log)) { Find.WindowStack.Add(new LogDialog()); }
        }

        private float DoLoadoutSettings(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.LoadoutSettings);
            Text.Font = font;
            Text.Anchor = anchor;
            var priorityRect = LabelInput.DoLabeledRect(
                new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width, UiHelpers.ListRowHeight),
                Strings.PriorityLabel, Strings.PriorityTooltip);
            SelectedLoadout.Priority = (int) Widgets.HorizontalSlider(priorityRect, SelectedLoadout.Priority, 0, 10,
                true, $"{SelectedLoadout.Priority:N0}", roundTo: 1f);
            var settingsRect = new Rect(rect.x, priorityRect.yMax + UiHelpers.ElementGap, rect.width,
                UiHelpers.ListRowHeight);
            var columnWidth = (settingsRect.width - (UiHelpers.ElementGap * (UiHelpers.BoolSettingsColumnCount - 1))) /
                UiHelpers.BoolSettingsColumnCount;
            for (var i = 1; i < UiHelpers.BoolSettingsColumnCount; i++)
            {
                UiHelpers.DoGapLineVertical(new Rect(
                    settingsRect.x + (i * (columnWidth + UiHelpers.ElementGap)) - UiHelpers.ElementGap, settingsRect.y,
                    UiHelpers.ElementGap, settingsRect.height));
            }
            var dropUnassignedWeaponsRect = UiHelpers.GetBoolSettingRect(settingsRect, 0, columnWidth);
            var checkboxRect = new Rect(dropUnassignedWeaponsRect.x, dropUnassignedWeaponsRect.y,
                dropUnassignedWeaponsRect.height, dropUnassignedWeaponsRect.height);
            Widgets.Checkbox(checkboxRect.x, checkboxRect.y, ref SelectedLoadout.DropUnassignedWeapons);
            var dropUnassignedWeaponsLabelRect = new Rect(checkboxRect.xMax + (UiHelpers.ElementGap / 2f),
                dropUnassignedWeaponsRect.y,
                dropUnassignedWeaponsRect.width - checkboxRect.width - (UiHelpers.ElementGap / 2f),
                dropUnassignedWeaponsRect.height);
            TooltipHandler.TipRegion(dropUnassignedWeaponsLabelRect, Strings.DropUnassignedWeaponsTooltip);
            anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(dropUnassignedWeaponsLabelRect, Strings.DropUnassignedWeapons);
            Text.Anchor = anchor;
            return dropUnassignedWeaponsRect.yMax - rect.yMin;
        }

        private void DoMeleeSidearmRules(Rect rect)
        {
            var rulesRect = LabelInput.DoLabeledRect(rect, Strings.MeleeSidearmRulesLabel);
            for (var i = 0; i < SelectedLoadout.MeleeSidearmRules.Count; i++)
            {
                var rule = SelectedLoadout.MeleeSidearmRules[i];
                var ruleRect = GetLabeledButtonListItemRect(rulesRect, i);
                var deleteButtonRect =
                    new Rect(ruleRect.x, ruleRect.y, ruleRect.height, ruleRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.MeleeSidearmRules.Remove(rule);
                    break;
                }
                var ruleButtonRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), ruleRect.y,
                    ruleRect.width - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), ruleRect.height);
                if (Widgets.ButtonText(ruleButtonRect, EquipmentManager.GetMeleeWeaponRule(rule).Label))
                {
                    Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetMeleeWeaponRules()
                        .Where(rwr => !SelectedLoadout.MeleeSidearmRules.Contains(rwr.Id)).Select(rwr =>
                            new FloatMenuOption(rwr.Label, () =>
                            {
                                SelectedLoadout.MeleeSidearmRules.Add(rwr.Id);
                                _ = SelectedLoadout.MeleeSidearmRules.Remove(rule);
                            })).ToList()));
                }
            }
            var newRule = EquipmentManager.GetMeleeWeaponRules()
                .FirstOrDefault(rwr => !SelectedLoadout.MeleeSidearmRules.Contains(rwr.Id));
            if (newRule != null)
            {
                var addButtonRect = GetLabeledButtonListItemRect(rulesRect, SelectedLoadout.MeleeSidearmRules.Count);
                if (Widgets.ButtonText(addButtonRect, Resources.Strings.Add))
                {
                    SelectedLoadout.MeleeSidearmRules.Add(newRule.Id);
                }
            }
        }

        private float DoPawnCapacities(Rect rect)
        {
            var columnWidth = (rect.width - UiHelpers.ElementGap) / 2f;
            var weightsRect = new Rect(rect.x, rect.y, columnWidth, 1f);
            var gapRect = new Rect(weightsRect.xMax, rect.y, UiHelpers.ElementGap, 1f);
            var limitsRect = new Rect(gapRect.xMax, rect.y, columnWidth, 1f);
            gapRect.height = Math.Max(DoPawnCapacityWeights(weightsRect), DoPawnCapacityLimits(limitsRect));
            UiHelpers.DoGapLineVertical(gapRect);
            return gapRect.height;
        }

        private float DoPawnCapacityLimits(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnCapacityLimits);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<PawnCapacityDef>.AllDefs
                    .Where(def =>
                        def.showOnHumanlikes &&
                        SelectedLoadout.PawnCapacityLimits.All(pcl => pcl.PawnCapacityDefName != def.defName))
                    .OrderBy(def => def.label).Select(def => new FloatMenuOption(def.LabelCap,
                        () => SelectedLoadout.PawnCapacityLimits.Add(new PawnCapacityLimit(def.defName)))).ToList()));
            }
            var rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width, 1f);
            for (var i = 0; i < SelectedLoadout.PawnCapacityLimits.Count; i++)
            {
                var limit = SelectedLoadout.PawnCapacityLimits[i];
                rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap + (UiHelpers.ListRowHeight * i),
                    rect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.PawnCapacityLimits.Remove(limit);
                    break;
                }
                var statLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (limit.PawnCapacityDef != null && !limit.PawnCapacityDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(statLabelRect, limit.PawnCapacityDef.description);
                }
                _ = Widgets.LabelFit(statLabelRect, limit.PawnCapacityDef?.LabelCap ?? limit.PawnCapacityDefName);
                var statInputRect = new Rect(statLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - statLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                var limitInputWidth = (statInputRect.width - (UiHelpers.ElementGap * 3)) / 2f;
                var minValueRect = new Rect(statInputRect.x, statInputRect.y, limitInputWidth, statInputRect.height);
                limit.MinValueBuffer = Widgets.TextField(minValueRect, limit.MinValueBuffer, 10);
                limit.MinValue = PawnCapacityLimit.Parse(ref limit.MinValueBuffer);
                var dashRect = new Rect(minValueRect.xMax, statInputRect.y, UiHelpers.ElementGap * 3,
                    statInputRect.height);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(dashRect, "-");
                Text.Anchor = TextAnchor.UpperLeft;
                var maxValueRect = new Rect(dashRect.xMax, statInputRect.y, limitInputWidth, statInputRect.height);
                limit.MaxValueBuffer = Widgets.TextField(maxValueRect, limit.MaxValueBuffer, 10);
                limit.MaxValue = PawnCapacityLimit.Parse(ref limit.MaxValueBuffer);
            }
            Text.Font = font;
            Text.Anchor = anchor;
            return rowRect.yMax - rect.yMin;
        }

        private float DoPawnCapacityWeights(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnCapacityWeights);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<PawnCapacityDef>.AllDefs
                    .Where(def =>
                        def.showOnHumanlikes &&
                        SelectedLoadout.PawnCapacityWeights.All(pcw => pcw.PawnCapacityDefName != def.defName))
                    .OrderBy(def => def.label).Select(def => new FloatMenuOption(def.LabelCap,
                        () => SelectedLoadout.PawnCapacityWeights.Add(new PawnCapacityWeight(def.defName)))).ToList()));
            }
            var rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width, 1f);
            for (var i = 0; i < SelectedLoadout.PawnCapacityWeights.Count; i++)
            {
                var weight = SelectedLoadout.PawnCapacityWeights[i];
                rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap + (UiHelpers.ListRowHeight * i),
                    rect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.PawnCapacityWeights.Remove(weight);
                    break;
                }
                var statLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (weight.PawnCapacityDef != null && !weight.PawnCapacityDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(statLabelRect, weight.PawnCapacityDef.description);
                }
                _ = Widgets.LabelFit(statLabelRect, weight.PawnCapacityDef?.LabelCap ?? weight.PawnCapacityDefName);
                var statInputRect = new Rect(statLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - statLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                weight.Weight = Widgets.HorizontalSlider(statInputRect, weight.Weight,
                    -1 * PawnCapacityWeight.WeightCap, PawnCapacityWeight.WeightCap, true, $"{weight.Weight:N1}",
                    roundTo: 0.1f);
            }
            Text.Font = font;
            Text.Anchor = anchor;
            return rowRect.yMax - rect.yMin;
        }

        private float DoPawnPassions(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnPassions);
            var settingsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                UiHelpers.ListRowHeight);
            var index = 0;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            foreach (var passionLimit in SelectedLoadout.PassionLimits.Where(pl => pl.SkillDef != null))
            {
                var passionRect = GetPawnSettingRect(settingsRect, index);
                var deleteButtonRect = new Rect(passionRect.x, passionRect.y, passionRect.height, passionRect.height)
                    .ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.PassionLimits.Remove(passionLimit);
                    break;
                }
                var passionIconRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), passionRect.y,
                    passionRect.height, passionRect.height).ContractedBy(4f);
                switch (passionLimit.Value)
                {
                    case PassionValue.None:
                        GUI.DrawTexture(passionIconRect, Widgets.CheckboxOffTex, ScaleMode.ScaleToFit);
                        if (Widgets.ButtonInvisible(passionRect))
                        {
                            SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                            passionLimit.Value = PassionValue.Minor;
                        }
                        break;
                    case PassionValue.Minor:
                        GUI.DrawTexture(passionIconRect, Resources.Textures.PassionMinor, ScaleMode.ScaleToFit);
                        if (Widgets.ButtonInvisible(passionRect))
                        {
                            SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                            passionLimit.Value = PassionValue.Major;
                        }
                        break;
                    case PassionValue.Major:
                        GUI.DrawTexture(passionIconRect, Resources.Textures.PassionMajor, ScaleMode.ScaleToFit);
                        if (Widgets.ButtonInvisible(passionRect))
                        {
                            SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                            passionLimit.Value = PassionValue.Any;
                        }
                        break;
                    case PassionValue.Any:
                        GUI.DrawTexture(
                            new Rect(passionIconRect.x, passionIconRect.y + (passionIconRect.height / 4f),
                                passionIconRect.width * 3f / 4f, passionIconRect.height * 3f / 4f).ContractedBy(2f),
                            Resources.Textures.PassionMinor, ScaleMode.ScaleToFit);
                        GUI.DrawTexture(
                            new Rect(passionIconRect.x + (passionIconRect.width / 4f), passionIconRect.y,
                                passionIconRect.width * 3 / 4f, passionIconRect.height * 3 / 4f).ContractedBy(2f),
                            Resources.Textures.PassionMajor, ScaleMode.ScaleToFit);
                        if (Widgets.ButtonInvisible(passionRect))
                        {
                            SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                            passionLimit.Value = PassionValue.None;
                        }
                        break;
                }
                var skillLabelRect = new Rect(passionIconRect.xMax + (UiHelpers.ElementGap / 2f), passionRect.y,
                    passionRect.width - passionIconRect.width - (UiHelpers.ElementGap / 2f), passionRect.height);
                if (!passionLimit.SkillDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(skillLabelRect, passionLimit.SkillDef.description);
                }
                Widgets.Label(skillLabelRect, passionLimit.SkillDef.skillLabel.CapitalizeFirst());
                index++;
            }
            var settingRect = GetPawnSettingRect(settingsRect, index);
            if (Widgets.ButtonText(settingRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<SkillDef>.AllDefsListForReading
                    .Where(def => !SelectedLoadout.PassionLimits.Select(pl => pl.SkillDefName).Contains(def.defName))
                    .OrderBy(def => def.defName).Select(def =>
                        new FloatMenuOption(
                            def.skillLabel.NullOrEmpty() ? def.defName : def.skillLabel.CapitalizeFirst(),
                            () => SelectedLoadout.PassionLimits.Add(new PassionLimit(def.defName)))).ToList()));
            }
            Text.Font = font;
            Text.Anchor = anchor;
            return settingRect.yMax - rect.yMin;
        }

        private static void DoPawnSetting(Rect rect, bool value, Action<bool> setter, Action deleteAction, string label,
            string tooltip)
        {
            var deleteButtonRect = new Rect(rect.x, rect.y, rect.height, rect.height).ContractedBy(4f);
            if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete)) { deleteAction(); }
            var checkboxRect =
                new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rect.y, rect.height, rect.height)
                    .ContractedBy(4f);
            CheckBox.DoCheckboxWithCallback(checkboxRect, value, false, setter);
            var labelRect = new Rect(checkboxRect.xMax + (UiHelpers.ElementGap / 2f), rect.y,
                rect.width - checkboxRect.width - (UiHelpers.ElementGap / 2f), rect.height);
            if (!tooltip.NullOrEmpty()) { TooltipHandler.TipRegion(labelRect, tooltip); }
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = anchor;
        }

        private float DoPawnSkillLimits(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnSkillLimits);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<SkillDef>.AllDefsListForReading
                    .Where(def => SelectedLoadout.SkillLimits.All(sl => sl.SkillDefName != def.defName)).Select(def =>
                        new FloatMenuOption(
                            def.skillLabel.NullOrEmpty() ? def.defName : def.skillLabel.CapitalizeFirst(),
                            () => SelectedLoadout.SkillLimits.Add(new SkillLimit(def.defName)))).ToList()));
            }
            var rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width, 1f);
            for (var i = 0; i < SelectedLoadout.SkillLimits.Count; i++)
            {
                var limit = SelectedLoadout.SkillLimits[i];
                rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap + (UiHelpers.ListRowHeight * i),
                    rect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.SkillLimits.Remove(limit);
                    break;
                }
                var skillLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (limit.SkillDef != null && !limit.SkillDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(skillLabelRect, limit.SkillDef?.description);
                }
                _ = Widgets.LabelFit(skillLabelRect,
                    limit.SkillDef?.skillLabel.NullOrEmpty() ?? true
                        ? limit.SkillDefName
                        : limit.SkillDef.skillLabel.CapitalizeFirst());
                var skillInputRect = new Rect(skillLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - skillLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                var limitInputWidth = (skillInputRect.width - (UiHelpers.ElementGap * 3)) / 2f;
                var minValueRect = new Rect(skillInputRect.x, skillInputRect.y, limitInputWidth, skillInputRect.height);
                limit.MinValueBuffer = Widgets.TextField(minValueRect, limit.MinValueBuffer, 10);
                limit.MinValue = SkillLimit.Parse(ref limit.MinValueBuffer);
                var dashRect = new Rect(minValueRect.xMax, skillInputRect.y, UiHelpers.ElementGap * 3,
                    skillInputRect.height);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(dashRect, "-");
                Text.Anchor = TextAnchor.UpperLeft;
                var maxValueRect = new Rect(dashRect.xMax, skillInputRect.y, limitInputWidth, skillInputRect.height);
                limit.MaxValueBuffer = Widgets.TextField(maxValueRect, limit.MaxValueBuffer, 10);
                limit.MaxValue = SkillLimit.Parse(ref limit.MaxValueBuffer);
            }
            Text.Font = font;
            Text.Anchor = anchor;
            return rowRect.yMax - rect.yMin;
        }

        private float DoPawnSkills(Rect rect)
        {
            var columnWidth = (rect.width - UiHelpers.ElementGap) / 2f;
            var weightsRect = new Rect(rect.x, rect.y, columnWidth, 1f);
            var gapRect = new Rect(weightsRect.xMax, rect.y, UiHelpers.ElementGap, 1f);
            var limitsRect = new Rect(gapRect.xMax, rect.y, columnWidth, 1f);
            gapRect.height = Math.Max(DoPawnSkillWeights(weightsRect), DoPawnSkillLimits(limitsRect));
            UiHelpers.DoGapLineVertical(gapRect);
            return gapRect.height;
        }

        private float DoPawnSkillWeights(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnSkillWeights);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<SkillDef>.AllDefsListForReading
                    .Where(def => SelectedLoadout.SkillWeights.All(sw => sw.SkillDefName != def.defName)).Select(def =>
                        new FloatMenuOption(
                            def.skillLabel.NullOrEmpty() ? def.defName : def.skillLabel.CapitalizeFirst(),
                            () => SelectedLoadout.SkillWeights.Add(new SkillWeight(def.defName)))).ToList()));
            }
            var rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width, 1f);
            for (var i = 0; i < SelectedLoadout.SkillWeights.Count; i++)
            {
                var weight = SelectedLoadout.SkillWeights[i];
                rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap + (UiHelpers.ListRowHeight * i),
                    rect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.SkillWeights.Remove(weight);
                    break;
                }
                var skillLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (weight.SkillDef != null && !weight.SkillDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(skillLabelRect, weight.SkillDef?.description);
                }
                _ = Widgets.LabelFit(skillLabelRect, weight.SkillDef?.LabelCap ?? weight.SkillDefName);
                var skillInputRect = new Rect(skillLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - skillLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                weight.Weight = Widgets.HorizontalSlider(skillInputRect, weight.Weight, -1 * SkillWeight.WeightCap,
                    SkillWeight.WeightCap, true, $"{weight.Weight:N1}", roundTo: 0.1f);
            }
            Text.Font = font;
            Text.Anchor = anchor;
            return rowRect.yMax - rect.yMin;
        }

        private float DoPawnStatLimits(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnStatLimits);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(StatHelper.DefaultPawnStatDefs
                    .Where(def => SelectedLoadout.StatLimits.All(sl => sl.StatDefName != def.defName)).Select(def =>
                        new FloatMenuOption($"{def.LabelCap} [{def.category?.LabelCap ?? "No category"}]",
                            () => SelectedLoadout.StatLimits.Add(new StatLimit(def.defName)))).ToList()));
            }
            var rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width, 1f);
            for (var i = 0; i < SelectedLoadout.StatLimits.Count; i++)
            {
                var limit = SelectedLoadout.StatLimits[i];
                rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap + (UiHelpers.ListRowHeight * i),
                    rect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.StatLimits.Remove(limit);
                    break;
                }
                var statLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (limit.StatDef != null && !limit.StatDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(statLabelRect, limit.StatDef?.description);
                }
                _ = Widgets.LabelFit(statLabelRect, limit.StatDef?.LabelCap ?? limit.StatDefName);
                var statInputRect = new Rect(statLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - statLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                var limitInputWidth = (statInputRect.width - (UiHelpers.ElementGap * 3)) / 2f;
                var minValueRect = new Rect(statInputRect.x, statInputRect.y, limitInputWidth, statInputRect.height);
                limit.MinValueBuffer = Widgets.TextField(minValueRect, limit.MinValueBuffer, 10);
                limit.MinValue = StatLimit.Parse(ref limit.MinValueBuffer);
                var dashRect = new Rect(minValueRect.xMax, statInputRect.y, UiHelpers.ElementGap * 3,
                    statInputRect.height);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(dashRect, "-");
                Text.Anchor = TextAnchor.UpperLeft;
                var maxValueRect = new Rect(dashRect.xMax, statInputRect.y, limitInputWidth, statInputRect.height);
                limit.MaxValueBuffer = Widgets.TextField(maxValueRect, limit.MaxValueBuffer, 10);
                limit.MaxValue = StatLimit.Parse(ref limit.MaxValueBuffer);
            }
            Text.Font = font;
            Text.Anchor = anchor;
            return rowRect.yMax - rect.yMin;
        }

        private float DoPawnStats(Rect rect)
        {
            var columnWidth = (rect.width - UiHelpers.ElementGap) / 2f;
            var weightsRect = new Rect(rect.x, rect.y, columnWidth, 1f);
            var gapRect = new Rect(weightsRect.xMax, rect.y, UiHelpers.ElementGap, 1f);
            var limitsRect = new Rect(gapRect.xMax, rect.y, columnWidth, 1f);
            gapRect.height = Math.Max(DoPawnStatWeights(weightsRect), DoPawnStatLimits(limitsRect));
            UiHelpers.DoGapLineVertical(gapRect);
            return gapRect.height;
        }

        private float DoPawnStatWeights(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnStatWeights);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(StatHelper.DefaultPawnStatDefs
                    .Where(def => SelectedLoadout.StatWeights.All(sw => sw.StatDefName != def.defName)).Select(def =>
                        new FloatMenuOption($"{def.LabelCap} [{def.category?.LabelCap ?? "No category"}]",
                            () => SelectedLoadout.StatWeights.Add(new StatWeight(def.defName, false)))).ToList()));
            }
            var rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width, 1f);
            for (var i = 0; i < SelectedLoadout.StatWeights.Count; i++)
            {
                var weight = SelectedLoadout.StatWeights[i];
                rowRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap + (UiHelpers.ListRowHeight * i),
                    rect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (!weight.Protected)
                {
                    if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                    {
                        _ = SelectedLoadout.StatWeights.Remove(weight);
                        break;
                    }
                }
                var statLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (weight.StatDef != null && !weight.StatDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(statLabelRect, weight.StatDef?.description);
                }
                _ = Widgets.LabelFit(statLabelRect, weight.StatDef?.LabelCap ?? weight.StatDefName);
                var statInputRect = new Rect(statLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - statLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                weight.Weight = Widgets.HorizontalSlider(statInputRect, weight.Weight, -1 * StatWeight.WeightCap,
                    StatWeight.WeightCap, true, $"{weight.Weight:N1}", roundTo: 0.1f);
            }
            Text.Font = font;
            Text.Anchor = anchor;
            return rowRect.yMax - rect.yMin;
        }

        private float DoPawnTraits(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnTraits);
            Text.Font = font;
            Text.Anchor = anchor;
            var settingsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                UiHelpers.ListRowHeight);
            var index = 0;
            foreach (var pawnTrait in SelectedLoadout.PawnTraits.ToList())
            {
                var traitRect = GetPawnSettingRect(settingsRect, index);
                var traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(pawnTrait.Key);
                string label;
                string description;
                if (traitDef == null) { label = description = pawnTrait.Key; }
                else
                {
                    label = traitDef.label.CapitalizeFirst();
                    if (label.NullOrEmpty()) { label = pawnTrait.Key; }
                    description = traitDef.description;
                    if (description.NullOrEmpty()) { description = pawnTrait.Key; }
                }
                DoPawnSetting(traitRect, pawnTrait.Value, value => SelectedLoadout.PawnTraits[pawnTrait.Key] = value,
                    () => _ = SelectedLoadout.PawnTraits.Remove(pawnTrait.Key), label, description);
                index++;
            }
            var settingRect = GetPawnSettingRect(settingsRect, index);
            if (Widgets.ButtonText(settingRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<TraitDef>.AllDefsListForReading
                    .Where(traitDef => !SelectedLoadout.PawnTraits.ContainsKey(traitDef.defName))
                    .OrderBy(traitDef => traitDef.defName).Select(traitDef =>
                        new FloatMenuOption(
                            traitDef.label.NullOrEmpty() ? traitDef.defName : traitDef.label.CapitalizeFirst(),
                            () => SelectedLoadout.PawnTraits[traitDef.defName] = true)).ToList()));
            }
            return settingRect.yMax - rect.yMin;
        }

        private float DoPawnWorkCapacities(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnWorkCapacities);
            Text.Font = font;
            Text.Anchor = anchor;
            var settingsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                UiHelpers.ListRowHeight);
            var index = 0;
            foreach (var pawnCapacity in SelectedLoadout.PawnWorkCapacities.ToList())
            {
                var tagRect = GetPawnSettingRect(settingsRect, index);
                var label = Enum.TryParse<WorkTags>(pawnCapacity.Key, out var tag)
                    ? tag.LabelTranslated().CapitalizeFirst()
                    : pawnCapacity.Key;
                DoPawnSetting(tagRect, pawnCapacity.Value,
                    value => SelectedLoadout.PawnWorkCapacities[pawnCapacity.Key] = value,
                    () => _ = SelectedLoadout.PawnWorkCapacities.Remove(pawnCapacity.Key), label, null);
                index++;
            }
            var settingRect = GetPawnSettingRect(settingsRect, index);
            if (Widgets.ButtonText(settingRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(WorkTags)).OfType<WorkTags>()
                    .Where(tag => !SelectedLoadout.PawnWorkCapacities.ContainsKey(tag.ToString()))
                    .OrderBy(tag => tag.LabelTranslated().CapitalizeFirst()).Select(tag =>
                        new FloatMenuOption(tag.LabelTranslated().CapitalizeFirst(),
                            () => SelectedLoadout.PawnWorkCapacities[tag.ToString()] = true)).ToList()));
            }
            return settingRect.yMax - rect.yMin;
        }

        private void DoPrimaryWeaponRule(Rect rect)
        {
            var inputRect = LabelInput.DoLabeledRect(rect, Strings.PrimaryWeaponLabel);
            var inputWidth = (inputRect.width - UiHelpers.ElementGap) / 2f;
            var typeRect = new Rect(inputRect.x, inputRect.y, inputWidth, inputRect.height);
            var ruleRect = new Rect(inputRect.x + inputWidth + UiHelpers.ElementGap, inputRect.y, inputWidth,
                inputRect.height);
            if (Widgets.ButtonText(typeRect, Strings.GetPrimaryWeaponTypeLabel(SelectedLoadout.PrimaryRuleType)))
            {
                Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(Loadout.PrimaryWeaponType))
                    .OfType<Loadout.PrimaryWeaponType>().Select(pwt =>
                        new FloatMenuOption(Strings.GetPrimaryWeaponTypeLabel(pwt),
                            () => SelectedLoadout.PrimaryRuleType = pwt)).ToList()));
            }
            switch (SelectedLoadout.PrimaryRuleType)
            {
                case Loadout.PrimaryWeaponType.None:
                    break;
                case Loadout.PrimaryWeaponType.RangedWeapon:
                    if (Widgets.ButtonText(ruleRect,
                            SelectedLoadout.PrimaryRangedWeaponRuleId == null
                                ? Resources.Strings.WeaponRules.NoRuleSelected
                                : EquipmentManager.GetRangedWeaponRule((int) SelectedLoadout.PrimaryRangedWeaponRuleId)
                                    .Label))
                    {
                        Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetRangedWeaponRules().Select(rule =>
                                new FloatMenuOption(rule.Label,
                                    () => SelectedLoadout.PrimaryRangedWeaponRuleId = rule.Id))
                            .ToList()));
                    }
                    break;
                case Loadout.PrimaryWeaponType.MeleeWeapon:
                    if (Widgets.ButtonText(ruleRect,
                            SelectedLoadout.PrimaryMeleeWeaponRuleId == null
                                ? Resources.Strings.WeaponRules.NoRuleSelected
                                : EquipmentManager.GetMeleeWeaponRule((int) SelectedLoadout.PrimaryMeleeWeaponRuleId)
                                    .Label))
                    {
                        Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetMeleeWeaponRules().Select(rule =>
                                new FloatMenuOption(rule.Label,
                                    () => SelectedLoadout.PrimaryMeleeWeaponRuleId = rule.Id))
                            .ToList()));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoRangedSidearmRules(Rect rect)
        {
            var rulesRect = LabelInput.DoLabeledRect(rect, Strings.RangedSidearmRulesLabel);
            for (var i = 0; i < SelectedLoadout.RangedSidearmRules.Count; i++)
            {
                var rule = SelectedLoadout.RangedSidearmRules[i];
                var ruleRect = GetLabeledButtonListItemRect(rulesRect, i);
                var deleteButtonRect =
                    new Rect(ruleRect.x, ruleRect.y, ruleRect.height, ruleRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.RangedSidearmRules.Remove(rule);
                    break;
                }
                var ruleButtonRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), ruleRect.y,
                    ruleRect.width - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), ruleRect.height);
                if (Widgets.ButtonText(ruleButtonRect, EquipmentManager.GetRangedWeaponRule(rule).Label))
                {
                    Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetRangedWeaponRules()
                        .Where(rwr => !SelectedLoadout.RangedSidearmRules.Contains(rwr.Id)).Select(rwr =>
                            new FloatMenuOption(rwr.Label, () =>
                            {
                                SelectedLoadout.RangedSidearmRules.Add(rwr.Id);
                                _ = SelectedLoadout.RangedSidearmRules.Remove(rule);
                            })).ToList()));
                }
            }
            var newRule = EquipmentManager.GetRangedWeaponRules()
                .FirstOrDefault(rwr => !SelectedLoadout.RangedSidearmRules.Contains(rwr.Id));
            if (newRule != null)
            {
                var addButtonRect = GetLabeledButtonListItemRect(rulesRect, SelectedLoadout.RangedSidearmRules.Count);
                if (Widgets.ButtonText(addButtonRect, Resources.Strings.Add))
                {
                    SelectedLoadout.RangedSidearmRules.Add(newRule.Id);
                }
            }
        }

        private float DoRules(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.Rules);
            Text.Font = font;
            Text.Anchor = anchor;
            var primaryWeaponRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                UiHelpers.ListRowHeight);
            DoPrimaryWeaponRule(primaryWeaponRect);
            var rangedSidearmsRowCount =
                (int) Math.Ceiling((SelectedLoadout.RangedSidearmRules.Count + 1f) / LabeledButtonListColumnCount);
            var rangedSidearmsRect = new Rect(rect.x, primaryWeaponRect.yMax + UiHelpers.ElementGap, rect.width,
                (UiHelpers.ButtonHeight * rangedSidearmsRowCount) +
                (UiHelpers.ButtonGap * (rangedSidearmsRowCount - 1)));
            DoRangedSidearmRules(rangedSidearmsRect);
            var meleeSidearmsRowCount =
                (int) Math.Ceiling((SelectedLoadout.MeleeSidearmRules.Count + 1f) / LabeledButtonListColumnCount);
            var meleeSidearmsRect = new Rect(rect.x, rangedSidearmsRect.yMax + UiHelpers.ElementGap, rect.width,
                (UiHelpers.ButtonHeight * meleeSidearmsRowCount) + (UiHelpers.ButtonGap * (meleeSidearmsRowCount - 1)));
            DoMeleeSidearmRules(meleeSidearmsRect);
            var toolRect = new Rect(rect.x, meleeSidearmsRect.yMax + UiHelpers.ElementGap, rect.width,
                UiHelpers.ListRowHeight);
            DoToolRule(toolRect);
            return toolRect.yMax - rect.yMin;
        }

        private void DoToolRule(Rect rect)
        {
            var inputRect = LabelInput.DoLabeledRect(rect, Strings.ToolsLabel);
            if (Widgets.ButtonText(inputRect,
                    SelectedLoadout.ToolRuleId == null
                        ? Resources.Strings.WeaponRules.NoRuleSelected
                        : EquipmentManager.GetToolRule((int) SelectedLoadout.ToolRuleId).Label))
            {
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetToolRules().Select(rule =>
                    new FloatMenuOption(rule.Label, () => SelectedLoadout.ToolRuleId = rule.Id)).ToList()));
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            var sectionHeaderHeight = Text.LineHeightOf(GameFont.Medium) + UiHelpers.ElementGap;
            var buttonRowRect = new Rect(inRect.x + UiHelpers.ButtonGap, inRect.y,
                inRect.width - (UiHelpers.ButtonGap * 2), UiHelpers.ButtonHeight);
            var labelRect = new Rect(inRect.x, buttonRowRect.yMax + UiHelpers.ElementGap, inRect.width,
                UiHelpers.LabelHeight);
            DoButtonRow(buttonRowRect);
            UiHelpers.DoGapLineHorizontal(new Rect(inRect.x, buttonRowRect.yMax, inRect.width, UiHelpers.ElementGap));
            if (SelectedLoadout == null) { LabelInput.DoLabelWithoutInput(labelRect, Strings.NoLoadoutSelected); }
            else
            {
                LabelInput.DoLabelInput(labelRect, Strings.LoadoutLabel, ref SelectedLoadout.Label);
                UiHelpers.DoGapLineHorizontal(new Rect(inRect.x, labelRect.yMax, inRect.width, UiHelpers.ElementGap));
                var availablePawnsHeight = sectionHeaderHeight + (AvailablePawnsRowHeight * AvailablePawnsRowCount) +
                    (UiHelpers.ElementGap * (AvailablePawnsRowCount - 1));
                var availablePawnsRect = new Rect(inRect.x, inRect.yMax - availablePawnsHeight, inRect.width,
                    availablePawnsHeight);
                var outerRect = new Rect(inRect.x, labelRect.yMax + UiHelpers.ElementGap, inRect.width,
                    availablePawnsRect.y - UiHelpers.ElementGap - (labelRect.yMax + UiHelpers.ElementGap));
                var scrollViewRect = new Rect(outerRect.x, outerRect.y,
                    outerRect.width - GUI.skin.verticalScrollbar.fixedWidth - 4f, _scrollViewHeight);
                var y = 0f;
                Widgets.BeginScrollView(outerRect, ref _scrollPosition, scrollViewRect);
                y += DoLoadoutSettings(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width,
                    UiHelpers.ElementGap));
                y += UiHelpers.ElementGap;
                y += DoRules(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width,
                    UiHelpers.ElementGap));
                y += UiHelpers.ElementGap;
                y += DoPawnTraits(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width,
                    UiHelpers.ElementGap));
                y += UiHelpers.ElementGap;
                y += DoPawnCapacities(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width,
                    UiHelpers.ElementGap));
                y += UiHelpers.ElementGap;
                y += DoPawnWorkCapacities(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width,
                    UiHelpers.ElementGap));
                y += UiHelpers.ElementGap;
                y += DoPawnSkills(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width,
                    UiHelpers.ElementGap));
                y += UiHelpers.ElementGap;
                y += DoPawnPassions(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width,
                    UiHelpers.ElementGap));
                y += UiHelpers.ElementGap;
                y += DoPawnStats(new Rect(scrollViewRect.x, scrollViewRect.y + y, scrollViewRect.width, 1f));
                if (Event.current.type == EventType.Layout) { _scrollViewHeight = y; }
                Widgets.EndScrollView();
                UiHelpers.DoGapLineHorizontal(new Rect(inRect.x, outerRect.yMax, inRect.width, UiHelpers.ElementGap));
                DoAvailablePawns(availablePawnsRect);
            }
        }

        private Rect GetLabeledButtonListItemRect(Rect rect, int index)
        {
            var rowIndex = Math.DivRem(index, LabeledButtonListColumnCount, out var columnIndex);
            var columnWidth =
                ((-1 * LabeledButtonListColumnCount * UiHelpers.ElementGap) + UiHelpers.ElementGap + rect.width) /
                LabeledButtonListColumnCount;
            return new Rect(rect.x + ((columnWidth + UiHelpers.ElementGap) * columnIndex),
                rect.y + ((UiHelpers.ButtonHeight + UiHelpers.ButtonGap) * rowIndex), columnWidth,
                UiHelpers.ButtonHeight);
        }

        private Rect GetPawnSettingRect(Rect rect, int index)
        {
            var rowIndex = Math.DivRem(index, PawnSettingsColumnCount, out var columnIndex);
            var columnWidth =
                ((-1 * PawnSettingsColumnCount * UiHelpers.ElementGap) + UiHelpers.ElementGap + rect.width) /
                PawnSettingsColumnCount;
            return new Rect(rect.x + ((columnWidth + UiHelpers.ElementGap) * columnIndex),
                rect.y + ((UiHelpers.ListRowHeight + UiHelpers.ElementGap) * rowIndex), columnWidth,
                UiHelpers.ListRowHeight);
        }

        public override void PreClose()
        {
            base.PreClose();
            CheckSelectedLoadoutHasName();
        }

        private static void ResetScrollPositions()
        {
            _scrollPosition.Set(0f, 0f);
            _availablePawnsScrollPosition.Set(0f, 0f);
        }
    }
}