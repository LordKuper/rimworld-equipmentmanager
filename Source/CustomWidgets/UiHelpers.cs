using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace EquipmentManager.CustomWidgets
{
    internal static class UiHelpers
    {
        public const float ActionButtonWidth = 200f;
        public const int BoolSettingsColumnCount = 2;
        public const float ButtonGap = 8f;
        public const float ButtonHeight = 32f;
        public const float ElementGap = 12f;
        public const float ListRowHeight = 32f;
        public static readonly Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");
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

        public static Rect GetBoolSettingRect(Rect rect, int index, float columnWidth)
        {
            var rowIndex = Math.DivRem(index, BoolSettingsColumnCount, out var columnIndex);
            return new Rect(rect.x + ((columnWidth + ElementGap) * columnIndex), rect.y + (ListRowHeight * rowIndex),
                columnWidth, ListRowHeight).ContractedBy(4f);
        }

        public static MultiCheckboxState GetSettingCheckboxState(bool? value)
        {
            return value == null ? MultiCheckboxState.Partial :
                value == false ? MultiCheckboxState.Off : MultiCheckboxState.On;
        }
    }
}