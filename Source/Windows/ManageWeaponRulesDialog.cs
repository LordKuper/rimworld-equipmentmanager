using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EquipmentManager.CustomWidgets;
using RimWorld;
using UnityEngine;
using Verse;
using Strings = EquipmentManager.Resources.Strings.WeaponRules;

namespace EquipmentManager.Windows
{
    internal partial class ManageWeaponRulesDialog : Window
    {
        private const float ItemIconGap = 4f;
        private const float ItemIconSize = 32f;
        private static Vector2 _blacklistScrollPosition;
        private static Vector2 _currentItemsScrollPosition;
        private static DialogTab _currentTab = DialogTab.MeleeWeapons;
        private static EquipmentManagerGameComponent _equipmentManager;
        private static Vector2 _globalItemsScrollPosition;
        private static Vector2 _statLimitsScrollPosition;
        private static Vector2 _statWeightsScrollPosition;
        private static Vector2 _whitelistScrollPosition;
        private readonly List<TabRecord> _tabs = new List<TabRecord>();
        private bool _initialized;

        public ManageWeaponRulesDialog()
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        private int AvailableItemIconsRowCount => InitialSize.y < MaxSize.y ? 2 : 5;

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        private int ExclusiveItemIconsRowCount => InitialSize.y < MaxSize.y ? 2 : 3;
        public override Vector2 InitialSize => UiHelpers.GetWindowSize(new Vector2(850f, 650f), MaxSize);
        private static Vector2 MaxSize => new Vector2(1000f, 1000f);

        private static void CheckSelectedItemRuleHasName(ItemRule rule)
        {
            if (rule == null || !rule.Label.NullOrEmpty()) { return; }
            rule.Label = $"{rule.Id}";
        }

        private static void DoAvailableItems(Rect rect, IReadOnlyList<ThingDef> globalItems,
            Action<ThingDef> globalItemRightClickAction, Func<ThingDef, string> globalTooltipGetter,
            IReadOnlyList<Thing> currentItems, Action<Thing> currentItemRightClickAction,
            Func<Thing, string> currentTooltipGetter, Action refreshAction)
        {
            var columnWidth = (rect.width - UiHelpers.ElementGap) / 2f;
            var globalRect = new Rect(rect.x, rect.y, columnWidth, rect.height);
            var gapRect = new Rect(globalRect.xMax, rect.y, UiHelpers.ElementGap, rect.height);
            var currentRect = new Rect(gapRect.xMax, rect.y, columnWidth, rect.height);
            DoGloballyAvailableItems(globalRect, globalItems, globalItemRightClickAction, refreshAction,
                globalTooltipGetter);
            UiHelpers.DoGapLineVertical(gapRect);
            DoCurrentlyAvailableItems(currentRect, currentItems, currentItemRightClickAction, refreshAction,
                currentTooltipGetter);
        }

        private static void DoBlacklist(Rect rect, IReadOnlyCollection<ThingDef> items, IEnumerable<ThingDef> allItems,
            Action<ThingDef> addAction, Action<ThingDef> rightClickAction, Func<ThingDef, string> tooltipGetter)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.BlacklistedItems);
            TooltipHandler.TipRegion(labelRect, Strings.BlacklistedItemsTooltip);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(allItems.Where(def => items.All(weight => weight != def))
                    .Select(def => new FloatMenuOption($"{def.LabelCap}", () => addAction(def))).ToList()));
            }
            Text.Font = font;
            Text.Anchor = anchor;
            ThingBox.DoThingDefBox(
                new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                    rect.yMax - (labelRect.yMax + UiHelpers.ElementGap)), new Color(1f, 0.5f, 0.5f, 0.05f),
                new Color(1f, 0.5f, 0.5f, 0.4f), ItemIconSize, ItemIconGap, ref _blacklistScrollPosition,
                items.ToList(), rightClickAction, tooltipGetter);
        }

        private static void DoCurrentlyAvailableItems(Rect rect, IReadOnlyList<Thing> items,
            Action<Thing> rightClickAction, Action refreshAction, Func<Thing, string> tooltipGetter)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.CurrentlyAvailableItems);
            TooltipHandler.TipRegion(labelRect, Strings.CurrentlyAvailableItemsTooltip);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Strings.Refresh)) { refreshAction(); }
            Text.Font = font;
            Text.Anchor = anchor;
            ThingBox.DoThingBox(
                new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                    rect.yMax - (labelRect.yMax + UiHelpers.ElementGap)), new Color(1f, 1f, 1f, 0.05f),
                new Color(1f, 1f, 1f, 0.4f), ItemIconSize, ItemIconGap, ref _currentItemsScrollPosition, items,
                rightClickAction, tooltipGetter);
        }

        private static void DoExclusiveItems(Rect rect, HashSet<ThingDef> allItems,
            IReadOnlyCollection<ThingDef> whitelistedItems, Action<ThingDef> whitelistedItemsRightClickAction,
            Action<ThingDef> addToWhitelistAction, IReadOnlyCollection<ThingDef> blacklistedItems,
            Action<ThingDef> blacklistedItemsRightClickAction, Action<ThingDef> addToBlacklistAction,
            Func<ThingDef, string> tooltipGetter)
        {
            var columnWidth = (rect.width - UiHelpers.ElementGap) / 2f;
            var whitelistRect = new Rect(rect.x, rect.y, columnWidth, rect.height);
            var gapRect = new Rect(whitelistRect.xMax, rect.y, UiHelpers.ElementGap, rect.height);
            var blacklistRect = new Rect(gapRect.xMax, rect.y, columnWidth, rect.height);
            DoWhitelist(whitelistRect, whitelistedItems, allItems, addToWhitelistAction,
                whitelistedItemsRightClickAction, tooltipGetter);
            UiHelpers.DoGapLineVertical(gapRect);
            DoBlacklist(blacklistRect, blacklistedItems, allItems, addToBlacklistAction,
                blacklistedItemsRightClickAction, tooltipGetter);
        }

        private static void DoGloballyAvailableItems(Rect rect, IReadOnlyList<ThingDef> items,
            Action<ThingDef> rightClickAction, Action refreshAction, Func<ThingDef, string> tooltipGetter)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.GloballyAvailableItems);
            TooltipHandler.TipRegion(labelRect, Strings.GloballyAvailableItemsTooltip);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Strings.Refresh)) { refreshAction(); }
            Text.Font = font;
            Text.Anchor = anchor;
            ThingBox.DoThingDefBox(
                new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                    rect.yMax - (labelRect.yMax + UiHelpers.ElementGap)), new Color(1f, 1f, 1f, 0.05f),
                new Color(1f, 1f, 1f, 0.4f), ItemIconSize, ItemIconGap, ref _globalItemsScrollPosition, items,
                rightClickAction, tooltipGetter);
        }

        private static void DoRuleSetting(Rect settingRect, Func<bool?> getter, Action<bool?> setter, string label,
            string tooltip)
        {
            var checkboxRect = new Rect(settingRect.x, settingRect.y, settingRect.height, settingRect.height);
            var state = UiHelpers.GetSettingCheckboxState(getter());
            var newState = Widgets.CheckboxMulti(checkboxRect, state);
            if (newState != state) { setter(UiHelpers.CycleSettingValue(state)); }
            var labelRect = new Rect(checkboxRect.xMax + (UiHelpers.ElementGap / 2f), settingRect.y,
                settingRect.width - checkboxRect.width - (UiHelpers.ElementGap / 2f), settingRect.height);
            TooltipHandler.TipRegion(labelRect, tooltip);
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = anchor;
        }

        private static void DoRuleStatLimits(Rect rect, IEnumerable<StatDef> statDefs,
            IReadOnlyList<StatLimit> statLimits, Action<StatDef> addAction, Action<string> deleteAction)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.StatLimits);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(statDefs
                    .Where(def => statLimits.All(weight => weight.StatDefName != def.defName)).Select(statDef =>
                        new FloatMenuOption($"{statDef.LabelCap} [{statDef.category?.LabelCap ?? "No category"}]",
                            () => addAction(statDef))).ToList()));
            }
            var listRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                rect.yMax - labelRect.yMax - UiHelpers.ElementGap);
            var scrollViewRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap,
                rect.width - GUI.skin.verticalScrollbar.fixedWidth - 4f, UiHelpers.ListRowHeight * statLimits.Count);
            Widgets.BeginScrollView(listRect, ref _statLimitsScrollPosition, scrollViewRect);
            for (var i = 0; i < statLimits.Count; i++)
            {
                var statLimit = statLimits[i];
                var rowRect = new Rect(scrollViewRect.x, scrollViewRect.y + (UiHelpers.ListRowHeight * i),
                    scrollViewRect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                {
                    deleteAction(statLimit.StatDefName);
                    break;
                }
                var statLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (statLimit.StatDef != null && !statLimit.StatDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(statLabelRect, statLimit.StatDef?.description);
                }
                _ = Widgets.LabelFit(statLabelRect, statLimit.StatDef?.LabelCap ?? statLimit.StatDefName);
                var statInputRect = new Rect(statLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - statLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                var limitInputWidth = (statInputRect.width - (UiHelpers.ElementGap * 3)) / 2f;
                var minValueRect = new Rect(statInputRect.x, statInputRect.y, limitInputWidth, statInputRect.height);
                statLimit.MinValueBuffer = Widgets.TextField(minValueRect, statLimit.MinValueBuffer, 10);
                statLimit.MinValue = ParseStatLimit(ref statLimit.MinValueBuffer);
                var dashRect = new Rect(minValueRect.xMax, statInputRect.y, UiHelpers.ElementGap * 3,
                    statInputRect.height);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(dashRect, "-");
                Text.Anchor = TextAnchor.UpperLeft;
                var maxValueRect = new Rect(dashRect.xMax, statInputRect.y, limitInputWidth, statInputRect.height);
                statLimit.MaxValueBuffer = Widgets.TextField(maxValueRect, statLimit.MaxValueBuffer, 10);
                statLimit.MaxValue = ParseStatLimit(ref statLimit.MaxValueBuffer);
            }
            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;
        }

        private static void DoRuleStats(Rect rect, IReadOnlyList<StatDef> statDefs,
            IReadOnlyList<StatWeight> statWeights, Action<StatDef> addWeightAction, Action<string> deleteWeightAction,
            IReadOnlyList<StatLimit> statLimits, Action<StatDef> addLimitAction, Action<string> deleteLimitAction)
        {
            var columnWidth = (rect.width - UiHelpers.ElementGap) / 2f;
            var weightsRect = new Rect(rect.x, rect.y, columnWidth, rect.height);
            var gapRect = new Rect(weightsRect.xMax, rect.y, UiHelpers.ElementGap, rect.height);
            var limitsRect = new Rect(gapRect.xMax, rect.y, columnWidth, rect.height);
            DoRuleStatWeights(weightsRect, statDefs, statWeights, addWeightAction, deleteWeightAction);
            UiHelpers.DoGapLineVertical(gapRect);
            DoRuleStatLimits(limitsRect, statDefs, statLimits, addLimitAction, deleteLimitAction);
        }

        private static void DoRuleStatWeights(Rect rect, IEnumerable<StatDef> statDefs,
            IReadOnlyList<StatWeight> statWeights, Action<StatDef> addAction, Action<string> deleteAction)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.StatWeights);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(statDefs
                    .Where(def => statWeights.All(weight => weight.StatDefName != def.defName)).Select(statDef =>
                        new FloatMenuOption($"{statDef.LabelCap} [{statDef.category?.LabelCap ?? "No category"}]",
                            () => addAction(statDef))).ToList()));
            }
            var listRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                rect.yMax - labelRect.yMax - UiHelpers.ElementGap);
            var scrollViewRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap,
                rect.width - GUI.skin.verticalScrollbar.fixedWidth - 4f, UiHelpers.ListRowHeight * statWeights.Count);
            Widgets.BeginScrollView(listRect, ref _statWeightsScrollPosition, scrollViewRect);
            for (var i = 0; i < statWeights.Count; i++)
            {
                var statWeight = statWeights[i];
                var rowRect = new Rect(scrollViewRect.x, scrollViewRect.y + (UiHelpers.ListRowHeight * i),
                    scrollViewRect.width, UiHelpers.ListRowHeight).ContractedBy(4f);
                var deleteButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.height, rowRect.height).ContractedBy(4f);
                if (!statWeight.Protected)
                {
                    if (Widgets.ButtonImageFitted(deleteButtonRect, Resources.Textures.Delete))
                    {
                        deleteAction(statWeight.StatDefName);
                        break;
                    }
                }
                var statLabelRect = new Rect(deleteButtonRect.xMax + (UiHelpers.ElementGap / 2f), rowRect.y,
                    (rowRect.width / 2f) - deleteButtonRect.width - (UiHelpers.ElementGap / 2f), rowRect.height);
                if (statWeight.StatDef != null && !statWeight.StatDef.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(statLabelRect, statWeight.StatDef?.description);
                }
                _ = Widgets.LabelFit(statLabelRect, statWeight.StatDef?.LabelCap ?? statWeight.StatDefName);
                var statInputRect = new Rect(statLabelRect.xMax + UiHelpers.ElementGap, rowRect.y,
                    rowRect.xMax - statLabelRect.xMax - UiHelpers.ElementGap, rowRect.height);
                statWeight.Weight = Widgets.HorizontalSlider(statInputRect, statWeight.Weight,
                    -1 * StatWeight.StatWeightCap, StatWeight.StatWeightCap, true, $"{statWeight.Weight:N1}",
                    roundTo: 0.1f);
            }
            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;
        }

        private static void DoWeaponRuleEquipMode(Rect rect, Func<ItemRule.WeaponEquipMode> getter,
            Action<ItemRule.WeaponEquipMode> setter)
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
            if (Widgets.ButtonText(inputRect, Strings.GetWeaponEquipModeLabel(getter())))
            {
                Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(ItemRule.WeaponEquipMode))
                    .OfType<ItemRule.WeaponEquipMode>().Select(mode =>
                        new FloatMenuOption(Strings.GetWeaponEquipModeLabel(mode), () => setter(mode))).ToList()));
            }
            Text.Font = font;
            Text.Anchor = anchor;
        }

        private static void DoWhitelist(Rect rect, IReadOnlyCollection<ThingDef> items, IEnumerable<ThingDef> allItems,
            Action<ThingDef> addAction, Action<ThingDef> rightClickAction, Func<ThingDef, string> tooltipGetter)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = new Rect(rect.x, rect.y, rect.width * 3f / 4f, Text.LineHeight);
            Widgets.Label(labelRect, Strings.WhitelistedItems);
            TooltipHandler.TipRegion(labelRect, Strings.WhitelistedItemsTooltip);
            Text.Font = GameFont.Small;
            var buttonRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, labelRect.height);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Add))
            {
                Find.WindowStack.Add(new FloatMenu(allItems.Where(def => items.All(weight => weight != def))
                    .Select(def => new FloatMenuOption($"{def.LabelCap}", () => addAction(def))).ToList()));
            }
            Text.Font = font;
            Text.Anchor = anchor;
            ThingBox.DoThingDefBox(
                new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                    rect.yMax - (labelRect.yMax + UiHelpers.ElementGap)), new Color(0.5f, 1f, 0.5f, 0.05f),
                new Color(0.5f, 1f, 0.5f, 0.4f), ItemIconSize, ItemIconGap, ref _whitelistScrollPosition,
                items.ToList(), rightClickAction, tooltipGetter);
        }

        public override void DoWindowContents(Rect inRect)
        {
            var tabDrawerRect = inRect;
            tabDrawerRect.yMin += 32f;
            _ = TabDrawer.DrawTabs(tabDrawerRect, _tabs, 500f);
            var activeTabRect = tabDrawerRect.ContractedBy(10);
            activeTabRect.xMax += 6f;
            switch (_currentTab)
            {
                case DialogTab.MeleeWeapons:
                    DoTab_MeleeWeapons(activeTabRect);
                    break;
                case DialogTab.RangedWeapons:
                    DoTab_RangedWeapons(activeTabRect);
                    break;
                case DialogTab.Tools:
                    DoTab_Tools(activeTabRect);
                    break;
                case DialogTab.WorkTypes:
                    DoTab_WorkTypes(activeTabRect);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GetWeaponRuleTabRects(Rect rect, int ruleSettingCount, int itemPropertiesCount,
            out Rect buttonRowRect, out Rect labelRect, out Rect equipModeRect, out Rect ruleSettingsRect,
            out Rect itemPropertiesRect, out Rect availableItemsRect, out Rect exclusiveItemsRect, out Rect statsRect)
        {
            var sectionHeaderHeight = Text.LineHeightOf(GameFont.Medium) + UiHelpers.ElementGap;
            buttonRowRect = new Rect(rect.x, rect.y, rect.width, UiHelpers.ButtonHeight);
            labelRect = new Rect(rect.x, buttonRowRect.yMax + UiHelpers.ElementGap,
                (rect.width - UiHelpers.ElementGap) / 2f, UiHelpers.LabelHeight);
            equipModeRect = new Rect(rect.center.x + (UiHelpers.ElementGap / 2f),
                buttonRowRect.yMax + UiHelpers.ElementGap, (rect.width - UiHelpers.ElementGap) / 2f,
                UiHelpers.LabelHeight);
            var itemPropertiesRowCount =
                (int) Math.Ceiling((double) itemPropertiesCount / UiHelpers.BoolSettingsColumnCount);
            if (ruleSettingCount != 0)
            {
                ruleSettingsRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                    sectionHeaderHeight + (UiHelpers.ListRowHeight * ruleSettingCount));
                itemPropertiesRect = new Rect(rect.x, ruleSettingsRect.yMax + UiHelpers.ElementGap, rect.width,
                    sectionHeaderHeight + (UiHelpers.ListRowHeight * itemPropertiesRowCount));
            }
            else
            {
                ruleSettingsRect = Rect.zero;
                itemPropertiesRect = new Rect(rect.x, labelRect.yMax + UiHelpers.ElementGap, rect.width,
                    sectionHeaderHeight + (UiHelpers.ListRowHeight * itemPropertiesRowCount));
            }
            var availableItemsBoxHeight = (ItemIconSize * AvailableItemIconsRowCount) +
                (ItemIconGap * (AvailableItemIconsRowCount + 1));
            availableItemsRect = new Rect(rect.x, rect.yMax - availableItemsBoxHeight - sectionHeaderHeight, rect.width,
                availableItemsBoxHeight + sectionHeaderHeight);
            var exclusiveItemsBoxHeight = (ItemIconSize * ExclusiveItemIconsRowCount) +
                (ItemIconGap * (ExclusiveItemIconsRowCount + 1));
            exclusiveItemsRect = new Rect(rect.x,
                availableItemsRect.y - exclusiveItemsBoxHeight - sectionHeaderHeight - UiHelpers.ElementGap, rect.width,
                exclusiveItemsBoxHeight + sectionHeaderHeight);
            statsRect = new Rect(rect.x, itemPropertiesRect.yMax + UiHelpers.ElementGap, rect.width,
                exclusiveItemsRect.y - UiHelpers.ElementGap - itemPropertiesRect.yMax - UiHelpers.ElementGap);
        }

        private void Initialize()
        {
            if (_initialized) { return; }
            _tabs.Add(new TabRecord(Strings.MeleeWeapons.Title, () =>
            {
                _currentTab = DialogTab.MeleeWeapons;
                UpdateAvailableItems_MeleeWeapons();
                ResetScrollPositions();
            }, () => _currentTab == DialogTab.MeleeWeapons));
            _tabs.Add(new TabRecord(Strings.RangedWeapons.Title, () =>
            {
                _currentTab = DialogTab.RangedWeapons;
                UpdateAvailableItems_RangedWeapons();
                ResetScrollPositions();
            }, () => _currentTab == DialogTab.RangedWeapons));
            _tabs.Add(new TabRecord(Strings.Tools.Title, () =>
            {
                _currentTab = DialogTab.Tools;
                UpdateAvailableItems_Tools();
                ResetScrollPositions();
            }, () => _currentTab == DialogTab.Tools));
            _tabs.Add(new TabRecord(Strings.WorkTypes.Title, () =>
            {
                _currentTab = DialogTab.WorkTypes;
                UpdateAvailableItems_WorkTypes();
                ResetScrollPositions();
            }, () => _currentTab == DialogTab.WorkTypes));
            _initialized = true;
        }

        private static float? ParseStatLimit(ref string buffer)
        {
            if (!float.TryParse(buffer, NumberStyles.Float, CultureInfo.InvariantCulture, out var limit))
            {
                return null;
            }
            var value = Mathf.Clamp(limit, -1 * StatLimit.StatLimitCap, StatLimit.StatLimitCap);
            buffer = $"{value:N2}";
            return value;
        }

        public override void PreClose()
        {
            base.PreClose();
            PreClose_MeleeWeapons();
            PreClose_RangedWeapons();
            PreClose_Tools();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            if (!_initialized) { Initialize(); }
            UpdateAvailableItems_MeleeWeapons();
            UpdateAvailableItems_RangedWeapons();
            UpdateAvailableItems_Tools();
            UpdateAvailableItems_WorkTypes();
        }

        private static void ResetScrollPositions()
        {
            _statWeightsScrollPosition.Set(0, 0);
            _statLimitsScrollPosition.Set(0, 0);
            _globalItemsScrollPosition.Set(0, 0);
            _currentItemsScrollPosition.Set(0, 0);
            _blacklistScrollPosition.Set(0, 0);
            _whitelistScrollPosition.Set(0, 0);
        }

        private enum DialogTab
        {
            MeleeWeapons,
            RangedWeapons,
            Tools,
            WorkTypes
        }
    }
}