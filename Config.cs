using System;
using System.Collections.Generic;

namespace Bot
{
    public static class Config 
    {
        private static JSONFileDictionary<string, dynamic> fileDictionary;
        private static Dictionary<string, dynamic> overrides;

        public static void Init()
        {
            fileDictionary = new JSONFileDictionary<string, dynamic>(Program.StartupArgs[0]);
            overrides = new Dictionary<string, dynamic>();
            if (Program.StartupArgs.Length > 1)
            {
                for (int i = 1; i < Program.StartupArgs.Length; i++)
                {
                    string arg = Program.StartupArgs[i];
                    var split = arg.Split('=');
                    if (split[1][0] != '"')
                    {
                        if (split[1]=="true") 
                            overrides[split[0]] = true;
                        else if (split[1]=="false") 
                            overrides[split[0]] = false;
                        else if (split[1].Contains('.')) 
                            overrides[split[0]] = float.Parse(split[1]);
                        else 
                            overrides[split[0]] = int.Parse(split[1]);
                    }
                }
            }
        }

        public static bool HasSetting(string key)
        {
            if (overrides.ContainsKey(key)) return true;
            else return fileDictionary.dictionary.ContainsKey(key);
        }

        public static T GetSetting<T>(string key)
        {
            if (overrides.ContainsKey(key)) return (T)overrides[key];
            else return (T)fileDictionary.dictionary[key];
        }

        public static T GetSetting<T>(string key, T defaultValue)
        {
            return HasSetting(key) ? GetSetting<T>(key) : defaultValue;
        }
    }
}