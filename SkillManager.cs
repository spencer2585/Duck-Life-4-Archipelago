using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DuckLife4Archipelago
{
    public class SkillManager
    {
        // Track raw training XP separately
        public Dictionary<string, float> TrainingXP = new Dictionary<string, float>
        {
            { "run", 0f },
            { "swim", 0f },
            { "fly", 0f },
            { "climb", 0f },
            { "jump", 0f },
            { "energy", 0f }
        };

        // Track which training levels we've already sent checks for
        public Dictionary<string, int> TrainingLevels = new Dictionary<string, int>
        {
            { "run", 0 },
            { "swim", 0 },
            { "fly", 0 },
            { "climb", 0 },
            { "jump", 0 },
            { "energy", 0 }
        };

        // Track AP-granted levels (what affects actual gameplay)
        public Dictionary<string, int> APLevels = new Dictionary<string, int>
        {
            { "run", 0 },
            { "swim", 0 },
            { "fly", 0 },
            { "climb", 0 },
            { "jump", 0 },
            { "energy", 0 }
        };

        private int skillSize;

        public SkillManager(int skillSize)
        {
            this.skillSize = skillSize;
        }

        // Add AP levels when receiving an item
        public void AddAPLevels(string skill, int amount)
        {
            APLevels[skill] += amount;
            Plugin.BepinLogger.LogInfo($"Received {amount} {skill} levels from AP. Total AP levels: {APLevels[skill]}");
            UpdateGameSkill(skill);
        }

        public Dictionary<string, int> LastSentMilestone = new Dictionary<string, int>
        {
            { "run", 0 },
            { "swim", 0 },
            { "fly", 0 },
            { "climb", 0 },
            { "jump", 0 },
            { "energy", 0 }
        };

        // Get list of all missed milestones for a skill (for sending checks)
        public List<int> GetMissedMilestones(string skill)
        {
            List<int> missedLevels = new List<int>();
            int currentTrainingLevel = TrainingLevels[skill];
            int lastSent = LastSentMilestone[skill];

            // Find all milestones between last sent and current
            for (int level = lastSent + skillSize; level <= currentTrainingLevel; level += skillSize)
            {
                missedLevels.Add(level);
            }

            return missedLevels;
        }
        // Get current level from AccessData (AP levels)
        public int GetCurrentGameLevel(string skill)
        {
            switch (skill)
            {
                case "run": return AccessData.runLevel;
                case "swim": return AccessData.swimLevel;
                case "fly": return AccessData.flyLevel;
                case "climb": return AccessData.climbLevel;
                case "jump": return AccessData.jumpLevel;
                case "energy": return AccessData.energyLevel;
                default: return 0;
            }
        }

        // Update the game's actual skill level to match AP levels
        private void UpdateGameSkill(string skill)
        {
            int level = APLevels[skill];

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
        // Add this constant at the top of the class
        private const string TRAINING_XP_KEY_PREFIX = "AP_TrainingXP_";

        // Modify AddTrainingXP to save after adding
        public void AddTrainingXP(string skill, float xp)
        {
            TrainingXP[skill] += xp;
            Plugin.BepinLogger.LogInfo($"Added {xp} training XP to {skill}. Total training XP: {TrainingXP[skill]}");

            // Update TrainingLevels based on XP (10 XP per level)
            TrainingLevels[skill] = Mathf.FloorToInt(TrainingXP[skill] / 10f);

            // Save to PlayerPrefs
            SaveTrainingXP(skill);
        }

        // Save training XP to disk
        private void SaveTrainingXP(string skill)
        {
            string key = TRAINING_XP_KEY_PREFIX + AccessData.currentDuckId + "_" + skill;
            PlayerPrefs.SetFloat(key, TrainingXP[skill]);
            PlayerPrefs.Save();
        }

        // Load training XP from disk (call this when SkillManager is created)
        public void LoadAllTrainingXP()
        {
            foreach (string skill in TrainingXP.Keys.ToList())
            {
                string key = TRAINING_XP_KEY_PREFIX + AccessData.currentDuckId + "_" + skill;
                if (PlayerPrefs.HasKey(key))
                {
                    TrainingXP[skill] = PlayerPrefs.GetFloat(key);
                    TrainingLevels[skill] = Mathf.FloorToInt(TrainingXP[skill] / 10f);
                    Plugin.BepinLogger.LogInfo($"Loaded training XP for {skill}: {TrainingXP[skill]} (level {TrainingLevels[skill]})");
                }
            }
        }
    }
}