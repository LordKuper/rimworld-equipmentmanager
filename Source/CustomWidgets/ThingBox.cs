using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EquipmentManager.CustomWidgets
{
    public static class ThingBox
    {
        public static void DoThingBox(Rect rect, Color backgroundColor, Color outlineColor, float iconSize,
            float iconGap, ref Vector2 scrollPosition, IReadOnlyList<Thing> things, Action<Thing> rightClickAction,
            Func<Thing, string> tooltipGetter)
        {
            var horizontalMargin = GUI.skin.verticalScrollbar.fixedWidth + (iconGap * 2);
            var itemsPerRow = (int) Math.Floor((rect.width - horizontalMargin) / (iconSize + iconGap));
            var rowCount = (int) Math.Ceiling((double) things.Count / itemsPerRow);
            Widgets.DrawBoxSolidWithOutline(rect, backgroundColor, outlineColor);
            var outRect = new Rect(rect.x + iconGap, rect.y + iconGap, rect.width - (iconGap * 1.5f),
                rect.height - (iconGap * 2));
            var itemBoxRect = new Rect(outRect.x, outRect.y, rect.width - horizontalMargin,
                (iconSize * rowCount) + (iconGap * (rowCount - 1)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, itemBoxRect);
            for (var i = 0; i < things.Count; i++)
            {
                var thing = things[i];
                var thingRect = GetThingRect(itemBoxRect, iconSize, iconGap, itemsPerRow, i);
                GUI.color = !Mouse.IsOver(thingRect) ? Color.white : GenUI.MouseoverColor;
                var texture = thing.StyleDef != null && thing.StyleDef.UIIcon != null ? thing.StyleDef.UIIcon :
                    !thing.def.uiIconPath.NullOrEmpty() ? thing.def.uiIcon :
                    thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(thing.def.defaultPlacingRot).mainTexture;
                GUI.DrawTexture(thingRect, texture, ScaleMode.ScaleToFit);
                GUI.color = Color.white;
                MouseoverSounds.DoRegion(thingRect);
                TooltipHandler.TipRegion(thingRect, tooltipGetter(thing));
                if (Event.current.type == EventType.MouseDown && Mouse.IsOver(thingRect))
                {
                    switch (Event.current.button)
                    {
                        case 0:
                            Find.WindowStack.Add(new Dialog_InfoCard(thing));
                            break;
                        case 1:
                            rightClickAction(thing);
                            break;
                    }
                }
            }
            Widgets.EndScrollView();
        }

        public static void DoThingDefBox(Rect rect, Color backgroundColor, Color outlineColor, float iconSize,
            float iconGap, ref Vector2 scrollPosition, IReadOnlyList<ThingDef> things,
            Action<ThingDef> rightClickAction, Func<ThingDef, string> tooltipGetter)
        {
            var horizontalMargin = GUI.skin.verticalScrollbar.fixedWidth + (iconGap * 2);
            var itemsPerRow = (int) Math.Floor((rect.width - horizontalMargin) / (iconSize + iconGap));
            var rowCount = (int) Math.Ceiling((double) things.Count / itemsPerRow);
            Widgets.DrawBoxSolidWithOutline(rect, backgroundColor, outlineColor);
            var outRect = new Rect(rect.x + iconGap, rect.y + iconGap, rect.width - (iconGap * 1.5f),
                rect.height - (iconGap * 2));
            var itemBoxRect = new Rect(outRect.x, outRect.y, rect.width - horizontalMargin,
                (iconSize * rowCount) + (iconGap * (rowCount - 1)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, itemBoxRect);
            for (var i = 0; i < things.Count; i++)
            {
                var thingDef = things[i];
                var thingRect = GetThingRect(itemBoxRect, iconSize, iconGap, itemsPerRow, i);
                GUI.color = !Mouse.IsOver(thingRect) ? Color.white : GenUI.MouseoverColor;
                GUI.DrawTexture(thingRect, thingDef.uiIcon, ScaleMode.ScaleToFit);
                GUI.color = Color.white;
                MouseoverSounds.DoRegion(thingRect);
                TooltipHandler.TipRegion(thingRect, tooltipGetter(thingDef));
                if (Event.current.type == EventType.MouseDown && Mouse.IsOver(thingRect))
                {
                    switch (Event.current.button)
                    {
                        case 0:
                            Find.WindowStack.Add(new Dialog_InfoCard(thingDef));
                            break;
                        case 1:
                            rightClickAction(thingDef);
                            break;
                    }
                }
            }
            Widgets.EndScrollView();
        }

        private static Rect GetThingRect(Rect rect, float iconSize, float iconGap, int columnCount, int index)
        {
            var rowIndex = Math.DivRem(index, columnCount, out var columnIndex);
            return new Rect(rect.x + ((iconSize + iconGap) * columnIndex), rect.y + ((iconSize + iconGap) * rowIndex),
                iconSize, iconSize);
        }
    }
}