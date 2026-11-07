using REPOssessed.Cheats.Core;
using REPOssessed.Handler;
using REPOssessed.Manager;
using REPOssessed.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOssessed.Cheats.VisualTab
{
    internal class ESP : ToggleCheat, IVariableCheat<float>
    {
        public static float Value = 5000f;
        private static SharedMemoryManager sharedMemory;
        private static List<ESPTextItem> espItems = new List<ESPTextItem>();
        private static bool useOverlay = true;

        public override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnDisable()
        {
            if (sharedMemory != null)
            {
                sharedMemory.WriteESPData(new List<ESPTextItem>(), 0, false);
            }
            base.OnDisable();
        }

        public override void OnGui()
        {
            if (Settings.b_UseExternalESP && useOverlay && sharedMemory == null)
            {
                sharedMemory = new SharedMemoryManager();
                if (!sharedMemory.Initialize())
                {
                    useOverlay = false;
                    sharedMemory.Dispose();
                    sharedMemory = null;
                }
            }

            espItems.Clear();

            try
            {
                if (Enabled)
                {
                    if (Settings.b_PlayerESP) DisplayPlayers();
                    if (Settings.b_ItemESP) DisplayItems();
                    if (Settings.b_EnemyESP) DisplayEnemies();
                    if (Settings.b_CartESP) DisplayCarts();
                    if (Settings.b_ExtractionESP) DisplayExtractions();
                    if (Settings.b_DeathHeadESP) DisplayDeathHeads();
                    if (Settings.b_TruckESP) DisplayTruck();
                }

                if (Settings.b_UseExternalESP && useOverlay && sharedMemory != null)
                {
                    sharedMemory.WriteESPData(espItems, espItems.Count, Enabled);
                }
                else
                {
                    if (sharedMemory != null)
                    {
                        sharedMemory.WriteESPData(espItems, 0, false);
                    }

                    if (Enabled)
                    {
                        foreach (var item in espItems)
                        {
                            VisualUtil.DrawString(
                                new Vector3(item.ScreenX, item.ScreenY, 0),
                                item.Text,
                                new RGBAColor(item.ColorR, item.ColorG, item.ColorB, item.ColorA),
                                true, true, false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void ToggleAll()
        {
            Settings.b_PlayerESP = !Settings.b_PlayerESP;
            Settings.b_EnemyESP = !Settings.b_EnemyESP;
            Settings.b_ItemESP = !Settings.b_ItemESP;
            Settings.b_CartESP = !Settings.b_CartESP;
            Settings.b_ExtractionESP = !Settings.b_ExtractionESP;
            Settings.b_DeathHeadESP = !Settings.b_DeathHeadESP;
            Settings.b_TruckESP = !Settings.b_TruckESP;
        }

        private void DisplayObjects<T>(IEnumerable<T> objects, Func<T, string> labelSelector, Func<T, RGBAColor?> colorSelector)
            where T : Component
        {
            if (objects == null) return;

            var camera = SemiFunc.MainCamera();
            if (camera?.transform == null) return;

            foreach (T obj in objects.Where(o => o != null && o.gameObject.activeInHierarchy))
            {
                if (obj.transform == null) continue;

                float distance = GetDistanceToPos(obj.transform.position);
                if (distance == 0f || distance > Value || !WorldToScreen(obj.transform.position, out Vector3 screen))
                    continue;

                string text = $"{labelSelector(obj)}\n{distance}m";
                RGBAColor? colorData = colorSelector(obj) ?? RGBAColor.Default;

                espItems.Add(new ESPTextItem
                {
                    ScreenX = screen.x,
                    ScreenY = screen.y,
                    Text = text,
                    ColorR = colorData?.r ?? 1f,
                    ColorG = colorData?.g ?? 1f,
                    ColorB = colorData?.b ?? 1f,
                    ColorA = colorData?.a ?? 1f
                });
            }
        }

        private void DisplayPlayers()
        {
            DisplayObjects(
                GameObjectManager.players.Where(p => p?.Handle() is PlayerHandler h && !h.IsLocalPlayer() && !h.IsDead()),
                player => player.Handle() is PlayerHandler h
                    ? $"{(h.IsTalking() ? "[VC] " : "")}{h.GetName()} ({h.GetHealth()}/{h.GetMaxHealth()})"
                    : "Unknown",
                player => Settings.c_espPlayer
            );
        }

        private void DisplayItems()
        {
            DisplayObjects(
                GameObjectManager.items.Where(i =>
                    i != null &&
                    i.Handle() is ObjectHandler h &&
                    !h.IsHinge() &&
                    (!h.IsCart() && h.IsShopItem() || h.IsValuable())),
                item => item.Handle() is ObjectHandler h
                    ? $"{h.GetName()}{(h.IsValuable() && !h.IsCosmeticCrate() ? $" ( {h.GetValue()} )" : "")}{(h.IsTrap() ? " ( Trap )" : "")}"
                    : "Unknown",
                item =>
                {
                    ObjectHandler h = item.Handle();
                    if (h != null && h.IsCosmeticCrate())
                    {
                        var c = h.GetCosmeticRarityColor();
                        return new RGBAColor(c.r, c.g, c.b, c.a);
                    }
                    if (!Settings.b_useValuableTiers) return Settings.c_espItem;
                    int index = Array.FindLastIndex(Settings.i_valuableValueThresholds, x => x <= h?.GetValue());
                    return index > -1 ? Settings.c_valuableValueColors[index] : Settings.c_espItem;
                }
            );
        }

        private void DisplayEnemies()
        {
            DisplayObjects(
                GameObjectManager.enemies.Where(e =>
                    e != null &&
                    e.Handle() is EnemyHandler h &&
                    !h.IsDead() &&
                    !h.IsDisabled()),
                enemy => enemy.Handle() is EnemyHandler h
                    ? $"{h.GetName()} ({h.GetHealth()}/{h.GetMaxHealth()})"
                    : "Unknown",
                enemy => Settings.c_espEnemy
            );
        }

        private void DisplayDeathHeads()
        {
            DisplayObjects(
                GameObjectManager.players
                    .Select(p => p?.Handle()?.GetPlayerDeathHead())
                    .Where(d => d != null)
                    .Cast<PlayerDeathHead>(),
                deathHead => deathHead.playerAvatar?.Handle() is PlayerHandler h
                    ? $"{h.GetName()}'s Death Head"
                    : "Unknown",
                deathHead => Settings.c_espDeathHead
            );
        }

        private void DisplayExtractions()
        {
            DisplayObjects(
                GameObjectManager.extractions.Where(e =>
                    e != null &&
                    e.Handle() is ExtractionPointHandler h &&
                    h.IsCompleted()),
                extraction => extraction.Handle() is ExtractionPointHandler h
                    ? (h.IsShop() ? "Shop" : "Extraction")
                    : "Unknown",
                extraction => Settings.c_espExtraction
            );
        }

        private void DisplayCarts()
        {
            DisplayObjects(
                GameObjectManager.carts.Where(c => c != null),
                cart => cart.isSmallCart ? "Small Cart" : "Cart",
                cart => Settings.c_espCart
            );
        }

        private void DisplayTruck()
        {
            TruckScreenText? truck = GameObjectManager.truck;
            if (truck == null) return;
            DisplayObjects(
                [truck],
                truck => "Truck",
                truck => Settings.c_espTruck
            );
        }
    }
}
