using System;
using System.Collections;
using UnityEngine;


public class CrateDropHandler : MonoBehaviour
{
    public Transform startPosition;
    public Transform endPosition;
    public Transform despawnPosition;
    public float speed;
    public float rotationSpeed;
    
    private float startTime;
    private float journeyLength;

    // Handle Despanw
    private bool despawn = false;
    private float distanceTravelled;
    public float despawnDistance;

    private void Start()
    {
        transform.position = startPosition.position;
        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(startPosition.position, endPosition.position);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        if (!despawn)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(startPosition.position, endPosition.position, fractionOfJourney);
        }
    }
    
    /*IEnumerator DespanwCrateCoroutine(string id)
    {
        yield return new WaitForSeconds(3f);
        
    }*/
}