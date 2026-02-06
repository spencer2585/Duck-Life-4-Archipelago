using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using System;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(TownUI))]
    public class TownUIPatches
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void Start_Prefix(TownUI __instance)
        {
            // Only control unlocks if connected
            if (!ArchipelagoClient.Authenticated)
                return;

            // Get which town this is
            int townNumber = Convert.ToInt32(Application.loadedLevelName[4].ToString() ?? "");

            Plugin.BepinLogger.LogInfo($"Entering town{townNumber}");

            // Check if this area should be locked
            string areaName = GetAreaName(townNumber);

            if (areaName != null && !Plugin.AreaUnlocks.Contains(areaName))
            {
                // This area should be locked - set unlock to 0
                PlayerPrefs.SetInt("unlockmap" + townNumber, 0);
                Plugin.BepinLogger.LogInfo($"Keeping {areaName} locked");
            }
            else if (areaName != null && Plugin.AreaUnlocks.Contains(areaName))
            {
                // Player has access - ensure it's unlocked
                PlayerPrefs.SetInt("unlockmap" + townNumber, 1);
                Plugin.BepinLogger.LogInfo($"Confirmed {areaName} unlocked");
            }

            PlayerPrefs.Save();
        }

        private static string GetAreaName(int townNumber)
        {
            return townNumber switch
            {
                1 => null, // Grasslands always accessible
                2 => "Swamp",
                3 => "Mountains",
                4 => "Glacier",
                5 => "City",
                6 => "Volcano",
                _ => null
            };
        }
    }
}