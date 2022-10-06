using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;
using ModsPlus;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class CyberPsychosisCard : CustomEffectCard<CyberPsycho>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Cyber Psychosis",
            Description = "Too much tech has fried your wetware, choom!",
            ModName     = "Cards+",
            Art         = null,
            Rarity      = CardInfo.Rarity.Rare,
            Theme       = CardThemeColor.CardThemeColorType.EvilPurple,
            OwnerOnly   = true,
            Stats = new[]
            {
                new CardInfoStat()
                {
                    stat = "HP",
                    amount = "300%",
                    simepleAmount = CardInfoStat.SimpleAmount.aHugeAmountOf,
                    positive = true
                },
                new CardInfoStat()
                {
                    stat = "Damage",
                    amount = "+50%",
                    simepleAmount = CardInfoStat.SimpleAmount.aHugeAmountOf,
                    positive = true
                },
                new CardInfoStat()
                {
                    stat = "Bullets",
                    amount = "+5",
                    simepleAmount = CardInfoStat.SimpleAmount.aLotOf,
                    positive = true
                },
                new CardInfoStat()
                {
                    stat = "Ammo",
                    amount = "+7",
                    simepleAmount = CardInfoStat.SimpleAmount.aLotOf,
                    positive = true
                },
                new CardInfoStat()
                {
                    stat = "Reload Speed",
                    amount = "-30%",
                    simepleAmount = CardInfoStat.SimpleAmount.lower,
                    positive = false
                },
                new CardInfoStat()
                {
                    stat = "Movement Speed",
                    amount = "-30%",
                    simepleAmount = CardInfoStat.SimpleAmount.lower,
                    positive = false
                }
            }
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.gameObject.AddComponent<CyberCardEffect>();

            cardInfo.allowMultiple = false;

            statModifiers.health = 3;
            statModifiers.movementSpeed = 0.7f;

            gun.damage = 1.5f;
            gun.numberOfProjectiles = 5;
            gun.ammo = 7;
            gun.spread = 0.35f;
            gun.multiplySpread = 1f;
            gun.evenSpread = 0f;
            gun.reloadTime = 0.7f;
        }

        protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            base.Added(player, gun, gunAmmo, data, health, gravity, block, characterStats);
        }
    }

    public class CyberPsycho : CardEffect
    {
        private const float BASE_GLITCH_LEVEL = 0.0075f;
        private const float MAX_GLITCH_LEVEL = 0.6f;
        private const float GLITCH_DURATION = 2f;

        private Kino.DigitalGlitch glitchEffect;

        private Coroutine glitchCoroutine;

        protected override void Start()
        {
            base.Start();
            glitchEffect = Camera.main.gameObject.AddComponent<Kino.DigitalGlitch>();
            glitchEffect.intensity = BASE_GLITCH_LEVEL;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(glitchEffect);
        }

        public override void OnShoot(GameObject projectile)
        {
            base.OnShoot(projectile);
            Instantiate(Assets.GlitchedParticleEffect, projectile.transform);
        }

        public override void OnTakeDamage(Vector2 damage, bool selfDamage)
        {
            if (glitchCoroutine != null) Unbound.Instance.StopCoroutine(glitchCoroutine);
            glitchCoroutine = Unbound.Instance.StartCoroutine(DoGlitchEffect());
        }

        private IEnumerator DoGlitchEffect()
        {
            glitchEffect.intensity = MAX_GLITCH_LEVEL;

            var time = Time.time;
            while (Time.time - time <= GLITCH_DURATION)
            {
                float percentage = 1 - (Time.time - time) / GLITCH_DURATION;
                glitchEffect.intensity = BASE_GLITCH_LEVEL + MAX_GLITCH_LEVEL * percentage;
                yield return null;
            }

            glitchEffect.intensity = BASE_GLITCH_LEVEL;
        }
    }
}
