using DuckLife4Archipelago.Archipelago;
using HarmonyLib;
using System.Collections.Generic;

namespace DuckLife4Archipelago.Patches
{
    [HarmonyPatch(typeof(AccessData))]
    public class AccessDataPatches
    {
        [HarmonyPatch(nameof(AccessData.AddXp))]
        [HarmonyPrefix]
        public static bool AddXp_Prefix(string typeexp, float exp)
        {
            // Only intercept if we're connected
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return true;
            
            // Convert skill name
            string skill = ConvertSkillType(typeexp);
            if (string.IsNullOrEmpty(skill))
                return true;
            
            // Apply ExpModifier from slot data
            float modifiedExp = exp * ArchipelagoClient.ServerData.ExpModifier;
            
            Plugin.BepinLogger.LogInfo($"Intercepted training XP: {exp} for {skill}, modified to {modifiedExp} (modifier: {ArchipelagoClient.ServerData.ExpModifier})");
            
            // SET lastAddExp so endscreen knows which skill was trained
            AccessData.lastAddExp = typeexp;

            //make sure our gained EXP won't put us over the level cap (Purely cosmetic, our level won't go over 150 anyways but the gainedXp screen will show it without this)
            float currentXp = GetPreTrainingXp(skill);
            if (currentXp + modifiedExp > AccessData.LevelToExp(150))
                modifiedExp = AccessData.LevelToExp(150) - currentXp;
            if (modifiedExp < 0f)
                modifiedExp = 0f;
            
            // Add to our separate training XP tracker with modified value
            Plugin.SkillManager.AddTrainingXp(skill, modifiedExp);
            
            // Check if we hit any milestones and send location checks
            List<int> missedMilestones = Plugin.SkillManager.GetMissedMilestones(skill);
            foreach (int level in missedMilestones)
            {
                Plugin.BepinLogger.LogInfo($"Sending training check for {skill} at level {level}");
                SendLocationCheckForLevel(skill, level);
                Plugin.SkillManager.LastSentMilestone[skill] = level;
            }
            
            // Return false to prevent original AddXp from running
            return false;
        }

        // Convert AccessData skill types to our internal names
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

        // Send a location check for a specific level
        private static void SendLocationCheckForLevel(string skill, int level)
        {
            string skillDisplay = ConvertSkillToDisplay(skill);
            string locationName = $"{skillDisplay} Training {level}";

            long locationId = Plugin.ArchipelagoClient.GetLocationId(locationName);

            if (locationId > 0)
            {
                Plugin.BepinLogger.LogInfo($"Sending location check: {locationName} (ID: {locationId})");
                Plugin.ArchipelagoClient.SendLocationCheck(locationId);
            }
            else
            {
                Plugin.BepinLogger.LogError($"Could not find location ID for: {locationName}");
            }
        }

        // Convert internal skill names to display names
        private static string ConvertSkillToDisplay(string skill)
        {
            switch (skill)
            {
                case "run": return "Running";
                case "swim": return "Swimming";
                case "fly": return "Flying";
                case "climb": return "Climbing";
                case "jump": return "Jumping";
                case "energy": return "Energy";
                default: return skill;
            }
        }

        // Check for backlog on connection
        public static void CheckAndSendMissedMilestones()
        {
            if (!ArchipelagoClient.Authenticated || Plugin.SkillManager == null)
                return;

            string[] skills = { "run", "swim", "fly", "climb", "jump", "energy" };

            foreach (string skill in skills)
            {
                List<int> missedLevels = Plugin.SkillManager.GetMissedMilestones(skill);
                foreach (int level in missedLevels)
                {
                    Plugin.BepinLogger.LogInfo($"Sending backlog check for {skill} at level {level}");
                    SendLocationCheckForLevel(skill, level);
                    Plugin.SkillManager.LastSentMilestone[skill] = level;  // CHANGED
                }
            }
        }
        // Track XP at the start of each training session
        private static Dictionary<string, float> _preTrainingXp = new Dictionary<string, float>
        {
            { "run", 0f },
            { "swim", 0f },
            { "fly", 0f },
            { "climb", 0f },
            { "jump", 0f },
            { "energy", 0f }
        };

        public static float GetPreTrainingXp(string skill)
        {
            return _preTrainingXp.ContainsKey(skill) ? _preTrainingXp[skill] : 0f;
        }

        public static void SavePreTrainingXp(string skill)
        {
            if (Plugin.SkillManager != null)
            {
                _preTrainingXp[skill] = Plugin.SkillManager.TrainingXp[skill];
                Plugin.BepinLogger.LogInfo($"Saved pre-training XP for {skill}: {_preTrainingXp[skill]}");
            }
            else
            {
                Plugin.BepinLogger.LogWarning("SkillManager is null, cannot save pre-training XP");
            }
        }
    }
}