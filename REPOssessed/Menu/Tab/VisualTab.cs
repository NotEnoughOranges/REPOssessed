using REPOssessed.Cheats.Core;
using REPOssessed.Cheats.VisualTab;
using REPOssessed.Menu.Core;
using REPOssessed.Util;
using UnityEngine;

namespace REPOssessed.Menu.Tab
{
    internal class VisualTab : MenuTab
    {
        public VisualTab() : base("VisualTab.Title") { }
        private Vector2 scrollPos = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;

        public override void Draw()
        {
            VisualContent();
            ESPContent();
        }

        private void VisualContent()
        {
            if (HackMenu.Instance == null) return;

            UI.VerticalGroup(ref scrollPos, () =>
            {
                UI.Label("VisualTab.Title", null, true, -1, true);
                UI.ToggleSlider(Cheat.Instance<FOV>(), "VisualTab.FOV", FOV.Value.ToString("F2"), ref FOV.Value, 1, 140);

            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - HackMenu.Instance.spaceFromLeft));
        }

        private void ESPContent()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(ref scrollPos2, () =>
            {
                UI.Label("VisualTab.ESP", null, true, -1, true);
                UI.ToggleSlider(Cheat.Instance<ESP>(), "VisualTab.ToggleESP", ESP.Value.ToString("F1"), ref ESP.Value, 0f, 5000f);
                UI.Button("VisualTab.ToggleAllESP", ESP.ToggleAll);
                UI.Checkbox("VisualTab.UseExternalESP", ref Settings.b_UseExternalESP);
                UI.Checkbox("VisualTab.UseValuableTiers", ref Settings.b_useValuableTiers);
                UI.Checkbox("VisualTab.PlayerESP", ref Settings.b_PlayerESP);
                UI.Checkbox("VisualTab.EnemyESP", ref Settings.b_EnemyESP);
                UI.Checkbox("VisualTab.ItemESP", ref Settings.b_ItemESP);
                UI.Checkbox("VisualTab.CartESP", ref Settings.b_CartESP);
                UI.Checkbox("VisualTab.ExtractionESP", ref Settings.b_ExtractionESP);
                UI.Checkbox("VisualTab.DeathHeadESP", ref Settings.b_DeathHeadESP);
                UI.Checkbox("VisualTab.TruckESP", ref Settings.b_TruckESP);
            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - HackMenu.Instance.spaceFromLeft));
        }
    }
}