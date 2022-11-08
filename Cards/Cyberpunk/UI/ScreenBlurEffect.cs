using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CardsPlusPlugin.Cards
{
    public class ScreenBlurEffect : Image
    {
        public Transform target;

        private const float BLUR_MAX = 15;
        private const float BLUR_RADIUS = 200;
        private const float BLUR_OFFSET = 100;

        private float Blur
        {
            get => material.GetFloat("_Size");
            set => material.SetFloat("_Size", value);
        }

        private float Radius
        {
            get => material.GetFloat("_Radius");
            set => material.SetFloat("_Radius", value);
        }

        private float Offset
        {
            get => material.GetFloat("_Offset");
            set => material.SetFloat("_Offset", value);
        }

        private Vector3 Center
        {
            get => material.GetVector("_Center");
            set => material.SetVector("_Center", value);
        }

        protected override void Start()
        {
            base.Start();

            material.SetFloat("_Blur", 0);
            material.SetFloat("_Radius", BLUR_RADIUS);
            material.SetFloat("_Offset", BLUR_OFFSET);

            raycastTarget = false;
        }

        public ScreenBlurEffect Initialize(float rampUpTime, Transform target)
        {
            StopAllCoroutines();
            StartCoroutine(DoShow(rampUpTime));
            this.target = target;
            return this;
        }

        void Update()
        {
            if (!target) return;
            var position = Camera.main.WorldToScreenPoint(target.position);
            position.y = Screen.height - position.y;
            Center = position;
        }

        public void Remove(float duration = 1f)
        {
            StopAllCoroutines();
            StartCoroutine(DoRemove(duration));
        }

        private IEnumerator DoShow(float duration)
        {
            Blur = 0;
            var time = Time.time;
            while (Time.time - time <= duration)
            {
                var progress = (Time.time - time) / duration;
                Blur = BLUR_MAX * progress;
                yield return null;
            }
            Blur = BLUR_MAX;
        }

        private IEnumerator DoRemove(float duration)
        {
            Blur = BLUR_MAX;
            var time = Time.time;
            while (Time.time - time <= duration)
            {
                var progress = 1 - ((Time.time - time) / duration);
                Blur = BLUR_MAX * progress;
                yield return null;
            }
            Blur = 0;
            Destroy(gameObject);
        }
    }
}