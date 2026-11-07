using HarmonyLib;
using Photon.Pun;
using REPOssessed.Util;
using System.Collections.Generic;
using UnityEngine;

namespace REPOssessed.Manager
{
    // Marker added only to CosmeticWorldObjects spawned through the Item Manager.
    internal sealed class ItemManagerCosmeticWorldObject : MonoBehaviour
    {
    }

    [HarmonyPatch]
    internal static class ItemManagerCosmeticWorldObjectPatch
    {
        private static RoundDirector? trackedRoundDirector;
        private static int itemManagerExtractedCount;
        private static int lastObservedExtractionCount;

        [HarmonyPatch(typeof(CosmeticWorldObject), "ExtractRPC"), HarmonyPrefix]
        private static bool ExtractRPC(CosmeticWorldObject __instance, PhotonMessageInfo _info)
        {
            if (__instance == null)
                return true;

            if (!SemiFunc.MasterOnlyRPC(_info))
                return false;

            if (__instance.Reflect().GetValue<bool>("inExtraction") != true)
                return false;

            RoundDirector roundDirector = RoundDirector.instance;
            List<SemiFunc.Rarity>? extracted = roundDirector.Reflect()
                .GetValue<List<SemiFunc.Rarity>>("cosmeticWorldObjectsExtracted");

            if (extracted == null)
                return true;

            if (trackedRoundDirector != roundDirector || extracted.Count < lastObservedExtractionCount)
            {
                trackedRoundDirector = roundDirector;
                itemManagerExtractedCount = 0;
            }
            lastObservedExtractionCount = extracted.Count;

            bool itemManagerSpawned = __instance.GetComponent<ItemManagerCosmeticWorldObject>() != null;
            int normalExtractedCount = Mathf.Max(0, extracted.Count - itemManagerExtractedCount);
            int expectedCount = ValuableDirector.instance.CosmeticWorldObjectLevelLoopsClampedGet();

            // Item Manager crates do not consume the extraction limit, but only works for you since the check is done locally.
            if (!itemManagerSpawned
                && ValuableDirector.instance.Reflect().GetValue<ValuableDirector.CosmeticWorldObjectDebug>("cosmeticWorldObjectDebug") == ValuableDirector.CosmeticWorldObjectDebug.Normal
                && normalExtractedCount > expectedCount)

            CosmeticWorldObjectUI.instance.Add(__instance.rarity);
            roundDirector.CosmeticWorldObjectExtracted(__instance.rarity);

            if (itemManagerSpawned)
                itemManagerExtractedCount++;

            lastObservedExtractionCount = extracted.Count;
            return false;
        }
    }
}
