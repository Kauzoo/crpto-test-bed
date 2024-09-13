using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Airdrop : MonoBehaviour
{
    [Header("General Settings")]
    public int targetFrameRate = 30;
    
    [Header("Drop Settings")] public KeyCode leftDropKey;
    public KeyCode standardDrop;
    public AnimationClip[] planeAnimations;

    [Header("Fields")] public List<GameObject> crates = new();
    private Stack<GameObject> availableCrates = new();
    public Transform[] dropLeftStartPostions;
    public Transform[] dropLeftEndPostions;
    public Transform[] dropAltStartPostions;
    public Transform[] dropAltEndPostions;
    public Animator planeAnimator;
    public Sprite[] sprites = Array.Empty<Sprite>();
    public GameObject cratePrefab;
    public GameObject crateContainer;
    public List<GameObject> debug_crates = new();
    public List<Sprite> debug_sprites = new();

    // Despawn Settings
    private FileStream _fileStream;
    private StreamReader _streamReader;
    private Dictionary<string, bool> _itemDictionary = new();
    private Dictionary<string, SpriteRenderer[]> _itemObjectDictionary = new();
    private const int NAME_OFFSET = 30;


    [Header("Despawn Settings")] public string jsonPath = @"R:\testclaim.json";

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
        Application.targetFrameRate = targetFrameRate;
        //foreach (var sprite in sprites)
        //foreach (var sprite in debug_sprites)
        foreach (var sprite in sprites)
        {
            var crate = Instantiate(cratePrefab);
            crate.transform.SetParent(crateContainer.transform);
            crate.name = $"cr_{sprite.name}";
            var render = crate.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in render)
            {
                renderer.sprite = sprite;
            }
            crate.SetActive(false);
            crates.Add(crate);
        }
        Setup();
        foreach (var entry in _itemObjectDictionary)
        {
            Debug.Log($"object keys: {entry.Key}");
        }

        availableCrates = new Stack<GameObject>();
        foreach (var crate in crates)
        {
            availableCrates.Push(crate);
        }
    }

    void Update()
    {
        CycleJSON();
        if (Input.GetKeyDown(leftDropKey))
        {
            TriggerDropLeft("plane");
        }

        if (Input.GetKeyDown(standardDrop))
        {
            TriggerDropLeft("planedrop");
        }
    }

    #region Despawn
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
            foreach (var code in crates)
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
        if (!_itemObjectDictionary.Any())
        {
            Debug.LogWarning("No item found. Either all codes claimed or claims.json is setup improperly.");
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
                return;
            }
        }
    }

    public void ParseJSON()
    {
        using (var streamReader =
               new StreamReader(new FileStream(jsonPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
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
            var qr_object = sprite.gameObject;
            var animator = qr_object.GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("Holo despawn");
        }
        // TODO Get ride of crate ater QR-Codes have faded
        Debug.Log($"Delete: {sprites[0].transform.parent.parent.name}");
        StartCoroutine(DespawnCrate(sprites[0].transform.parent.parent));
        Debug.Log($"Code {id} was claimed");
    }

    IEnumerator DespawnCrate(Transform crate)
    {
        yield return new WaitForSeconds(3f);
        crate.gameObject.SetActive(false);
    }
    #endregion


    void TriggerDropLeft(string clip_name)
    {
        planeAnimator.enabled = true;
        planeAnimator.Play(clip_name);
        StartCoroutine(DropCoroutine(clip_name, 4));
    }

    IEnumerator DropCoroutine(string clip_name, int crate_count)
    {
        float duration = 5f;
        foreach (var clip in planeAnimations)
        {
            if (clip.name == clip_name)
            {
                duration = clip.length;
            }
        }
        yield return new WaitForSeconds(duration);
        
        int limit = 4;
        if (availableCrates.Count < 4)
        {
            Debug.LogWarning("Less than 4 crates left, something went wrong");
            limit = availableCrates.Count;
        }
        

        Transform[] start_postitions;
        Transform[] end_postitions;
        if (clip_name == "plane")
        {
            start_postitions = dropLeftStartPostions;
            end_postitions = dropLeftEndPostions;
        }
        else
        {
            start_postitions = dropAltStartPostions;
            end_postitions = dropAltEndPostions;
        }
        for (int i = 0; i < limit; i++)
        {
            if (availableCrates.TryPop(out var crate))
            {
                var drop_handler = crate.GetComponent<CrateDropHandler>();
                drop_handler.startPosition = start_postitions[i];
                drop_handler.endPosition = end_postitions[i];
                crate.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Failed to pop crate, something went wrong");
            }
        }
    }
}