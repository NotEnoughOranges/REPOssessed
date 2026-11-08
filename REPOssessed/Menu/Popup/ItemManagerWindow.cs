using Photon.Pun;
using REPOssessed.Handler;
using REPOssessed.Manager;
using REPOssessed.Menu.Core;
using REPOssessed.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOssessed.Menu.Popup
{
    internal class ItemManagerWindow : PopupMenu
    {
        public ItemManagerWindow(int id) : base("ItemManager.Title", new Rect(50f, 50f, 600f, 300f), id) { }

        private Vector2 scrollPos = Vector2.zero;
        private string search = "";
        private string amount = "1";
        private string value = "1000";
        private bool randomValue = false;

        private Dictionary<GameObject, string>? Items;

        public override void DrawContent(int windowID)
        { 
            UI.VerticalGroup(ref scrollPos, () =>
            {
                if (!GameUtil.IsMasterClient())
                {
                    UI.Label("General.HostRequired", null, false, -1, false, Settings.c_error);
                    GUI.DragWindow();
                    return;
                }
                if (!GameUtil.IsInGame())
                {
                    UI.Label("General.MustBeIngame", null, false, -1, false, Settings.c_error);
                    GUI.DragWindow();
                    return;
                }

                UI.Label(["ItemManager.Title", "General.HostTag"], null, true);
                GUILayout.BeginHorizontal();
                UI.Textbox("General.Search", ref search, "", 50);
                UI.Textbox("ItemManager.Amount", ref amount, @"[^0-9]", 3);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                UI.Checkbox("ItemManager.RandomValue", ref randomValue);
                if (!randomValue) UI.Textbox("ItemManager.Value", ref value, @"[^0-9]", 6);
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                if (Items == null) Items = GameUtil.GetAllItems();
                UI.ButtonGrid(Items.Where(i => i.Key != null && !string.IsNullOrEmpty(i.Value)).ToList(), (i) => i.Key?.GetComponent<PhysGrabObject>()?.Handle()?.GetName() ?? "Unknown", search, (i) =>
                {
                    Transform? cameraTransform = SemiFunc.MainCamera()?.transform;
                    if (cameraTransform == null || !int.TryParse(amount, out int spawnAmount) || spawnAmount <= 0) return;
                    if (!randomValue && !int.TryParse(value, out _)) return;

                    string path = i.Value;
                    GameObject item = i.Key;
                    string resourcePath = ValuableDirector.instance?.Reflect()?.GetValue<string>("resourcePath")?.Replace("/", "") ?? "";
                    if (string.IsNullOrEmpty(resourcePath) && !path.StartsWith("cosmetic:", System.StringComparison.Ordinal)) return;

                    for (int j = 0; j < spawnAmount; j++)
                    {
                        GameObject? spawnedItem = null;
                        if (GameManager.Multiplayer())
                        {
                            string spawnPath = path.StartsWith("cosmetic:", System.StringComparison.Ordinal)
                                ? path.Substring("cosmetic:".Length)
                                : path switch
                                {
                                    "shop" => $"Items/{item.name}",
                                    "surplus" => $"{resourcePath}/{item.name}",
                                    "enemy" => $"{resourcePath}/{item.name}",
                                    _ => $"{resourcePath}/{path}/{item.name}"
                                };
                            if (!string.IsNullOrEmpty(spawnPath)) spawnedItem = PhotonNetwork.InstantiateRoomObject(spawnPath, cameraTransform.position, default);
                        }
                        else spawnedItem = Object.Instantiate(item, cameraTransform.position, default);

                        // Mark Item Manager-spawned cosmetic crates so their extraction
                        // does not consume the game's normal level-based extraction limit.
                        // This only works for you cause the check is done locally.
                        if (spawnedItem?.GetComponent<CosmeticWorldObject>() is not null
                            && spawnedItem.GetComponent<ItemManagerCosmeticWorldObject>() is null)
                        {
                            spawnedItem.AddComponent<ItemManagerCosmeticWorldObject>();
                        }

                        ValuableObject? valuable = spawnedItem?.GetComponent<ValuableObject>();
                        if (valuable == null) continue;

                        if (randomValue)
                        {
                            valuable.DollarValueSetLogic();
                        }
                        else
                        {
                            spawnedItem?.GetComponent<PhysGrabObject>()?.Handle()?.SetValue(int.Parse(value));
                        }
                    }
                }, 3);
            });
            GUI.DragWindow();
        }
    }
}