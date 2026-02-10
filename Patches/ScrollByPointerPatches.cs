using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(ScrollByPointer))]
    public class ScrollByPointerPatches
    {
        [HarmonyPatch("OnMouseUp")]
        [HarmonyPrefix]
        public static bool OnMouseUp_Prefix(ScrollByPointer __instance)
        {
            if (!ArchipelagoClient.Authenticated)
                return true; // Not connected, run original

            if (__instance.gameObject.transform.name == "Box")
            {
                Plugin.BepinLogger.LogInfo("=== Box clicked (AP override) ===");
                return false;
            }

            return true; // Run original for everything else
        }

        [HarmonyPatch("OnMouseDown")]
        [HarmonyPrefix]
        public static void OnMouseDown_Prefix()
        {
            // When clicking anywhere, hide blocking popups
            
            // Hide "No Ticket" popup if showing
            GameObject noTicket = GameObject.Find("No Ticket");
            if (noTicket != null)
            {
                var rectTransform = noTicket.GetComponent<RectTransform>();
                if (rectTransform != null && rectTransform.anchoredPosition == Vector2.zero)
                {
                    // Move it off screen
                    rectTransform.anchoredPosition = new Vector2(0f, 10000f);
                    Plugin.BepinLogger.LogInfo("Hidden No Ticket popup");
                }
            }
            
            // Hide keybg popup if showing
            GameObject keybg = GameObject.Find("keybg");
            if (keybg != null)
            {
                var rectTransform = keybg.GetComponent<RectTransform>();
                if (rectTransform != null && rectTransform.anchoredPosition == Vector2.zero)
                {
                    rectTransform.anchoredPosition = new Vector2(0f, 10000f);
                    Plugin.BepinLogger.LogInfo("Hidden keybg popup");
                }
            }
        }
    }
}