using HarmonyLib;
using REPOssessed.Cheats.Core;
using REPOssessed.Manager;
using REPOssessed.Handler;
using REPOssessed.Util;

namespace REPOssessed.Cheats.SelfTab
{
    [HarmonyPatch]
    internal class UnlimitedBattery : ToggleCheat
    {
        [HarmonyPatch(typeof(ItemBattery), "Update"), HarmonyPostfix]
        public static void Update(ItemBattery __instance)
        {
            if (!Instance<UnlimitedBattery>().Enabled || !SemiFunc.IsMasterClientOrSingleplayer()) return;

            PhysGrabObject? physGrabObject = __instance.Reflect()?.GetValue<PhysGrabObject>("physGrabObject");
            if (GameObjectManager.LocalPlayer?.Handle()?.GetHeldPhysGrabObject() != physGrabObject) return;

            __instance.SetBatteryLife(100);
        }
    }
}
