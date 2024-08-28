using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EAC.util
{

    public class RotateObject : MonoBehaviour
    {
        public Vector3 rotation;


        // Update is called once per frame
        void LateUpdate()
        {
            transform.Rotate(rotation, Space.Self);
        }
    }
}
