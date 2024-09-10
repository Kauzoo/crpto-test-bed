using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CarEngine : MonoBehaviour
{
    public bool useCord;
    public Transform start;
    public Transform end;
    public Vector3 startCord;
    public Vector3 endCord;
    public float speed = 1.0f;
    public float epsilon = 1.0f;

    private float startTime;
    private float journeyLength;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (!useCord)
        {
            startCord = start.position;
            endCord = end.position;
        }

        var sMesh = start.gameObject.GetComponent<MeshRenderer>();
        var eMesh = end.gameObject.GetComponent<MeshRenderer>();

        sMesh.enabled = false;
        eMesh.enabled = false;

        startTime = Time.time;
        journeyLength = Vector3.Distance(startCord, endCord);
        // Vector3 dir = Vector3.Normalize(endCord - startCord);
        // transform.rotation.SetLookRotation(dir);
    }

    // Update is called once per frame
    void Update()
    {
        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - startTime) * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.Lerp(startCord, endCord, fractionOfJourney);
        
        // Reset on end reached
        
        if (Vector3.Distance(transform.position,endCord) <= epsilon)
        {
            startTime = Time.time;
            transform.position = startCord;
        }
    }
}
