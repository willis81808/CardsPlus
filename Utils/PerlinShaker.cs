using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    public class PerlinShaker : MonoBehaviour
    {
        Vector3 startPos;

        public float xScale = 1f;
        public float yScale = 1f;
        public float offsetScale = 1f;

        void Start()
        {
            startPos = transform.position;
        }

        void Update()
        {
            var dx = Mathf.PerlinNoise(Time.time * xScale, 0) - 0.5f;
            var dy = Mathf.PerlinNoise(0, Time.time * yScale) - 0.5f;
            var offset = new Vector3(dx, dy, 0);
            transform.position = startPos + offset * offsetScale;
        }
    }
}
