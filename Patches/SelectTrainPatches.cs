using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(selecttrain))]
    public class SelectTrainPatches
    {
        // Initialize the display when the scene loads
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix()
        {
            // Only update if connected
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return;

            // Clear the EXP text on start (it will update when user selects)
            GameObject expText = GameObject.Find("EXP Text");
            if (expText != null)
            {
                expText.GetComponent<Text>().text = "Select a training";
            }
        }
        [HarmonyPatch("select")]
        [HarmonyPrefix]
        public static void Select_Prefix(selecttrain __instance)
        {
            // Only track if connected
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return;

            // Check if this is the train button (it has name "B Train")
            if (__instance.gameObject.name != "B Train")
                return;

            // Figure out which skill we're about to train
            string currentMinigame = selecttrain.currentminigame.ToLower();
            string skill = null;

            if (currentMinigame.Contains("run")) skill = "run";
            else if (currentMinigame.Contains("swim")) skill = "swim";
            else if (currentMinigame.Contains("fly")) skill = "fly";
            else if (currentMinigame.Contains("climb")) skill = "climb";
            else if (currentMinigame.Contains("jump")) skill = "jump";

            if (skill != null)
            {
                Plugin.BepinLogger.LogInfo($"About to train {skill}, saving pre-training XP");
                AccessDataPatches.SavePreTrainingXP(skill);
            }
        }
        // Patch the select method to show training levels
        [HarmonyPatch("select")]
        [HarmonyPostfix]
        public static void Select_Postfix(selecttrain __instance)
        {
            // Only override if connected
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return;

            // Check if this is a training selection (not the train button or back button)
            if (__instance.gameObject.name == "B Train")
                return;
            if (__instance.gameObject.name == "B Back")
                return;

            // Get the skill being selected
            string[] parts = __instance.gameObject.name.Split(' ');
            if (parts.Length < 3)
                return;

            int index = System.Convert.ToInt32(parts[2]);
            string skillType = GetSkillType(index);

            if (string.IsNullOrEmpty(skillType))
                return;

            string skill = ConvertSkillType(skillType);
            if (string.IsNullOrEmpty(skill))
                return;

            // Get training level from SkillManager
            int trainingLevel = Mathf.Min(Plugin.SkillManager.TrainingLevels[skill], 150);

            // Get level cap
            float levelCap = PlayerPrefs.HasKey("EndSinglePlayerMode") ? 250f : TownUI._levelBlock;

            // Update the UI
            GameObject expText = GameObject.Find("EXP Text");
            GameObject expGage = GameObject.Find("Exp Gage");

            if (expText != null)
            {
                string skillName = GetSkillNameFromIndex(index);
                expText.GetComponent<Text>().text = $"{skillName} level: {trainingLevel}/{levelCap}";
            }

            if (expGage != null)
            {
                expGage.GetComponent<Image>().fillAmount = trainingLevel / levelCap;
            }

            Plugin.BepinLogger.LogInfo($"Updated select screen for {skill}: level {trainingLevel}");
        }

        // Map index to skill type (based on trainingname2 array in selecttrain)
        private static string GetSkillType(int index)
        {
            // This maps to the trainingname2 array structure
            switch (index)
            {
                case 1: case 6: case 11: case 25: return AccessData.Skill.run;
                case 2: case 7: case 12: case 26: return AccessData.Skill.swim;
                case 3: case 8: case 13: case 27: return AccessData.Skill.fly;
                case 4: case 9: case 14: case 28: return AccessData.Skill.climb;
                case 5: case 10: case 15: case 29: return AccessData.Skill.jump;
                default: return null;
            }
        }

        private static string ConvertSkillType(string typeexp)
        {
            if (typeexp == AccessData.Skill.run) return "run";
            if (typeexp == AccessData.Skill.swim) return "swim";
            if (typeexp == AccessData.Skill.fly) return "fly";
            if (typeexp == AccessData.Skill.climb) return "climb";
            if (typeexp == AccessData.Skill.jump) return "jump";
            return null;
        }

        private static string GetSkillNameFromIndex(int index)
        {
            string skillType = GetSkillType(index);
            if (skillType == AccessData.Skill.run) return "Running";
            if (skillType == AccessData.Skill.swim) return "Swimming";
            if (skillType == AccessData.Skill.fly) return "Flying";
            if (skillType == AccessData.Skill.climb) return "Climbing";
            if (skillType == AccessData.Skill.jump) return "Jumping";
            return "Unknown";
        }
    }
}