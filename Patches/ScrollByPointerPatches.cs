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

                // Check if player has all 3 keys
                bool hasAllKeys = PlayerPrefs.HasKey("key1") &&
                                 PlayerPrefs.HasKey("key2") &&
                                 PlayerPrefs.HasKey("key3");

                Plugin.BepinLogger.LogInfo($"Has all keys: {hasAllKeys}");

                if (hasAllKeys)
                {
                    // Skip the broken UI, go straight to the race
                    PlayerPrefs.SetString("t6boxopened", "ok");
                    PlayerPrefs.Save();

                    DuckManagement.backScene = Application.loadedLevelName;
                    DuckManagement.nextScene = "area_6_final";
                    DuckManagement.sceneEvent = DuckManagement.eventList.select_duck;
                    raceres.town = Application.loadedLevelName;
                    raceres.index = "-1";
                    raceres.boss = true;

                    Plugin.BepinLogger.LogInfo("All keys present - opening Fire Duck race directly!");
                    ScrollByPointer.LoadLevel("UI Duck Select");

                    return false; // Block original behavior
                }
                else
                {
                    Plugin.BepinLogger.LogInfo("Missing keys - need all 3 to unlock");
                    return false; // Block the broken UI
                }
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