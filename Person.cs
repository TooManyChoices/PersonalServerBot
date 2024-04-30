using System;
using System.IO;
using System.Timers;
using Newtonsoft.Json.Linq;

namespace Bot
{
    public static class Person 
    {
        private static JSONFileDictionary<string, dynamic> fileDictionary;

        private static DateTime lastModified;

        public static void Init()
        {
            fileDictionary = new JSONFileDictionary<string, dynamic>(Path.Combine(Program.StartupArgs[0], Config.GetSetting<string>("person")));

            Timer timer = new Timer {
                Interval = 30000
            };
            timer.Elapsed += CheckFile;
            CheckFile(null, null);
        }
        
        public static void CheckFile(object sender, ElapsedEventArgs e)
        {
            DateTime check = File.GetLastWriteTime(fileDictionary.path);
            if (lastModified != check)
            {
                lastModified = check;
                fileDictionary.ReadFile();
            }
        }

        public static string GetRandomItem(string key)
        {
            JArray array = fileDictionary.dictionary[key];
            string value = (string)array[Program.rng.Next(0, array.Count)];
            return value;
        }
    }
}