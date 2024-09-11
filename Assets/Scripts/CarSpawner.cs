using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject car;
    public GameObject center;
    
    [Header("Settings")]
    public Transform startMarker;
    public Transform endMarker;
    public int carCount;
    public float minGap;
    public float maxGap;
    public bool printDebugInfo = true;
    
    // Start is called before the first frame update
    void Start()
    {
        var meshR = center.GetComponent<MeshRenderer>();
        meshR.enabled = false;
        StartCoroutine(SpawnCar());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnCar()
    {
        float[] gaps = new float[carCount];

        for (var i = 0; i < carCount; i++)
        {
            var gap = Random.Range(minGap, maxGap);
            yield return new WaitForSecondsRealtime(gap);
            var nCar = Instantiate(car, startMarker, true);
            nCar.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(endMarker.position - startMarker.position));
            var engine = nCar.GetComponent<CarEngine>();
            engine.start = startMarker;
            engine.end = endMarker;
        }

        if (printDebugInfo)
        {
            Debug.Log($"CarSpawner Info @ {gameObject.name}");
            Debug.Log($"CarCount: {carCount.ToString()}");
            for (var i = 0; i < carCount; i++)
            {
                Debug.Log($"Car{i} : Gap{gaps[i].ToString()}");
            }
        }
    }
}
