using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot
{
    public static class Person 
    {
        public static Dictionary<string, dynamic> personDictionary;

        public static void Init()
        {
            personDictionary = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(
                File.ReadAllText(Path.Combine(Program.StartupArgs[0], Config.GetSetting<string>("person")))
            );
        }

        public static string GetRandomItem(string key)
        {
            JArray array = personDictionary[key];
            string value = (string)array[Program.rng.Next(0, array.Count-1)];
            return value;
        }
    }
}