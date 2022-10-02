using CardsPlusPlugin.Utils;
using ModdingUtils.AIMinion.Extensions;
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
    public class SmokeGrenade : CustomEffectCard<SmokeLauncher>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Smoke Grenade",
            Description = "Your first bullet fired after locking releases a blinding smoke",
            ModName     = "Cards+",
            Art         = Assets.SmokeGrenadeArt,
            Rarity      = CardInfo.Rarity.Common,
            Theme       = CardThemeColor.CardThemeColorType.TechWhite,
            OwnerOnly   = true
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
        }
    }

    public class SmokeLauncher : CardEffect
    {
        private bool primed;

        public override void OnShoot(GameObject projectile)
        {
            var spawnedAttack = projectile.GetComponent<SpawnedAttack>();
            if (!spawnedAttack || !primed) return;

            projectile.AddComponent<Grenade>();
            spawnedAttack.SetColor(Color.white);

            primed = false;
        }

        public override void OnBlock(BlockTrigger.BlockTriggerType trigger)
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

        private Player activePlayer;
        private float maxIntensity = 0f;
        private bool active = true;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            smokeEffect = Instantiate(Assets.SmokeEffect, canvas.transform).GetComponent<CanvasGroup>();

            activePlayer = (from p in PlayerManager.instance.players
                            where p.data.view.IsMine && !p.data.GetAdditionalData().isAIMinion
                            select p).FirstOrDefault();

            if (!activePlayer)
            {
                Remove();
                return;
            }


        }

        private void Start()
        {
            StartCoroutine(FadeInOut());
            PlayerManager.instance.AddPlayerDiedAction(OnPlayerDeath);
        }

        private void OnDestroy()
        {
            PlayerManager.instance.RemovePlayerDiedAction(OnPlayerDeath);
        }

        private void OnPlayerDeath(Player deadPlayer, int deadPlayersCount)
        {
            if (activePlayer.playerID != deadPlayer.playerID) return;

            Remove();
        }

        private void Update()
        {
            if (!active) return;

            var distanceScalar = 1 - (Vector3.Distance(activePlayer.transform.position, transform.position) / RANGE);
            smokeEffect.alpha = maxIntensity * Math.Max(0, Math.Min(1, distanceScalar * 1.5f));

            if (activePlayer.data.dead) Remove();
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

        public void Remove(float fullDestroyAfter = 5f)
        {
            if (!active) return;

            StopAllCoroutines();
            active = false;
            particles.Stop();
            Destroy(smokeEffect.gameObject);
            Destroy(gameObject, fullDestroyAfter);
        }
    }
}
