using DuckLife4Archipelago.Archipelago;
using HarmonyLib;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(DuckManagement))]
    public class DuckManagementPatches
    {
        // Patch when a duck is selected to re-apply AP skill levels
        [HarmonyPatch("onSelect")]
        [HarmonyPostfix]
        public static void OnSelect_Postfix(int duckId)
        {
            // Only re-apply if we're connected and have SkillManager
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return;

            Plugin.BepinLogger.LogInfo($"Duck {duckId} selected, re-applying AP skill levels");

            // Re-apply all AP skill levels to the newly loaded duck
            foreach (var skill in Plugin.SkillManager.APLevels.Keys)
            {
                int level = Plugin.SkillManager.APLevels[skill];
                if (level > 0)
                {
                    Plugin.BepinLogger.LogInfo($"Re-applying {skill}: {level} levels");

                    // Directly set the levels using AccessData
                    switch (skill)
                    {
                        case "run":
                            AccessData.runLevel = level;
                            break;
                        case "swim":
                            AccessData.swimLevel = level;
                            break;
                        case "fly":
                            AccessData.flyLevel = level;
                            break;
                        case "climb":
                            AccessData.climbLevel = level;
                            break;
                        case "jump":
                            AccessData.jumpLevel = level;
                            break;
                        case "energy":
                            AccessData.energyLevel = level;
                            break;
                    }
                }
            }
        }
    }
}