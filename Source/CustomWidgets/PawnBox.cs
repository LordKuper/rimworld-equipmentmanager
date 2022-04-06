using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EquipmentManager.CustomWidgets
{
    public static class PawnBox
    {
        public static void DoPawnBox(Rect rect, Color backgroundColor, Color outlineColor, int columnCount,
            float entryGap, ref Vector2 scrollPosition, IReadOnlyList<Pawn> pawns)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            var horizontalMargin = GUI.skin.verticalScrollbar.fixedWidth + (entryGap * 2);
            var entryWidth = ((-1 * columnCount * entryGap) + entryGap + (rect.width - horizontalMargin)) / columnCount;
            var rowCount = (int) Math.Ceiling((double) pawns.Count / columnCount);
            Widgets.DrawBoxSolidWithOutline(rect, backgroundColor, outlineColor);
            var outRect = new Rect(rect.x + entryGap, rect.y + entryGap, rect.width - (entryGap * 1.5f),
                rect.height - (entryGap * 2));
            var boxRect = new Rect(outRect.x, outRect.y, rect.width - horizontalMargin,
                (Text.LineHeight * rowCount) + (entryGap * (rowCount - 1)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, boxRect);
            for (var i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                var rowIndex = Math.DivRem(i, columnCount, out var columnIndex);
                var entryRect = new Rect(boxRect.x + ((entryWidth + entryGap) * columnIndex),
                    boxRect.y + ((Text.LineHeight + entryGap) * rowIndex), entryWidth,
                    Text.LineHeightOf(GameFont.Small));
                GUI.color = !Mouse.IsOver(entryRect) ? Color.white : GenUI.MouseoverColor;
                _ = Widgets.LabelFit(entryRect, pawn.LabelCap);
                MouseoverSounds.DoRegion(entryRect);
                TooltipHandler.TipRegion(entryRect, pawn.NameFullColored);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(entryRect))
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(pawn));
                }
            }
            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;
        }
    }
}