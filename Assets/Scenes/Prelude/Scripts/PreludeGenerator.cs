using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class PreludeGenerator : MonoBehaviour
{
    [SerializeField] public Texture2D prelude_image;
    [SerializeField] public Transform canvas_cube_prefab = null;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Prelude Generator");
        SamplePrelude();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Sample 851 x 851 image to recreate it 
    /// </summary>
    void SamplePrelude()
    {
        // Assumptions
        /*
         * All rectangle should be roughly 53 pixels wide with 15 per row + cut off one's at the edges
         * Assume cut off one is roughly cut in half (56 / 2) = 33
         * Recatangles are roughly 25 pixels high
         */ 
        
        // Inner
        int offset = 40;
        Color[,] colors = new Color[15, 34];
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 34; j++)
            {
                colors[i, j] = prelude_image.GetPixel(offset + i * 54, j * 24);
            }
        }

        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 34; j++)
            {
                Vector3 pos = i * 2 * Vector3.right + j * Vector3.up;
                var cube = Instantiate(canvas_cube_prefab, pos, Quaternion.identity);
                Material material = new Material(Shader.Find("HDRP/Lit"));
                material.color = colors[i, j];
                cube.GetComponent<MeshRenderer>().material = material;
            }
        }
    }
}
