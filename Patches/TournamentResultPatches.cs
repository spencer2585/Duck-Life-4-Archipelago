using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using System;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(tournamentres))]
    public class TournamentResultPatches
    {
        [HarmonyPatch("backtotown")]
        [HarmonyPrefix]
        public static void BackToTown_Prefix(tournamentres __instance)
        {
            // Only control unlocks if connected
            if (!ArchipelagoClient.Authenticated)
                return;

            // Check if this would unlock a new area
            if (__instance.cutscene)
            {
                int nextTownNumber = Convert.ToInt32(raceres.town[4].ToString() ?? "") + 1;
                string nextArea = GetAreaName(nextTownNumber);

                if (nextArea != null)
                {
                    Plugin.BepinLogger.LogInfo($"Tournament completed - game wants to unlock {nextArea}");

                    // Check if player actually has the access item
                    if (!Plugin.AreaUnlocks.Contains(nextArea))
                    {
                        Plugin.BepinLogger.LogInfo($"Preventing auto-unlock of {nextArea} - player needs {nextArea} Access item");
                        // Don't set the unlock - just log it
                        // The original method will try to unlock but we'll override it
                    }
                    else
                    {
                        Plugin.BepinLogger.LogInfo($"Player has {nextArea} Access - allowing unlock");
                    }
                }
            }
        }

        [HarmonyPatch("backtotown")]
        [HarmonyPostfix]
        public static void BackToTown_Postfix(tournamentres __instance)
        {
            Plugin.BepinLogger.LogInfo("=== BackToTown_Postfix running ===");

            // Only control unlocks if connected
            if (!ArchipelagoClient.Authenticated)
            {
                Plugin.BepinLogger.LogInfo("Not authenticated, skipping");
                return;
            }

            // After the original method runs, check and revert any unauthorized unlocks
            if (__instance.cutscene)
            {
                Plugin.BepinLogger.LogInfo("Cutscene flag is true");

                int nextTownNumber = Convert.ToInt32(raceres.town[4].ToString() ?? "") + 1;
                string nextArea = GetAreaName(nextTownNumber);

                Plugin.BepinLogger.LogInfo($"Next town: {nextTownNumber}, Area: {nextArea}");

                if (nextArea != null && !Plugin.AreaUnlocks.Contains(nextArea))
                {
                    // Relock it
                    string key = "unlockmap" + nextTownNumber;
                    Plugin.BepinLogger.LogInfo($"Setting {key} to 'false'");

                    PlayerPrefs.SetString(key, "false");
                    PlayerPrefs.DeleteKey(key); // Also try deleting the key entirely
                    PlayerPrefs.Save();

                    Plugin.BepinLogger.LogInfo($"Relocked {nextArea}, verification: {PlayerPrefs.GetString(key, "not set")}");
                }
                else
                {
                    Plugin.BepinLogger.LogInfo($"Not relocking - has access: {Plugin.AreaUnlocks.Contains(nextArea ?? "")}");
                }
            }
            else
            {
                Plugin.BepinLogger.LogInfo("Cutscene flag is false");
            }
        }

        private static string GetAreaName(int townNumber)
        {
            return townNumber switch
            {
                1 => "Grasslands",
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