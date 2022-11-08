using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    public class KeepRotationForces : MonoBehaviour
    {
        public Rigidbody2D rb;

        public float forceMultiplier = 1f;

        private Vector3 startingUp;

        void Start()
        {
            startingUp = transform.up;
        }

        void FixedUpdate()
        {
            var rotation = Vector3.Cross(rb.transform.up, startingUp);
            rb.AddTorque(rotation.z * forceMultiplier, ForceMode2D.Force);
        }
    }
}
