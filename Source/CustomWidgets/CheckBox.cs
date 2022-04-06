using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EquipmentManager.CustomWidgets
{
    public static class CheckBox
    {
        private static void DoCheckbox(Rect rect, bool value, bool disabled, Texture2D texChecked = null,
            Texture2D texUnchecked = null)
        {
            var color = GUI.color;
            if (disabled) { GUI.color = Widgets.InactiveColor; }
            GUI.DrawTexture(rect, !value ? texUnchecked ?? Widgets.CheckboxOffTex : texChecked ?? Widgets.CheckboxOnTex,
                ScaleMode.ScaleToFit);
            if (disabled) { GUI.color = color; }
        }

        public static void DoCheckboxWithCallback(Rect rect, bool value, bool disabled, Action<bool> callback)
        {
            if (!disabled && Widgets.ButtonInvisible(rect))
            {
                value = !value;
                if (value) { SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(); }
                else { SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(); }
                callback(value);
            }
            DoCheckbox(rect, value, disabled);
        }
    }
}