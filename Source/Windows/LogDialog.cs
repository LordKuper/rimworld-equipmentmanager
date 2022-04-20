using System.Text;
using EquipmentManager.CustomWidgets;
using UnityEngine;
using Verse;

namespace EquipmentManager.Windows
{
    internal class LogDialog : Window
    {
        private static EquipmentManagerGameComponent _equipmentManager;
        private static Vector2 _scrollPosition;
        private float _listingViewHeight;

        public LogDialog()
        {
            resizeable = true;
            draggable = true;
            preventCameraMotion = false;
            forcePause = false;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = false;
        }

        private static EquipmentManagerGameComponent EquipmentManager =>
            _equipmentManager ?? (_equipmentManager = Current.Game.GetComponent<EquipmentManagerGameComponent>());

        public override Vector2 InitialSize =>
            UiHelpers.GetWindowSize(new Vector2(500f, 500f), new Vector2(1000f, 1000f));

        private static void CopyAllMessagesToClipboard()
        {
            var stringBuilder = new StringBuilder();
            foreach (var message in EquipmentManager.GetLog()) { _ = stringBuilder.AppendLine(message); }
            GUIUtility.systemCopyBuffer = stringBuilder.ToString();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            var widgetRow = new WidgetRow(0.0f, 0.0f, maxWidth: inRect.width);
            if (widgetRow.ButtonText("Copy to clipboard", "Copy all messages to the clipboard."))
            {
                CopyAllMessagesToClipboard();
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            var outRect = inRect;
            outRect.yMin += 26f;
            Widgets.DrawBoxSolidWithOutline(outRect, new Color(1f, 1f, 1f, 0.05f), new Color(1f, 1f, 1f, 0.4f));
            var viewRect = outRect.ContractedBy(UiHelpers.ElementGap / 2f);
            viewRect.width -= GUI.skin.verticalScrollbar.fixedWidth + UiHelpers.ElementGap;
            viewRect.height = _listingViewHeight + UiHelpers.ElementGap;
            var y = 0f;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            foreach (var message in EquipmentManager.GetLog())
            {
                var height = Text.CalcHeight(message, viewRect.width);
                var rect = new Rect(viewRect.x, viewRect.y + y, viewRect.width, height);
                Widgets.Label(rect, message);
                y += height;
            }
            if (Event.current.type == EventType.Layout) { _listingViewHeight = y; }
            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;
        }
    }
}