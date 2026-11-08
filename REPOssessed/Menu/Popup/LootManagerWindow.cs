using REPOssessed.Handler;
using REPOssessed.Manager;
using REPOssessed.Menu.Core;
using REPOssessed.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace REPOssessed.Menu.Popup
{
    internal class LootManagerWindow : PopupMenu
    {
        public LootManagerWindow(int id) : base("LootManager.Title", new Rect(50f, 50f, 600f, 300f), id) { }

        private string search = "";
        private Vector2 scrollPos = Vector2.zero;
        private bool ignoreExtractionItems = false;
        private bool ignoreCartItems = false;
        private bool ignoreShopItems = false;

        public override void DrawContent(int windowID)
        {
            if (!GameUtil.IsInGame())
            {
                UI.Label("General.MustBeIngame", null, false, -1, false, Settings.c_error);
                GUI.DragWindow();
                return;
            }

            List<PhysGrabObject> items = GameObjectManager.items?.Where(i => i != null && i.Handle() is ObjectHandler h && !h.CurrentlyHeld() && (h.IsValuable() || h.IsShopItem()) && (!ignoreExtractionItems || !h.IsInExtraction()) && (!ignoreCartItems || !h.IsInCart()) && (!ignoreShopItems || !h.IsShopItem())).ToList() ?? new List<PhysGrabObject>();
            Dictionary<string, List<PhysGrabObject>> groupedItems = items.GroupBy(i => i.Handle()?.GetName() ?? "Unknown").ToDictionary(g => g.Key, g => g.ToList());
            UI.VerticalGroup(ref scrollPos, () =>
            {
                GUILayout.BeginHorizontal();
                UI.Textbox("General.Search", ref search, "", 50);
                UI.HostOnly(() => UI.Button(["LootManager.TeleportAllItems", "General.HostTag"], () => items?.Where(i => i != null).ToList().ForEach(p => Teleport(p))));
                UI.Checkbox("LootManager.IgnoreExtractionItems", ref ignoreExtractionItems);
                UI.Checkbox("LootManager.IgnoreCartsItems", ref ignoreCartItems);
                UI.Checkbox("LootManager.IgnoreShopItems", ref ignoreShopItems);
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                string masterClientTag = TranslationUtil.Translate("General.HostTag");
                UI.ButtonGrid(groupedItems.Keys.ToList(), name => $"{name} {groupedItems[name].Count}x {masterClientTag}", search, name =>
                {
                    List<PhysGrabObject> matchingItems = groupedItems[name];
                    if (matchingItems.Count == 0) return;
                    Teleport(matchingItems[Random.Range(0, matchingItems.Count)]);
                }, 3, hostOnly: true);
            });
            GUI.DragWindow();
        }

        private static void Teleport(PhysGrabObject physGrabObject)
        {
            Transform? cameraTransform = SemiFunc.MainCamera()?.transform;
            if (cameraTransform == null) return;
            physGrabObject?.Handle()?.Teleport(cameraTransform.position, cameraTransform.rotation);
        }
    }
}
