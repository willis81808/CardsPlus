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
    public class HamperCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (!player.data.view.IsMine) return;

            var handler = player.GetComponent<CyberpunkHandler>();
            if (!handler)
            {
                player.gameObject.AddComponent<CyberpunkHandler>();
            }

            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.HAMPER));
        }

        public override void OnRemoveCard()
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.HAMPER);
        }

        protected override string GetTitle() => "Hamper";
        protected override string GetDescription() => $"<color=\"purple\">Quickhack</color>\n<color=\"red\">{QuickhackMenuOption.Costs[QuickhackMenuOption.QuickhackType.HAMPER]} RAM</color>\nHalf your target's movement speed for 5 seconds";
        public override string GetModName() => "Cards+";
        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;
        protected override CardThemeColor.CardThemeColorType GetTheme() => CardThemeColor.CardThemeColorType.FirepowerYellow;
        protected override CardInfoStat[] GetStats() => null;
        protected override GameObject GetCardArt() => Assets.HamperArt;

        public static void DoQuickHack(Player target)
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
