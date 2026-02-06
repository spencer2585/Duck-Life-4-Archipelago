using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using UnityEngine;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(minigame_endscene))]
    public class MinigameEndscenePatches
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]  // Changed to Postfix - let original run, then override values
        public static void Start_Postfix(minigame_endscene __instance)
        {
            Plugin.BepinLogger.LogInfo("=== Endscreen Start_Postfix called ===");

            Plugin.BepinLogger.LogInfo($"Authenticated: {ArchipelagoClient.Authenticated}");
            Plugin.BepinLogger.LogInfo($"SkillManager null: {Plugin.SkillManager == null}");

            // Only override if connected
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
            {
                Plugin.BepinLogger.LogWarning("Exiting early - not connected or no SkillManager");
                return;
            }

            string skillType = AccessData.lastAddExp;
            Plugin.BepinLogger.LogInfo($"skillType: {skillType}");

            string skill = ConvertSkillType(skillType);
            Plugin.BepinLogger.LogInfo($"converted skill: {skill}");

            if (string.IsNullOrEmpty(skill))
            {
                Plugin.BepinLogger.LogWarning("Skill is null or empty, exiting");
                return;
            }
            // Get XP values
            float oldTrainingXP = AccessDataPatches.GetPreTrainingXP(skill);
            float currentTrainingXP = Plugin.SkillManager.TrainingXP[skill];

            int oldTrainingLevel = Mathf.Min(Mathf.FloorToInt(oldTrainingXP / 10f), 150);
            int currentTrainingLevel = Mathf.Min(Mathf.FloorToInt(currentTrainingXP / 10f), 150);

            Plugin.BepinLogger.LogInfo($"Endscreen: XP {oldTrainingXP} -> {currentTrainingXP}, Levels {oldTrainingLevel} -> {currentTrainingLevel}");

            // Override the values AFTER original Start runs
            __instance.OldLevel = oldTrainingLevel;
            __instance.CurrentLevel = currentTrainingLevel;
            __instance.oldMax = 10f;
            __instance.newMax = 10f;
            __instance.o_exp = oldTrainingXP % 10f;
            __instance.c_exp = currentTrainingXP % 10f;
            __instance.levelUp = (oldTrainingLevel < currentTrainingLevel);

            // Reset the gauge to start animation from correct position
            __instance.gage.fillAmount = __instance.o_exp / __instance.oldMax;

            // Update UI texts
            __instance.score.text = Mathf.Floor(currentTrainingXP - oldTrainingXP).ToString();
            __instance.texttype.text = AccessData.typeToString(skillType) + ": LV " + oldTrainingLevel.ToString();
        }

        private static string ConvertSkillType(string typeexp)
        {
            if (typeexp == AccessData.Skill.run) return "run";
            if (typeexp == AccessData.Skill.swim) return "swim";
            if (typeexp == AccessData.Skill.fly) return "fly";
            if (typeexp == AccessData.Skill.climb) return "climb";
            if (typeexp == AccessData.Skill.jump) return "jump";
            if (typeexp == AccessData.Skill.energy) return "energy";
            return null;
        }
    }
}