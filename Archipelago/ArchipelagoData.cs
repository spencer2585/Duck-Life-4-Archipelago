using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DuckLife4Archipelago.Archipelago;

public class ArchipelagoData
{
    public string Uri;
    public string SlotName;
    public string Password;
    public int Index;

    public List<long> CheckedLocations;

    public int SkillSize { get; private set; }
    public float ExpModifier { get; private set; }

    /// <summary>
    /// seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    /// load is valid to the room it's connecting to.
    /// </summary>
    private string seed;

    private Dictionary<string, object> slotData;

    public bool NeedSlotData => slotData == null;

    public ArchipelagoData()
    {
        Uri = "Archipelago.gg:38281";
        SlotName = "Player1";
        CheckedLocations = new();
    }

    public ArchipelagoData(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
        CheckedLocations = new();
    }

    /// <summary>
    /// assigns the slot data and seed to our data handler. any necessary setup using this data can be done here.
    /// </summary>
    /// <param name="roomSlotData">slot data of your slot from the room</param>
    /// <param name="roomSeed">seed name of this session</param>
    public void SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
    {
        slotData = roomSlotData;
        seed = roomSeed;

        if (slotData.ContainsKey("SkillSize"))
        {
            SkillSize = Convert.ToInt32(slotData["SkillSize"]);
        }
        else
        {
            SkillSize = 5; // default fallback
        }

        // Get ExpModifier from slot data
        if (slotData.ContainsKey("XpModifier"))
        {
            ExpModifier = Convert.ToSingle(slotData["XpModifier"]);
        }
        else
        {
            ExpModifier = 1.0f; // default (no modification)
        }

        Plugin.BepinLogger.LogInfo($"Connected with SkillSize: {SkillSize}, ExpModifier: {ExpModifier}");

    }

    /// <summary>
    /// returns the object as a json string to be written to a file which you can then load
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}