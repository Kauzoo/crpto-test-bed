using System;
using UnityEngine;

namespace Scenes.City.Scripts
{
    public class DomeMover : MonoBehaviour
    {
        public Transform target;
        public float axisSpeed = 10f;
        public float turnSpeed = 10f;
        public float verticalSpeed = 10f;
        private Vector3 old_pos;
        private Quaternion old_rot;
        

        private void Start()
        {
            old_pos = target.transform.position;
            old_rot = target.transform.rotation;
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                
                target.transform.position += Vector3.forward * verticalSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.S))
            {
                target.transform.position += Vector3.forward * -verticalSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A))
            {
                target.transform.position += Vector3.left * verticalSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.D))
            {
                target.transform.position += Vector3.right * verticalSpeed * Time.deltaTime;
            }
            // Turn right
            if (Input.GetKey(KeyCode.E))
            {
                target.transform.Rotate(Vector3.up, axisSpeed * Time.deltaTime, Space.Self);
            }
            // Turn left
            if (Input.GetKey(KeyCode.Q))
            {
                target.transform.Rotate(Vector3.up, -axisSpeed * Time.deltaTime, Space.Self);

            }
            // Up
            if (Input.GetKey(KeyCode.Space))
            {
                target.transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
            }
            // Down
            if (Input.GetKey(KeyCode.LeftShift))
            {
                target.transform.position += Vector3.up * -verticalSpeed * Time.deltaTime;

            }
            // flip forward
            if (Input.GetKey(KeyCode.Z))
            {
                target.transform.Rotate(Vector3.right, -axisSpeed * Time.deltaTime, Space.Self);
            }
            // flip backward
            if (Input.GetKey(KeyCode.C))
            {
                target.transform.Rotate(Vector3.right, axisSpeed * Time.deltaTime, Space.Self);
            }
            // flip forward
            if (Input.GetKey(KeyCode.H))
            {
                target.transform.Rotate(Vector3.forward, -axisSpeed * Time.deltaTime, Space.Self);
            }
            // flip backward
            if (Input.GetKey(KeyCode.J))
            {
                target.transform.Rotate(Vector3.forward, axisSpeed * Time.deltaTime, Space.Self);
            }
            // reset
            if (Input.GetKey(KeyCode.R))
            {
                target.transform.position = old_pos;
                target.transform.rotation = old_rot;
            }
        }
    }
}