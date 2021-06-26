using System;
using UnboundLib.Cards;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using System.Collections;
using HarmonyLib;

namespace CardsPlusPlugin.Cards
{
    public class PhantomCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            var block = gameObject.AddComponent<Block>();
            block.InvokeMethod("ResetStats");
            block.cdAdd = 3f;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effect = player.GetComponent<PhantomEffect>();
            if (effect == null)
            {
                effect = player.gameObject.AddComponent<PhantomEffect>();
            }
            effect.AddDuration(Math.Max(1f, effect.duration));
        }
        public override void OnRemoveCard()
        {
            //throw new NotImplementedException();
        }

        protected override string GetTitle()
        {
            return "Phantom";
        }

        protected override string GetDescription()
        {
            return "Blocking causes you and your bullets to become Ethereal.\nWhile Ethereal, <color=\"red\">-50%</color> Damage";
        }

        protected override GameObject GetCardArt()
        {
            return Assets.PhantomArt;
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Uncommon;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = false,
                    amount = "3s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    stat = "Block cooldown"
                }
            };
        }

        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.TechWhite;
        }
    }

    [HarmonyPatch]
    public class PhantomEffect : MonoBehaviour
    {
        private Player player;

        private float startTime = float.MaxValue;

        public float duration = 0f;

        private bool active = false;

        // player data backups
        private Color projectileColor;
        private float damageOriginal;
        private SetTeamColor[] playerColors;
        private PlayerSkinParticle[] particles;
        private Collider2D[] colliders;
        private Color colorMin, colorMax;

        void Awake()
        {
            player = GetComponent<Player>();
            playerColors = GetComponentsInChildren<SetTeamColor>();
            particles = GetComponentsInChildren<PlayerSkinParticle>();
            colliders = GetComponentsInChildren<Collider2D>();
        }
        void Start()
        {
            projectileColor = player.data.weaponHandler.gun.projectileColor;
            damageOriginal = player.data.weaponHandler.gun.damage;
        }
        void Update()
        {
            if (!active) return;
            if (Time.time - startTime > duration)
            {
                active = false;
                EndEffect();
            }
        }

        void StartEffect()
        {
            ResetTimer();

            if (active) return;

            active = true;

            var p = player;
            var g = p.data.weaponHandler.gun;
            projectileColor = g.projectileColor;
            damageOriginal = g.damage;

            colorMin = Color.white;
            colorMax = Color.white;

            var colorOriginal = Color.white;
            foreach (var tc in playerColors)
            {
                var rend = tc.GetFieldValue("meshRend");
                if (rend != null)
                {
                    colorOriginal = ((MeshRenderer)rend).material.color;
                }
            }

            foreach (var pl in particles)
            {
                var system = (ParticleSystem)pl.GetFieldValue("part");
                var main = system.main;
                var startColor = system.main.startColor;
                colorMax = startColor.colorMax;
                colorMin = startColor.colorMin;
                startColor.colorMin = Color.black;
                startColor.colorMax = Color.white;
                main.startColor = startColor;
            }
            
            foreach (var pl in playerColors)
            {
                pl.Set(new PlayerSkin()
                {
                    color = Color.white,
                    backgroundColor = Color.white,
                    winText = Color.white,
                    particleEffect = Color.white
                });
            }

            g.ignoreWalls = true;
            g.projectileColor = Color.white;
            g.damage = damageOriginal * 0.5f;
            
            foreach (var c in colliders)
            {
                c.enabled = false;
            }
        }
        void EndEffect()
        {
            var p = player;
            var g = p.data.weaponHandler.gun;
            g.ignoreWalls = false;
            g.projectileColor = projectileColor;
            g.damage = damageOriginal;

            foreach (var c in colliders)
            {
                c.enabled = true;
            }

            foreach (var pl in particles)
            {
                var system = (ParticleSystem)pl.GetFieldValue("part");
                var main = system.main;
                var startColor = system.main.startColor;
                startColor.colorMin = colorMin;
                startColor.colorMax = colorMax;
                main.startColor = startColor;
            }

            foreach (var pl in playerColors)
            {
                pl.Set(new PlayerSkin()
                {
                    color = colorMax,
                    backgroundColor = colorMax,
                    winText = colorMax,
                    particleEffect = colorMax
                });
            }
        }

        public void ResetTimer()
        {
            startTime = Time.time;
        }
        public void AddDuration(float duration)
        {
            this.duration += duration;
        }

        [HarmonyPatch(typeof(Block), "RPCA_DoBlock")]
        [HarmonyPostfix]
        static void Block_PostFix(Block __instance, bool firstBlock, bool dontSetCD, BlockTrigger.BlockTriggerType triggerType, Vector3 useBlockPos, bool onlyBlockEffects)
        {
            var phantomEffect = __instance.GetComponent<PhantomEffect>();
            if ( (phantomEffect == null) || (triggerType != BlockTrigger.BlockTriggerType.Default) ) return;
            phantomEffect.StartEffect();
        }

        [HarmonyPatch(typeof(Block), "ResetStats")]
        [HarmonyPostfix]
        static void ResetStats_PostFix(Block __instance)
        {
            var phantomEffect = __instance.GetComponent<PhantomEffect>();
            if (phantomEffect != null)
            {
                Destroy(phantomEffect);
            }
        }
    }
}