using System;
using UnboundLib.Cards;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using System.Collections;
using HarmonyLib;
using ModsPlus;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;

namespace CardsPlusPlugin.Cards
{
    public class PhantomCard : CustomEffectCard<PhantomEffect>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Phantom",
            Description = "Blocking causes you and your bullets to become Ethereal.\nWhile Ethereal, <color=\"red\">-15%</color> Damage",
            ModName     = "Cards+",
            Art         = Assets.PhantomArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.TechWhite,
            Stats = new []
            {
                new CardInfoStat()
                {
                    positive = false,
                    amount = "+2s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    stat = "Block cooldown"
                }
            }
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            block.InvokeMethod("ResetStats");
            block.cdAdd = 2f;

            cardInfo.categories = new CardCategory[] { CustomCardCategories.instance.CardCategory("Phantom") };
            cardInfo.blacklistedCategories = new CardCategory[] { CustomCardCategories.instance.CardCategory("Ghost") };
        }
    }

    public class PhantomEffect : CardEffect
    {
        private float startTime = float.MaxValue;

        public float duration = 3f;

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
            playerColors = GetComponentsInChildren<SetTeamColor>();
            particles = GetComponentsInChildren<PlayerSkinParticle>();
            colliders = GetComponentsInChildren<Collider2D>();
        }
        protected override void Start()
        {
            base.Start();
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

        public override void OnBlock(BlockTrigger.BlockTriggerType blockTriggerType)
        {
            if (blockTriggerType != BlockTrigger.BlockTriggerType.Default) return;
            StartEffect();
        }

        public override void OnUpgradeCard()
        {
            AddDuration(3f);
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
            g.damage = damageOriginal * 0.85f;
            
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
    }
}