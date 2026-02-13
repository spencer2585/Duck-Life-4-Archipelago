using HarmonyLib;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(ShopManagement))]
    public class ShopManagementPatches
    {
        [HarmonyPatch("onClick")]
        [HarmonyPrefix]
        public static bool onClick_Prefix(GameObject button, ShopManagement __instance)
        {
            // Block buying eggs (new ducks) when mod is loaded
            if (button.name == "buy" && __instance.GetType().GetField("currentItemType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(__instance).ToString() == "egg")
            {
                Plugin.BepinLogger.LogInfo("Blocked purchase of additional duck (egg)");

                // Show message to player (optional - could create a popup)
                Debug.Log("Extra ducks are disabled with the Archipelago mod");

                return false; // Prevent purchase
            }

            // Allow hats, hair, costumes
            return true;
        }
    }
}