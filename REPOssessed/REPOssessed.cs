using HarmonyLib;
using REPOssessed.Cheats.Core;
using REPOssessed.Cheats.Executable;
using REPOssessed.Cheats.SettingsTab;
using REPOssessed.Manager;
using REPOssessed.Menu.Core;
using REPOssessed.Menu.Popup;
using REPOssessed.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace REPOssessed
{
    public class REPOssessed : MonoBehaviour
    {
        private List<ToggleCheat> cheats = new List<ToggleCheat>();
        private HackMenu? hackMenu;
        public Harmony? harmony;

        public static REPOssessed? Instance;

        public void Start()
        {
            Instance = this;
            TranslationUtil.Initialize();
            LoadCheats();
            DoPatching();
            GameObjectManager.CollectObjects();
        }

        private void DoPatching()
        {
            try
            {
                harmony = new Harmony("REPOssessed");
                Harmony.DEBUG = false;
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void LoadCheats()
        {
            Settings.Changelog.ReadChanges();
            hackMenu = new HackMenu();
            foreach (Type cheat in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Cheat)) && t.Namespace.StartsWith("REPOssessed.Cheats") && !t.Namespace.Contains(".Core") && !t.Namespace.Contains(".Components")))
            {
                if (cheat.IsSubclassOf(typeof(ToggleCheat))) cheats.Add((ToggleCheat)Activator.CreateInstance(cheat));
                else Activator.CreateInstance(cheat);
                Debug.Log($"Loaded Cheat: {cheat.Name}");
            }
            Settings.Config.SaveDefaultConfig();
            Settings.Config.LoadConfig();
        }

        public void FixedUpdate()
        {
            try
            {
                cheats?.ForEach(c => c.FixedUpdate());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Update()
        {
            try
            {
                if (Cheat.instances.Where(c => c.WaitingForKeybind).Count() == 0)
                {
                    Cheat.instances.FindAll(c => c.HasKeybind && Input.GetKeyDown(c.keybind)).ForEach(c =>
                    {
                        if (c.GetType().IsSubclassOf(typeof(ToggleCheat))) ((ToggleCheat)c).Toggle();
                        else if (c.GetType().IsSubclassOf(typeof(ExecutableCheat))) ((ExecutableCheat)c).Execute();
                        else Debug.LogError($"REPOssessed Cheat Type: {c.GetType().Name}");
                    });
                }
                if (GameUtil.IsInGame()) cheats?.ForEach(c => c.Update());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void OnGUI()
        {
            try
            {
                if (Event.current.type == EventType.Repaint)
                {
                    if (Settings.b_ShowCredits)
                        VisualUtil.DrawString(new Vector2(5f, 2f), $"REPOssessed {Settings.s_Version} By Dustin | Menu Toggle: {(Cheat.Instance<ToggleMenu>().HasKeybind ? Cheat.Instance<ToggleMenu>().keybind.ToString() : KeyCode.None.ToString())} | Unload Toggle: {(Cheat.Instance<UnloadMenu>().HasKeybind ? Cheat.Instance<UnloadMenu>().keybind.ToString() : "None")}{(Cheat.Instance<FPSCounter>().Enabled ? $" | FPS: {Cheat.Instance<FPSCounter>().FPS}" : "")}", Settings.c_primary.GetColor(), false, false, true, 14);
                    if (MenuUtil.resizing)
                    {
                        VisualUtil.DrawString(new Vector2(Screen.width / 2, 35f), TranslationUtil.Translate("SettingsTab.ResizeTitle", "SettingsTab.ResizeConfirm"), Settings.c_primary, true, true, true, 22);
                        MenuUtil.ResizeMenu();
                    }
                    if (Cheat.Instance<DebugMode>().Enabled) VisualUtil.DrawString(new Vector2(5f, 20f), "[DEBUG MODE]", new RGBAColor(50, 205, 50, 1f), false, false, true, 10);
                }
                if (GameUtil.IsInGame()) cheats?.Where(c => c.Enabled).ToList().ForEach(c => c.OnGui());
                hackMenu?.Draw();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Unload()
        {
            StopAllCoroutines();
            harmony?.UnpatchAll("REPOssessed");
            if (GameUtil.IsInGame()) MenuUtil.HideCursor();
            Loader.Unload();
        }
    }
}
