using System;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EquipmentManager.CustomWidgets;

public static class CheckBox
{
    private static void DoCheckbox(Rect rect, bool value, bool disabled,
        [CanBeNull] Texture2D texChecked = null, [CanBeNull] Texture2D texUnchecked = null)
    {
        var color = GUI.color;
        if (disabled) { GUI.color = Widgets.InactiveColor; }
        GUI.DrawTexture(rect,
            !value ? texUnchecked ? texUnchecked : Widgets.CheckboxOffTex :
            texChecked ? texChecked : Widgets.CheckboxOnTex, ScaleMode.ScaleToFit);
        if (disabled) { GUI.color = color; }
    }

    public static void DoCheckboxWithCallback(Rect rect, bool value, bool disabled,
        Action<bool> callback)
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