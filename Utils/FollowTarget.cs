using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    public class FollowTarget : MonoBehaviour
    {
        private Transform target;
        private Vector3 offset;

        public void Initialize(Transform target, Vector3 offset)
        {
            this.target = target;
            this.offset = offset;
        }

        private void Update()
        {
            transform.position = target.position + offset;
        }
    }
}
