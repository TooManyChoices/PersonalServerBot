using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;

namespace Bot
{
    public class JSONFileDictionary<TKey, TValue>
    {
        public string path;
        public Dictionary<TKey, TValue> dictionary;

        /// <summary>
        /// Create a new JSONFileDictionary.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public JSONFileDictionary(string path)
        {
            this.path = path;
            ReadFile();
        }

        /// <summary>
        /// Refresh the dictionary by reading the file.
        /// </summary>
        public void ReadFile()
        {
            dictionary = JsonConvert.DeserializeObject<Dictionary<TKey,TValue>>(
                File.ReadAllText(path)
            );
        }

        /// <summary>
        /// Serialize the dictionary and save to the path file.
        /// </summary>
        public void SaveFile()
        {
            File.WriteAllText(
                path,
                System.Text.Json.JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true })
            );
        }
    }
}