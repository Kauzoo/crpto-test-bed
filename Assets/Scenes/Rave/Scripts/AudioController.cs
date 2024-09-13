using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioVisualizer;

public class AudioController : MonoBehaviour
{
    public AudioSpectrumEngine audioSpectrumEngine;
    //public Transform pyramidTransform;

    /*public Transform[] ob = new Transform[8];*/
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (audioSpectrumEngine.GetBeat())
        {
            pyramidTransform.Rotate(Vector3.up, 5);
        }
        */

        var bands = audioSpectrumEngine.GetBands();
        foreach (var val in bands)
        {
            Debug.Log(val);
        }/*
        for (int i=0; i < 8; i++)
        {
            float temp = ob[i].localPosition.z;
            ob[i].localPosition = new Vector3(0f, bands[i]*13, temp);
        }*/
    
    }
}
