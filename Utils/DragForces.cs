using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    public class DragForces : MonoBehaviour
    {
        public Rigidbody2D rb;

        public float linearDrag, angularDrag;

        private float startLinearDrag, startAngularDrag;

        void OnEnable()
        {
            startLinearDrag = rb.drag;
            startAngularDrag = rb.angularDrag;

            rb.drag = linearDrag;
            rb.angularDrag = angularDrag;
        }

        void OnDisable()
        {
            rb.drag = startLinearDrag;
            rb.angularDrag = startAngularDrag;
        }
    }
}
