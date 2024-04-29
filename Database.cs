using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Bot
{
    class Database
    {
        public static Dictionary<string, ServerData> serverDatabase { get; private set; }
        private static string databasePath;

        public static void Init()
        {
            databasePath = Path.Combine(Program.StartupArgs[0], Config.GetSetting<string>("database"));
            serverDatabase = JsonConvert.DeserializeObject<Dictionary<string, ServerData>>(
                File.ReadAllText(databasePath)
            );
        }

        public static ServerData GuildIdToServerData(ulong uid)
        {
            var sid = uid.ToString();
            return serverDatabase[sid];
        }
        public static ServerData[] GetAllServerData()
        {
            return serverDatabase.Values.ToArray();
        }

        public static void SaveToFile()
        {
            File.WriteAllText(
                databasePath,
                System.Text.Json.JsonSerializer.Serialize(serverDatabase)
            );
        }
    }

    struct ServerData 
    {
        public RoleData linked_roles;
        public ChannelData linked_channels;
    }
    struct RoleData 
    {
        public ulong member;
    }
    struct ChannelData 
    {
        public ulong on_connect;
        public ulong insane_ramblings;
    }
}