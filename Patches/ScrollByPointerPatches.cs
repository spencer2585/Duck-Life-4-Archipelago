using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Wix;

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

            //if (__instance.gameObject.transform.name == "Box")
            //{
            //    Plugin.BepinLogger.LogInfo("=== Box clicked (AP override) ===");
            //    return false;
            //}

            return true; // Run original for everything else
        }

        [HarmonyPatch("OnMouseUp")]
        [HarmonyPostfix]
        public static void OnMouseUp_Postfix(ScrollByPointer __instance)
        {
            if (__instance.gameObject.transform.name == "tournament")
            {
                bool hasKey = PlayerPrefs.HasKey("ticket" + SceneManager.GetActiveScene().name[4].ToString());
                if (!hasKey)
                {
                    GameObject noTicket = GameObject.Find("No Ticket");
                    GameObject messages = GameObject.Find("Messages");
                    if (noTicket != null && messages != null)
                    {
                        noTicket.GetComponent<RectTransform>().anchoredPosition =
                            new Vector2(-messages.transform.localPosition.x, -messages.transform.localPosition.y);
                        Text ticketText = noTicket.transform.Find("Text").GetComponent<Text>();
                        ticketText.text =
                            "TO ENTER THE TOURNAMENT YOU MUST HAVE AN INVITE. YOU CAN RECEIVE ONE AS AN ITEM FROM A GAME IN THE MULTIWORLD";
                    }
                }
            }
        }
    }
    
}