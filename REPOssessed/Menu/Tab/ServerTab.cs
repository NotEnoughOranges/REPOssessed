using REPOssessed.Cheats.Core;
using REPOssessed.Cheats.ServerTab;
using REPOssessed.Handler;
using REPOssessed.Manager;
using REPOssessed.Menu.Core;
using REPOssessed.Util;
using System.Linq;
using UnityEngine;

namespace REPOssessed.Menu.Tab
{
    internal class ServerTab : MenuTab
    {
        public ServerTab() : base("ServerTab.Title") { }
        private Vector2 scrollPos = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        private Vector2 scrollPos3 = Vector2.zero;
        private Vector2 scrollPos4 = Vector2.zero;
        private string currency = "3000";

        public override void Draw()
        {
            GUILayout.BeginVertical();
            ServerMenuContent();
            ExtractionContent();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            ManagersContent();
            InfoMenuContent();
            GUILayout.EndVertical();
        }

        private void ServerMenuContent()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(ref scrollPos, () =>
            {
                UI.Label("ServerTab.ServerCheats", null, true, -1, true);
                UI.HostOnly(() => UI.Textbox(["ServerTab.SetCurrency", "General.HostTag"], ref currency, @"[^0-9]", 10, new UIButton("General.Set", () =>
                {
                    SetCurrency.Currency = int.Parse(currency) / 1000;
                    Cheat.Instance<SetCurrency>().Execute();
                })));
                UI.HostOnly(() => UI.Button(["ServerTab.BreakAllObjects", "General.HostTag"], () => GameObjectManager.items.Select(i => i?.Handle()).Where(h => h != null).ToList().ForEach(h => h?.Break(false))));
                UI.HostOnly(() => UI.Button(["ServerTab.ForceThiefPunishment", "General.HostTag"], () => Cheat.Instance<ForceThiefPunishment>().Execute()));
            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - HackMenu.Instance.spaceFromLeft));
        }


        private void ExtractionContent()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(ref scrollPos2, () =>
            {
                UI.Label("ServerTab.Extractions", null, true, -1, true);
                UI.HostOnly(() => UI.Button(["ServerTab.CompleteAllExtractions", "General.HostTag"], () => GameObjectManager.extractions.Select(e => e?.Handle()).ToList().ForEach(h => h?.CompleteExtraction())));
                ExtractionsCompleteContent();

            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - HackMenu.Instance.spaceFromLeft));
        }

        private void InfoMenuContent()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(ref scrollPos3, () =>
            {
                UI.Label("ServerTab.InfoDisplay", null, true, -1, true);
                UI.Checkbox("ServerTab.ToggleInfoDisplay", Cheat.Instance<InfoDisplay>());
                UI.Button("ServerTab.ToggleAllInfoDisplays", InfoDisplay.ToggleAll);
                UI.Checkbox("ServerTab.DisplayMapObjects", ref Settings.b_DisplayMapObjects);
                UI.Checkbox("ServerTab.DisplayDeathHeads", ref Settings.b_DisplayDeathHeads);
                UI.Checkbox("ServerTab.DisplayPlayers", ref Settings.b_DisplayPlayers);
                UI.Checkbox("ServerTab.DisplayEnemies", ref Settings.b_DisplayEnemies);
            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - HackMenu.Instance.spaceFromLeft));
        }

        private void ManagersContent()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(ref scrollPos4, () =>
            {
                UI.Label("ServerTab.Managers", null, true, -1, true);
                UI.Toggle("LootManager.Title", ref HackMenu.Instance.LootManagerWindow.isOpen, "General.Open", "General.Close");
                UI.Toggle("ItemManager.Title", ref HackMenu.Instance.ItemManagerWindow.isOpen, "General.Open", "General.Close");
                UI.Toggle("LevelManager.Title", ref HackMenu.Instance.LevelManagerWindow.isOpen, "General.Open", "General.Close");
            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.5f - HackMenu.Instance.spaceFromLeft));
        }

        private void ExtractionsCompleteContent()
        {
            int Index = 1;
            string extractionTranslation = TranslationUtil.Translate("General.Extraction");
            string masterClientTag = TranslationUtil.Translate("General.HostTag");
            GameObjectManager.extractions.Select(e => e?.Handle()).Where(e => e != null && !e.IsShop() && e.GetCurrentState() != ExtractionPoint.State.Complete).ToList().ForEach(h => UI.HostOnly(() => UI.Button($"{extractionTranslation} {Index++} {masterClientTag}", () => h?.CompleteExtraction(), "General.Complete")));
        }
    }
}
