using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace EquipmentManager.CustomWidgets;

public static class ButtonImageToggle
{
    public static void DoButtonImageToggle([NotNull] Func<bool> getter, Action<bool> setter,
        Rect rect, Texture2D enabledTexture, Texture2D disabledTexture)
    {
        if (Widgets.ButtonImage(rect, getter() ? enabledTexture : disabledTexture, Color.white,
                GenUI.MouseoverColor)) { setter(!getter()); }
    }
}