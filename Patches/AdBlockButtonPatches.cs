using HarmonyLib;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(AdBlockButton))]
    public class AdBlockButtonPatches
    {
        // Patch Start to always hide the button
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void Start_Prefix()
        {
            // Set AdManager.adBlock to true so the button destroys itself
            AdManager.adBlock = true;
            Plugin.BepinLogger.LogInfo("Hiding Remove Ads button (Steam version doesn't have ads)");
        }
    }
}