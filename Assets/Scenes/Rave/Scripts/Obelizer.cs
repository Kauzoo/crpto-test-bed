using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioVisualizer;


public class Obelizer : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSpectrumEngine audioSpectrumEngine;
    public Transform[] ob = new Transform[8];

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var bands = audioSpectrumEngine.GetBands();
        for (int i=0; i < 8; i++)
        {
            float temp = ob[i].localPosition.z;
            ob[i].localPosition = new Vector3(0f, bands[i]*13, temp);
        }
    }
}
