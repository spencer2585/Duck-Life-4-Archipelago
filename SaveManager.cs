using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;


namespace DuckLife4Archipelago;

public class SaveManager
{
    private static readonly string SaveDirectory =
        Path.Combine(Path.GetDirectoryName(typeof(SaveManager).Assembly.Location), "Saves");
    
    private enum PrefType {String, Int, Float }

    private static readonly Dictionary<string, PrefType> KnownKeys = new Dictionary<string, PrefType>
    {
        // Duck data
        { "duckGroupData", PrefType.String },
        { "last_duck_id", PrefType.Int },
        // Progression
        { "unlockmap1", PrefType.Int },
        { "unlockmap2", PrefType.Int },
        { "unlockmap3", PrefType.Int },
        { "unlockmap4", PrefType.Int },
        { "unlockmap5", PrefType.Int },
        { "unlockmap6", PrefType.Int },
        { "ticket1", PrefType.String },
        { "ticket2", PrefType.String },
        { "ticket3", PrefType.String },
        { "ticket4", PrefType.String },
        { "ticket5", PrefType.String },
        { "key1", PrefType.String },
        { "key2", PrefType.String },
        { "key3", PrefType.String },
        { "town", PrefType.String },
        { "tutorialOK", PrefType.String },
        { "t6boxopened", PrefType.String },
        { "t6removebg41", PrefType.String },
        // Misc
        { "GlobalSave.coin", PrefType.Int },
        { "checkIzzyName", PrefType.String },
        { "CheckingOldSave", PrefType.String },
        { "setting_sound_e", PrefType.String },
        { "setting_sound", PrefType.String },
        { "useKeyboardCo", PrefType.String },
        //AP Specific additions
        {"filename", PrefType.String },
    };

    private static readonly Dictionary<string, PrefType> PerDuckKeys = new Dictionary<string, PrefType>
    {
        // Duck data
        { "duckData{0}_id", PrefType.String },
        { "duckData{0}_name", PrefType.String },
        { "duckData{0}_hair", PrefType.Int },
        { "duckData{0}_eye", PrefType.Int },
        { "duckData{0}_eyecolor", PrefType.String },
        { "duckData{0}_texture", PrefType.Int },
        { "duckData{0}_texturecolor", PrefType.String },
        { "duckData{0}_hat", PrefType.Int },
        { "duckData{0}_costume", PrefType.Int },
        {"duckData{0}_bodycolor", PrefType.String },
        // Skill XP
        { "{0}.run_exp", PrefType.Float },
        { "{0}.climb_exp", PrefType.Float },
        { "{0}.fly_exp", PrefType.Float },
        { "{0}.int_exp", PrefType.Float },
        { "{0}.jump_exp", PrefType.Float },
        { "{0}.swim_exp", PrefType.Float },
        { "{0}.en_exp", PrefType.Float },
        //AP XP
        {"AP_TrainingXP_{0}_run", PrefType.Float },
        {"AP_TrainingXP_{0}_swim", PrefType.Float },
        {"AP_TrainingXP_{0}_fly", PrefType.Float },
        {"AP_TrainingXP_{0}_climb", PrefType.Float },
        {"AP_TrainingXP_{0}_jump", PrefType.Float },
        {"AP_TrainingXP_{0}_energy", PrefType.Float },
    };

    public static string GetSaveName(string seed, string slotName)
    {
        return $"{seed}_{slotName}.json";
    }

    private static string GetSavePath(string seed, string slotName)
    {
        string fileName = GetSaveName(seed, slotName);
        return Path.Combine(SaveDirectory, fileName);
    }

    private static string GetVanillaSavePath()
    {
        return Path.Combine(SaveDirectory, "vanilla.json");
    }

    public static void SaveByName(string name)
    {
        SaveToFile(Path.Combine(SaveDirectory, name));
    }

    private static void SaveToFile(string filePath)
    {
        Dictionary<string, string> prefs = new Dictionary<string, string>();

        foreach (var kvp in KnownKeys)
        { 
            if (!PlayerPrefs.HasKey(kvp.Key)) continue;

            string value = kvp.Value switch
            {
                PrefType.Int => PlayerPrefs.GetInt(kvp.Key).ToString(),
                PrefType.Float => PlayerPrefs.GetFloat(kvp.Key).ToString(),
                _ => PlayerPrefs.GetString(kvp.Key)
            };
            
            prefs[kvp.Key] = value;
        }
        int[] indexes = [1];
        foreach (int index in indexes)
        {
            foreach (var key in PerDuckKeys)
            {
                string realKey = string.Format(key.Key,index);
                if (!PlayerPrefs.HasKey(realKey)) continue;
                string value = key.Value switch
                {
                    PrefType.Int => PlayerPrefs.GetInt(realKey).ToString(),
                    PrefType.Float => PlayerPrefs.GetFloat(realKey).ToString(),
                    _ => PlayerPrefs.GetString(realKey)
                };
                
                prefs[realKey] = value;
            }
        }
        
        string json = JsonConvert.SerializeObject(new { playerPrefs = prefs }, Formatting.Indented);
        Directory.CreateDirectory(SaveDirectory);
        File.WriteAllText(filePath, json);
        Plugin.BepinLogger.LogInfo($"SaveManager: saved to {filePath}");
    }

    private static void ClearManagedKeys()
    {
        int[] indexes = [1];
        foreach (int index in indexes)
        {
            foreach (var key in PerDuckKeys.Keys)
            {
                string realKey = string.Format(key,index);
                PlayerPrefs.DeleteKey(realKey);
            }
        }
        foreach (var key in KnownKeys.Keys)
        {
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
    }

    public static void LoadFromFile(string filePath)
    {
        ClearManagedKeys();
        if (!File.Exists(filePath))
        {
            Plugin.BepinLogger.LogInfo($"SaveManager: No save file found at {filePath}, starting fresh.");
            return;
        }

        string json = File.ReadAllText(filePath);
        var wrapper = JsonConvert.DeserializeObject<SaveFile>(json);

        if (wrapper == null || wrapper.playerPrefs == null)
        {
            Plugin.BepinLogger.LogWarning($"SaveManager: Failed to deserialize {filePath}");
            return;
        }
        
        Dictionary<string, PrefType> fullKeys = new Dictionary<string, PrefType>(KnownKeys);
        int[] indexes = [1];
        foreach (int index in indexes)
        {
            foreach (var key in PerDuckKeys)
            {
                string realKey = string.Format(key.Key,index);
                fullKeys.Add(realKey, key.Value);
            }
        }

        foreach (KeyValuePair<string, string> kvp in wrapper.playerPrefs)
        {
            Plugin.BepinLogger.LogInfo($"Loading: {kvp.Key}: {kvp.Value}");
            if (!fullKeys.TryGetValue(kvp.Key, out PrefType type)) continue;

            switch (type)
            {
                case PrefType.Int:
                    if (int.TryParse(kvp.Value, out int intVal))
                        PlayerPrefs.SetInt(kvp.Key, intVal);
                    break;
                case PrefType.Float:
                    if (float.TryParse(kvp.Value, out float floatVal))
                        PlayerPrefs.SetFloat(kvp.Key, floatVal);
                    break;
                default:
                    PlayerPrefs.SetString(kvp.Key, kvp.Value);
                    break;
            }
        }

        PlayerPrefs.Save();
        Plugin.BepinLogger.LogInfo($"SaveManager: Loaded from {filePath}");
    }
    
    public static void SaveVanilla() => SaveToFile(GetVanillaSavePath());
    
    public static void SaveSlot(string seed, string slotname) => SaveToFile(GetSavePath(seed, slotname));
    
    public static void LoadVanilla() => LoadFromFile(GetVanillaSavePath());
    
    public static void LoadSlot(string seed, string slotname) => LoadFromFile(GetSavePath(seed, slotname));
}

public class SaveFile
{
    public Dictionary<string, string> playerPrefs { get; set; }
}