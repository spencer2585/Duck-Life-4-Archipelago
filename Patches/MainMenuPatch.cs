using HarmonyLib;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    // Patch whatever class loads the main menu
    [HarmonyPatch(typeof(GameSwitcher))]
    public class MainMenuPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void Awake_Prefix()
        {
            // Set tutorial complete flag
            PlayerPrefs.SetString("tutorialOK", "OK");
            PlayerPrefs.Save();
            Plugin.BepinLogger.LogInfo("Tutorial flag set");
        }
    }
}