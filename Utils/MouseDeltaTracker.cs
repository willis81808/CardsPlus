using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    public class MouseDeltaTracker : MonoBehaviour
    {
        private Vector3 lastPosition;
        private Vector3 delta;

        private void Update()
        {
            delta = lastPosition - Input.mousePosition;
            lastPosition = Input.mousePosition;
        }

        public Vector3 GetDelta()
        {
            return delta;
        }
    }
}
