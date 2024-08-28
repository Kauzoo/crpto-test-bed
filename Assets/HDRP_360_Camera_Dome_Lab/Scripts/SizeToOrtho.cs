using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Resize an object to fit in the specified camera
public class SizeToOrtho : MonoBehaviour {

	// The camera with the size to watch
	public Camera orthoCam;
	public float lastScreenWidth = 0f;

    private float height;
    private float width;

	void Start()
	{
		lastScreenWidth = Screen.width;
        if (orthoCam != null && orthoCam.targetTexture != null)
            lastScreenWidth = orthoCam.targetTexture.width;
            
		StartCoroutine("AdjustScale");
	}

	void Update()
	{
		if ( lastScreenWidth != Screen.width ){
			lastScreenWidth = Screen.width;
			StartCoroutine("AdjustScale");
		}

	}

	// Adjustment
	IEnumerator AdjustScale ()
	{
		if (orthoCam == null)
			yield break;

		// Base the height on the orthographic size of the camera	
		height = (float) (orthoCam.orthographicSize);

        // If this is a renderTexture camera, set the width based on the rendertexture's ratio
        if (orthoCam.targetTexture != null)
            width = (float)(height * orthoCam.targetTexture.width / orthoCam.targetTexture.height);
        else
            width = (float) (height * Screen.width / Screen.height);

        transform.localScale = new Vector3(width, height, height);
		yield return null;
	}
		
}
