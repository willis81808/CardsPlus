using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using UnboundLib;

namespace CardsPlusPlugin.Utils
{
    public class CustomHealthBar : MonoBehaviour
    {
        private HealthBar healthBar;

        private Func<float> currentHealthProvider;
        private Func<float> maxHealthProvider;

        public void Initialize(Func<float> currentHealthProvider, Func<float> maxHealthProvider)
        {
            this.currentHealthProvider = currentHealthProvider;
            this.maxHealthProvider = maxHealthProvider;
            OnTakeDamage(Vector2.zero, false);
        }

        private void Awake()
        {
            healthBar = Instantiate(Assets.BaseHealthBar.gameObject, transform).GetComponent<HealthBar>();
        }

        private void Start()
        {
            healthBar.transform.Find("Canvas/PlayerName").gameObject.SetActive(false);
        }

        public void OnTakeDamage(Vector2 dmg, bool selfDmg)
        {
            healthBar.TakeDamage(dmg, selfDmg);
        }

        public float? GetCurrentHealth()
        {
            return currentHealthProvider?.Invoke();
        }

        public float? GetMaxHealth()
        {
            return maxHealthProvider?.Invoke();
        }
    }

    [HarmonyPatch(typeof(HealthBar))]
    public static class HealthBarPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        static bool StartPrefix(HealthBar __instance)
        {
            var customHealthBar = __instance.GetComponentInParent<CustomHealthBar>();
            return customHealthBar == null;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        static bool UpdatePrefix(HealthBar __instance, ref Image ___hp, ref Image ___white, ref float ___drag, ref float ___spring, ref float ___hpCur, ref float ___hpVel, ref float ___hpTarg, ref float ___whiteCur, ref float ___whiteVel, ref float ___whiteTarg, ref float ___sinceDamage)
        {
            var customHealthBar = __instance.GetComponentInParent<CustomHealthBar>();
            if (customHealthBar == null) return true;

            float? currentHp = customHealthBar.GetCurrentHealth();
            float? maxHp = customHealthBar.GetMaxHealth();

            if (!currentHp.HasValue || !maxHp.HasValue)
            {
                CardsPlus.GetLogger().LogError($"[CustomHealthBar] Failed to provide a current and max HP value");
                return false;
            }

            ___hpTarg = currentHp.Value / maxHp.Value;
            ___sinceDamage += TimeHandler.deltaTime;
            ___hpVel = FRILerp.Lerp(___hpVel, (___hpTarg - ___hpCur) * ___spring, ___drag);
            ___whiteVel = FRILerp.Lerp(___whiteVel, (___whiteTarg - ___whiteCur) * ___spring, ___drag);
            ___hpCur += ___hpVel * TimeHandler.deltaTime;
            ___whiteCur += ___whiteVel * TimeHandler.deltaTime;
            ___hp.fillAmount = ___hpCur;
            ___white.fillAmount = ___whiteCur;
            if (___sinceDamage > 0.5f)
            {
                ___whiteTarg = ___hpTarg;
            }

            return false;
        }
    }
}
