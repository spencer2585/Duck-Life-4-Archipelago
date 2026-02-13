using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(TutorialRace))]
    public class TutorialRaceSkipPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix(TutorialRace __instance)
        {
            Plugin.BepinLogger.LogInfo("Tutorial race started, scheduling skip");

            // Set tutorial complete
            PlayerPrefs.SetString("tutorialOK", "OK");
            PlayerPrefs.Save();

            // Start coroutine to transition after a brief delay
            __instance.StartCoroutine(SkipToTown());
        }

        private static IEnumerator SkipToTown()
        {
            // Wait one frame for scene to fully load
            yield return null;

            Plugin.BepinLogger.LogInfo("Loading town1 scene directly");
            SceneManager.LoadScene("town1");
        }
    }
}