using System.Collections.Generic;

namespace Bot
{
    public static class Config 
    {
        private static JSONFileDictionary<string, dynamic> fileDictionary;

        public static void Init()
        {
            fileDictionary = new JSONFileDictionary<string, dynamic>(Program.StartupArgs[0]);
        }

        public static bool HasSetting(string key)
        {
            return fileDictionary.dictionary.ContainsKey(key);
        }

        public static T GetSetting<T>(string key)
        {
            return (T)fileDictionary.dictionary[key];
        }

        public static T GetSetting<T>(string key, T defaultValue)
        {
            return HasSetting(key) ? (T)fileDictionary.dictionary[key] : defaultValue;
        }
    }
}