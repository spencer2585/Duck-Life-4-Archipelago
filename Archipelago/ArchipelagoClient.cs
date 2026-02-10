using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using DuckLife4Archipelago.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DuckLife4Archipelago.Archipelago;

public class ArchipelagoClient
{
    public const string APVersion = "0.5.0";
    private const string Game = "Duck Life 4";

    public static bool Authenticated;
    private bool attemptingConnection;

    public static ArchipelagoData ServerData = new();
    private DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;



    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    /// <returns></returns>
    public void Connect()
    {
        if (Authenticated || attemptingConnection) return;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
            SetupSession();
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
        }

        TryConnect();
    }

    /// <summary>
    /// add handlers for Archipelago events
    /// </summary>
    private void SetupSession()
    {
        session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
        session.Items.ItemReceived += OnItemReceived;
        session.Socket.ErrorReceived += OnSessionErrorReceived;
        session.Socket.SocketClosed += OnSessionSocketClosed;
    }

    /// <summary>
    /// attempt to connect to the server with our connection info
    /// </summary>
    private void TryConnect()
    {
        try
        {
            // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
            ThreadPool.QueueUserWorkItem(
                _ => HandleConnectResult(
                    session.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems,
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: ServerData.NeedSlotData
                    )));
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            attemptingConnection = false;
        }
    }

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult(LoginResult result)
    {
        string outText;
        if (result.Successful)
        {
            var success = (LoginSuccessful)result;

            ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
            Plugin.SkillManager = new SkillManager(ServerData.SkillSize);
            DuckCreator.loadData();  // Make sure duck data is loaded
            if (DuckCreator.duckGroupData.Count > 0)
            {
                string duckId = DuckCreator.duckGroupData[0].ToString();
                AccessData.currentDuckId = duckId;
                Plugin.BepinLogger.LogInfo($"Set current duck to ID: {duckId}");
            }
            else
            {
                Plugin.BepinLogger.LogWarning("No ducks found! Using default duck ID");
                AccessData.currentDuckId = "1";  // Fallback
            }
            Plugin.SkillManager.LoadAllTrainingXP();
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
            {
                Patches.AccessDataPatches.CheckAndSendMissedMilestones();
            });
            Authenticated = true;

            DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
            session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
            outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

            ArchipelagoConsole.LogMessage(outText);
        }
        else
        {
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
            outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Plugin.BepinLogger.LogError(outText);

            Authenticated = false;
            Disconnect();
        }

        ArchipelagoConsole.LogMessage(outText);
        attemptingConnection = false;
    }

    /// <summary>
    /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    public void Disconnect()
    {
        if (session == null)
            return;

        Plugin.BepinLogger.LogDebug("disconnecting from server...");

        try
        {
            // Set authenticated to false first to stop any ongoing operations
            Authenticated = false;

            if (session?.Socket != null && session.Socket.Connected)
            {
                // Send disconnect synchronously
                session.Socket.DisconnectAsync().Wait(500); // Wait max 500ms
            }
        }
        catch (System.AggregateException)
        {
            // Expected during shutdown, socket already closed
        }
        catch (System.Exception e)
        {
            Plugin.BepinLogger.LogDebug($"Disconnect exception (expected): {e.Message}");
        }
        finally
        {
            session = null;
        }
    }

    public void SendMessage(string message)
    {
        session.Socket.SendPacketAsync(new SayPacket { Text = message });
    }

    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        var receivedItem = helper.DequeueItem();

        if (helper.Index <= ServerData.Index) return;

        ServerData.Index++;

        // Get the item name directly from the helper
        string itemName = session.Items.GetItemName(receivedItem.ItemId);
        Plugin.BepinLogger.LogInfo($"Received item: {itemName}");

        // Parse skill level items (e.g., "Running Level", "Swimming Level")
        if (itemName.EndsWith(" Level"))
        {
            // Extract skill name
            string skillDisplay = itemName.Replace(" Level", "");
            Plugin.BepinLogger.LogInfo($"Skill display name: {skillDisplay}");

            string skill = ConvertSkillName(skillDisplay);
            Plugin.BepinLogger.LogInfo($"Converted to internal skill: {skill}");

            if (!string.IsNullOrEmpty(skill))
            {
                // Each item gives SkillSize levels
                int amount = ServerData.SkillSize;
                Plugin.BepinLogger.LogInfo($"Granting {amount} levels of {skill}");
                Plugin.SkillManager.AddAPLevels(skill, amount);
            }
            else
            {
                Plugin.BepinLogger.LogWarning($"Could not convert skill name: {skillDisplay}");
            }
        }
        // Handle coin items
        else if (itemName == "Coins" || itemName.Contains("Coin"))
        {
            // Add 10 coins to pocket
            CoinSystem.coinInPocket += 10;
            Plugin.BepinLogger.LogInfo($"Received 10 coins! Total in pocket: {CoinSystem.coinInPocket}");
        }
        // Handle area access items
        else if (itemName.EndsWith(" Access"))
        {
            string area = itemName.Replace(" Access", "");
            Plugin.AreaUnlocks.Add(area);

            // Set the game's unlock flag
            int townNumber = GetTownNumber(area);
            if (townNumber > 0)
            {
                PlayerPrefs.SetInt("unlockmap" + townNumber, 1);
                PlayerPrefs.Save();
                Plugin.BepinLogger.LogInfo($"Unlocked area: {area} (town{townNumber})");
            }
        }
        // Handle tournament tickets
        else if (itemName.EndsWith(" Tournament Ticket"))
        {
            string area = itemName.Replace(" Tournament Ticket", "");
            Plugin.TournamentUnlocks.Add(area);

            // Set the game's ticket flag
            string ticketKey = GetTournamentTicketKey(area);
            if (ticketKey != null)
            {
                PlayerPrefs.SetString(ticketKey, "ok");
                PlayerPrefs.Save();
                Plugin.BepinLogger.LogInfo($"Unlocked tournament: {area} ({ticketKey})");
            }
        }
        // Handle keys
        // Handle keys
        else if (itemName.EndsWith(" Key"))
        {
            string keyColor = itemName.Replace(" Key", "").ToLower();
            Plugin.KeysCollected.Add(keyColor);

            // Set the game's key flag
            string keyNumber = GetKeyNumber(keyColor);
            if (keyNumber != null)
            {
                string keyPref = "key" + keyNumber;
                PlayerPrefs.SetString(keyPref, "ok");
                PlayerPrefs.Save();

                Plugin.BepinLogger.LogInfo($"Collected {keyColor} key ({keyPref})");

                // Check if player now has all 3 keys
                if (PlayerPrefs.HasKey("key1") && PlayerPrefs.HasKey("key2") && PlayerPrefs.HasKey("key3"))
                {
                    Plugin.BepinLogger.LogInfo("All 3 keys collected! Unlocking Fire Duck access");
                    PlayerPrefs.SetString("t6boxopened", "ok");
                    PlayerPrefs.SetString("t6removebg41", "ok");
                    PlayerPrefs.Save();

                    // If currently in town6, reload to show changes
                    if (Application.loadedLevelName == "town6")
                    {
                        Plugin.BepinLogger.LogInfo("In Volcano - reloading to show Fire Duck");
                        ScrollByPointer.LoadLevel("town6");
                    }
                }
            }
        }
        else if (itemName== "Victory")
        {
            session.SetGoalAchieved();
        }
    }

    private static string GetTournamentTicketKey(string area)
    {
        return area switch
        {
            "Grasslands" => "ticket1",
            "Swamp" => "ticket2",
            "Mountains" => "ticket3",
            "Glacier" => "ticket4",
            "City" => "ticket5",
            "Volcano" => null, // Volcano doesn't have a ticket, it's the final boss
            _ => null
        };
    }

    private static string GetKeyNumber(string color)
    {
        return color switch
        {
            "red" => "1",
            "orange" => "2",
            "green" => "3",
            _ => null
        };
    }

    // Helper to convert display names to internal skill names
    private string ConvertSkillName(string displayName)
    {
        switch (displayName)
        {
            case "Running": return "run";
            case "Swimming": return "swim";
            case "Flying": return "fly";
            case "Climbing": return "climb";
            case "Jumping": return "jump";
            case "Energy": return "energy";
            default: return null;
        }
    }

    /// <summary>
    /// something went wrong with our socket connection
    /// </summary>
    /// <param name="e">thrown exception from our socket</param>
    /// <param name="message">message received from the server</param>
    private void OnSessionErrorReceived(Exception e, string message)
    {
        Plugin.BepinLogger.LogError(e);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
        Disconnect();
    }
    public long GetLocationId(string locationName)
    {
        return session?.Locations.GetLocationIdFromName(Game, locationName) ?? -1;
    }

    public void SendLocationCheck(long locationId)
    {
        if (!Authenticated || session == null)
        {
            Plugin.BepinLogger.LogWarning($"Tried to send location check {locationId} but not connected!");
            return;
        }

        session.Locations.CompleteLocationChecksAsync(locationId);
        Plugin.BepinLogger.LogInfo($"Sent location check: {locationId}");
    }
    private static int GetTownNumber(string area)
    {
        return area switch
        {
            "Grasslands" => 1,
            "Swamp" => 2,
            "Mountains" => 3,
            "Glacier" => 4,
            "City" => 5,
            "Volcano" => 6,
            _ => 0
        };
    }

}