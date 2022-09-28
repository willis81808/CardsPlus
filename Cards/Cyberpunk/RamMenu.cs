using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class RamMenu : MonoBehaviour
    {
        public static RamMenu Instance { get; private set; }

        private static List<RamSlot> ramSlots = new List<RamSlot>();

        public static int AvailableRam => ramSlots.Where(r => r.Active).Count();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }

        private void Start()
        {
            // Create starting RAM
            CreateRam(4);
        }

        public static void CreateRam(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = Instantiate(Assets.RamSlot, Instance.transform).GetComponent<RamSlot>();
                slot.Restore();
                ramSlots.Add(slot);
            }
        }

        public static bool SpendRam(int amount)
        {
            if (amount > AvailableRam) return false;

            float reloadProgress = 0f;
            var reloadingRamSlot = ramSlots.Where(r => !r.Active && r.Progress > 0f).FirstOrDefault();
            if (reloadingRamSlot)
            {
                reloadProgress = reloadingRamSlot.Progress;
                reloadingRamSlot.Use();
            }

            // disable spent RAM
            for (int i = ramSlots.Count - 1; i >= 0 && amount > 0; i--)
            {
                var slot = ramSlots[i];

                if (!slot.Active) continue;

                slot.Use();
                amount--;
            }

            // start regenerating RAM
            ramSlots.Where(r => !r.Active)
                .First()
                .StartRefill(OnFinishedRefilling, reloadProgress);

            return true;
        }

        public static void Empty()
        {
            if (Instance == null) return;

            foreach (var slot in ramSlots)
            {
                slot.Use();
            }
            ramSlots[0].StartRefill(OnFinishedRefilling);
        }

        private static void OnFinishedRefilling(RamSlot slot)
        {
            int newIndex = ramSlots.IndexOf(slot) + 1;
            if (newIndex >= ramSlots.Count) return;
            ramSlots[newIndex].StartRefill(OnFinishedRefilling, 0f);
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }

    public class RamSlot : MonoBehaviour
    {
        private static float refillTime = 5f;

        public bool Active { get; private set; }
        public float Progress => fillMeter.value;

        public Color Color
        {
            get
            {
                return fillImage.color;
            }
            set
            {
                fillImage.color = value;
            }
        }

        [SerializeField]
        private Slider fillMeter;

        [SerializeField]
        private Image fillImage;

        [SerializeField]
        private Color readyColor;

        [SerializeField]
        private Color refillingColor;

        private IEnumerator DoRefill(Action<RamSlot> finishedCallback, float startPercent = 0)
        {
            float startTime = Time.time - (refillTime * startPercent);
            while (Time.time - startTime <= refillTime)
            {
                float percentage = (Time.time - startTime) / refillTime;
                fillMeter.value = percentage;
                yield return null;
            }

            finishedCallback?.Invoke(this);
            Restore();
        }

        public void Use()
        {
            Active = false;
            fillMeter.value = 0;
            Color = refillingColor;

            StopAllCoroutines();
        }

        public void StartRefill(Action<RamSlot> finishedCallback, float startPercent = 0)
        {
            StartCoroutine(DoRefill(finishedCallback, startPercent));
        }

        public void Restore()
        {
            Active = true;
            fillMeter.value = 1;
            Color = readyColor;

            StopAllCoroutines();
        }
    }
}
