using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Bot
{
    public static class Config 
    {
        public static Dictionary<string, dynamic> configDictionary;

        public static void Init()
        {
            configDictionary = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(File.ReadAllText(Program.StartupArgs[0]));
        }

        public static bool HasSetting(string key)
        {
            return configDictionary.ContainsKey(key);
        }

        public static T GetSetting<T>(string key)
        {
            return (T)configDictionary[key];
        }

        public static T GetSetting<T>(string key, T defaultValue)
        {
            return HasSetting(key) ? (T)configDictionary[key] : defaultValue;
        }
    }
}