using BepInEx;
using BepInEx.Logging;
using DuckLife4Archipelago.Archipelago;
using DuckLife4Archipelago.Utils;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DuckLife4Archipelago;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGUID = "com.yourName.projectName";
    public const string PluginName = "DuckLife4Archipelago";
    public const string PluginVersion = "1.0.0";

    public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
    private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
    public static ManualLogSource BepinLogger;
    public static ArchipelagoClient ArchipelagoClient;
    public static SkillManager SkillManager;
    public static HashSet<string> AreaUnlocks = new HashSet<string>();
    public static HashSet<string> TournamentUnlocks = new HashSet<string>();
    public static HashSet<string> KeysCollected = new HashSet<string>();

    private void Awake()
    {
        // Plugin startup logic
        BepinLogger = Logger;

        var harmony = new Harmony("com.yourname.ducklife4archipelago");
        harmony.PatchAll();

        var patchedMethods = harmony.GetPatchedMethods();
        foreach (var method in patchedMethods)
        {
            BepinLogger.LogInfo($"Patched: {method.DeclaringType?.Name}.{method.Name}");
        }

        ArchipelagoClient = new ArchipelagoClient();
        ArchipelagoConsole.Awake();

        ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
        Application.quitting += OnQuit;
    }
    private void OnQuit()
    {
        BepinLogger.LogInfo("Application quitting - cleaning up");
        if (ArchipelagoClient != null)
        {
            ArchipelagoClient.Disconnect();
        }
    }
    private void OnDestroy()
    {
        // Disconnect from Archipelago when game closes
        if (ArchipelagoClient != null)
        {
            BepinLogger.LogInfo("Game closing, disconnecting from Archipelago...");
            ArchipelagoClient.Disconnect();
        }
    }

    private void OnApplicationQuit()
    {
        // Also handle application quit
        if (ArchipelagoClient != null)
        {
            BepinLogger.LogInfo("Application quitting, disconnecting from Archipelago...");
            ArchipelagoClient.Disconnect();
        }
    }
    private void OnGUI()
    {
        // show the mod is currently loaded in the corner
        GUI.Label(new Rect(16, 16, 300, 20), ModDisplayInfo);
        ArchipelagoConsole.OnGUI();

        string statusMessage;
        // show the Archipelago Version and whether we're connected or not
        if (ArchipelagoClient.Authenticated)
        {
            // if your game doesn't usually show the cursor this line may be necessary
            // Cursor.visible = false;

            statusMessage = " Status: Connected";
            GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
        }
        else
        {
            // if your game doesn't usually show the cursor this line may be necessary
            // Cursor.visible = true;

            statusMessage = " Status: Disconnected";
            GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
            GUI.Label(new Rect(16, 70, 150, 20), "Host: ");
            GUI.Label(new Rect(16, 90, 150, 20), "Player Name: ");
            GUI.Label(new Rect(16, 110, 150, 20), "Password: ");

            ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 70, 150, 20),
                ArchipelagoClient.ServerData.Uri);
            ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 90, 150, 20),
                ArchipelagoClient.ServerData.SlotName);
            ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 110, 150, 20),
                ArchipelagoClient.ServerData.Password);

            // requires that the player at least puts *something* in the slot name
            if (GUI.Button(new Rect(16, 130, 100, 20), "Connect") &&
                !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
            {
                ArchipelagoClient.Connect();
            }
        }
        // this is a good place to create and add a bunch of debug buttons
    }
}