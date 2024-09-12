using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Palmmedia.ReportGenerator.Core.Common;
using UnityEngine;

namespace Scenes.City.Scripts
{
    public class Despawner : MonoBehaviour
    {
        public string jsonPath = @"R:\testclaim.json";
        private FileStream _fileStream;
        private StreamReader _streamReader;

        private void Awake()
        {
            try
            {
                _fileStream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite);
                _streamReader = new StreamReader(_fileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.LogError(e);
                Debug.LogError("Missing json file");
                throw;
            }
            
        }

        private void Start()
        {
            ParseJSON();
        }

        void Update()
        {
            
        }

        public void ParseJSON()
        {
            var content = _streamReader.ReadToEnd();
            content = content.Trim('[');
            content = content.Trim(']');
            var items = content.Split(",", StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, bool> itemDictionary = new Dictionary<string, bool>();
            for (int i = 0; i < items.Length; i += 2)
            {
                var link = items[i].Substring(items[i].IndexOf(':') + 1).Trim('"');
                link = link.Trim('{');
                link = link.Trim('}');
                var is_claimed = bool.Parse(items[i + 1].Split(':')[1].Trim('}'));
                itemDictionary.Add(link, is_claimed);
            }

            foreach (var entry in itemDictionary)
            {
                if (entry.Value == true)
                {
                    Debug.Log(entry.Key);
                }
            }
        }
    }
}