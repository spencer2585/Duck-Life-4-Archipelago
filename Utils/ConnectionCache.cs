using System.IO;
using Newtonsoft.Json;
using DuckLife4Archipelago.Archipelago;

namespace DuckLife4Archipelago.Utils;

public static class ConnectionCache
{
    private static readonly string CachePath = Path.Combine(
        Path.GetDirectoryName(typeof(ConnectionCache).Assembly.Location),
        "lastconnection.json"
    );

    public static void Save(string uri, string slotName, string password)
    {
        var data = new CacheData { Uri = uri, SlotName = slotName, Password = password };
        File.WriteAllText(CachePath, JsonConvert.SerializeObject(data));
    }

    private class CacheData
    {
        public string Uri { get; set; }
        public string SlotName { get; set; }
        public string Password { get; set; }
    }

    public static void Load()
    {
        if (!File.Exists(CachePath)) return;

        var data = JsonConvert.DeserializeObject<CacheData>(File.ReadAllText(CachePath));
        if (data == null) return;

        ArchipelagoClient.ServerData.Uri      = data.Uri;
        ArchipelagoClient.ServerData.SlotName = data.SlotName;
        ArchipelagoClient.ServerData.Password = data.Password;
    }
}