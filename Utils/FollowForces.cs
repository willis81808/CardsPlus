using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    public class FollowForces : MonoBehaviour
    {
        public Rigidbody2D rb;
        public Transform target;

        public float forceMultiplier = 1f;

        void FixedUpdate()
        {
            var force = (target.transform.position - rb.transform.position) * forceMultiplier;
            rb.AddForce(force, ForceMode2D.Force);
        }
    }
}
