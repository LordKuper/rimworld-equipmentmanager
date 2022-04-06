using System.Collections.Generic;
using System.Linq;
using EquipmentManager.Windows;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EquipmentManager.PawnColumnWorkers
{
    [UsedImplicitly]
    internal class Loadout : PawnColumnWorker
    {
        private static EquipmentManagerGameComponent _equipmentManager;

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        private static IEnumerable<Widgets.DropdownMenuElement<EquipmentManager.Loadout>> Button_GenerateMenu(Pawn pawn)
        {
            return new[]
            {
                new Widgets.DropdownMenuElement<EquipmentManager.Loadout>
                {
                    option = new FloatMenuOption(Resources.Strings.Loadouts.AutoSelect,
                        () => EquipmentManager.SetPawnLoadout(pawn, EquipmentManager.GetLoadout(0), true))
                }
            }.Union(EquipmentManager.GetLoadouts().Select(currentLoadout =>
                new Widgets.DropdownMenuElement<EquipmentManager.Loadout>
                {
                    option = new FloatMenuOption(currentLoadout.Label,
                        () => EquipmentManager.SetPawnLoadout(pawn, currentLoadout, false)),
                    payload = currentLoadout
                }));
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return (EquipmentManager.GetPawnLoadout(a)?.LoadoutId ?? int.MinValue).CompareTo(
                EquipmentManager.GetPawnLoadout(b)?.LoadoutId ?? int.MinValue);
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            var loadoutButtonRect = new Rect(rect.x, rect.y + 2f, Mathf.FloorToInt((float) ((rect.width - 4.0) * 0.7)),
                rect.height - 4f);
            if (pawn.IsQuestLodger())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(loadoutButtonRect, "Unchangeable".Translate().Truncate(loadoutButtonRect.width));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                var pawnLoadout = EquipmentManager.GetPawnLoadout(pawn);
                var loadout = EquipmentManager.GetLoadout(pawnLoadout.LoadoutId);
                var label = pawnLoadout.Automatic
                    ? $"{loadout.Label} (*)".Truncate(loadoutButtonRect.width)
                    : loadout.Label;
                Widgets.Dropdown(loadoutButtonRect, pawn, p => EquipmentManager.GetLoadout(pawn), Button_GenerateMenu,
                    label, dragLabel: label, paintable: true);
            }
            var editButtonRect = new Rect(rect.x + loadoutButtonRect.width + 4f, rect.y + 2f,
                Mathf.FloorToInt((float) ((rect.width - 4.0) * 0.3)), rect.height - 4f);
            if (!pawn.IsQuestLodger() && Widgets.ButtonText(editButtonRect, "AssignTabEdit".Translate()))
            {
                Find.WindowStack.Add(new ManageLoadoutsDialog(EquipmentManager.GetLoadout(pawn)));
            }
        }

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            MouseoverSounds.DoRegion(rect);
            var buttonRect = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
            if (Widgets.ButtonText(buttonRect, Resources.Strings.Loadouts.ManageLoadouts))
            {
                Find.WindowStack.Add(new ManageLoadoutsDialog(null));
            }
        }

        public override int GetMinHeaderHeight(PawnTable table)
        {
            return Mathf.Max(base.GetMinHeaderHeight(table), 65);
        }

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
        }

        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(Mathf.CeilToInt(251f), GetMinWidth(table), GetMaxWidth(table));
        }
    }
}