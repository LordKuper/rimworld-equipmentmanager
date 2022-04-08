using System;
using System.Linq;
using EquipmentManager.CustomWidgets;
using RimWorld;
using UnityEngine;
using Verse;
using Strings = EquipmentManager.Resources.Strings.Loadouts;

namespace EquipmentManager.Windows
{
    internal class ManageLoadoutsDialog : Window
    {
        private const int AvailablePawnsColumnCount = 5;
        private const int AvailablePawnsRowCount = 3;
        private const int LabeledButtonListColumnCount = 3;
        private const int PawnSettingsColumnCount = 4;
        private static Vector2 _availablePawnsScrollPosition;
        private static EquipmentManagerGameComponent _equipmentManager;
        private static Vector2 _scrollPosition;
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

        private static float AvailablePawnsRowHeight => Text.LineHeightOf(GameFont.Small) + UiHelpers.ElementGap;

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public override Vector2 InitialSize => new Vector2(1200f, 1000f);

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
                ref _availablePawnsScrollPosition, SelectedLoadout.GetAvailablePawns());
        }

        private void DoButtonRow(Rect rect)
        {
            const int buttonCount = 6;
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
                Find.WindowStack.Add(new FloatMenu(EquipmentManager.GetLoadouts().Where(loadout => !loadout.Protected)
                    .Select(loadout => new FloatMenuOption(loadout.Label, () =>
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
        }

        private void DoLoadoutSettings(Rect rect)
        {
            var priorityRect = LabelInput.DoLabeledRect(new Rect(rect.x, rect.y, rect.width, UiHelpers.ListRowHeight),
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
            var labelRect = new Rect(checkboxRect.xMax + (UiHelpers.ElementGap / 2f), dropUnassignedWeaponsRect.y,
                dropUnassignedWeaponsRect.width - checkboxRect.width - (UiHelpers.ElementGap / 2f),
                dropUnassignedWeaponsRect.height);
            TooltipHandler.TipRegion(labelRect, Strings.DropUnassignedWeaponsTooltip);
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, Strings.DropUnassignedWeapons);
            Text.Anchor = anchor;
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

        private void DoPawnCapacities(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnCapacities);
            Text.Font = font;
            Text.Anchor = anchor;
            var settingsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                UiHelpers.ListRowHeight);
            var index = 0;
            foreach (var pawnCapacity in SelectedLoadout.PawnCapacities.ToList())
            {
                var tagRect = GetPawnSettingRect(settingsRect, index);
                var label = Enum.TryParse<WorkTags>(pawnCapacity.Key, out var tag)
                    ? tag.LabelTranslated().CapitalizeFirst()
                    : pawnCapacity.Key;
                DoPawnSetting(tagRect, pawnCapacity.Value,
                    value => SelectedLoadout.PawnCapacities[pawnCapacity.Key] = value,
                    () => _ = SelectedLoadout.PawnCapacities.Remove(pawnCapacity.Key), label, null);
                index++;
            }
            if (Widgets.ButtonText(GetPawnSettingRect(settingsRect, index), Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(WorkTags)).OfType<WorkTags>()
                    .Where(tag => !SelectedLoadout.PawnCapacities.ContainsKey(tag.ToString()))
                    .OrderBy(tag => tag.LabelTranslated().CapitalizeFirst()).Select(tag =>
                        new FloatMenuOption(tag.LabelTranslated().CapitalizeFirst(),
                            () => SelectedLoadout.PawnCapacities[tag.ToString()] = true)).ToList()));
            }
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

        private void DoPawnSkills(Rect rect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width, Text.LineHeight);
            Widgets.Label(labelRect, Strings.PawnSkills);
            Text.Font = font;
            Text.Anchor = anchor;
            var preferredSkillsRowCount =
                (int) Math.Ceiling((SelectedLoadout.PreferredSkills.Count + 1f) / LabeledButtonListColumnCount);
            var preferredSkillsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                (UiHelpers.ButtonHeight * preferredSkillsRowCount) +
                (UiHelpers.ButtonGap * (preferredSkillsRowCount - 1)));
            DoPreferredSkills(preferredSkillsRect);
            var undesirableSkillsRowCount =
                (int) Math.Ceiling((SelectedLoadout.UndesirableSkills.Count + 1f) / LabeledButtonListColumnCount);
            var undesirableSkillsRect = new Rect(rect.x, preferredSkillsRect.yMax + UiHelpers.ElementGap, rect.width,
                (UiHelpers.ButtonHeight * undesirableSkillsRowCount) +
                (UiHelpers.ButtonGap * (undesirableSkillsRowCount - 1)));
            DoUndesirableSkills(undesirableSkillsRect);
        }

        private void DoPawnTraits(Rect rect)
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
                    label = traitDef.LabelCap.ToString();
                    if (label.NullOrEmpty()) { label = pawnTrait.Key; }
                    description = traitDef.description;
                    if (description.NullOrEmpty()) { description = pawnTrait.Key; }
                }
                DoPawnSetting(traitRect, pawnTrait.Value, value => SelectedLoadout.PawnTraits[pawnTrait.Key] = value,
                    () => _ = SelectedLoadout.PawnTraits.Remove(pawnTrait.Key), label, description);
                index++;
            }
            if (Widgets.ButtonText(GetPawnSettingRect(settingsRect, index), Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(DefDatabase<TraitDef>.AllDefsListForReading
                    .Where(traitDef => !SelectedLoadout.PawnTraits.ContainsKey(traitDef.defName))
                    .OrderBy(traitDef => traitDef.defName).Select(traitDef =>
                        new FloatMenuOption(
                            traitDef.LabelCap.NullOrEmpty() ? traitDef.defName : traitDef.LabelCap.ToString(),
                            () => SelectedLoadout.PawnTraits[traitDef.defName] = true)).ToList()));
            }
        }

        private void DoPreferredSkills(Rect rect)
        {
            var skillsRect =
                LabelInput.DoLabeledRect(rect, Strings.PreferredSkillsLabel, Strings.PreferredSkillsTooltip);
            var index = 0;
            foreach (var skill in SelectedLoadout.PreferredSkills)
            {
                var skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
                var skillRect = GetLabeledButtonListItemRect(skillsRect, index);
                var deleteButtonRect =
                    new Rect(skillRect.x, skillRect.y, skillRect.height, skillRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.PreferredSkills.Remove(skill);
                    break;
                }
                var buttonRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), skillRect.y,
                    skillRect.width - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), skillRect.height);
                if (Widgets.ButtonText(buttonRect, skillDef == null ? skill : skillDef.LabelCap.ToString()))
                {
                    Find.WindowStack.Add(new FloatMenu(DefDatabase<SkillDef>.AllDefs
                        .Where(def =>
                            !SelectedLoadout.PreferredSkills.Union(SelectedLoadout.UndesirableSkills)
                                .Contains(def.defName)).Select(def => new FloatMenuOption(def.LabelCap, () =>
                        {
                            _ = SelectedLoadout.PreferredSkills.Add(def.defName);
                            _ = SelectedLoadout.PreferredSkills.Remove(skill);
                        })).ToList()));
                }
                index++;
            }
            var newSkill = DefDatabase<SkillDef>.AllDefs.FirstOrDefault(def =>
                !SelectedLoadout.PreferredSkills.Union(SelectedLoadout.UndesirableSkills).Contains(def.defName));
            if (newSkill != null)
            {
                var addButtonRect = GetLabeledButtonListItemRect(skillsRect, index);
                if (Widgets.ButtonText(addButtonRect, Resources.Strings.Add))
                {
                    _ = SelectedLoadout.PreferredSkills.Add(newSkill.defName);
                }
            }
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

        private void DoRules(Rect rect)
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

        private void DoUndesirableSkills(Rect rect)
        {
            var skillsRect =
                LabelInput.DoLabeledRect(rect, Strings.UndesirableSkillsLabel, Strings.UndesirableSkillsTooltip);
            var index = 0;
            foreach (var skill in SelectedLoadout.UndesirableSkills)
            {
                var skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
                var skillRect = GetLabeledButtonListItemRect(skillsRect, index);
                var deleteButtonRect =
                    new Rect(skillRect.x, skillRect.y, skillRect.height, skillRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    _ = SelectedLoadout.UndesirableSkills.Remove(skill);
                    break;
                }
                var buttonRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), skillRect.y,
                    skillRect.width - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), skillRect.height);
                if (Widgets.ButtonText(buttonRect, skillDef == null ? skill : skillDef.LabelCap.ToString()))
                {
                    Find.WindowStack.Add(new FloatMenu(DefDatabase<SkillDef>.AllDefs
                        .Where(def =>
                            !SelectedLoadout.PreferredSkills.Union(SelectedLoadout.UndesirableSkills)
                                .Contains(def.defName)).Select(def => new FloatMenuOption(def.LabelCap, () =>
                        {
                            _ = SelectedLoadout.UndesirableSkills.Add(def.defName);
                            _ = SelectedLoadout.UndesirableSkills.Remove(skill);
                        })).ToList()));
                }
                index++;
            }
            var newSkill = DefDatabase<SkillDef>.AllDefs.FirstOrDefault(def =>
                !SelectedLoadout.PreferredSkills.Union(SelectedLoadout.UndesirableSkills).Contains(def.defName));
            if (newSkill != null)
            {
                var addButtonRect = GetLabeledButtonListItemRect(skillsRect, index);
                if (Widgets.ButtonText(addButtonRect, Resources.Strings.Add))
                {
                    _ = SelectedLoadout.UndesirableSkills.Add(newSkill.defName);
                }
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
                const int settingsRowCount = 2;
                const float settingsHeight = (UiHelpers.ListRowHeight * settingsRowCount) +
                    (UiHelpers.ElementGap * (settingsRowCount - 1));
                var rangedSidearmsRowCount =
                    (int) Math.Ceiling((SelectedLoadout.RangedSidearmRules.Count + 1f) / LabeledButtonListColumnCount);
                var meleeSidearmsRowCount =
                    (int) Math.Ceiling((SelectedLoadout.MeleeSidearmRules.Count + 1f) / LabeledButtonListColumnCount);
                var rulesHeight = sectionHeaderHeight //header
                    + UiHelpers.ListRowHeight //primary weapon rule
                    + UiHelpers.ElementGap + (UiHelpers.ButtonHeight * rangedSidearmsRowCount) +
                    (UiHelpers.ButtonGap * (rangedSidearmsRowCount - 1)) //ranged sidearms
                    + UiHelpers.ElementGap + (UiHelpers.ButtonHeight * meleeSidearmsRowCount) +
                    (UiHelpers.ButtonGap * (meleeSidearmsRowCount - 1)) //melee sidearms
                    + UiHelpers.ElementGap + UiHelpers.ListRowHeight; //tool rule
                var pawnTraitsRowCount =
                    (int) Math.Ceiling((SelectedLoadout.PawnTraits.Count + 1f) / PawnSettingsColumnCount);
                var pawnTraitsHeight = sectionHeaderHeight + (UiHelpers.ListRowHeight * pawnTraitsRowCount) +
                    (UiHelpers.ElementGap * (pawnTraitsRowCount - 1));
                var pawnCapacitiesRowCount =
                    (int) Math.Ceiling((SelectedLoadout.PawnCapacities.Count + 1f) / PawnSettingsColumnCount);
                var pawnCapacitiesHeight = sectionHeaderHeight + (UiHelpers.ListRowHeight * pawnCapacitiesRowCount) +
                    (UiHelpers.ElementGap * (pawnCapacitiesRowCount - 1));
                var preferredSkillsRowCount = (int) Math.Ceiling(
                    (SelectedLoadout.PreferredSkills.Count + 1f) / LabeledButtonListColumnCount);
                var undesirableSkillsRowCount = (int) Math.Ceiling(
                    (SelectedLoadout.UndesirableSkills.Count + 1f) / LabeledButtonListColumnCount);
                var pawnSkillsHeight = sectionHeaderHeight //header
                    + (UiHelpers.ButtonHeight * preferredSkillsRowCount) +
                    (UiHelpers.ButtonGap * (preferredSkillsRowCount - 1)) //preferred skills
                    + UiHelpers.ElementGap + (UiHelpers.ButtonHeight * undesirableSkillsRowCount) +
                    (UiHelpers.ButtonGap * (undesirableSkillsRowCount - 1)); //undesirable skills
                var scrollViewContentHeight = settingsHeight + UiHelpers.ElementGap + rulesHeight +
                    UiHelpers.ElementGap + pawnTraitsHeight + UiHelpers.ElementGap + pawnCapacitiesHeight +
                    UiHelpers.ElementGap + pawnSkillsHeight;
                var scrollViewRect = new Rect(outerRect.x, outerRect.y,
                    outerRect.width - GUI.skin.verticalScrollbar.fixedWidth - 4f, scrollViewContentHeight);
                Widgets.BeginScrollView(outerRect, ref _scrollPosition, scrollViewRect);
                var settingsRect = new Rect(scrollViewRect.x, scrollViewRect.y, scrollViewRect.width, settingsHeight);
                DoLoadoutSettings(settingsRect);
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, settingsRect.yMax, scrollViewRect.width,
                    UiHelpers.ElementGap));
                var rulesRect = new Rect(scrollViewRect.x, settingsRect.yMax + UiHelpers.ElementGap,
                    scrollViewRect.width, rulesHeight);
                DoRules(rulesRect);
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, rulesRect.yMax, scrollViewRect.width,
                    UiHelpers.ElementGap));
                var pawnTraitsRect = new Rect(scrollViewRect.x, rulesRect.yMax + UiHelpers.ElementGap,
                    scrollViewRect.width, pawnTraitsHeight);
                DoPawnTraits(pawnTraitsRect);
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, pawnTraitsRect.yMax, scrollViewRect.width,
                    UiHelpers.ElementGap));
                var pawnCapacitiesRect = new Rect(scrollViewRect.x, pawnTraitsRect.yMax + UiHelpers.ElementGap,
                    scrollViewRect.width, pawnCapacitiesHeight);
                DoPawnCapacities(pawnCapacitiesRect);
                UiHelpers.DoGapLineHorizontal(new Rect(scrollViewRect.x, pawnCapacitiesRect.yMax, scrollViewRect.width,
                    UiHelpers.ElementGap));
                var pawnSkillsRect = new Rect(scrollViewRect.x, pawnCapacitiesRect.yMax + UiHelpers.ElementGap,
                    scrollViewRect.width, pawnCapacitiesHeight);
                DoPawnSkills(pawnSkillsRect);
                Widgets.EndScrollView();
                UiHelpers.DoGapLineHorizontal(new Rect(inRect.x, outerRect.yMax, inRect.width, UiHelpers.ElementGap));
                DoAvailablePawns(availablePawnsRect);
            }
        }

        private static Rect GetLabeledButtonListItemRect(Rect rect, int index)
        {
            var rowIndex = Math.DivRem(index, LabeledButtonListColumnCount, out var columnIndex);
            var columnWidth =
                ((-1 * LabeledButtonListColumnCount * UiHelpers.ElementGap) + UiHelpers.ElementGap + rect.width) /
                LabeledButtonListColumnCount;
            return new Rect(rect.x + ((columnWidth + UiHelpers.ElementGap) * columnIndex),
                rect.y + ((UiHelpers.ButtonHeight + UiHelpers.ButtonGap) * rowIndex), columnWidth,
                UiHelpers.ButtonHeight);
        }

        private static Rect GetPawnSettingRect(Rect rect, int index)
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