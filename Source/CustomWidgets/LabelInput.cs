using UnityEngine;
using Verse;

namespace EquipmentManager.CustomWidgets
{
    internal static class LabelInput
    {
        public static Rect DoLabeledRect(Rect rect, string label, string tooltip = null)
        {
            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;
            var labelRect = new Rect(rect.x, rect.y, rect.width / 4f, rect.height);
            Widgets.Label(labelRect, label);
            Text.Anchor = anchor;
            if (!tooltip.NullOrEmpty()) { TooltipHandler.TipRegion(labelRect, tooltip); }
            return new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, rect.height);
        }

        public static void DoLabelInput(Rect rect, string label, ref string inputValue)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleRight;
            var labelRect = new Rect(rect.x, rect.y, rect.width / 4f, rect.height);
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.MiddleLeft;
            var inputRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, rect.height);
            inputValue = Widgets.TextField(inputRect, inputValue, 30, UiHelpers.ValidNameRegex);
            Text.Font = font;
            Text.Anchor = anchor;
        }

        public static void DoLabelInputReadOnly(Rect rect, string label, string inputValue)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleRight;
            var labelRect = new Rect(rect.x, rect.y, rect.width / 4f, rect.height);
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.MiddleLeft;
            var inputRect = new Rect(labelRect.xMax + UiHelpers.ElementGap, rect.y,
                rect.width - labelRect.width - UiHelpers.ElementGap, rect.height);
            Widgets.Label(inputRect, inputValue);
            Text.Font = font;
            Text.Anchor = anchor;
        }

        public static void DoLabelWithoutInput(Rect rect, string label)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, label);
            Text.Font = font;
            Text.Anchor = anchor;
        }
    }
}