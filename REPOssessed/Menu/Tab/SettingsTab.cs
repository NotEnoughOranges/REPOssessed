using REPOssessed.Cheats.Core;
using REPOssessed.Cheats.SettingsTab;
using REPOssessed.Menu.Core;
using REPOssessed.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOssessed.Menu.Tab
{
    internal class SettingsTab : MenuTab
    {
        public SettingsTab() : base("SettingsTab.Title") { }

        private Vector2 scrollPos = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        private Vector2 scrollPos3 = Vector2.zero;

        private int i_languageIndex = -1;
        private int i_themeIndex = -1;
        private bool settingsValuesInitialized;

        private string s_kbSearch = "";

        private string s_primaryColor = Settings.c_primary?.GetHexCode() ?? "";
        private string s_menuText = Settings.c_menuText?.GetHexCode() ?? "";
        private string s_espPlayer = Settings.c_espPlayer?.GetHexCode() ?? "";
        private string s_espItem = Settings.c_espItem?.GetHexCode() ?? "";
        private string s_espEnemy = Settings.c_espEnemy?.GetHexCode() ?? "";
        private string s_espCart = Settings.c_espCart?.GetHexCode() ?? "";
        private string s_espExtraction = Settings.c_espExtraction?.GetHexCode() ?? "";
        private string s_espDeathHead = Settings.c_espDeathHead?.GetHexCode() ?? "";
        private string s_espTruck = Settings.c_espTruck?.GetHexCode() ?? "";

        private string s_lootTierColors = string.Join(",", Array.ConvertAll(Settings.c_valuableValueColors, x => x.GetHexCode()));
        private string s_lootTiers = string.Join(",", Settings.i_valuableValueThresholds);

        public override void Draw()
        {
            if (!settingsValuesInitialized) RefreshSettingsUI();
            if (i_languageIndex == -1) i_languageIndex = Array.IndexOf(TranslationUtil.GetLanguages(), Settings.Language);
            if (i_themeIndex == -1) i_themeIndex = Array.IndexOf(ThemeUtil.GetThemes(), ThemeUtil.Name);
            MenuContent();
            KeybindContent();
        }

        private void RefreshSettingsUI()
        {
            i_languageIndex = Array.IndexOf(TranslationUtil.GetLanguages(), Settings.Language);
            i_themeIndex = Array.IndexOf(ThemeUtil.GetThemes(), ThemeUtil.Name);
            s_primaryColor = Settings.c_primary?.GetHexCode() ?? "";
            s_menuText = Settings.c_menuText?.GetHexCode() ?? "";
            s_espPlayer = Settings.c_espPlayer?.GetHexCode() ?? "";
            s_espItem = Settings.c_espItem?.GetHexCode() ?? "";
            s_espEnemy = Settings.c_espEnemy?.GetHexCode() ?? "";
            s_espCart = Settings.c_espCart?.GetHexCode() ?? "";
            s_espExtraction = Settings.c_espExtraction?.GetHexCode() ?? "";
            s_espDeathHead = Settings.c_espDeathHead?.GetHexCode() ?? "";
            s_espTruck = Settings.c_espTruck?.GetHexCode() ?? "";
            s_lootTierColors = string.Join(",", Array.ConvertAll(Settings.c_valuableValueColors, x => x.GetHexCode()));
            s_lootTiers = string.Join(",", Settings.i_valuableValueThresholds);
            settingsValuesInitialized = true;
        }

        private void ResetSettings()
        {
            Settings.Config.RegenerateConfig();
            RefreshSettingsUI();
        }

        private void ReloadSettings()
        {
            Settings.Config.LoadConfig();
            RefreshSettingsUI();
        }

        private void MenuContent()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(ref scrollPos, () =>
            {
                UI.Label("SettingsTab.Title", null, true, -1, true);

                UI.HorizontalGroup(() =>
                {
                    UI.Button("SettingsTab.ResetSettings", ResetSettings, "");
                    UI.Button("SettingsTab.SaveSettings", () => Settings.Config.SaveConfig(), "");
                    UI.Button("SettingsTab.ReloadSettings", ReloadSettings, "");
                });
                UI.Button("SettingsTab.OpenSettings", () => Settings.Config.OpenConfig(), "");

                UI.Label("SettingsTab.GeneralSettings", null, true, -1, true);

                UI.Select("SettingsTab.Theme", ref i_themeIndex, ThemeUtil.GetThemes().Select(x => new UIOption(x, () => ThemeUtil.SetTheme(x))).ToArray());
                UI.Select("SettingsTab.Language", ref i_languageIndex, TranslationUtil.GetLanguages().Select(x => new UIOption(x, () => Settings.Language = x)).ToArray());

                UI.Checkbox("SettingsTab.ShowCredits", ref Settings.b_ShowCredits);
                UI.Checkbox("SettingsTab.FPSCounter", Cheat.Instance<FPSCounter>());
                UI.Checkbox("SettingsTab.DebugMode", Cheat.Instance<DebugMode>());

                UI.Label("SettingsTab.Colors", null, true, -1, true);
                UI.Textbox("SettingsTab.MenuText", ref s_menuText, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
                {
                    if (Settings.c_menuText != null) SetColor(ref Settings.c_menuText, s_menuText); 
                }));
                UI.Textbox("SettingsTab.Primary", ref s_primaryColor, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
                {
                    if (Settings.c_primary != null) SetColor(ref Settings.c_primary, s_primaryColor);
                }));
                UI.Textbox("SettingsTab.PlayerColor", ref s_espPlayer, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () => 
                { 
                    if (Settings.c_espPlayer != null) SetColor(ref Settings.c_espPlayer, s_espPlayer); 
                }));
                UI.Textbox("SettingsTab.ItemColor", ref s_espItem, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () => 
                { 
                    if (Settings.c_espItem != null) SetColor(ref Settings.c_espItem, s_espItem); 
                }));
                UI.Textbox("SettingsTab.EnemyColor", ref s_espEnemy, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () =>
                { 
                    if (Settings.c_espEnemy != null) SetColor(ref Settings.c_espEnemy, s_espEnemy);
                }));
                UI.Textbox("SettingsTab.CartColor", ref s_espCart, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () => 
                {
                    if (Settings.c_espCart != null) SetColor(ref Settings.c_espCart, s_espCart);
                }));
                UI.Textbox("SettingsTab.ExtractionColor", ref s_espExtraction, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () => 
                {
                    if (Settings.c_espExtraction != null) SetColor(ref Settings.c_espExtraction, s_espExtraction); 
                }));
                UI.Textbox("SettingsTab.DeathHeadColor", ref s_espDeathHead, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () => 
                { 
                    if (Settings.c_espDeathHead != null) SetColor(ref Settings.c_espDeathHead, s_espDeathHead);
                }));
                UI.Textbox("SettingsTab.TruckColor", ref s_espTruck, @"[^0-9A-Za-z]", 8, new UIButton("General.Set", () => 
                {
                    if (Settings.c_espTruck != null) SetColor(ref Settings.c_espTruck, s_espTruck); 
                }));
                UI.Button(["SettingsTab.TieredLootColors", $"({GetTiersColored()})"], () => EditTierColors(), "General.Set");
                UI.Textbox("SettingsTab.Tiers", ref s_lootTiers, @"[^0-9,]", 50);
                UI.Textbox("SettingsTab.TierColors", ref s_lootTierColors, @"[^0-9A-Za-z,]", 50);

            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.55f - HackMenu.Instance.spaceFromLeft));
        }

        // come backl
        private void KeybindContent()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(ref scrollPos3, async () =>
            {
                UI.Label("SettingsTab.Keybinds", null, true, -1, true);
                UI.Textbox("SettingsTab.Search", ref s_kbSearch, "", 50);
                List<Cheat> cheats = Cheat.instances.FindAll(c => !c.Hidden);
                foreach (Cheat cheat in cheats)
                {
                    if (!cheat.GetType().Name.ToLower().Contains(s_kbSearch.ToLower())) continue;
                    GUILayout.BeginHorizontal();
                    UI.Label(cheat.GetType().Name);
                    GUILayout.FlexibleSpace();
                    if (cheat.HasKeybind && GUILayout.Button("-")) cheat.keybind = KeyCode.None;
                    string btnText = cheat.WaitingForKeybind ? TranslationUtil.Translate("General.Waiting") : cheat.HasKeybind ? cheat.keybind.ToString() : TranslationUtil.Translate("General.None");
                    if (GUILayout.Button(btnText, GUILayout.Width(85)))
                    {
                        await KBUtil.BeginChangeKeybind(cheat);
                    }
                    GUILayout.EndHorizontal();
                }
            }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.45f - HackMenu.Instance.spaceFromLeft));
        }

        private void SetColor(ref RGBAColor color, string hexCode)
        {
            color = new RGBAColor(hexCode.PadRight(6, '0'));
        }

        private void EditTierColors()
        {
            int[] thresholds = Array.ConvertAll(s_lootTiers.Split(','), x => int.TryParse(x, out int i) ? i : 0);
            RGBAColor[] rgbaColors = Array.ConvertAll(s_lootTierColors.Split(','), x => new RGBAColor(x));
            if (thresholds.Length != rgbaColors.Length) return;
            Settings.i_valuableValueThresholds = thresholds;
            Settings.c_valuableValueColors = rgbaColors;
        }

        private string GetTiersColored()
        {
            return string.Join(",", Settings.i_valuableValueThresholds.Zip(Settings.c_valuableValueColors, (t, c) => c.AsString(t.ToString())));
        }
    }
}
