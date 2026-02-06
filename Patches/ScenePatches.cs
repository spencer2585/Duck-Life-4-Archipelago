using DuckLife4Archipelago.Archipelago;
using HarmonyLib;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(Transition))]
    public class ScenePatches
    {
        // Patch when scenes are loaded
        [HarmonyPatch("ToScene")]
        [HarmonyPrefix]
        public static void ToScene_Prefix(string sceneName)
        {
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return;

            Plugin.BepinLogger.LogInfo($"Loading scene: {sceneName}");

            if (sceneName == "cutscene_ending")
            {
                Plugin.BepinLogger.LogInfo("Fire Duck defeated! Sending location check...");

                long locationId = Plugin.ArchipelagoClient.GetLocationId("Volcano - Fire Duck Race Won");

                if (locationId > 0)
                {
                    Plugin.BepinLogger.LogInfo($"Sending Fire Duck location check (ID: {locationId})");
                    Plugin.ArchipelagoClient.SendLocationCheck(locationId);
                }
                else
                {
                    Plugin.BepinLogger.LogError("Could not find location ID for Fire Duck race");
                }

                return;
            }

            // Detect training minigame scenes and save pre-training XP
            string skill = null;

            string lowerScene = sceneName.ToLower();
            if (lowerScene.Contains("run"))
                skill = "run";
            else if (lowerScene.Contains("swim"))
                skill = "swim";
            else if (lowerScene.Contains("fly"))
                skill = "fly";
            else if (lowerScene.Contains("climb"))
                skill = "climb";
            else if (lowerScene.Contains("jump"))
                skill = "jump";

            if (skill != null)
            {
                Plugin.BepinLogger.LogInfo($"Entering training for {skill}, saving pre-training XP");
                AccessDataPatches.SavePreTrainingXP(skill);
            }
        }
    }
}