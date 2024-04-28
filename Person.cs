using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot
{
    public static class Person 
    {
        public static Dictionary<string, dynamic> personDictionary;

        private static DateTime lastModified;
        private static string personPath;

        public static void Init()
        {
            personPath = Path.Combine(Program.StartupArgs[0], Config.GetSetting<string>("person"));
            Timer timer = new Timer {
                Interval = 30000
            };
            timer.Elapsed += CheckFile;
            CheckFile(null, null);
        }
        
        public static void CheckFile(object sender, ElapsedEventArgs e)
        {
            DateTime check = File.GetLastWriteTime(personPath);
            if (lastModified != check)
            {
                lastModified = check;
                personDictionary = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(File.ReadAllText(personPath));
            }
        }

        public static string GetRandomItem(string key)
        {
            JArray array = personDictionary[key];
            string value = (string)array[Program.rng.Next(0, array.Count-1)];
            return value;
        }
    }
}