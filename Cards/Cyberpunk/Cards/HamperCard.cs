using ModsPlus;
using ModdingUtils.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Networking;
using UnityEngine;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class HamperCard : CustomEffectCard<CyberpunkHandler>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Hamper",
            Description = $"<color=\"purple\">Quickhack</color>\n" +
                          $"<color=\"red\">{QuickhackMenuOption.Costs[QuickhackMenuOption.QuickhackType.HAMPER]} RAM</color>\n" +
                          $"Half your target's movement speed for 5 seconds",
            ModName     = "Cards+",
            Art         = Assets.HamperArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.FirepowerYellow,
            OwnerOnly   = true
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.HAMPER));
        }

        protected override void Removed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.HAMPER);
        }

        public static void DoQuickHack(Player target, Player source)
        {
            var delta = target.data.movement.force / 2;
            NetworkingManager.RPC(typeof(HamperCard), nameof(RPC_ApplySlow), target.playerID, delta);
        }

        [UnboundRPC]
        private static void RPC_ApplySlow(int playerId, float forceDelta)
        {
            var target = PlayerManager.instance.players.Where(p => p.playerID == playerId).First();
            target.gameObject.AddComponent<TemporarySlowModifier>()
                .Initialize(5f, 0.5f);
        }
    }

    public class TemporarySlowModifier : ReversibleEffect
    {
        private float duration;
        private float speedMultiplier;

        public void Initialize(float duration, float speedMultiplier)
        {
            this.duration = duration;
            this.speedMultiplier = speedMultiplier;
        }

        public override void OnStart()
        {
            characterStatModifiersModifier.movementSpeed_mult = speedMultiplier;
            StartCoroutine(DoCountdown());
        }

        private IEnumerator DoCountdown()
        {
            ApplyModifiers();
            yield return new WaitForSeconds(duration);
            Destroy();
        }
    }
}
