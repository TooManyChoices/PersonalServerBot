using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bot
{
    class Database
    {
        private static JSONFileDictionary<string, ServerData> fileDictionary;

        public static void Init()
        {
            fileDictionary = new JSONFileDictionary<string, ServerData>(
                Path.Combine(Program.GetConfigPath(), Config.GetSetting<string>("database"))
            );
        }

        public static ServerData ServerDataFromId(ulong uid)
        {
            var sid = uid.ToString();
            return fileDictionary.dictionary[sid];
        }
        public static ServerData[] GetAllServers()
        {
            return fileDictionary.dictionary.Values.ToArray();
        }
        public static IEnumerable<KeyValuePair<string, ServerData>> GetServersEnumerable()
        {
            return fileDictionary.dictionary.AsEnumerable();
        }
        public static void UpdateServerData(ulong uid, ServerData serverData)
        {
            fileDictionary.dictionary[uid.ToString()] = serverData;
            fileDictionary.SaveFile();
        }
    }

    struct ServerData 
    {
        public RoleData linked_roles;
        public ChannelData linked_channels;
        public Dictionary<string, ulong> personal_roles;
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