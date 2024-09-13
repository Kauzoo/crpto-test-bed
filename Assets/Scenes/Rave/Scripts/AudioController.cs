using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioVisualizer;

public class AudioController : MonoBehaviour
{
    public AudioSpectrumEngine audioSpectrumEngine;
    public Transform pyramidTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSpectrumEngine.GetBeat())
        {
            pyramidTransform.Rotate(Vector3.up, 30);
        }

        var bands = audioSpectrumEngine.GetBands();
        foreach (var val in bands)
        {
            Debug.Log(val);
        }
    }
}
