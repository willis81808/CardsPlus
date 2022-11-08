using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnityEngine;
using UnityEngine.UI;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class RamMenu : MonoBehaviour
    {
        public static RamMenu Instance { get; private set; }

        private const float BASE_REFILL_TIME = 4f;
        public static float RefillTime { get; private set; } = BASE_REFILL_TIME;

        private static List<RamSlot> ramSlots = new List<RamSlot>();

        public static int AvailableRam => ramSlots.Where(r => r.Active).Count();

        private CanvasGroup canvasGroup;
        private Vector3 baseScale;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(Instance.gameObject);
            }
            Instance = this;
        }

        // Create starting RAM
        private void Start()
        {
            baseScale = transform.localScale;
            canvasGroup = GetComponent<CanvasGroup>();
            CreateRam(4);
        }

        private void Update()
        {
            transform.localScale = baseScale * CardsPlus.ramMenuScale.Value;
            canvasGroup.alpha = CardsPlus.ramMenuVisible.Value ? 1 : 0;
        }

        private void OnDestroy()
        {
            RefillTime = BASE_REFILL_TIME;
            Instance = null;
            ramSlots.Clear();
        }

        public static void SetRefillTime(float time)
        {
            RefillTime = Math.Max(1, Math.Min(RefillTime, time));
        }

        public static void CreateRam(int count = 1, bool active = true)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = Instantiate(Assets.RamSlot, Instance.transform).GetComponent<RamSlot>();
                ramSlots.Add(slot);

                if (active)
                    slot.Restore();
                else
                    slot.Use();
            }
            
            // refresh regeneration if we've added additional slots
            if (count != ramSlots.Count)
            {
                SpendRam(0);
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

            var regenerateStartSlot = ramSlots.Where(r => !r.Active).FirstOrDefault();

            if (regenerateStartSlot == null)
            {
                CardsPlus.LOGGER.LogInfo("All ram is active, nothing to regenerate");
                return true;
            }

            regenerateStartSlot.StartRefill(OnFinishedRefilling, RefillTime, reloadProgress);

            return true;
        }

        public static void Empty()
        {
            if (Instance == null) return;

            foreach (var slot in ramSlots)
            {
                slot.Use();
            }
            ramSlots[0].StartRefill(OnFinishedRefilling, RefillTime);
        }

        private static void OnFinishedRefilling(RamSlot slot)
        {
            int newIndex = ramSlots.IndexOf(slot) + 1;
            if (newIndex >= ramSlots.Count) return;
            ramSlots[newIndex].StartRefill(OnFinishedRefilling, RefillTime, 0f);
        }
    }

    public class RamSlot : MonoBehaviour
    {
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

        private void OnDestroy()
        {
            Use();
        }

        private IEnumerator DoRefill(Action<RamSlot> finishedCallback, float refillTime, float startPercent = 0)
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

        public void StartRefill(Action<RamSlot> finishedCallback, float refillTime, float startPercent = 0)
        {
            StartCoroutine(DoRefill(finishedCallback, refillTime, startPercent));
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
