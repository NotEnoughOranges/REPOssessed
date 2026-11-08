using REPOssessed.Handler;
using REPOssessed.Manager;
using REPOssessed.Menu.Core;
using REPOssessed.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOssessed.Menu.Tab
{
    internal class EnemyTab : MenuTab
    {
        public EnemyTab() : base("EnemyTab.Title") 
        {
            enemySetups = GameUtil.GetEnemySetups();
        }

        private Vector2 scrollPos = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        private Vector2 scrollPos3 = Vector2.zero;
        private int selectedTab = 0;
        private static int selectedEnemy = -1;
        private static int selectedEnemySetup = -1;
        private string s_spawnAmount = "1";
        private string damage = "50";
        private string heal = "10";
        private string freeze = "5";
        public List<EnemySetup> enemySetups = new List<EnemySetup>();
        private bool noEnemyOrb = true;

        private PlayerAvatar? selectedPlayer => PlayersTab.selectedPlayer;

        public override void Draw()
        {
            if (HackMenu.Instance == null) return;
            UI.VerticalGroup(() =>
            {
                selectedTab = GUILayout.Toolbar(selectedTab, TranslationUtil.TranslateArray("EnemyTab.EnemyList", "EnemyTab.SpawnEnemies"));
                UI.HorizontalGroup(() =>
                {
                    UI.VerticalGroup(() => EnemyList(), GUILayout.Width(HackMenu.Instance.contentWidth * 0.3f - HackMenu.Instance.spaceFromLeft));
                    UI.VerticalGroup(ref scrollPos, () =>
                    {
                        switch (selectedTab)
                        {
                            case 0:
                                GeneralActions();
                                EnemyActions();
                                break;
                            case 1:
                                EnemySpawnerContent();
                                break;
                        }
                    }, GUILayout.Width(HackMenu.Instance.contentWidth * 0.7f - HackMenu.Instance.spaceFromLeft));
                });
            }, GUILayout.Width(HackMenu.Instance.contentWidth - HackMenu.Instance.spaceFromLeft));

        }

        private void EnemyList()
        {
            if (HackMenu.Instance == null) return;
            float width = HackMenu.Instance.contentWidth * 0.3f - HackMenu.Instance.spaceFromLeft * 2;
            float height = HackMenu.Instance.contentHeight - 45;
            switch (selectedTab)
            {
                case 0:
                    List<Enemy> enemies = GameObjectManager.enemies.Where(e => e?.Handle() is EnemyHandler h && !h.IsDead() && !h.IsDisabled()).OrderBy(e => e?.Handle()?.GetName()).ToList();
                    if (!enemies.Exists(e => e.GetInstanceID() == selectedEnemy)) selectedEnemy = -1;
                    DrawList<Enemy>("EnemyTab.EnemyList", enemies, e => e.Handle()?.GetName() ?? "Unknown", ref scrollPos2, ref selectedEnemy);
                    break;
                case 1:
                    List<EnemySetup> enemySetups = GameUtil.GetEnemySetups().Where(e => e != null).OrderBy(e => e.GetName()).ToList();
                    if (!enemySetups.Exists(e => e.GetInstanceID() == selectedEnemySetup)) selectedEnemySetup = -1;
                    DrawList<EnemySetup>("EnemyTab.EnemyType", enemySetups, e => e.GetName(), ref scrollPos3, ref selectedEnemySetup);
                   break;
            }
        }

        private void DrawList<T>(string title, IEnumerable<T> objects, Func<T, string> label, ref Vector2 scroll, ref int instanceID) where T : Object
        {
            if (HackMenu.Instance == null) return;
            float width = HackMenu.Instance.contentWidth * 0.3f - HackMenu.Instance.spaceFromLeft * 2;
            float height = HackMenu.Instance.contentHeight - 45;

            Rect rect = new Rect(0, 30, width, height);
            GUI.Box(rect, TranslationUtil.Translate(title));

            GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.Space(25);
            scrollPos3 = GUILayout.BeginScrollView(scrollPos3);

            foreach (T item in objects)
            {
                if (instanceID == -1) instanceID = item.GetInstanceID();

                if (instanceID == item.GetInstanceID()) GUI.contentColor = Settings.c_success.GetColor();

                if (GUILayout.Button(label(item), GUI.skin.label)) instanceID = item.GetInstanceID();

                GUI.contentColor = Settings.c_menuText.GetColor();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void GeneralActions()
        {
            UI.Label("EnemyTab.GeneralActions", null, true, -1, true);

            UI.HostOnly(() => UI.Button(["EnemyTab.KillAll", "General.HostTag"], () => GameObjectManager.enemies.Select(e => e?.Handle()).Where(h => h != null && !h.IsDead() && !h.IsDisabled()).ToList().ForEach(h => h?.Kill(noEnemyOrb))));
            UI.HostOnly(() => UI.Button(["EnemyTab.PermaKillAll", "General.HostTag"], () => GameObjectManager.enemies.Select(e => e?.Handle()).Where(h => h != null && !h.IsDead() && !h.IsDisabled()).ToList().ForEach(h => h?.PermaKill())));
            UI.Checkbox("EnemyTab.NoEnemyOrb", ref noEnemyOrb);
            UI.HostOnly(() => UI.Button(["EnemyTab.TeleportAllEnemies", "General.HostTag"], () => GameObjectManager.enemies.Select(e => e?.Handle()).Where(h => h != null && !h.IsDead() && !h.IsDisabled()).ToList().ForEach(h =>
            {
                Transform? playerTransform = selectedPlayer?.transform;
                if (playerTransform != null) h?.Teleport(playerTransform.position);
            })));
            UI.HostOnly(() => UI.Button(["EnemyTab.LureAll", "General.HostTag"], () => GameObjectManager.enemies.Select(e => e?.Handle()).Where(h => h != null && !h.IsDead() && !h.IsDisabled()).ToList().ForEach(h =>
            {
                Transform? playerTransform = selectedPlayer?.transform;
                if (playerTransform != null) h?.Lure(playerTransform.position);
            })));
            UI.Label("EnemyTab.SelectedPlayer", selectedPlayer?.Handle()?.GetName() ?? TranslationUtil.Translate("General.None"));
        }

        private void EnemyActions()
        {
            Enemy enemy = GameObjectManager.enemies.FirstOrDefault(x => x.GetInstanceID() == selectedEnemy);
            if (enemy == null) return;
            EnemyHandler? enemyHandler = enemy?.Handle();
            if (enemyHandler == null || enemyHandler.IsDead() || enemyHandler.IsDisabled()) return;
            Transform? enemyTransform = enemy?.transform;
            if (enemyTransform == null) return;

            UI.Label("EnemyTab.EnemyActions", null, true, -1, true);
            UI.Label("EnemyTab.Health", enemyHandler.GetHealth().ToString());
            UI.Label("EnemyTab.MaxHealth", enemyHandler.GetMaxHealth().ToString());
            UI.Label("EnemyTab.EnemyTarget", enemyHandler.GetEnemyTarget()?.Handle()?.GetName() ?? TranslationUtil.Translate("General.None"));
            UI.HostOnly(() => UI.Button(["EnemyTab.Kill", "General.HostTag"], () => enemyHandler.Kill(noEnemyOrb)));
            UI.HostOnly(() => UI.Button(["EnemyTab.PermaKill", "General.HostTag"], () => enemyHandler.PermaKill()));
            UI.HostOnly(() => UI.Button(["EnemyTab.Lure", "General.HostTag"], () =>
            {
                Transform? playerTransform = selectedPlayer?.transform;
                if (playerTransform != null) enemyHandler.Lure(playerTransform.position);
            }));
            UI.Button("EnemyTab.TeleportToEnemy", () => GameObjectManager.LocalPlayer?.Handle()?.Teleport(enemyTransform.position, enemyTransform.rotation));
            UI.HostOnly(() => UI.Button(["EnemyTab.TeleportEnemyToPlayer", "General.HostTag"], () =>
            {
                Transform? playerTransform = selectedPlayer?.transform;
                if (playerTransform != null) enemyHandler.Teleport(playerTransform.position);
            }));
            UI.Button(["EnemyTab.TeleportPlayerToEnemy", "General.MasterOrOwnerTag"], () => selectedPlayer?.Handle()?.Teleport(enemyTransform.position, enemyTransform.rotation));
            UI.HostOnly(() => UI.Textbox(["EnemyTab.Damage", "General.HostTag"], ref damage, @"[^0-9]", 3, new UIButton("General.Set", () => enemyHandler.Hurt(int.Parse(damage)))));
            UI.HostOnly(() => UI.Textbox(["EnemyTab.Heal", "General.HostTag"], ref heal, @"[^0-9]", 3, new UIButton("General.Set", () => enemyHandler.Heal(int.Parse(heal)))));
            UI.HostOnly(() => UI.Textbox(["EnemyTab.Freeze", "General.HostTag"], ref freeze, @"[^0-9]", 3, new UIButton("General.Set", () => enemyHandler.Freeze(int.Parse(freeze)))));
        }

        private void EnemySpawnerContent()
        {
            if (selectedEnemySetup == -1) return;
            EnemySetup enemySetup = enemySetups.Find(x => x.GetInstanceID() == selectedEnemySetup);
            if (enemySetup == null) return;

            if (SemiFunc.IsMultiplayer() && !GameUtil.IsMasterClient())
            {
                UI.Label("General.HostRequired", null, false, -1, false, Settings.c_error);
                return;
            }

            UI.Label(["EnemyTab.EnemySpawnerContent", "General.HostTag"], null, true, -1, true);

            UI.Label("EnemyTab.SelectedEnemy", enemySetup.GetName());
            UI.Textbox("EnemyTab.SpawnAmount", ref s_spawnAmount, @"[^0-9]", 3);

            UI.Button("EnemyTab.Spawn", () => GameUtil.SpawnEnemy(enemySetup, int.Parse(s_spawnAmount)));
        }
    }
}
