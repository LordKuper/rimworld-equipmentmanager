using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace EquipmentManager.CustomWidgets;

internal static class UiHelpers
{
    public const float ActionButtonWidth = 200f;
    public const int BoolSettingsColumnCount = 2;
    public const float ButtonGap = 8f;
    public const float ButtonHeight = 32f;
    public const float ElementGap = 12f;
    public const float ListRowHeight = 32f;
    public static readonly Regex ValidNameRegex = new(@"^[\p{L}0-9 '\-]*$");
    public static float LabelHeight => Text.LineHeightOf(GameFont.Medium) + 4f;

    public static bool? CycleSettingValue(MultiCheckboxState state)
    {
        switch (state)
        {
            case MultiCheckboxState.On:
                return false;
            case MultiCheckboxState.Off:
                return null;
            case MultiCheckboxState.Partial:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public static void DoGapLineHorizontal(Rect rect)
    {
        var color = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.4f);
        Widgets.DrawLineHorizontal(rect.x, rect.center.y, rect.width);
        GUI.color = color;
    }

    public static void DoGapLineVertical(Rect rect)
    {
        var color = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.4f);
        Widgets.DrawLineVertical(rect.center.x, rect.y, rect.height);
        GUI.color = color;
    }

    public static Rect DoLabeledRect(Rect rect, string label, [CanBeNull] string tooltip = null,
        float labelWidthFactor = 0.25f)
    {
        var anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleRight;
        var labelRect = new Rect(rect.x, rect.y, rect.width * labelWidthFactor, rect.height);
        Widgets.Label(labelRect, label);
        Text.Anchor = anchor;
        if (!string.IsNullOrEmpty(tooltip)) { TooltipHandler.TipRegion(labelRect, tooltip); }
        return new Rect(labelRect.xMax + ElementGap, rect.y,
            rect.width - labelRect.width - ElementGap, rect.height);
    }

    public static void DoLabeledText(Rect rect, string label, [CanBeNull] string value,
        float labelWidthFactor = 0.25f)
    {
        var font = Text.Font;
        var anchor = Text.Anchor;
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleRight;
        var labelRect = new Rect(rect.x, rect.y, rect.width * labelWidthFactor, rect.height);
        Widgets.Label(labelRect, label);
        Text.Anchor = TextAnchor.MiddleLeft;
        var valueRect = new Rect(labelRect.xMax + ElementGap, rect.y,
            rect.width - labelRect.width - ElementGap, rect.height);
        Widgets.Label(valueRect, value ?? string.Empty);
        Text.Font = font;
        Text.Anchor = anchor;
    }

    public static Rect GetBoolSettingRect(Rect rect, int index, float columnWidth)
    {
        var rowIndex = Math.DivRem(index, BoolSettingsColumnCount, out var columnIndex);
        return new Rect(rect.x + (columnWidth + ElementGap) * columnIndex,
            rect.y + ListRowHeight * rowIndex, columnWidth, ListRowHeight).ContractedBy(4f);
    }

    public static MultiCheckboxState GetSettingCheckboxState(bool? value)
    {
        return value switch
        {
            null => MultiCheckboxState.Partial,
            false => MultiCheckboxState.Off,
            _ => MultiCheckboxState.On
        };
    }

    public static Vector2 GetWindowSize(Vector2 minSize, Vector2 maxSize)
    {
        var width = Mathf.Clamp(Prefs.ScreenWidth / Prefs.UIScale * 0.9f, minSize.x, maxSize.x);
        var height = Mathf.Clamp(Prefs.ScreenHeight / Prefs.UIScale * 0.9f, minSize.y, maxSize.y);
        return new Vector2(width, height);
    }
}