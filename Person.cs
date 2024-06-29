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
            fileDictionary = new JSONFileDictionary<string, dynamic>(Path.Combine(Program.GetConfigPath(), Config.GetSetting<string>("person")));

            Timer timer = new Timer {
                Interval = 30000
            };
            timer.Elapsed += (object a, ElapsedEventArgs b) => CheckFile();
            CheckFile();
            timer.Start();
        }
        
        public static void CheckFile()
        {
            DateTime check = File.GetLastWriteTime(fileDictionary.path);
            if (lastModified != check)
            {
                lastModified = check;
                fileDictionary.ReadFile();
            }
        }

        public static string GetRandomItem(string key, params string[] args)
        {
            JArray array = fileDictionary.dictionary[key];
            string value = (string)array[Program.rng.Next(0, array.Count)];
            for (int i = 0; i < args.Length; i++)
            {
                value = value.Replace($"[{i}]", args[i]);            
            }
            return value;
        }
    }
}