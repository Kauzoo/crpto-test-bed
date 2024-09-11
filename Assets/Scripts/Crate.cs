using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    //private Transform transform;
    // Start is called before the first frame update
    void Start()
    {
        //transform = GetComponent<transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        transform.Translate(0,1,0);
    }
}
