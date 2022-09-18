using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace CardsPlusPlugin.Cards
{
    public class SmokeGrenade : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var launcher = player.gameObject.AddComponent<SmokeLauncher>();
            launcher.Initialize(player, gun, block);
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var launcher = player.gameObject.GetComponent<SmokeLauncher>();
            if (launcher)
            {
                launcher.Remove();
            }
        }

        protected override string GetTitle() => "Smoke Grenade";
        protected override string GetDescription() => "Your first bullet fired after blocking releases a blinding smoke";
        public override string GetModName() => "Cards+";
        protected override CardThemeColor.CardThemeColorType GetTheme() => CardThemeColor.CardThemeColorType.TechWhite;
        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Common;
        protected override GameObject GetCardArt() => Assets.SmokeGrenadeArt;
        protected override CardInfoStat[] GetStats() => null;
    }

    public class SmokeLauncher : MonoBehaviour
    {
        private Player player;
        private Gun gun;
        private Block block;

        private bool primed;

        public void Initialize(Player player, Gun gun, Block block)
        {
            this.player = player;
            this.gun = gun;
            this.block = block;

            gun.ShootPojectileAction += Attack;
            block.BlockAction += Block;
        }

        public void Remove()
        {
            gun.ShootPojectileAction -= Attack;
            block.BlockAction -= Block;

            Destroy(this);
        }

        private void Attack(GameObject projectile)
        {
            var spawnedAttack = projectile.GetComponent<SpawnedAttack>();
            if (!spawnedAttack || !primed) return;

            projectile.AddComponent<Grenade>();
            spawnedAttack.SetColor(Color.white);

            primed = false;
        }

        private void Block(BlockTrigger.BlockTriggerType trigger)
        {
            switch (trigger)
            {
                case BlockTrigger.BlockTriggerType.Echo:
                case BlockTrigger.BlockTriggerType.Empower:
                case BlockTrigger.BlockTriggerType.ShieldCharge:
                    return;
            }

            primed = true;
        }
    }

    public class Grenade : RayHitEffect
    {
        private bool done;

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            if (done) return HasToReturn.canContinue;

            var smoke = PhotonNetwork.Instantiate(Assets.SmokeObject.name, transform.position, Quaternion.identity);

            done = true;
            return HasToReturn.canContinue;
        }
    }

    [RequireComponent(typeof(ParticleSystem))]
    public class Smoke : MonoBehaviour
    {
        private static Canvas canvas => Unbound.Instance.canvas;

        private static readonly float RANGE = 10;
        private static readonly float LIFETIME = 10f;

        private ParticleSystem particles;
        private CanvasGroup smokeEffect;

        private float maxIntensity = 0f;
        private bool active = true;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            smokeEffect = Instantiate(Assets.SmokeEffect, canvas.transform).GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            StartCoroutine(FadeInOut());
        }

        private void Update()
        {
            if (!active) return;

            var players = from p in PlayerManager.instance.players
                          where p.data.view.IsMine
                          let distance = Vector3.Distance(p.transform.position, transform.position)
                          where distance < RANGE
                          select new
                          {
                              Player = p,
                              Scalar = 1 - (distance / RANGE)
                          };

            var target = players.FirstOrDefault();
            smokeEffect.alpha = target == null ? 0 : maxIntensity * Math.Max(0, Math.Min(1, target.Scalar * 1.5f));
        }

        private IEnumerator FadeInOut()
        {
            float time = Time.time;
            while (Time.time - time < 1)
            {
                maxIntensity = Time.time - time;
                yield return null;
            }
            maxIntensity = 1;

            yield return new WaitForSeconds(LIFETIME - 2);

            time = Time.time;
            while (Time.time - time < 1)
            {
                maxIntensity = 1 - (Time.time - time);
                yield return null;
            }
            maxIntensity = 0;

            yield return new WaitForSeconds(5);
            Remove();
        }

        private void Remove()
        {
            active = false;
            particles.Stop();
            Destroy(smokeEffect.gameObject);
            Destroy(gameObject, 5f);
        }
    }
}
