using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Scenes.City.Scripts
{
    public class Despawner : MonoBehaviour
    {
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private Dictionary<string, bool> _itemDictionary = new();
        private Dictionary<string, SpriteRenderer[]> _itemObjectDictionary = new();
        private const int NAME_OFFSET = 30;
        
        
        [Header("Settings")]
        public string jsonPath = @"R:\testclaim.json";
        public Transform[] qrcodes = new Transform[1];

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
            Application.targetFrameRate = 30;
            Setup();
            foreach (var entry in _itemObjectDictionary)
            {
                Debug.Log($"object keys: {entry.Key}");
            }
        }

        void Update()
        {
            CycleJSON();
        }

        public void Setup()
        {
            // Initialize
            // Setup initial dictionary
            var content = _streamReader.ReadToEnd();
            content = content.Trim('[');
            content = content.Trim(']');
            var items = content.Split(",", StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < items.Length; i += 2)
            {
                var link = items[i].Substring(items[i].IndexOf(':') + 1).Trim('"');
                link = link.Trim('{');
                link = link.Trim('}');
                var unique_id = link[30..].Trim('"');
                Debug.Log($"ID {i}: {unique_id} | Length: {unique_id.Length}");
                var is_claimed = bool.Parse(items[i + 1].Split(':')[1].Trim('}'));
                _itemDictionary.Add(unique_id, is_claimed);
            }
            
            // Fill id to sprite map
            foreach (var entry in _itemDictionary)
            {
                foreach (var code in qrcodes)
                {
                    var sprites = code.GetComponentsInChildren<SpriteRenderer>();
                    Debug.Log($"Sprite Name: {sprites[0].sprite.name}");
                    if (sprites[0].sprite.name == entry.Key.Replace('/', '€'))
                    {
                        _itemObjectDictionary.Add(entry.Key, sprites);
                    }
                }
            }
            _streamReader.Close();
            _fileStream.Close();
        }
        
        public void CycleJSON()
        {
            ParseJSON();
            if (_itemObjectDictionary.Count == 0)
            {
                Debug.LogWarning("No item found");
                return;
            }
            foreach (var item in _itemObjectDictionary)
            {
                // Removed QR-codes that have been claimed
                if (!_itemDictionary.TryGetValue(item.Key.Replace('€', '/'), out var is_claimed)) continue;
                if (is_claimed)
                {
                    FadeOutQR(item.Key);
                    _itemObjectDictionary.Remove(item.Key);
                }
            }
        }

        public void ParseJSON()
        {
            using (var streamReader = new StreamReader(new FileStream(jsonPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var content = streamReader.ReadToEnd();
                content = content.Trim('[');
                content = content.Trim(']');
                var items = content.Split(",", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < items.Length; i += 2)
                {
                    var link = items[i].Substring(items[i].IndexOf(':') + 1).Trim('"');
                    link = link.Trim('{');
                    link = link.Trim('}');
                    var unique_id = link[30..].Trim('"');
                    //Debug.Log($"ID {i}: {unique_id} | Length: {unique_id.Length}");
                    var is_claimed = bool.Parse(items[i + 1].Split(':')[1].Trim('}'));
                    if (_itemDictionary.ContainsKey(unique_id))
                    {
                        _itemDictionary[unique_id] = is_claimed;
                    }
                }
            }
        }

        public void FadeOutQR(string id)
        {
            var sprites = _itemObjectDictionary[id];
            foreach (var sprite in sprites)
            {
                sprite.enabled = false;
            }
            Debug.Log($"Code {id} was claimed");
        }
    }
}