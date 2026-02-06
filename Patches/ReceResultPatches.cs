using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(tournamentducksetting))]
    public class TournamentDuckSettingPatches
    {
        [HarmonyPatch("applydata")]
        [HarmonyPostfix]
        public static void ApplyData_Postfix(tournamentducksetting __instance)
        {
            Plugin.BepinLogger.LogInfo("=== Opponent Duck Stats ===");

            for (int i = 0; i < tournamentducksetting.currentmapdata.Length; i++)
            {
                Plugin.BepinLogger.LogInfo($"Race {i + 1}:");
                var race = tournamentducksetting.currentmapdata[i];

                for (int j = 0; j < race.duck.Length; j++)
                {
                    var duck = race.duck[j];
                    Plugin.BepinLogger.LogInfo($"  Duck {j}: Run={duck.run}, Swim={duck.swim}, Fly={duck.fly}, Climb={duck.climb}, Jump={duck.jump}");
                }
            }
        }
    }
    [HarmonyPatch(typeof(raceres))]
    public class RaceResultPatches
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix()
        {
            // Only send checks if connected
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return;

            // Check if we won (1st place)
            int rank = tournamentducksetting.teamrank[tournamentlist.currentindex - 1];

            if (rank != 1) // Only care about wins
                return;

            Plugin.BepinLogger.LogInfo($"Won race! Town: {raceres.town}, Index: {raceres.index}, Tournament races: {tournamentlist.readrunlist?.Length ?? 0}");

            // Determine location name
            string locationName = GetRaceLocationName(
                raceres.town,
                raceres.index,
                tournamentlist.readrunlist?.Length ?? 1,
                tournamentlist.currentindex
            );

            if (!string.IsNullOrEmpty(locationName))
            {
                long locationId = Plugin.ArchipelagoClient.GetLocationId(locationName);

                if (locationId > 0)
                {
                    Plugin.BepinLogger.LogInfo($"Sending race location check: {locationName} (ID: {locationId})");
                    Plugin.ArchipelagoClient.SendLocationCheck(locationId);
                }
                else
                {
                    Plugin.BepinLogger.LogError($"Could not find location ID for: {locationName}");
                }
            }

            if (ArchipelagoClient.Authenticated)
            {
                RevertUnauthorizedUnlocks();
            }
        }

        private static string GetRaceLocationName(string town, string index, int tournamentLength, int currentRaceIndex)
        {
            // Map town codes to area names
            string area = town switch
            {
                "town1" => "Grasslands",
                "town2" => "Swamp",
                "town3" => "Mountains",
                "town4" => "Glacier",
                "town5" => "City",
                "town6" => "Volcano",
                _ => null
            };

            if (area == null)
            {
                Plugin.BepinLogger.LogError($"Unknown town: {town}");
                return null;
            }

            // Determine if this is a side race or tournament
            bool isTournament = tournamentLength > 1;

            if (isTournament)
            {
                // Tournament races
                if (currentRaceIndex <= 3)
                {
                    // Race 1, 2, or 3
                    return $"{area} - Tournament Race {currentRaceIndex} Won";
                }
                else if (currentRaceIndex == 4)
                {
                    // Final tournament race
                    return $"{area} - Tournament Won";
                }
            }
            else
            {
                // Side races - need to map index to opponent name
                return GetSideRaceLocationName(area, index);
            }

            return null;
        }

        private static string GetSideRaceLocationName(string area, string index)
        {
            // Map area + index to specific opponent
            string key = $"{area}_{index}";

            return key switch
            {
                // Grasslands
                "Grasslands_1" => "Grasslands - Olive Duck Race Won",
                "Grasslands_2" => "Grasslands - Brown Duck Race Won",

                // Swamp
                "Swamp_1" => "Swamp - Grey Duck Race Won",
                "Swamp_2" => "Swamp - Red Duck Race Won",
                "Swamp_3" => "Swamp - Blue Duck Race Won",

                // Mountains
                "Mountains_1" => "Mountains - Green Duck Race Won",
                "Mountains_2" => "Mountains - Yellow Duck Race Won",
                "Mountains_3" => "Mountains - White Duck Race Won",

                // Glacier
                "Glacier_1" => "Glacier - Black Spotted Duck Race Won",
                "Glacier_2" => "Glacier - Grey Duck Race Won",
                "Glacier_3" => "Glacier - White Duck Race Won",

                // City
                "City_1" => "City - Purple Duck Race Won",
                "City_2" => "City - Green Duck Race Won",
                "City_3" => "City - Yellow Duck Race Won",

                // Volcano
                "Volcano_1" => "Volcano - Black Duck Race Won",
                "Volcano_2" => "Volcano - Lilac Duck Race Won",
                "Volcano_3" => "Volcano - Light Blue Duck Race Won",

                _ => null
            };
        }
        private static void RevertUnauthorizedUnlocks()
        {
            // Check tickets
            string[] ticketAreas = { "Grasslands", "Swamp", "Mountains", "Glacier", "City" };
            for (int i = 0; i < ticketAreas.Length; i++)
            {
                string ticketKey = $"ticket{i + 1}";
                string area = ticketAreas[i];

                if (PlayerPrefs.HasKey(ticketKey) && !Plugin.TournamentUnlocks.Contains(area))
                {
                    PlayerPrefs.DeleteKey(ticketKey);
                    Plugin.BepinLogger.LogInfo($"Blocked auto-unlock of {area} tournament ticket");
                }
            }

            // Check keys
            string[] keyColors = { "red", "orange", "green" };
            for (int i = 0; i < keyColors.Length; i++)
            {
                string keyNumber = $"key{i + 1}";
                string color = keyColors[i];

                if (PlayerPrefs.HasKey(keyNumber) && !Plugin.KeysCollected.Contains(color))
                {
                    PlayerPrefs.DeleteKey(keyNumber);
                    Plugin.BepinLogger.LogInfo($"Blocked auto-collection of {color} key");
                }
            }

            PlayerPrefs.Save();
        }
    }
}